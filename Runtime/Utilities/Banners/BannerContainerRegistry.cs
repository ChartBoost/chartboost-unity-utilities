using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace Chartboost.Banners
{
    /// <summary>
    /// Routes native container callbacks (any thread) to the live <see cref="NativeBannerContainer"/> on the
    /// Unity main thread. Entries are weak, so an undisposed container can still be collected.
    /// </summary>
    internal static class BannerContainerRegistry
    {
        // Not AdCache<T>: it warns on a miss, and late callbacks after Dispose are expected here.
        private static readonly ConcurrentDictionary<int, WeakReference<NativeBannerContainer>> Containers = new();
        private static SynchronizationContext _mainContext;
        private static int _mainThreadId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CaptureMainThread() => CaptureMainThread(SynchronizationContext.Current);

        /// <summary>Test seam: sets the context callbacks are posted to, as if called from the main thread.</summary>
        internal static void CaptureMainThread(SynchronizationContext context)
        {
            _mainContext = context;
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        internal static int Count => Containers.Count;

        internal static void Register(int id, NativeBannerContainer container)
            => Containers[id] = new WeakReference<NativeBannerContainer>(container);

        internal static void Release(int id) => Containers.TryRemove(id, out _);

        /// <summary>Runs <paramref name="action"/> for container <paramref name="id"/> on the main thread, if it is still alive.</summary>
        internal static void Dispatch(int id, Action<NativeBannerContainer> action)
        {
            if (!Containers.TryGetValue(id, out var weak) || !weak.TryGetTarget(out var container))
            {
                Release(id);
                return;
            }

            if (_mainContext == null || Thread.CurrentThread.ManagedThreadId == _mainThreadId)
            {
                Invoke(container, action);
                return;
            }

            _mainContext.Post(_ => Invoke(container, action), null);
        }

        private static void Invoke(NativeBannerContainer container, Action<NativeBannerContainer> action)
        {
            if (container.IsDisposed)
                return;
            try
            {
                action(container);
            }
            catch (Exception e)
            {
                // A subscriber's exception must not escape into the native callback thread.
                Debug.LogException(e);
            }
        }
    }
}
