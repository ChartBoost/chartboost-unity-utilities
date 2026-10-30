#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN

/// Matches BannerAnchor in C# (explicit values cross P-Invoke).
typedef NS_ENUM(NSInteger, CBUBannerAnchor) {
    CBUBannerAnchorTopLeft = 0,
    CBUBannerAnchorTopCenter = 1,
    CBUBannerAnchorTopRight = 2,
    CBUBannerAnchorCenter = 3,
    CBUBannerAnchorBottomLeft = 4,
    CBUBannerAnchorBottomCenter = 5,
    CBUBannerAnchorBottomRight = 6,
};

/// Matches BannerHorizontalAlignment / BannerVerticalAlignment in C#.
typedef NS_ENUM(NSInteger, CBUBannerAlignment) {
    CBUBannerAlignmentStart = 0,
    CBUBannerAlignmentCenter = 1,
    CBUBannerAlignmentEnd = 2,
};

/// Event kinds sent to `CBUBannerContainer.callback`.
typedef NS_ENUM(int, CBUBannerContainerEvent) {
    CBUBannerContainerEventDragBegan = 0,  // a, b = origin in pixels
    CBUBannerContainerEventDragged = 1,    // a, b = origin in pixels
    CBUBannerContainerEventDragEnded = 2,  // a, b = origin in pixels; c, d = stored position in points
    CBUBannerContainerEventBounds = 3,     // a, b, c, d = frame in window points
};

/// One process-wide callback for every container, routed by container id. Called on the main thread.
typedef void (*CBUBannerContainerCallback)(int containerId, int event, float a, float b, float c, float d);

/// Size value meaning "wrap the content".
FOUNDATION_EXPORT const CGFloat CBUBannerContainerWrap;

/// Shared host for one SDK banner view. The SDK view is the only subview and never changes parent; the
/// container is what gets placed, sized, shown, dragged and moved between hosts. Main thread only; units are
/// points, except drag events, which report pixels. No Unity symbols here: the Unity bridge supplies the root.
@interface CBUBannerContainer : UIView

/// Receives drag and bounds events for every container.
@property (class, nonatomic, nullable) CBUBannerContainerCallback callback;

/// Root view for unhosted containers (the Unity view). Set once by the Unity bridge; a seam for tests.
@property (class, nonatomic, copy, nullable) UIView * _Nullable (^rootViewProvider)(void);

/// Wraps `content`. When `fillsContent` (Mediation, Monetization) the content takes the container's size;
/// otherwise (Prebid) it keeps its own size and is aligned inside.
- (instancetype)initWithContent:(UIView *)content fillsContent:(BOOL)fillsContent NS_DESIGNATED_INITIALIZER;
- (instancetype)initWithFrame:(CGRect)frame NS_UNAVAILABLE;
- (nullable instancetype)initWithCoder:(NSCoder *)coder NS_UNAVAILABLE;

/// The container holding `content`, or nil when it is not in one.
+ (nullable instancetype)containerOf:(UIView *)content;

@property (nonatomic, readonly) int containerId;
@property (nonatomic, readonly) UIView *content;

/// Wrapped size when no explicit size is set; defaults to the content's intrinsic size, else its frame size.
/// Wrappers set it whenever the SDK reports a new ad size: the container does not watch the content.
@property (nonatomic) CGSize contentSize;

@property (nonatomic) BOOL clipContent;          // YES: contained (capped to the host); NO: overflow
@property (nonatomic) BOOL interactionEnabled;   // NO: touches pass through
@property (nonatomic) BOOL draggable;            // root mode only

- (void)setAnchor:(CBUBannerAnchor)anchor useSafeArea:(BOOL)useSafeArea;
- (void)setAbsolutePosition:(CGPoint)position pivot:(CGPoint)pivot;
- (void)setSizeWidth:(CGFloat)width height:(CGFloat)height;
- (void)setAlignmentHorizontal:(CBUBannerAlignment)horizontal vertical:(CBUBannerAlignment)vertical;

/// Moves into `host`, centred, or back to the root at the stored placement when nil (a no-op before the first attach).
- (void)setHost:(nullable UIView *)host;
/// Attaches if not attached yet: into the host, else the root.
- (void)attach;
/// Removes from the superview; the content stays inside.
- (void)detach;
/// Detaches and releases the content. The wrapper tears down the SDK view itself.
- (void)dispose;

@end

NS_ASSUME_NONNULL_END
