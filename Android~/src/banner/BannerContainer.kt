@file:Suppress("PackageDirectoryMismatch")
package com.chartboost.unity.utilities.banner

import android.annotation.SuppressLint
import android.content.Context
import android.content.res.Configuration
import android.os.Build
import android.view.Gravity
import android.view.MotionEvent
import android.view.View
import android.view.ViewGroup
import android.view.WindowInsets
import android.widget.FrameLayout
import com.chartboost.unity.utilities.view.UnityViewUtils
import java.util.concurrent.atomic.AtomicInteger
import kotlin.math.hypot
import kotlin.math.roundToInt

/**
 * Shared host for one SDK banner view. The SDK view is the only child and never changes parent; this
 * container is what gets placed, sized, shown, dragged and moved between hosts.
 *
 * Setters may be called from any thread: state is stored immediately and applied on the UI thread, in
 * call order. Units are dp, except drag callbacks, which report pixels.
 */
@SuppressLint("ViewConstructor")
class BannerContainer private constructor(context: Context, initialContent: View?) : FrameLayout(context) {

    companion object {
        /** Size value meaning "wrap the content". */
        const val WRAP = -1f

        // BannerAnchor in C#; values are explicit there too.
        const val ANCHOR_TOP_LEFT = 0
        const val ANCHOR_TOP_CENTER = 1
        const val ANCHOR_TOP_RIGHT = 2
        const val ANCHOR_CENTER = 3
        const val ANCHOR_BOTTOM_LEFT = 4
        const val ANCHOR_BOTTOM_CENTER = 5
        const val ANCHOR_BOTTOM_RIGHT = 6

        // BannerHorizontalAlignment / BannerVerticalAlignment in C#.
        const val ALIGN_START = 0
        const val ALIGN_CENTER = 1
        const val ALIGN_END = 2

        private const val DRAG_THRESHOLD_PX = 10f
        private const val DRAG_CALLBACK_INTERVAL_NS = 16_000_000L
        private const val MIN_VISIBLE_WIDE_FRACTION = 0.6f

        private val nextId = AtomicInteger(1)

        /** Receives drag and bounds events for every container. Set once by the Unity bridge. */
        @JvmStatic
        @Volatile
        var listener: IBannerContainerListener? = null

        /**
         * Wraps [content] in a new, detached container. Call on the UI thread. The content keeps its own
         * width/height (some SDKs size their view on show); the container only sets its gravity.
         */
        @JvmStatic
        fun wrap(content: View): BannerContainer {
            content.removeFromParent()
            return BannerContainer(content.context, content)
        }

        /** The container holding [view], or null when it is not in one. */
        @JvmStatic
        fun of(view: View): BannerContainer? = view.parent as? BannerContainer

        /**
         * Creates an empty container on any thread, for SDKs whose view is built later on the UI thread
         * (a detached view may be constructed off it). Add the SDK view with [setContent].
         */
        @JvmStatic
        fun create(context: Context): BannerContainer = BannerContainer(context, null)

        internal fun anchorGravity(anchor: Int): Int = when (anchor) {
            ANCHOR_TOP_LEFT -> Gravity.TOP or Gravity.LEFT
            ANCHOR_TOP_CENTER -> Gravity.TOP or Gravity.CENTER_HORIZONTAL
            ANCHOR_TOP_RIGHT -> Gravity.TOP or Gravity.RIGHT
            ANCHOR_CENTER -> Gravity.CENTER
            ANCHOR_BOTTOM_LEFT -> Gravity.BOTTOM or Gravity.LEFT
            ANCHOR_BOTTOM_RIGHT -> Gravity.BOTTOM or Gravity.RIGHT
            else -> Gravity.BOTTOM or Gravity.CENTER_HORIZONTAL
        }

        internal fun alignmentGravity(horizontal: Int, vertical: Int): Int {
            val h = when (horizontal) { ALIGN_START -> Gravity.LEFT; ALIGN_END -> Gravity.RIGHT; else -> Gravity.CENTER_HORIZONTAL }
            val v = when (vertical) { ALIGN_START -> Gravity.TOP; ALIGN_END -> Gravity.BOTTOM; else -> Gravity.CENTER_VERTICAL }
            return h or v
        }

        private fun View.removeFromParent() {
            (parent as? ViewGroup)?.removeView(this)
        }
    }

    /** The SDK view: the only child. */
    var content: View? = null
        private set

