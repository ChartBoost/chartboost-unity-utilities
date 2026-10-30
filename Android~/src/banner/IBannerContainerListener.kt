@file:Suppress("PackageDirectoryMismatch")
package com.chartboost.unity.utilities.banner

/**
 * Process-wide listener for every [BannerContainer], keyed by [BannerContainer.containerId] so Unity
 * needs a single proxy. Called on the Android UI thread.
 */
interface IBannerContainerListener {
    /** A drag started; [x]/[y] are the container's origin in its parent, in pixels. */
    fun onDragBegin(containerId: Int, x: Float, y: Float)

    /** The container moved during a drag (throttled to ~60 per second), in pixels. */
    fun onDrag(containerId: Int, x: Float, y: Float)

    /** The drag ended or was cancelled: origin [x]/[y] in pixels, stored [positionX]/[positionY] in dp. */
    fun onDragEnd(containerId: Int, x: Float, y: Float, positionX: Float, positionY: Float)

    /** The container's frame in window coordinates changed, in dp. */
    fun onBoundsChanged(containerId: Int, x: Float, y: Float, width: Float, height: Float)
}
