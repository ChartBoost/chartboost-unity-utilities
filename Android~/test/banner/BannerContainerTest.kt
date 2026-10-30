@file:Suppress("PackageDirectoryMismatch")
package com.chartboost.unity.utilities.banner

import android.app.Activity
import android.os.Looper
import android.os.SystemClock
import android.view.Gravity
import android.view.MotionEvent
import android.view.View
import android.view.ViewGroup
import android.widget.FrameLayout
import android.widget.LinearLayout
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNull
import org.junit.Assert.assertSame
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import com.chartboost.unity.utilities.view.UnityViewUtils
import org.robolectric.Robolectric
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment
import org.robolectric.Shadows.shadowOf
import org.robolectric.annotation.Config

// xhdpi: density 2, so dp → px conversions are visible.
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [34], qualifiers = "xhdpi")
class BannerContainerTest {

    private val context get() = RuntimeEnvironment.getApplication()
    private lateinit var activity: Activity
    private lateinit var root: FrameLayout
    private lateinit var content: View
    private lateinit var container: BannerContainer
    private val defaultActivityProvider = UnityViewUtils.activityProvider

    @Before
    fun setUp() {
        activity = Robolectric.buildActivity(Activity::class.java).setup().get()
        UnityViewUtils.activityProvider = { activity }
        root = activity.findViewById(android.R.id.content)
        content = View(context).apply { layoutParams = ViewGroup.LayoutParams(300, 100) }
        container = BannerContainer.wrap(content)
    }

    @After
    fun tearDown() {
        UnityViewUtils.activityProvider = defaultActivityProvider
        BannerContainer.listener = null
    }

    @Test
    fun anchorsMapToTheirGravity() {
        val expected = listOf(
            Gravity.TOP or Gravity.LEFT, Gravity.TOP or Gravity.CENTER_HORIZONTAL, Gravity.TOP or Gravity.RIGHT,
            Gravity.CENTER,
            Gravity.BOTTOM or Gravity.LEFT, Gravity.BOTTOM or Gravity.CENTER_HORIZONTAL, Gravity.BOTTOM or Gravity.RIGHT,
        )
        expected.forEachIndexed { anchor, gravity -> assertEquals("anchor $anchor", gravity, BannerContainer.anchorGravity(anchor)) }
        assertEquals("unknown anchors fall back to BottomCenter", expected[5], BannerContainer.anchorGravity(99))
    }

    @Test
    fun ofFindsOnlyTheHoldingContainer() {
        assertSame(container, BannerContainer.of(content))
        val loose = View(context)
        assertNull(BannerContainer.of(loose))
        FrameLayout(context).addView(loose)
        assertNull(BannerContainer.of(loose))
    }

    @Test
    fun alignmentMapsToContentGravity() {
        assertEquals(Gravity.LEFT or Gravity.TOP, BannerContainer.alignmentGravity(0, 0))
        assertEquals(Gravity.CENTER_HORIZONTAL or Gravity.CENTER_VERTICAL, BannerContainer.alignmentGravity(1, 1))
        assertEquals(Gravity.RIGHT or Gravity.BOTTOM, BannerContainer.alignmentGravity(2, 2))
    }

    @Test
    fun wrapKeepsTheContentAsOnlyChildAtItsOwnSize() {
        assertEquals(1, container.childCount)
        assertSame(content, container.getChildAt(0))
        assertSame(content, container.content)
        val lp = content.layoutParams as FrameLayout.LayoutParams
        assertEquals(300, lp.width)
        assertEquals(100, lp.height)
        assertEquals(Gravity.CENTER, lp.gravity)
    }

    @Test
    fun wrapTakesTheContentOutOfItsOldParent() {
        val other = View(context)
        val oldParent = FrameLayout(context).apply { addView(other) }
        val wrapped = BannerContainer.wrap(other)
        assertEquals(0, oldParent.childCount)
        assertSame(wrapped, other.parent)
    }

