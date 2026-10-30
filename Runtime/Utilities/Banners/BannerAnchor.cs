namespace Chartboost.Banners
{
    /// <summary>Edge or corner a banner container is anchored to, inside the safe area by default. Values cross JNI/P-Invoke.</summary>
    public enum BannerAnchor
    {
        TopLeft = 0,
        TopCenter = 1,
        TopRight = 2,
        Center = 3,
        BottomLeft = 4,
        BottomCenter = 5,
        BottomRight = 6
    }

    /// <summary>Horizontal alignment of the banner inside its container.</summary>
    public enum BannerHorizontalAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    /// <summary>Vertical alignment of the banner inside its container.</summary>
    public enum BannerVerticalAlignment
    {
        Top = 0,
        Center = 1,
        Bottom = 2
    }
}
