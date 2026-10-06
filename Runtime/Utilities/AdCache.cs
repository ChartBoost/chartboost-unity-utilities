using System;
using System.Collections.Concurrent;
using Chartboost.Logging;

namespace Chartboost.Caching
{
    /// <summary>
    /// Generic, thread-safe weak-reference cache for native-backed ad objects, keyed by a native
    /// unique identifier (a Java <c>hashCode()</c> on Android / a native pointer on iOS). Handles
    /// concurrent access from native callbacks and the Unity main thread.
    /// <para>
    /// Each closed generic (e.g. <c>AdCache&lt;IAd&gt;</c>) owns an isolated store, so ads from
    /// different SDKs do not collide on a shared key space as long as each SDK caches under a
    /// distinct <typeparamref name="T"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The cached ad reference type (typically an SDK's ad interface).</typeparam>
    public static class AdCache<T> where T : class
    {
        /// <summary>
        /// Weak reference cache to <typeparamref name="T"/> ads. Thread-safe dictionary to handle
        /// concurrent access from native callbacks and the Unity main thread.
        /// </summary>
        private static readonly ConcurrentDictionary<long, WeakReference<T>> Ads = new();

        /// <summary>Cached display name of <typeparamref name="T"/> for diagnostics, computed once per closed generic.</summary>
        private static readonly string TypeName = typeof(T).Name;

        /// <summary>
        /// Keeps track of a <typeparamref name="T"/> ad with a weak reference so it can be disposed by GC.
        /// </summary>
        /// <param name="uniqueId">Associated unique identifier.</param>
        /// <param name="ad">Ad to cache.</param>
        public static void TrackAd(long uniqueId, T ad)
        {
            Ads[uniqueId] = new WeakReference<T>(ad, false);
            LogController.Log($"Tracking {ad.GetType()} with UniqueId: {uniqueId}", LogLevel.Verbose);
        }

        /// <inheritdoc cref="TrackAd(long, T)"/>
        public static void TrackAd(IntPtr uniqueId, T ad)
            => TrackAd(uniqueId.ToInt64(), ad);

        /// <summary>
        /// Retrieves a <typeparamref name="T"/> ad by unique identifier if any.
        /// </summary>
        /// <param name="uniqueId">Associated unique identifier.</param>
        /// <returns>Cached ad, or <c>null</c> if not found or already collected.</returns>
        public static T GetAd(long uniqueId)
        {
            if (!Ads.TryGetValue(uniqueId, out var value))
            {
                LogController.Log($"Failed to get WeakReference<{TypeName}> for: {uniqueId}, reference was most likely disposed, returning null.", LogLevel.Warning);
                return null;
            }

            var found = value.TryGetTarget(out var ad);
            return found ? ad : null;
        }

        /// <inheritdoc cref="GetAd(long)"/>
        public static T GetAd(IntPtr uniqueId)
            => GetAd(uniqueId.ToInt64());

        /// <summary>
        /// Releases a <typeparamref name="T"/> ad from the cache.
        /// </summary>
        /// <param name="uniqueId">Associated unique identifier.</param>
        public static void ReleaseAd(long uniqueId)
        {
            Ads.TryRemove(uniqueId, out _);
        }

        /// <inheritdoc cref="ReleaseAd(long)"/>
        public static void ReleaseAd(IntPtr uniqueId)
            => ReleaseAd(uniqueId.ToInt64());

        /// <summary>
        /// Human-readable snapshot of the cache size, for diagnostics.
        /// </summary>
        public static string CacheInfo() => $"AdCache<{TypeName}>: {Ads.Count} tracked";

        /// <summary>Number of tracked entries. Test/diagnostics seam, not part of the public API.</summary>
        internal static int Count => Ads.Count;

        /// <summary>Removes all tracked entries. Test-only seam; production code must not clear the cache.</summary>
        internal static void ClearForTesting() => Ads.Clear();
    }
}