    @Test
    fun anEmptyContainerTakesItsContentLaterAtTheContentsOwnSize() {
        val empty = BannerContainer.create(context)
        assertNull(empty.content)
        val later = View(context).apply { layoutParams = ViewGroup.LayoutParams(640, 100) }
        empty.setContent(later)

        assertSame(later, empty.content)
        assertEquals(1, empty.childCount)
        assertEquals(640, later.layoutParams.width)
        val replacement = View(context)
        empty.setContent(replacement)
        assertEquals("content is replaced, never stacked", 1, empty.childCount)
        assertNull(later.parent)
    }

    @Test
    fun hostingInAFrameLayoutCentresAndClearsOffsets() {
        val host = FrameLayout(context)
        container.translationX = 50f
        container.setHost(host)

        assertSame(host, container.parent)
        assertEquals(Gravity.CENTER, (container.layoutParams as FrameLayout.LayoutParams).gravity)
        assertEquals(0f, container.translationX)
    }

    @Test
    fun reparentThenResizeKeepsParamsMatchingTheNewParent() {
        // Regression for a5d42d2a: params chosen from stale state threw ClassCastException on resize.
        container.setHost(FrameLayout(context))
        val linear = LinearLayout(context)
        container.setHost(linear)
        container.setSize(320f, 50f)

        assertSame(linear, container.parent)
        val lp = container.layoutParams
        assertTrue("params must match the LinearLayout parent", lp is LinearLayout.LayoutParams)
        assertEquals(640, lp.width)
        assertEquals(100, lp.height)
    }

    @Test
    fun negativeSizeWrapsThatAxis() {
        container.setHost(FrameLayout(context))
        container.setSize(BannerContainer.WRAP, 50f)

        assertEquals(ViewGroup.LayoutParams.WRAP_CONTENT, container.layoutParams.width)
        assertEquals(100, container.layoutParams.height)
    }

    @Test
    fun resizingNeverTouchesTheContentSize() {
        container.setHost(FrameLayout(context))
        container.setSize(400f, 80f)
        container.setAlignment(0, 2)

        val lp = content.layoutParams as FrameLayout.LayoutParams
        assertEquals(300, lp.width)
        assertEquals(100, lp.height)
        assertEquals(Gravity.LEFT or Gravity.BOTTOM, lp.gravity)
    }

    @Test
    fun anchorWithoutSafeAreaUsesGravityAndNoMargins() {
        container.attach()
        container.setAnchor(2, false)

        assertSame(root, container.parent)
        val lp = container.layoutParams as FrameLayout.LayoutParams
        assertEquals(Gravity.TOP or Gravity.RIGHT, lp.gravity)
        assertEquals(listOf(0, 0, 0, 0), listOf(lp.leftMargin, lp.topMargin, lp.rightMargin, lp.bottomMargin))
    }

    @Test
    fun absolutePlacementOffsetsByThePivot() {
        container.attach()
        container.setSize(100f, 50f)
        container.setAbsolute(10f, 20f, 0.5f, 0.5f)
        layOut()

        assertEquals(Gravity.TOP or Gravity.LEFT, (container.layoutParams as FrameLayout.LayoutParams).gravity)
        assertEquals(10f * 2 - 0.5f * 200, container.translationX)
        assertEquals(20f * 2 - 0.5f * 100, container.translationY)
    }

    @Test
    fun clearHostReturnsToTheStoredAnchor() {
        container.attach()
        container.setAnchor(5, false)
        container.setHost(FrameLayout(context))
        container.clearHost()

        assertSame(root, container.parent)
        assertEquals(Gravity.BOTTOM or Gravity.CENTER_HORIZONTAL, (container.layoutParams as FrameLayout.LayoutParams).gravity)
    }

    @Test
    fun clearHostBeforeAttachStaysDetached() {
        container.clearHost()
        assertNull(container.parent)
    }

    @Test
    fun attachIntoTheRootGoesAboveTheFirstChild() {
        val unityView = View(context)
        root.addView(unityView)
        root.addView(View(context)) // e.g. an overlay added later
        container.attach()

        assertEquals(1, root.indexOfChild(container))
    }

    @Test
    fun hiddenKeepsTheSizeAndLeavesTheContentVisible() {
        container.setShown(false)

        assertEquals(View.INVISIBLE, container.visibility)
        assertEquals(View.VISIBLE, content.visibility)
    }

