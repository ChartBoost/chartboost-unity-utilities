using UnityEngine;

namespace Chartboost.Banners
{
    /// <summary>Android side of a <see cref="NativeBannerContainer"/>: forwards to the Kotlin <c>BannerContainer</c>.</summary>
    internal sealed class AndroidBannerContainer : IBannerContainerPlatform
    {
        internal const string ClassBannerContainer = "com.chartboost.unity.utilities.banner.BannerContainer";

        private AndroidJavaObject _container;

        internal AndroidBannerContainer(AndroidJavaObject container)
        {
            _container = container;
            Id = container.Call<int>("getContainerId");
        }

        public int Id { get; }

        // The AndroidJavaObject is finalized along with its owner.
        public bool CanDisposeFromFinalizer => false;

        public void SetAnchor(BannerAnchor anchor, bool useSafeArea) => Call("setAnchor", (int)anchor, useSafeArea);
        public void SetAbsolute(Vector2 position, Vector2 pivot) => Call("setAbsolute", position.x, position.y, pivot.x, pivot.y);
        public void SetSize(float width, float height) => Call("setSize", width, height);
        public void SetAlignment(BannerHorizontalAlignment horizontal, BannerVerticalAlignment vertical) => Call("setAlignment", (int)horizontal, (int)vertical);
        public void SetBackgroundColor(Color color) => Call("setContainerBackgroundColor", ToArgb(color));
        public void SetClipContent(bool clip) => Call("setClipContent", clip);
        public void SetVisible(bool visible) => Call("setShown", visible);
        public void SetInteractionEnabled(bool enabled) => Call("setInteractionEnabled", enabled);
        public void SetDraggable(bool draggable) => Call("setDraggable", draggable);
        public void Attach() => Call("attach");
        public void Detach() => Call("detach");

        public void SetHost(NativeView host)
        {
            if (host.JavaObject == null)
                Call("clearHost");
            else
                Call("setHost", host.JavaObject);
        }

        public void Dispose()
        {
            Call("dispose");
            _container?.Dispose();
            _container = null;
        }

        private void Call(string method, params object[] args) => _container?.Call(method, args);

        private static int ToArgb(Color color)
        {
            Color32 c = color;
            return (c.a << 24) | (c.r << 16) | (c.g << 8) | c.b;
        }
    }
}
