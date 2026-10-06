@file:Suppress("PackageDirectoryMismatch")
package com.chartboost.unity.utilities.banner

import android.view.Gravity
import android.view.View
import android.view.ViewGroup
import android.widget.FrameLayout

/**
 * Reparents a banner view into an automation/host container (e.g. the maestro
 * <c>automation_banner_container</c> overlay slot). Shared by the Chartboost Mediation and
 * Monetization wrappers — the HB-11391 banner-hosting mechanism, generalized to any [View].
 */
object BannerHost {
    /**
     * Reparents [view] into [host] (no-op when already there), centered, clearing any stale translation
     * (e.g. left over from the wrapper's own off-screen positioning). The host owns position only: an
     * existing width/height is carried across the reparent, never reset. Must be called on the UI thread.
     */
    fun attachToHost(view: View, host: ViewGroup) {
        if (view.parent !== host) {
            // Keep the size: the Monetization SDK sizes its banner once, during show, and a WRAP_CONTENT reset
            // lets the MATCH_PARENT ad child stretch it to the whole slot (creative at the top, full-width).
            val old = view.layoutParams
            detachFromParent(view)
            host.addView(view, old?.width ?: ViewGroup.LayoutParams.WRAP_CONTENT, old?.height ?: ViewGroup.LayoutParams.WRAP_CONTENT)
        }
        view.translationX = 0f
        view.translationY = 0f
        (view.layoutParams as? FrameLayout.LayoutParams)?.let {
            it.gravity = Gravity.CENTER
            view.layoutParams = it
        }
    }

    /** Removes [view] from its current parent, if any. */
    fun detachFromParent(view: View) {
        (view.parent as? ViewGroup)?.removeView(view)
    }
}
