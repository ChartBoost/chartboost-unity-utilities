#import "CBUBannerContainer.h"
#import "ChartboostUnityUtilities.h"

// Unity side of CBUBannerContainer. The wrappers create containers natively (they own the SDK view) and hand
// C# a retained pointer; every call below hops to the main thread, in call order.

extern UIViewController *UnityGetGLViewController(void);

static inline CBUBannerContainer *container(void *ptr) { return (__bridge CBUBannerContainer *)ptr; }

// Captures a strong reference now, so a queued call never outlives a Dispose that ran first.
static inline void onMain(void *ptr, void (^work)(CBUBannerContainer *c))
{
    CBUBannerContainer *c = container(ptr);
    if (!c)
        return;
    toMain(^{ work(c); });
}

extern "C" {
    void _CBUBannerContainerInstall(CBUBannerContainerCallback callback)
    {
        CBUBannerContainer.callback = callback;
        CBUBannerContainer.rootViewProvider = ^UIView * { return UnityGetGLViewController().view; };
    }

    int _CBUBannerContainerGetId(void *ptr) { return ptr ? container(ptr).containerId : 0; }

    void _CBUBannerContainerSetAnchor(void *ptr, int anchor, bool useSafeArea)
    {
        onMain(ptr, ^(CBUBannerContainer *c) { [c setAnchor:(CBUBannerAnchor)anchor useSafeArea:useSafeArea]; });
    }

    void _CBUBannerContainerSetAbsolute(void *ptr, float x, float y, float pivotX, float pivotY)
    {
        onMain(ptr, ^(CBUBannerContainer *c) { [c setAbsolutePosition:CGPointMake(x, y) pivot:CGPointMake(pivotX, pivotY)]; });
    }

    void _CBUBannerContainerSetSize(void *ptr, float width, float height)
    {
        onMain(ptr, ^(CBUBannerContainer *c) { [c setSizeWidth:width height:height]; });
    }

    void _CBUBannerContainerSetAlignment(void *ptr, int horizontal, int vertical)
    {
        onMain(ptr, ^(CBUBannerContainer *c) { [c setAlignmentHorizontal:(CBUBannerAlignment)horizontal vertical:(CBUBannerAlignment)vertical]; });
    }

    void _CBUBannerContainerSetBackgroundColor(void *ptr, float r, float g, float b, float a)
    {
        onMain(ptr, ^(CBUBannerContainer *c) { c.backgroundColor = [UIColor colorWithRed:r green:g blue:b alpha:a]; });
    }

    void _CBUBannerContainerSetClipContent(void *ptr, bool clip) { onMain(ptr, ^(CBUBannerContainer *c) { c.clipContent = clip; }); }
    void _CBUBannerContainerSetVisible(void *ptr, bool visible) { onMain(ptr, ^(CBUBannerContainer *c) { c.hidden = !visible; }); }
    void _CBUBannerContainerSetInteractionEnabled(void *ptr, bool enabled) { onMain(ptr, ^(CBUBannerContainer *c) { c.interactionEnabled = enabled; }); }
    void _CBUBannerContainerSetDraggable(void *ptr, bool draggable) { onMain(ptr, ^(CBUBannerContainer *c) { c.draggable = draggable; }); }

    void _CBUBannerContainerSetHost(void *ptr, void *host)
    {
        UIView *hostView = (__bridge UIView *)host;
        onMain(ptr, ^(CBUBannerContainer *c) { [c setHost:hostView]; });
    }

    void _CBUBannerContainerAttach(void *ptr) { onMain(ptr, ^(CBUBannerContainer *c) { [c attach]; }); }
    void _CBUBannerContainerDetach(void *ptr) { onMain(ptr, ^(CBUBannerContainer *c) { [c detach]; }); }

    /// Disposes and drops the retain C# held since the wrapper handed the pointer over.
    void _CBUBannerContainerDispose(void *ptr)
    {
        if (!ptr)
            return;
        CBUBannerContainer *owned = (__bridge_transfer CBUBannerContainer *)ptr;
        toMain(^{ [owned dispose]; });
        // A handler may dispose inside the container's own callback (drag, bounds): release after that stack unwinds.
        dispatch_async(dispatch_get_main_queue(), ^{ (void)owned; });
    }
}
