using UnityEngine;

namespace Chartboost.Banners
{
    /// <summary>Native side of a <see cref="NativeBannerContainer"/>; one implementation per platform.</summary>
    internal interface IBannerContainerPlatform
    {
        /// <summary>Native id that routes drag and bounds callbacks back to this container.</summary>
        int Id { get; }

        /// <summary>False when the native handle is itself finalizable (Android), so a finalizer must not dispose it.</summary>
        bool CanDisposeFromFinalizer { get; }

        void SetAnchor(BannerAnchor anchor, bool useSafeArea);
        void SetAbsolute(Vector2 position, Vector2 pivot);
        void SetSize(float width, float height);
        void SetAlignment(BannerHorizontalAlignment horizontal, BannerVerticalAlignment vertical);
        void SetBackgroundColor(Color color);
        void SetClipContent(bool clip);
        void SetVisible(bool visible);
        void SetInteractionEnabled(bool enabled);
        void SetDraggable(bool draggable);
        void SetHost(NativeView host);
        void Attach();
        void Detach();
        void Dispose();
    }
}