    @Test
    fun showingLaysTheContentOutAgain() {
        val counted = countedContainer()
        counted.second.setShown(false)
        layOut()
        val before = counted.first.layouts

        counted.second.setShown(true)
        idleMainLooper()
        layOut()

        assertEquals("a creative laid out while hidden must lay out again", before + 1, counted.first.layouts)
    }

    @Test
    fun movingBackOnScreenLaysTheContentOutAgain() {
        val counted = countedContainer()
        counted.second.setAbsolute(10f, 5000f, 0f, 0f) // off-screen, like the old orientation's spot mid-rotation
        layOut()
        val before = counted.first.layouts

        counted.second.setAbsolute(10f, 20f, 0f, 0f)
        idleMainLooper()
        layOut()

        assertEquals("a creative last laid out off-screen must lay out again", before + 1, counted.first.layouts)
    }

    @Test
    fun movingWhileOnScreenLeavesTheContentLayoutAlone() {
        val counted = countedContainer()
        layOut()
        idleMainLooper()
        val before = counted.first.layouts

        counted.second.setAbsolute(30f, 40f, 0f, 0f)
        idleMainLooper()
        layOut()

        assertEquals(before, counted.first.layouts)
    }

    @Test
    fun disabledInteractionPassesTouchesThrough() {
        container.setInteractionEnabled(false)
        assertFalse(touch(MotionEvent.ACTION_DOWN, 1f, 1f, dispatch = true))
    }

    @Test
    fun disposeDetachesAndReleasesTheContent() {
        container.attach()
        container.dispose()

        assertNull(container.parent)
        assertNull(content.parent)
    }

    @Test
    fun layoutReportsBoundsInDp() {
        val reported = mutableListOf<FloatArray>()
        BannerContainer.listener = object : NoOpListener() {
            override fun onBoundsChanged(containerId: Int, x: Float, y: Float, width: Float, height: Float) {
                assertEquals(container.containerId, containerId)
                reported += floatArrayOf(width, height)
            }
        }
        container.attach()
        container.setSize(160f, 50f)
        layOut()

        val last = reported.last()
        assertEquals(160f, last[0])
        assertEquals(50f, last[1])
    }

    @Test
    fun movingAnAbsoluteContainerReportsItsNewBounds() {
        val reported = mutableListOf<FloatArray>()
        BannerContainer.listener = object : NoOpListener() {
            override fun onBoundsChanged(containerId: Int, x: Float, y: Float, width: Float, height: Float) {
                reported += floatArrayOf(x, y)
            }
        }
        container.attach()
        container.setSize(160f, 50f)
        container.setAbsolute(10f, 10f, 0f, 0f)
        layOut()

        container.setAbsolute(40f, 20f, 0f, 0f) // translation only: no layout pass

        val last = reported.last()
        assertEquals(40f, last[0], 0.01f)
        assertEquals(20f, last[1], 0.01f)
    }

    @Test
    fun aNewerBannerAttachesAboveOlderOnes() {
        root.addView(View(context)) // the Unity view
        val older = BannerContainer.wrap(View(context)).apply { attach() }
        root.addView(View(context)) // an overlay added later
        val newer = BannerContainer.wrap(View(context)).apply { attach() }

        assertTrue(root.indexOfChild(newer) > root.indexOfChild(older))
        assertTrue("still below the later overlay", root.indexOfChild(newer) < root.childCount - 1)
    }

    @Test
    @Config(qualifiers = "420dpi") // density 2.625: 30 dp is 78.75 px
    fun sizesRoundToTheNearestPixel() {
        container.attach()
        container.setSize(30f, 30f)
        layOut()

        assertEquals(79, container.measuredWidth)
        assertEquals(79, container.measuredHeight)
    }

    @Test
    fun containerIdsAreUnique() {
        val ids = (1..50).map { BannerContainer.wrap(View(context)).containerId }
        assertEquals(ids.size, ids.toSet().size)
    }

    @Test
    fun overflowKeepsWideContentAtItsFullSize() {
        val wide = View(context).apply { minimumWidth = 640 } // wraps to 640 px when unbounded
        val overflow = BannerContainer.wrap(wide)
        overflow.attach()
        overflow.setClipContent(false)
        overflow.setSize(100f, 50f) // 200 px: narrower than the content
        layOut()

        assertEquals(200, overflow.measuredWidth)
        assertEquals("content is not capped to the container", 640, wide.measuredWidth)
        assertFalse(overflow.clipChildren)
    }

