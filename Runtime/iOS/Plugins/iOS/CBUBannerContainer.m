#import "CBUBannerContainer.h"

const CGFloat CBUBannerContainerWrap = -1;

static const CGFloat kDragCallbackInterval = 0.016;     // ~60 per second
static const CGFloat kMinVisibleWideFraction = 0.6;      // a banner wider than the safe area keeps 60% on screen

static CBUBannerContainerCallback sCallback = NULL;
static UIView * _Nullable (^sRootViewProvider)(void) = nil;
static int sNextId = 1;

@interface CBUBannerContainer () <UIGestureRecognizerDelegate>
@end

@implementation CBUBannerContainer {
    BOOL _fillsContent;
    CBUBannerAnchor _anchor;
    BOOL _useSafeArea;
    BOOL _absolute;
    CGPoint _position;
    CGPoint _pivot;
    CGFloat _width;
    CGFloat _height;
    CBUBannerAlignment _alignH;
    CBUBannerAlignment _alignV;
    __weak UIView *_host;
    NSArray<NSLayoutConstraint *> *_constraints;
    NSLayoutConstraint *_left;
    NSLayoutConstraint *_top;
    NSLayoutConstraint *_widthConstraint;
    NSLayoutConstraint *_heightConstraint;
    UIPanGestureRecognizer *_pan;
    BOOL _isDragging;
    BOOL _needsRebuild;
    CFTimeInterval _lastDragCallback;
    CGRect _reportedBounds;
}

+ (CBUBannerContainerCallback)callback { return sCallback; }
+ (void)setCallback:(CBUBannerContainerCallback)callback { sCallback = callback; }
+ (UIView * _Nullable (^)(void))rootViewProvider { return sRootViewProvider; }
+ (void)setRootViewProvider:(UIView * _Nullable (^)(void))provider { sRootViewProvider = [provider copy]; }

+ (instancetype)containerOf:(UIView *)content
{
    id parent = content.superview;
    return [parent isKindOfClass:CBUBannerContainer.class] ? parent : nil;
}

- (instancetype)initWithContent:(UIView *)content fillsContent:(BOOL)fillsContent
{
    if (self = [super initWithFrame:CGRectZero]) {
        _containerId = sNextId++;
        _content = content;
        _fillsContent = fillsContent;
        _anchor = CBUBannerAnchorBottomCenter;
        _useSafeArea = YES;
        _width = CBUBannerContainerWrap;
        _height = CBUBannerContainerWrap;
        _alignH = CBUBannerAlignmentCenter;
        _alignV = CBUBannerAlignmentCenter;
        _clipContent = YES;
        _interactionEnabled = YES;
        _reportedBounds = CGRectNull;
        self.clipsToBounds = YES;
        self.translatesAutoresizingMaskIntoConstraints = NO;
        // Frame-laid-out content keeps its own size until layoutSubviews places it.
        content.translatesAutoresizingMaskIntoConstraints = YES;
        content.autoresizingMask = UIViewAutoresizingNone;
        [self addSubview:content];
    }
    return self;
}

#pragma mark - State

- (BOOL)isHosted { return _host != nil && self.superview == _host; }

- (CGSize)naturalContentSize
{
    if (_contentSize.width > 0 && _contentSize.height > 0)
        return _contentSize;
    CGSize intrinsic = _content.intrinsicContentSize;
    if (intrinsic.width > 0 && intrinsic.height > 0)
        return intrinsic;
    return _content.frame.size;
}

- (CGSize)targetSize
{
    CGSize natural = [self naturalContentSize];
    return CGSizeMake(_width >= 0 ? _width : natural.width, _height >= 0 ? _height : natural.height);
}

- (void)setContentSize:(CGSize)contentSize
{
    _contentSize = contentSize;
    [self applySize];
}

- (void)setClipContent:(BOOL)clipContent
{
    _clipContent = clipContent;
    self.clipsToBounds = clipContent;
    if ([self isHosted])
        [self rebuildConstraints]; // the host cap exists only when contained
}

