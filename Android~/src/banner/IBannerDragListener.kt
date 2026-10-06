@file:Suppress("PackageDirectoryMismatch")
package com.chartboost.unity.utilities.banner

/**
 * Listener for banner drag events, used by [BannerLayout] to communicate drag gestures to the
 * hosting bridge. Provides low-level drag callbacks (origin x/y in pixels) that the bridge forwards
 * to its SDK-specific banner listener with additional context. Shared across the Chartboost Unity
 * wrappers.
 */
interface IBannerDragListener {

    /**
     * Called when a drag gesture begins, after the touch movement exceeds the drag threshold.
     *
     * @param x The X coordinate of the banner's origin in pixels
     * @param y The Y coordinate of the banner's origin in pixels
     */
    fun onDragBegin(x: Float, y: Float)

    /**
     * Called continuously while the banner is being dragged (once per throttled touch move).
     *
     * @param x The current X coordinate of the banner's origin in pixels
     * @param y The current Y coordinate of the banner's origin in pixels
     */
    fun onDrag(x: Float, y: Float)

    /**
     * Called when a drag gesture ends (finger lifted or the drag is cancelled).
     *
     * @param x The final X coordinate of the banner's origin in pixels
     * @param y The final Y coordinate of the banner's origin in pixels
     */
    fun onDragEnd(x: Float, y: Float)
}