    /** Unique id the Unity side uses to route listener callbacks. */
    val containerId: Int = nextId.getAndIncrement()

    @Volatile private var absolute = false
    @Volatile private var anchor = ANCHOR_BOTTOM_CENTER
    @Volatile private var safeArea = true
    @Volatile private var positionX = 0f
    @Volatile private var positionY = 0f
    @Volatile private var pivotX = 0f
    @Volatile private var pivotY = 0f
    @Volatile private var sizeWidth = WRAP
    @Volatile private var sizeHeight = WRAP
    @Volatile private var alignH = ALIGN_CENTER
    @Volatile private var alignV = ALIGN_CENTER
    @Volatile private var clipContent = true
    @Volatile private var interactionEnabled = true
    @Volatile private var draggable = false
    @Volatile private var host: ViewGroup? = null

    private var insetLeft = 0
    private var insetTop = 0
    private var insetRight = 0
    private var insetBottom = 0
    private val location = IntArray(2)
    private val reportedBounds = IntArray(4) { -1 }

    private var isDragging = false
    private var relayoutPending = false
    private var dragStartX = 0f
    private var dragStartY = 0f
    private var dragLastX = 0f
    private var dragLastY = 0f
    private var lastDragCallbackNs = 0L

    private val density: Float get() = resources.displayMetrics.density
    private val isHosted: Boolean get() = host != null && parent === host

    init {
        initialContent?.let { adopt(it) }
    }

    /** Puts [view] in as the only child, replacing any previous content. UI thread via [ui]. */
    fun setContent(view: View) = ui {
        if (content === view) return@ui
        content?.let { removeView(it) }
        view.removeFromParent()
        adopt(view)
        applyLayout()
    }

    private fun adopt(view: View) {
        // Keep the content's own size (Prebid sizes its view before wrapping; Monetization sizes it on show).
        val old = view.layoutParams
        addView(view, LayoutParams(old?.width ?: LayoutParams.WRAP_CONTENT, old?.height ?: LayoutParams.WRAP_CONTENT, alignmentGravity(alignH, alignV)))
        view.isClickable = true
        content = view
    }

    // region Public API (any thread)

    /** Anchors the container to an edge or corner of its root parent, inset by the safe area when [useSafeArea]. */
    fun setAnchor(anchor: Int, useSafeArea: Boolean) {
        this.anchor = anchor
        this.safeArea = useSafeArea
        absolute = false
        ui { applyLayout() }
    }

    /** Places the container at [x]/[y] dp in its root parent, offset by [pivotX]/[pivotY] (0..1) of its size. */
    fun setAbsolute(x: Float, y: Float, pivotX: Float, pivotY: Float) {
        val wasAbsolute = absolute
        positionX = x
        positionY = y
        this.pivotX = pivotX
        this.pivotY = pivotY
        absolute = true
        // Already absolute: only the translation moves, so skip the container relayout.
        ui {
            val wasOffscreen = isOffscreen()
            // A translation-only move skips onLayout, so report the new bounds here (deduped).
            if (wasAbsolute && !isHosted) { applyTranslation(); reportBounds() } else applyLayout()
            if (wasOffscreen && !isOffscreen()) scheduleContentRelayout()
        }
    }

    /** Sets the container size in dp; [WRAP] on an axis wraps the content. */
    fun setSize(width: Float, height: Float) {
        sizeWidth = width
        sizeHeight = height
        ui { applyLayout() }
    }

    /** Aligns the content inside the container: [ALIGN_START], [ALIGN_CENTER] or [ALIGN_END]. */
    fun setAlignment(horizontal: Int, vertical: Int) {
        alignH = horizontal
        alignV = vertical
        ui { applyContentGravity() }
    }

    /** Sets the container background (ARGB). */
    fun setContainerBackgroundColor(argb: Int) = ui { setBackgroundColor(argb) }

    /** Clips the content to the container when true (contained); false lets it draw past (overflow). */
    fun setClipContent(clip: Boolean) {
        clipContent = clip
        ui { clipChildren = clip; clipToPadding = clip; requestLayout() }
    }

    /** Shows the container (VISIBLE) or hides it (INVISIBLE, keeping its size). The content stays VISIBLE. */
    fun setShown(shown: Boolean) = ui {
        val wasHidden = visibility != VISIBLE
        visibility = if (shown) VISIBLE else INVISIBLE
        if (shown && wasHidden) scheduleContentRelayout()
    }

