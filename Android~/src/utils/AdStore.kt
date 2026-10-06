@file:Suppress("PackageDirectoryMismatch")
package com.chartboost.unity.utilities.utils

import java.util.concurrent.ConcurrentHashMap

/**
 * Thread-safe map that retains native ad objects so they aren't garbage-collected while Unity holds a
 * managed handle, and looks them up by an integer identity (typically the object's hashCode) for
 * cache/show/destroy calls coming from C#.
 *
 * Instantiable so callers can keep isolated stores (e.g. one per ad type). Dependency-free — the native
 * analog of the managed `Chartboost.Caching.AdCache<T>`. Shared by the Chartboost Mediation and
 * Monetization Unity wrappers.
 */
class AdStore<T : Any> {
    private val ads = ConcurrentHashMap<Int, T>()

    /** Retains [value] under [key] (typically `value.hashCode()`). */
    fun track(key: Int, value: T) {
        ads[key] = value
    }

    /** Returns the ad retained under [key], or null if none. */
    fun get(key: Int): T? = ads[key]

    /** Releases and returns the ad retained under [key], or null if none. */
    fun remove(key: Int): T? = ads.remove(key)

    /** A snapshot of the retained ads, for bulk cleanup before [clear]. */
    fun values(): Collection<T> = ArrayList(ads.values)

    /** Releases every retained ad. */
    fun clear() = ads.clear()

    /** The number of ads currently retained. */
    val size: Int get() = ads.size
}
