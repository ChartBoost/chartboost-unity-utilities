@file:Suppress("PackageDirectoryMismatch")
package com.chartboost.unity.utilities.view

import android.app.Activity
import android.os.Handler
import android.os.Looper
import android.content.res.Resources
import com.unity3d.player.UnityPlayer

/**
 * Shared Unity/Android view + activity helpers used across the Chartboost Unity plugin bridges
 * (Mediation, Monetization). Extracted so each bridge stops re-implementing the same accessors.
 */
object UnityViewUtils {
    private val mainHandler = Handler(Looper.getMainLooper())

    /** Supplies the current activity; a seam so JVM tests can provide one without Unity. */
    internal var activityProvider: () -> Activity? = { UnityPlayer.currentActivity }

    /** The current Unity activity, or null if unavailable. */
    fun currentActivity(): Activity? = activityProvider()

    /**
     * Display density (px per dp): the current activity's, else the system's, which needs no activity.
     * This is the UI scale factor Unity uses to convert dp positions to pixels.
     */
    fun displayDensity(): Float =
        currentActivity()?.resources?.displayMetrics?.density ?: Resources.getSystem().displayMetrics.density

    /**
     * Runs [block] on the Android UI thread: inline when already there, otherwise posted in call order.
     * Unity's main thread is NOT the Android UI thread, so any View mutation crossing JNI goes through here.
     */
    fun runOnUiThread(block: () -> Unit) {
        if (Looper.myLooper() == Looper.getMainLooper()) block() else mainHandler.post(block)
    }
}