    // A creative last laid out hidden or off-screen stays blank when only shown or moved; redo its layout and draw.
    private fun relayoutContent() {
        content?.let { forceLayoutTree(it); requestLayout(); it.invalidate() }
    }

    // Fully outside the parent, e.g. the old orientation's position right after a rotation.
    private fun isOffscreen(): Boolean {
        val p = parent as? View ?: return false
        if (width == 0 || height == 0) return false // not laid out yet, so nothing stale to redo
        val x = left + translationX
        val y = top + translationY
        return x + width <= 0 || y + height <= 0 || x >= p.width || y >= p.height
    }

    // One relayout per frame, however many moves land in it.
    private fun scheduleContentRelayout() {
        if (relayoutPending) return
        relayoutPending = true
        post {
            relayoutPending = false
            relayoutContent()
        }
    }

    private fun forceLayoutTree(view: View) {
        view.forceLayout()
        (view as? ViewGroup)?.let { group -> for (i in 0 until group.childCount) forceLayoutTree(group.getChildAt(i)) }
    }

    /** When false, touches pass through to whatever is behind the container. */
    fun setInteractionEnabled(enabled: Boolean) {
        interactionEnabled = enabled
    }

    /** Enables dragging in root mode. Disabling cancels an active drag. */
    fun setDraggable(draggable: Boolean) {
        this.draggable = draggable
        if (!draggable) ui { endDrag() }
    }

    /**
     * Moves the container into [newHost], centred by the host, or back to the root parent at its stored
     * placement when null (a no-op before the first attach). Centring needs a FrameLayout host.
     */
    fun setHost(newHost: ViewGroup?) {
        host = newHost
        ui {
            if (newHost != null) moveTo(newHost)
            else if (parent != null) attachToRoot()
        }
    }

    /** [setHost] with null; Unity's JNI cannot pass a bare null argument. */
    fun clearHost() = setHost(null)

    /** Attaches the container if it is not attached yet: into its host, else the root parent. */
    fun attach() = ui {
        if (parent != null) return@ui
        host?.let { moveTo(it) } ?: attachToRoot()
    }

    /** Removes the container from its parent; the content stays inside it. */
    fun detach() = ui { removeFromParent() }

    /** Detaches and releases the content. The wrapper tears down the SDK view itself. */
    fun dispose() = ui {
        endDrag()
        removeFromParent()
        content?.let { removeView(it) }
        content = null
        host = null
    }

    // endregion

    // region Layout (UI thread)

    private fun ui(block: () -> Unit) = UnityViewUtils.runOnUiThread(block)

    private fun attachToRoot() {
        val root = UnityViewUtils.currentActivity()?.findViewById<ViewGroup>(android.R.id.content) ?: return
        // Above the newest banner (else just above the Unity view at 0), below overlays added later with addContentView.
        val topBanner = (root.childCount - 1 downTo 0).firstOrNull { root.getChildAt(it).let { v -> v is BannerContainer && v !== this } }
        moveTo(root, topBanner?.plus(1) ?: minOf(1, root.childCount))
    }

    private fun moveTo(target: ViewGroup, index: Int = -1) {
        if (parent !== target) {
            removeFromParent()
            // The parent converts these to its own LayoutParams type.
            target.addView(this, index, ViewGroup.LayoutParams(ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT))
        }
        applyLayout()
    }

    private fun dimension(dp: Float): Int = if (dp < 0f) ViewGroup.LayoutParams.WRAP_CONTENT else (dp * density).roundToInt()

    private fun applyLayout() {
        val lp = layoutParams ?: return
        var changed = false
        val width = dimension(sizeWidth)
        val height = dimension(sizeHeight)
        if (lp.width != width || lp.height != height) {
            lp.width = width
            lp.height = height
            changed = true
        }
        (lp as? LayoutParams)?.let {
            val anchored = !isHosted && !absolute
            val gravity = when {
                isHosted -> Gravity.CENTER
                absolute -> Gravity.TOP or Gravity.LEFT
                else -> anchorGravity(anchor)
            }
            val inset = anchored && safeArea
            val l = if (inset) insetLeft else 0
            val t = if (inset) insetTop else 0
            val r = if (inset) insetRight else 0
            val b = if (inset) insetBottom else 0
            if (it.gravity != gravity || it.leftMargin != l || it.topMargin != t || it.rightMargin != r || it.bottomMargin != b) {
                it.gravity = gravity
                it.setMargins(l, t, r, b)
                changed = true
            }
        }
        // Assigning layoutParams requests a layout; skip it when nothing moved.
        if (changed) layoutParams = lp
        applyContentGravity()
        applyTranslation()
    }

