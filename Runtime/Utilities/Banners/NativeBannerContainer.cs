using System;
using UnityEngine;

namespace Chartboost.Banners
{
    /// <summary>
    /// Shared native host for one SDK banner view. The SDK view is its only child and never changes parent;
    /// the container is what gets placed, sized, shown, dragged and moved between hosts. Units are native
    /// (dp on Android, points on iOS), except drag events, which report pixels.
    /// </summary>
    /// <remarks>
    /// Created by a wrapper's platform bridge around the SDK view it owns. Setters skip unchanged values and
    /// apply on the UI thread in call order; events are raised on the Unity main thread.
    /// </remarks>
    public sealed class NativeBannerContainer : IDisposable
    {
        /// <summary>Size value that wraps the content on that axis.</summary>
        public const float Wrap = -1f;

        private readonly IBannerContainerPlatform _platform;
        private BannerHorizontalAlignment _horizontal = BannerHorizontalAlignment.Center;
        private BannerVerticalAlignment _vertical = BannerVerticalAlignment.Center;
        private bool _clipContent = true;
        private bool _visible = true;
        private bool _interactionEnabled = true;
        private bool _draggable;

        internal NativeBannerContainer(IBannerContainerPlatform platform)
        {
            _platform = platform ?? throw new ArgumentNullException(nameof(platform));
            BannerContainerRegistry.Register(platform.Id, this);
        }

        /// <summary>True once <see cref="Dispose"/> has run.</summary>
        public bool IsDisposed { get; private set; }

        /// <summary>False when an owner's finalizer must not dispose this container (Android: its Java handle is finalized too).</summary>
        public bool CanDisposeFromFinalizer => _platform.CanDisposeFromFinalizer;

        /// <summary>Anchor used when the container is in the Unity view and not placed absolutely.</summary>
        public BannerAnchor Anchor { get; private set; } = BannerAnchor.BottomCenter;

        /// <summary>True when the container is placed with <see cref="SetAbsolute"/> or by a drag rather than an anchor.</summary>
        public bool IsAbsolute { get; private set; }

        /// <summary>Absolute position set by <see cref="SetAbsolute"/> or the last drag, in native units.</summary>
        public Vector2 Position { get; private set; }

        /// <summary>Container size in native units; <see cref="Wrap"/> on an axis wraps the content.</summary>
        public Vector2 Size { get; private set; } = new(Wrap, Wrap);

        /// <summary>Last frame the native side reported, in native units, in window coordinates.</summary>
        public Rect Bounds { get; private set; }

        /// <summary>Clips the content to the container when true; false lets wide creatives draw past it.</summary>
        public bool ClipContent
        {
            get => _clipContent;
            set { if (Change(ref _clipContent, value)) _platform.SetClipContent(value); }
        }

        /// <summary>Shows or hides the container. Hidden keeps its size (INVISIBLE / hidden), so it stays measurable.</summary>
        public bool Visible
        {
            get => _visible;
            set { if (Change(ref _visible, value)) _platform.SetVisible(value); }
        }

        /// <summary>When false, touches pass through to whatever is behind the container.</summary>
        public bool InteractionEnabled
        {
            get => _interactionEnabled;
            set { if (Change(ref _interactionEnabled, value)) _platform.SetInteractionEnabled(value); }
        }

        /// <summary>Lets the user drag the container while it is in the Unity view. Ignored while hosted.</summary>
        public bool Draggable
        {
            get => _draggable;
            set { if (Change(ref _draggable, value)) _platform.SetDraggable(value); }
        }

        /// <summary>A drag started; the container's origin in pixels.</summary>
        public event Action<NativeBannerContainer, Vector2> DragBegan;

        /// <summary>The container moved during a drag; its origin in pixels.</summary>
        public event Action<NativeBannerContainer, Vector2> Dragged;

        /// <summary>A drag ended; its origin in pixels. <see cref="Position"/> is already updated.</summary>
        public event Action<NativeBannerContainer, Vector2> DragEnded;

        /// <summary>The native frame changed; see <see cref="Bounds"/>.</summary>
        public event Action<NativeBannerContainer, Rect> BoundsChanged;

        /// <summary>Anchors the container to an edge or corner of the Unity view. Always forwarded: rare and idempotent natively.</summary>
        public void SetAnchor(BannerAnchor anchor, bool useSafeArea = true)
        {
            if (IsDisposed)
                return;
            Anchor = anchor;
            IsAbsolute = false;
            _platform.SetAnchor(anchor, useSafeArea);
        }

        /// <summary>Places the container at <paramref name="position"/>, offset by <paramref name="pivot"/> (0..1) of its size.</summary>
        public void SetAbsolute(Vector2 position, Vector2 pivot)
        {
            if (IsDisposed)
                return;
            Position = position;
            IsAbsolute = true;
            _platform.SetAbsolute(position, pivot);
        }

        /// <summary>Sets the container size; <see cref="Wrap"/> on an axis wraps the content.</summary>
        public void SetSize(float width, float height)
        {
            var size = new Vector2(width, height);
            if (IsDisposed || Size == size)
                return;
            Size = size;
            _platform.SetSize(width, height);
        }

        /// <summary>Aligns the content inside the container.</summary>
        public void SetAlignment(BannerHorizontalAlignment horizontal, BannerVerticalAlignment vertical)
        {
            if (IsDisposed || (_horizontal == horizontal && _vertical == vertical))
                return;
            _horizontal = horizontal;
            _vertical = vertical;
            _platform.SetAlignment(horizontal, vertical);
        }

        /// <summary>Sets the container background.</summary>
        public void SetBackgroundColor(Color color)
        {
            if (!IsDisposed)
                _platform.SetBackgroundColor(color);
        }

        /// <summary>
        /// Moves the container into <paramref name="host"/>, centred and laid out by the host, or back to its
        /// stored placement with <see cref="NativeView.None"/>. Hosting attaches the container. On Android,
        /// centring needs a <c>FrameLayout</c> host.
        /// </summary>
        public void SetHost(NativeView host)
        {
            if (!IsDisposed)
                _platform.SetHost(host);
        }

        /// <summary>Attaches the container if it is not attached yet: into its host, else the Unity view.</summary>
        public void Attach()
        {
            if (!IsDisposed)
                _platform.Attach();
        }

        /// <summary>Takes the container off screen; the SDK view stays inside it.</summary>
        public void Detach()
        {
            if (!IsDisposed)
                _platform.Detach();
        }

        /// <summary>Detaches the container and releases the SDK view. The wrapper tears down the SDK view itself.</summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            BannerContainerRegistry.Release(_platform.Id);
            _platform.Dispose();
            DragBegan = Dragged = DragEnded = null;
            BoundsChanged = null;
        }

        internal void OnDragBegan(Vector2 origin) => DragBegan?.Invoke(this, origin);
        internal void OnDragged(Vector2 origin) => Dragged?.Invoke(this, origin);

        internal void OnDragEnded(Vector2 origin, Vector2 position)
        {
            // Native already stored the dropped spot as an absolute placement; mirror it.
            IsAbsolute = true;
            Position = position;
            DragEnded?.Invoke(this, origin);
        }

        internal void OnBoundsChanged(Rect bounds)
        {
            if (Bounds == bounds)
                return;
            Bounds = bounds;
            BoundsChanged?.Invoke(this, bounds);
        }

        private bool Change<T>(ref T field, T value)
        {
            if (IsDisposed || Equals(field, value))
                return false;
            field = value;
            return true;
        }
    }
}