    @Test
    fun overflowWrapGrowsPastTheParent() {
        val wider = 1280 // the root is 1080 px
        val wide = View(context).apply { minimumWidth = wider }
        val overflow = BannerContainer.wrap(wide)
        overflow.attach()
        overflow.setClipContent(false)
        layOut()

        assertEquals("a wrapping overflow container is not capped to the parent", wider, overflow.measuredWidth)
    }

    @Test
    fun containedWrapStaysWithinTheParent() {
        val wide = View(context).apply { minimumWidth = 1280 }
        val contained = BannerContainer.wrap(wide)
        contained.attach()
        layOut()

        assertEquals(1080, contained.measuredWidth)
    }

    @Test
    fun firstDragFromAnAnchorStartsWhereTheBannerIs() {
        var began: Pair<Float, Float>? = null
        var ended: FloatArray? = null
        BannerContainer.listener = object : NoOpListener() {
            override fun onDragBegin(containerId: Int, x: Float, y: Float) { began = x to y }
            override fun onDragEnd(containerId: Int, x: Float, y: Float, positionX: Float, positionY: Float) {
                ended = floatArrayOf(x, y, positionX, positionY)
            }
        }
        container.attach()
        container.setAnchor(5, false)
        container.setDraggable(true)
        layOut()
        val startX = container.x
        val startY = container.y

        touch(MotionEvent.ACTION_DOWN, 500f, 1800f)
        touch(MotionEvent.ACTION_MOVE, 500f, 1780f) // past the 10 px threshold: dragged 20 px up

        assertEquals(startX to startY, began)
        assertEquals(startX, container.x)
        assertEquals(startY - 20f, container.y)

        touch(MotionEvent.ACTION_UP, 500f, 1780f)
        assertEquals("the drop keeps its spot", startY - 20f, container.y)
        val dropped = ended!!
        assertEquals(startX / 2, dropped[2]) // stored position in dp, pivot 0
        assertEquals((startY - 20f) / 2, dropped[3])
        layOut()
        assertEquals("and stays there after the switch to absolute", startY - 20f, container.y)
    }

    private fun touch(action: Int, x: Float, y: Float, dispatch: Boolean = false): Boolean {
        val event = MotionEvent.obtain(SystemClock.uptimeMillis(), SystemClock.uptimeMillis(), action, x, y, 0)
        val handled = if (dispatch) container.dispatchTouchEvent(event) else container.onInterceptTouchEvent(event)
        event.recycle()
        return handled
    }

    private open class NoOpListener : IBannerContainerListener {
        override fun onDragBegin(containerId: Int, x: Float, y: Float) = Unit
        override fun onDrag(containerId: Int, x: Float, y: Float) = Unit
        override fun onDragEnd(containerId: Int, x: Float, y: Float, positionX: Float, positionY: Float) = Unit
        override fun onBoundsChanged(containerId: Int, x: Float, y: Float, width: Float, height: Float) = Unit
    }

    private fun idleMainLooper() = shadowOf(Looper.getMainLooper()).idle()

    private class LayoutCountingView(context: android.content.Context) : View(context) {
        var layouts = 0
        override fun onLayout(changed: Boolean, left: Int, top: Int, right: Int, bottom: Int) { layouts++ }
    }

    // An attached, absolutely placed container whose content counts its layout passes.
    private fun countedContainer(): Pair<LayoutCountingView, BannerContainer> {
        val view = LayoutCountingView(context).apply { layoutParams = ViewGroup.LayoutParams(300, 100) }
        val holder = BannerContainer.wrap(view)
        holder.attach()
        holder.setSize(100f, 50f)
        holder.setAbsolute(10f, 20f, 0f, 0f)
        return view to holder
    }

    private fun layOut() {
        root.measure(
            View.MeasureSpec.makeMeasureSpec(1080, View.MeasureSpec.EXACTLY),
            View.MeasureSpec.makeMeasureSpec(1920, View.MeasureSpec.EXACTLY),
        )
        root.layout(0, 0, 1080, 1920)
    }
}
