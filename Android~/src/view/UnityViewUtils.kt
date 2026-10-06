@file:Suppress("PackageDirectoryMismatch")
package com.chartboost.unity.utilities.view

import android.app.Activity
import android.util.DisplayMetrics
import com.unity3d.player.UnityPlayer

/**
 * Shared Unity/Android view + activity helpers used across the Chartboost Unity plugin bridges
 * (Mediation, Monetization). Extracted so each bridge stops re-implementing the same accessors.
 */
object UnityViewUtils {
    /** The current Unity activity, or null if unavailable. */
    fun currentActivity(): Activity? = UnityPlayer.currentActivity

    /**
     * Display density (px per dp) of the current activity, or the platform default if the activity
     * is unavailable. This is the UI scale factor Unity uses to convert dp positions to pixels.
     */
    fun displayDensity(): Float {
        val activity = currentActivity() ?: return DisplayMetrics.DENSITY_DEFAULT.toFloat()
        return activity.resources?.displayMetrics?.density ?: DisplayMetrics.DENSITY_DEFAULT.toFloat()
    }

    /**
     * Runs [block] on the Android UI thread. Unity's main thread is NOT the Android UI thread, so any
     * View mutation crossing the JNI boundary must be dispatched here. No-op if no activity is available.
     */
    fun runOnUiThread(block: () -> Unit) {
        currentActivity()?.runOnUiThread { block() }
    }
}