    private fun applyTranslation() {
        if (isDragging) return
        if (absolute && !isHosted) {
            translationX = positionX * density - pivotX * width
            translationY = positionY * density - pivotY * height
        } else {
            translationX = 0f
            translationY = 0f
        }
    }

    private fun applyContentGravity() {
        // Gravity only: never touch the content's width/height.
        val view = content ?: return
        val lp = view.layoutParams as? LayoutParams ?: return
        val gravity = alignmentGravity(alignH, alignV)
        if (lp.gravity == gravity) return
        lp.gravity = gravity
        view.layoutParams = lp
    }

    override fun onMeasure(widthMeasureSpec: Int, heightMeasureSpec: Int) {
        // Overflow: a wrapping axis grows past the parent to the content's full size (as on iOS).
        val lp = layoutParams
        if (clipContent || lp == null) return super.onMeasure(widthMeasureSpec, heightMeasureSpec)
        super.onMeasure(unbounded(widthMeasureSpec, lp.width), unbounded(heightMeasureSpec, lp.height))
    }

    private fun unbounded(spec: Int, dimension: Int): Int =
        if (dimension == ViewGroup.LayoutParams.WRAP_CONTENT) MeasureSpec.makeMeasureSpec(0, MeasureSpec.UNSPECIFIED) else spec

    override fun measureChildWithMargins(child: View, parentWidthSpec: Int, widthUsed: Int, parentHeightSpec: Int, heightUsed: Int) {
        if (clipContent) {
            super.measureChildWithMargins(child, parentWidthSpec, widthUsed, parentHeightSpec, heightUsed)
            return
        }
        // Overflow: the content keeps its full size, never capped by the container, and draws past it.
        val lp = child.layoutParams
        child.measure(childSpec(lp.width), childSpec(lp.height))
    }

    private fun childSpec(dimension: Int): Int =
        if (dimension >= 0) MeasureSpec.makeMeasureSpec(dimension, MeasureSpec.EXACTLY)
        else MeasureSpec.makeMeasureSpec(0, MeasureSpec.UNSPECIFIED)

    override fun onLayout(changed: Boolean, left: Int, top: Int, right: Int, bottom: Int) {
        super.onLayout(changed, left, top, right, bottom)
        applyTranslation() // the pivot depends on the measured size
        reportBounds()
    }

    private fun reportBounds() {
        val l = listener ?: return
        getLocationInWindow(location) // includes translation
        val r = reportedBounds
        // Runs on every move, so compare in place and only report a change.
        if (r[0] == location[0] && r[1] == location[1] && r[2] == width && r[3] == height) return
        r[0] = location[0]; r[1] = location[1]; r[2] = width; r[3] = height
        val d = density
        l.onBoundsChanged(containerId, r[0] / d, r[1] / d, r[2] / d, r[3] / d)
    }

    // endregion

    // region Insets

    override fun onAttachedToWindow() {
        super.onAttachedToWindow()
        readInsets()
        applyLayout()
    }

    override fun onConfigurationChanged(newConfig: Configuration?) {
        super.onConfigurationChanged(newConfig)
        post { if (readInsets()) applyLayout() }
    }

    override fun onApplyWindowInsets(insets: WindowInsets?): WindowInsets? {
        // System bars can change without a configuration change.
        if (readInsets()) post { applyLayout() }
        return super.onApplyWindowInsets(insets)
    }

