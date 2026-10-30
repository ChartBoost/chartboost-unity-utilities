using System;
using System.Runtime.InteropServices;
using Chartboost.Constants;
using UnityEngine;

namespace Chartboost.Banners
{
    /// <summary>iOS side of a <see cref="NativeBannerContainer"/>: forwards to the native <c>CBUBannerContainer</c>.</summary>
    internal sealed class IOSBannerContainer : IBannerContainerPlatform
    {
        private IntPtr _container;

        internal IOSBannerContainer(IntPtr container)
        {
            _container = container;
            Id = _CBUBannerContainerGetId(container);
        }

        public int Id { get; }

        public bool CanDisposeFromFinalizer => true;

        public void SetAnchor(BannerAnchor anchor, bool useSafeArea) => _CBUBannerContainerSetAnchor(_container, (int)anchor, useSafeArea);
        public void SetAbsolute(Vector2 position, Vector2 pivot) => _CBUBannerContainerSetAbsolute(_container, position.x, position.y, pivot.x, pivot.y);
        public void SetSize(float width, float height) => _CBUBannerContainerSetSize(_container, width, height);
        public void SetAlignment(BannerHorizontalAlignment horizontal, BannerVerticalAlignment vertical) => _CBUBannerContainerSetAlignment(_container, (int)horizontal, (int)vertical);
        public void SetBackgroundColor(Color color) => _CBUBannerContainerSetBackgroundColor(_container, color.r, color.g, color.b, color.a);
        public void SetClipContent(bool clip) => _CBUBannerContainerSetClipContent(_container, clip);
        public void SetVisible(bool visible) => _CBUBannerContainerSetVisible(_container, visible);
        public void SetInteractionEnabled(bool enabled) => _CBUBannerContainerSetInteractionEnabled(_container, enabled);
        public void SetDraggable(bool draggable) => _CBUBannerContainerSetDraggable(_container, draggable);
        public void SetHost(NativeView host) => _CBUBannerContainerSetHost(_container, host.Pointer);
        public void Attach() => _CBUBannerContainerAttach(_container);
        public void Detach() => _CBUBannerContainerDetach(_container);

        public void Dispose()
        {
            // Releases the retain the wrapper's bridge handed over with the pointer.
            _CBUBannerContainerDispose(_container);
            _container = IntPtr.Zero;
        }

        [DllImport(SharedIOSConstants.DLLImport)] private static extern int _CBUBannerContainerGetId(IntPtr container);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBUBannerContainerSetAnchor(IntPtr container, int anchor, [MarshalAs(UnmanagedType.I1)] bool useSafeArea);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBUBannerContainerSetAbsolute(IntPtr container, float x, float y, float pivotX, float pivotY);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBUBannerContainerSetSize(IntPtr container, float width, float height);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBUBannerContainerSetAlignment(IntPtr container, int horizontal, int vertical);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBUBannerContainerSetBackgroundColor(IntPtr container, float r, float g, float b, float a);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBUBannerContainerSetClipContent(IntPtr container, [MarshalAs(UnmanagedType.I1)] bool clip);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBUBannerContainerSetVisible(IntPtr container, [MarshalAs(UnmanagedType.I1)] bool visible);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBUBannerContainerSetInteractionEnabled(IntPtr container, [MarshalAs(UnmanagedType.I1)] bool enabled);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBUBannerContainerSetDraggable(IntPtr container, [MarshalAs(UnmanagedType.I1)] bool draggable);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBUBannerContainerSetHost(IntPtr container, IntPtr host);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBUBannerContainerAttach(IntPtr container);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBUBannerContainerDetach(IntPtr container);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBUBannerContainerDispose(IntPtr container);
    }
}