- (void)setDraggable:(BOOL)draggable
{
    _draggable = draggable;
    if (draggable && !_pan) {
        _pan = [[UIPanGestureRecognizer alloc] initWithTarget:self action:@selector(handlePan:)];
        _pan.delegate = self;
        [self addGestureRecognizer:_pan];
    }
    _pan.enabled = draggable; // disabling cancels an active drag
}

- (void)setAnchor:(CBUBannerAnchor)anchor useSafeArea:(BOOL)useSafeArea
{
    _anchor = anchor;
    _useSafeArea = useSafeArea;
    _absolute = NO;
    [self rebuildConstraints];
}

- (void)setAbsolutePosition:(CGPoint)position pivot:(CGPoint)pivot
{
    BOOL wasAbsolute = _absolute;
    _position = position;
    _pivot = pivot;
    _absolute = YES;
    if (wasAbsolute && _left && ![self isHosted] && !_isDragging)
        [self applyAbsoluteConstantsForSize:[self targetSize]]; // only the offsets move
    else
        [self rebuildConstraints];
}

- (void)setSizeWidth:(CGFloat)width height:(CGFloat)height
{
    _width = width;
    _height = height;
    [self applySize];
}

// Size changes (frequent: every ad-size report) update the live constants instead of rebuilding.
- (void)applySize
{
    if (!_widthConstraint || _isDragging) {
        [self rebuildConstraints];
        return;
    }
    CGSize size = [self targetSize];
    _widthConstraint.constant = size.width;
    _heightConstraint.constant = size.height;
    if (_left)
        [self applyAbsoluteConstantsForSize:size];
    [self setNeedsLayout];
}

- (void)setAlignmentHorizontal:(CBUBannerAlignment)horizontal vertical:(CBUBannerAlignment)vertical
{
    _alignH = horizontal;
    _alignV = vertical;
    [self setNeedsLayout];
}

#pragma mark - Hosting

- (void)setHost:(UIView *)host
{
    _host = host;
    if (host)
        [self moveTo:host];
    else if (self.superview)
        [self attachToRoot];
}

- (void)attach
{
    if (self.superview)
        return;
    UIView *host = _host;
    if (host)
        [self moveTo:host];
    else
        [self attachToRoot];
}

- (void)attachToRoot
{
    UIView *root = sRootViewProvider ? sRootViewProvider() : nil;
    if (root)
        [self moveTo:root];
}

- (void)moveTo:(UIView *)target
{
    // addSubview: alone moves the view; the SDK view inside never sees a superview change.
    if (self.superview != target) {
        [self deactivateConstraints];
        [target addSubview:self];
    }
    [self rebuildConstraints];
}

- (void)detach
{
    [self deactivateConstraints];
    [self removeFromSuperview];
}

- (void)dispose
{
    _pan.enabled = NO;
    [self detach];
    [_content removeFromSuperview];
    _host = nil;
}

#pragma mark - Layout

- (void)deactivateConstraints
{
    if (_constraints.count)
        [NSLayoutConstraint deactivateConstraints:_constraints];
    _constraints = nil;
    _left = nil;
    _top = nil;
    _widthConstraint = nil;
    _heightConstraint = nil;
}

