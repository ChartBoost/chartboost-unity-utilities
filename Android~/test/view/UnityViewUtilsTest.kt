@file:Suppress("PackageDirectoryMismatch")
package com.chartboost.unity.utilities.view

import android.app.Activity
import android.content.res.Resources
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotEquals
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.Robolectric
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config

// xhdpi: density 2.
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [34], qualifiers = "xhdpi")
class UnityViewUtilsTest {

    private val defaultActivityProvider = UnityViewUtils.activityProvider

    @After
    fun tearDown() {
        UnityViewUtils.activityProvider = defaultActivityProvider
    }

    @Test
    fun densityComesFromTheActivity() {
        val activity = Robolectric.buildActivity(Activity::class.java).setup().get()
        activity.resources.displayMetrics.density = 3f // differs from the system's 2, so the source is clear
        assertNotEquals(3f, Resources.getSystem().displayMetrics.density)
        UnityViewUtils.activityProvider = { activity }

        assertEquals(3f, UnityViewUtils.displayDensity())
    }

    // Before the activity exists it used to return DENSITY_DEFAULT (160, a dpi), not a density.
    @Test
    fun withoutAnActivityDensityComesFromTheSystem() {
        UnityViewUtils.activityProvider = { null }

        assertEquals(2f, UnityViewUtils.displayDensity())
    }
}
