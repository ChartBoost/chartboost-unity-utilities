using System;
using System.Runtime.InteropServices;
using AOT;
using Chartboost.Constants;
using UnityEngine;

namespace Chartboost.Banners
{
    /// <summary>Creates <see cref="NativeBannerContainer"/>s for native iOS <c>CBUBannerContainer</c> instances.</summary>
    public static class IOSBannerContainers
    {
        private delegate void ContainerCallback(int containerId, int kind, float a, float b, float c, float d);

        // Matches CBUBannerContainerEvent.
        private const int DragBegan = 0;
        private const int Dragged = 1;
        private const int DragEnded = 2;
        private const int Bounds = 3;

        // Held so the delegate handed to native is never collected.
        private static readonly ContainerCallback Callback = OnContainerEvent;
        private static bool _installed;

        /// <summary>
        /// Wraps a <c>CBUBannerContainer</c> pointer a wrapper's bridge handed over with a +1 retain
        /// (<c>__bridge_retained</c>). Takes ownership: disposing the result releases it.
        /// </summary>
        public static NativeBannerContainer Wrap(IntPtr container)
        {
            if (container == IntPtr.Zero)
                throw new ArgumentNullException(nameof(container));
            if (!_installed)
            {
                _CBUBannerContainerInstall(Callback);
                _installed = true;
            }
            return new NativeBannerContainer(new IOSBannerContainer(container));
        }

        // Static and attributed so IL2CPP can hand native a stable function pointer. Called on the main thread.
        [MonoPInvokeCallback(typeof(ContainerCallback))]
        private static void OnContainerEvent(int containerId, int kind, float a, float b, float c, float d)
        {
            switch (kind)
            {
                case DragBegan:
                    BannerContainerRegistry.Dispatch(containerId, x => x.OnDragBegan(new Vector2(a, b)));
                    break;
                case Dragged:
                    BannerContainerRegistry.Dispatch(containerId, x => x.OnDragged(new Vector2(a, b)));
                    break;
                case DragEnded:
                    BannerContainerRegistry.Dispatch(containerId, x => x.OnDragEnded(new Vector2(a, b), new Vector2(c, d)));
                    break;
                case Bounds:
                    BannerContainerRegistry.Dispatch(containerId, x => x.OnBoundsChanged(new Rect(a, b, c, d)));
                    break;
            }
        }

        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBUBannerContainerInstall(ContainerCallback callback);
    }
}