- (void)rebuildConstraints
{
    if (_isDragging) {
        _needsRebuild = YES; // applied at the drop; the finger owns the position until then
        return;
    }
    _needsRebuild = NO;
    [self deactivateConstraints];
    UIView *parent = self.superview;
    if (!parent)
        return;

    CGSize size = [self targetSize];
    NSMutableArray<NSLayoutConstraint *> *c = [NSMutableArray array];
    NSLayoutConstraint *w = [self.widthAnchor constraintEqualToConstant:size.width];
    NSLayoutConstraint *h = [self.heightAnchor constraintEqualToConstant:size.height];
    w.priority = UILayoutPriorityDefaultHigh;
    h.priority = UILayoutPriorityDefaultHigh;
    [c addObjectsFromArray:@[w, h]];
    _widthConstraint = w;
    _heightConstraint = h;

    if ([self isHosted]) {
        [c addObject:[self.centerXAnchor constraintEqualToAnchor:parent.centerXAnchor]];
        [c addObject:[self.centerYAnchor constraintEqualToAnchor:parent.centerYAnchor]];
        if (_clipContent) {
            // Contained: capped to the host, as the native canary slot does.
            [c addObject:[self.widthAnchor constraintLessThanOrEqualToAnchor:parent.widthAnchor]];
            [c addObject:[self.heightAnchor constraintLessThanOrEqualToAnchor:parent.heightAnchor]];
        }
    } else if (_absolute) {
        _left = [self.leftAnchor constraintEqualToAnchor:parent.leftAnchor];
        _top = [self.topAnchor constraintEqualToAnchor:parent.topAnchor];
        [c addObjectsFromArray:@[_left, _top]];
        [self applyAbsoluteConstantsForSize:size];
    } else {
        // UIView and UILayoutGuide share these anchors. left/right, not leading/trailing: no RTL mirroring.
        id src = _useSafeArea ? parent.safeAreaLayoutGuide : parent;
        switch (_anchor) {
            case CBUBannerAnchorTopLeft:
            case CBUBannerAnchorBottomLeft:
                [c addObject:[self.leftAnchor constraintEqualToAnchor:[src leftAnchor]]];
                break;
            case CBUBannerAnchorTopRight:
            case CBUBannerAnchorBottomRight:
                [c addObject:[self.rightAnchor constraintEqualToAnchor:[src rightAnchor]]];
                break;
            default:
                [c addObject:[self.centerXAnchor constraintEqualToAnchor:[src centerXAnchor]]];
                break;
        }
        switch (_anchor) {
            case CBUBannerAnchorTopLeft:
            case CBUBannerAnchorTopCenter:
            case CBUBannerAnchorTopRight:
                [c addObject:[self.topAnchor constraintEqualToAnchor:[src topAnchor]]];
                break;
            case CBUBannerAnchorCenter:
                [c addObject:[self.centerYAnchor constraintEqualToAnchor:[src centerYAnchor]]];
                break;
            default:
                [c addObject:[self.bottomAnchor constraintEqualToAnchor:[src bottomAnchor]]];
                break;
        }
    }

    _constraints = c;
    [NSLayoutConstraint activateConstraints:c];
    [self setNeedsLayout];
}

- (void)applyAbsoluteConstantsForSize:(CGSize)size
{
    _left.constant = _position.x - _pivot.x * size.width;
    _top.constant = _position.y - _pivot.y * size.height;
}

- (void)layoutSubviews
{
    [super layoutSubviews];
    CGRect bounds = self.bounds;
    if (_fillsContent) {
        _content.frame = bounds;
    } else {
        CGSize size = [self naturalContentSize];
        _content.frame = CGRectMake([CBUBannerContainer offsetFor:_alignH free:bounds.size.width - size.width],
                                    [CBUBannerContainer offsetFor:_alignV free:bounds.size.height - size.height],
                                    size.width, size.height);
    }
    [self reportBounds];
}

// layoutSubviews only runs on size changes; Auto Layout moves a same-size view through setCenter:.
- (void)setCenter:(CGPoint)center
{
    [super setCenter:center];
    [self reportBounds];
}

+ (CGFloat)offsetFor:(CBUBannerAlignment)alignment free:(CGFloat)free
{
    switch (alignment) {
        case CBUBannerAlignmentStart: return 0;
        case CBUBannerAlignmentEnd: return free;
        default: return free / 2;
    }
}

- (void)reportBounds
{
    if (!sCallback || !self.window || _isDragging)
        return; // a drag reports its own events; bounds follow at the drop
    CGRect frame = [self convertRect:self.bounds toView:nil];
    if (CGRectEqualToRect(frame, _reportedBounds))
        return;
    _reportedBounds = frame;
    sCallback(_containerId, CBUBannerContainerEventBounds, frame.origin.x, frame.origin.y, frame.size.width, frame.size.height);
}

#pragma mark - Touch and drag