    /** Window insets (system bars + cutout) minus the part the parent already sits inside of. True when they changed. */
    private fun readInsets(): Boolean {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M) return false
        val insets = rootWindowInsets ?: return false
        var l: Int; var t: Int; var r: Int; var b: Int
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            val i = insets.getInsets(WindowInsets.Type.systemBars() or WindowInsets.Type.displayCutout())
            l = i.left; t = i.top; r = i.right; b = i.bottom
        } else {
            @Suppress("DEPRECATION")
            run { l = insets.systemWindowInsetLeft; t = insets.systemWindowInsetTop; r = insets.systemWindowInsetRight; b = insets.systemWindowInsetBottom }
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) insets.displayCutout?.let {
                l = maxOf(l, it.safeInsetLeft); t = maxOf(t, it.safeInsetTop); r = maxOf(r, it.safeInsetRight); b = maxOf(b, it.safeInsetBottom)
            }
        }
        val p = parent as? View ?: return false
        p.getLocationInWindow(location)
        val root = rootView
        val newLeft = maxOf(0, l - location[0])
        val newTop = maxOf(0, t - location[1])
        val newRight = maxOf(0, r - (root.width - location[0] - p.width))
        val newBottom = maxOf(0, b - (root.height - location[1] - p.height))
        if (newLeft == insetLeft && newTop == insetTop && newRight == insetRight && newBottom == insetBottom) return false
        insetLeft = newLeft; insetTop = newTop; insetRight = newRight; insetBottom = newBottom
        return true
    }

    // endregion

    // region Touch and drag

    override fun dispatchTouchEvent(event: MotionEvent?): Boolean =
        if (interactionEnabled) super.dispatchTouchEvent(event) else false

    override fun onInterceptTouchEvent(event: MotionEvent?): Boolean {
        event ?: return super.onInterceptTouchEvent(event)
        if (!draggable || isHosted) return super.onInterceptTouchEvent(event)
        when (event.actionMasked) {
            MotionEvent.ACTION_DOWN -> {
                dragStartX = event.rawX; dragStartY = event.rawY
                dragLastX = dragStartX; dragLastY = dragStartY
                isDragging = false
            }
            MotionEvent.ACTION_MOVE -> {
                trackMove(event)
                return isDragging
            }
            MotionEvent.ACTION_UP, MotionEvent.ACTION_CANCEL -> {
                val wasDragging = isDragging
                endDrag()
                return wasDragging
            }
        }
        return super.onInterceptTouchEvent(event)
    }

    @SuppressLint("ClickableViewAccessibility")
    override fun onTouchEvent(event: MotionEvent?): Boolean {
        // Once intercepted, the rest of the gesture arrives here.
        if (!isDragging || event == null) return super.onTouchEvent(event)
        when (event.actionMasked) {
            MotionEvent.ACTION_MOVE -> trackMove(event)
            MotionEvent.ACTION_UP, MotionEvent.ACTION_CANCEL -> endDrag()
        }
        return true
    }

    private fun trackMove(event: MotionEvent) {
        val dx = event.rawX - dragLastX
        val dy = event.rawY - dragLastY
        dragLastX = event.rawX; dragLastY = event.rawY
        if (!isDragging && hypot(dragLastX - dragStartX, dragLastY - dragStartY) > DRAG_THRESHOLD_PX) {
            isDragging = true
            lastDragCallbackNs = System.nanoTime()
            listener?.onDragBegin(containerId, x, y)
        }
        if (isDragging) dragBy(dx, dy)
    }

    // Moves x/y (left + translation), so it works from any layout origin, anchored or absolute.
    private fun dragBy(dx: Float, dy: Float) {
        val p = parent as? View ?: return
        val w = width.toFloat()
        val h = height.toFloat()
        val safeWidth = (p.width - insetLeft - insetRight).toFloat()
        val minX: Float
        val maxX: Float
        if (w > safeWidth) {
            // Wider than the safe area: allow overhang but keep 60% on screen.
            val visible = minOf(w * MIN_VISIBLE_WIDE_FRACTION, safeWidth)
            minX = insetLeft - (w - visible)
            maxX = p.width - insetRight - visible
        } else {
            minX = insetLeft.toFloat()
            maxX = p.width - insetRight - w
        }
        val minY = insetTop.toFloat()
        val maxY = p.height - insetBottom - h
        x = (x + dx).coerceIn(minX, maxOf(minX, maxX))
        y = (y + dy).coerceIn(minY, maxOf(minY, maxY))
        val now = System.nanoTime()
        if (now - lastDragCallbackNs >= DRAG_CALLBACK_INTERVAL_NS) {
            lastDragCallbackNs = now
            listener?.onDrag(containerId, x, y)
        }
    }

    private fun endDrag() {
        if (!isDragging) return
        isDragging = false
        // Keep the dropped spot as an absolute placement: refreshes and unhost restore it.
        if (!absolute) {
            pivotX = 0f
            pivotY = 0f
            absolute = true
        }
        val droppedX = x
        val droppedY = y
        positionX = (droppedX + pivotX * width) / density
        positionY = (droppedY + pivotY * height) / density
        listener?.onDragEnd(containerId, droppedX, droppedY, positionX, positionY)
        applyLayout()
        // Until the next layout pass moves left/top to the absolute origin, hold the dropped spot.
        x = droppedX
        y = droppedY
        reportBounds()
    }

    // endregion
}
