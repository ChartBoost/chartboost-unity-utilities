using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Chartboost.Banners
{
    /// <summary>Creates <see cref="NativeBannerContainer"/>s for Android <c>BannerContainer</c> instances.</summary>
    public static class AndroidBannerContainers
    {
        // Static so the proxy outlives every container (an AndroidJavaProxy in local scope can be collected).
        private static readonly ContainerListener Listener = new();
        private static bool _listenerInstalled;

        /// <summary>
        /// Wraps a Kotlin <c>com.chartboost.unity.utilities.banner.BannerContainer</c> created by a wrapper's
        /// bridge. Takes ownership of <paramref name="container"/>: disposing the result disposes it.
        /// </summary>
        public static NativeBannerContainer Wrap(AndroidJavaObject container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            InstallListener();
            return new NativeBannerContainer(new AndroidBannerContainer(container));
        }

        private static void InstallListener()
        {
            if (_listenerInstalled)
                return;
            using var containerClass = new AndroidJavaClass(AndroidBannerContainer.ClassBannerContainer);
            containerClass.CallStatic("setListener", Listener);
            _listenerInstalled = true;
        }

        private sealed class ContainerListener : AndroidJavaProxy
        {
            public ContainerListener() : base("com.chartboost.unity.utilities.banner.IBannerContainerListener") { }

            // Called by JNI on the Android UI thread; [Preserve] keeps IL2CPP stripping from removing them.
            [Preserve]
            public void onDragBegin(int containerId, float x, float y)
                => BannerContainerRegistry.Dispatch(containerId, c => c.OnDragBegan(new Vector2(x, y)));

            [Preserve]
            public void onDrag(int containerId, float x, float y)
                => BannerContainerRegistry.Dispatch(containerId, c => c.OnDragged(new Vector2(x, y)));

            [Preserve]
            public void onDragEnd(int containerId, float x, float y, float positionX, float positionY)
                => BannerContainerRegistry.Dispatch(containerId, c => c.OnDragEnded(new Vector2(x, y), new Vector2(positionX, positionY)));

            [Preserve]
            public void onBoundsChanged(int containerId, float x, float y, float width, float height)
                => BannerContainerRegistry.Dispatch(containerId, c => c.OnBoundsChanged(new Rect(x, y, width, height)));
        }
    }
}