- (UIView *)hitTest:(CGPoint)point withEvent:(UIEvent *)event
{
    if (!_interactionEnabled)
        return nil;
    UIView *hit = [super hitTest:point withEvent:event];
    // Only the banner (or the pan on it) takes touches; the container's empty area passes them through.
    return hit == self && !_draggable ? nil : hit;
}

- (BOOL)gestureRecognizerShouldBegin:(UIGestureRecognizer *)gestureRecognizer
{
    return gestureRecognizer != _pan || (_draggable && ![self isHosted]);
}

- (CGFloat)pixelScale
{
    UIScreen *screen = self.window.screen;
    return screen ? screen.scale : 1;
}

- (void)handlePan:(UIPanGestureRecognizer *)pan
{
    UIView *parent = self.superview;
    if (!parent)
        return;
    CGFloat scale = [self pixelScale];
    switch (pan.state) {
        case UIGestureRecognizerStateBegan:
            if (!_absolute) {
                // Anchored: continue from where the banner is, as an absolute placement with no pivot.
                _position = self.frame.origin;
                _pivot = CGPointZero;
                _absolute = YES;
                [self rebuildConstraints];
            }
            _isDragging = YES;
            _lastDragCallback = CACurrentMediaTime();
            if (sCallback)
                sCallback(_containerId, CBUBannerContainerEventDragBegan, _left.constant * scale, _top.constant * scale, 0, 0);
            break;
        case UIGestureRecognizerStateChanged: {
            CGPoint t = [pan translationInView:parent];
            [pan setTranslation:CGPointZero inView:parent]; // incremental, like Android's dragBy
            CGPoint origin = [self clampedOrigin:CGPointMake(_left.constant + t.x, _top.constant + t.y) inParent:parent];
            _left.constant = origin.x;
            _top.constant = origin.y;
            CFTimeInterval now = CACurrentMediaTime();
            if (sCallback && now - _lastDragCallback >= kDragCallbackInterval) {
                _lastDragCallback = now;
                sCallback(_containerId, CBUBannerContainerEventDragged, origin.x * scale, origin.y * scale, 0, 0);
            }
            break;
        }
        case UIGestureRecognizerStateEnded:
        case UIGestureRecognizerStateCancelled:
        case UIGestureRecognizerStateFailed: {
            if (!_isDragging)
                break;
            _isDragging = NO;
            // The drop wins over placement set mid-drag, as on Android. Constraints may be gone if the
            // container moved mid-drag, so read the frame instead.
            CGPoint origin = _left ? CGPointMake(_left.constant, _top.constant) : self.frame.origin;
            CGSize size = self.bounds.size;
            _absolute = YES;
            _position = CGPointMake(origin.x + _pivot.x * size.width, origin.y + _pivot.y * size.height);
            if (sCallback)
                sCallback(_containerId, CBUBannerContainerEventDragEnded, origin.x * scale, origin.y * scale, _position.x, _position.y);
            if (_needsRebuild || !_left)
                [self rebuildConstraints];
            [self reportBounds];
            break;
        }
        default:
            break;
    }
}

- (CGPoint)clampedOrigin:(CGPoint)origin inParent:(UIView *)parent
{
    UIEdgeInsets safe = parent.safeAreaInsets;
    CGSize size = self.bounds.size;
    CGSize area = parent.bounds.size;
    CGFloat safeWidth = area.width - safe.left - safe.right;
    CGFloat minX, maxX;
    if (size.width > safeWidth) {
        CGFloat visible = MIN(size.width * kMinVisibleWideFraction, safeWidth);
        minX = safe.left - (size.width - visible);
        maxX = area.width - safe.right - visible;
    } else {
        minX = safe.left;
        maxX = area.width - safe.right - size.width;
    }
    CGFloat minY = safe.top;
    CGFloat maxY = area.height - safe.bottom - size.height;
    return CGPointMake(MIN(MAX(origin.x, minX), MAX(minX, maxX)), MIN(MAX(origin.y, minY), MAX(minY, maxY)));
}

@end
