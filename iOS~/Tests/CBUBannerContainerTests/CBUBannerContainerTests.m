#import <XCTest/XCTest.h>
#import "CBUBannerContainer.h"

@interface CBUBannerContainer (Testing)
- (CGPoint)clampedOrigin:(CGPoint)origin inParent:(UIView *)parent;
@end

static NSMutableArray<NSArray<NSNumber *> *> *sEvents;

static void RecordEvent(int containerId, int event, float a, float b, float c, float d)
{
    [sEvents addObject:@[@(containerId), @(event), @(a), @(b), @(c), @(d)]];
}

@interface CBUBannerContainerTests : XCTestCase
@end

@implementation CBUBannerContainerTests {
    UIWindow *_window;
    UIView *_root;
}

- (void)setUp
{
    _window = [[UIWindow alloc] initWithFrame:CGRectMake(0, 0, 400, 800)];
    _root = [[UIView alloc] initWithFrame:_window.bounds];
    [_window addSubview:_root];
    _window.hidden = NO;
    UIView *root = _root;
    CBUBannerContainer.rootViewProvider = ^UIView * { return root; };
    sEvents = [NSMutableArray array];
    CBUBannerContainer.callback = NULL;
}

- (void)tearDown
{
    CBUBannerContainer.rootViewProvider = nil;
    CBUBannerContainer.callback = NULL;
    _window.hidden = YES;
    _window = nil;
}

- (CBUBannerContainer *)bannerOfSize:(CGSize)size fills:(BOOL)fills
{
    UIView *content = [[UIView alloc] initWithFrame:CGRectMake(0, 0, size.width, size.height)];
    return [[CBUBannerContainer alloc] initWithContent:content fillsContent:fills];
}

- (void)layOut { [_window layoutIfNeeded]; }

- (void)testContainerIdsAreUnique
{
    NSMutableSet *ids = [NSMutableSet set];
    for (int i = 0; i < 50; i++)
        [ids addObject:@([self bannerOfSize:CGSizeMake(1, 1) fills:NO].containerId)];
    XCTAssertEqual(ids.count, 50u);
}

- (void)testContainerOfFindsOnlyItsOwnContent
{
    CBUBannerContainer *container = [self bannerOfSize:CGSizeMake(1, 1) fills:NO];
    XCTAssertEqual([CBUBannerContainer containerOf:container.subviews.firstObject], container);
    UIView *loose = [[UIView alloc] init];
    XCTAssertNil([CBUBannerContainer containerOf:loose]);
    [[[UIView alloc] init] addSubview:loose];
    XCTAssertNil([CBUBannerContainer containerOf:loose]);
}

- (void)testContentIsTheOnlySubviewAndKeepsItsSize
{
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(320, 50) fills:NO];
    XCTAssertEqual(c.subviews.count, 1u);
    XCTAssertEqual(c.subviews.firstObject, c.content);
    XCTAssertTrue(CGSizeEqualToSize(c.content.frame.size, CGSizeMake(320, 50)));
}

- (void)testHostedContainedIsCentredAndCappedToTheHost
{
    UIView *host = [[UIView alloc] initWithFrame:CGRectMake(50, 100, 300, 250)];
    [_root addSubview:host];
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(728, 90) fills:YES];
    [c setHost:host];
    [self layOut];

    XCTAssertEqual(c.superview, host);
    XCTAssertEqualWithAccuracy(c.frame.size.width, 300, 0.01, @"capped to the host width");
    XCTAssertEqualWithAccuracy(CGRectGetMidX(c.frame), 150, 0.01);
    XCTAssertEqualWithAccuracy(CGRectGetMidY(c.frame), 125, 0.01);
    XCTAssertTrue(CGRectEqualToRect(c.content.frame, c.bounds), @"fill content shrinks with the container");
}

- (void)testHostedOverflowKeepsTheFullSizeAndDoesNotClip
{
    UIView *host = [[UIView alloc] initWithFrame:CGRectMake(0, 0, 300, 250)];
    [_root addSubview:host];
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(728, 90) fills:YES];
    c.clipContent = NO;
    [c setHost:host];
    [self layOut];

    XCTAssertEqualWithAccuracy(c.frame.size.width, 728, 0.01);
    XCTAssertEqualWithAccuracy(CGRectGetMidX(c.frame), 150, 0.01);
    XCTAssertFalse(c.clipsToBounds);
}

- (void)testAnchorWithoutSafeAreaPinsToTheEdge
{
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(320, 50) fills:NO];
    [c attach];
    [c setAnchor:CBUBannerAnchorBottomRight useSafeArea:NO];
    [self layOut];

    XCTAssertEqual(c.superview, _root);
    XCTAssertTrue(CGPointEqualToPoint(c.frame.origin, CGPointMake(80, 750)));
}

- (void)testAbsolutePlacementOffsetsByThePivot
{
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(320, 50) fills:NO];
    [c attach];
    [c setAbsolutePosition:CGPointMake(200, 400) pivot:CGPointMake(0.5, 0.5)];
    [self layOut];
    XCTAssertTrue(CGPointEqualToPoint(c.frame.origin, CGPointMake(40, 375)));

    [c setAbsolutePosition:CGPointMake(210, 410) pivot:CGPointMake(0.5, 0.5)]; // offsets only
    [self layOut];
    XCTAssertTrue(CGPointEqualToPoint(c.frame.origin, CGPointMake(50, 385)));
}

- (void)testExplicitSizeOverridesTheContent
{
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(320, 50) fills:NO];
    [c attach];
    [c setSizeWidth:360 height:CBUBannerContainerWrap];
    [c setAlignmentHorizontal:CBUBannerAlignmentEnd vertical:CBUBannerAlignmentStart];
    [self layOut];

    XCTAssertEqualWithAccuracy(c.bounds.size.width, 360, 0.01);
    XCTAssertEqualWithAccuracy(c.bounds.size.height, 50, 0.01, @"wrapped axis follows the content");
    XCTAssertTrue(CGRectEqualToRect(c.content.frame, CGRectMake(40, 0, 320, 50)), @"aligned to the end");
}

- (void)testClearingTheHostReturnsToTheStoredAnchor
{
    UIView *host = [[UIView alloc] initWithFrame:CGRectMake(0, 300, 400, 250)];
    [_root addSubview:host];
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(320, 50) fills:NO];
    [c attach];
    [c setAnchor:CBUBannerAnchorTopLeft useSafeArea:NO];
    [c setHost:host];
    [c setHost:nil];
    [self layOut];

    XCTAssertEqual(c.superview, _root);
    XCTAssertTrue(CGPointEqualToPoint(c.frame.origin, CGPointZero));
}

- (void)testClearingTheHostBeforeAttachStaysDetached
{
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(320, 50) fills:NO];
    [c setHost:nil];
    XCTAssertNil(c.superview);
}

- (void)testHiddenKeepsTheSize
{
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(320, 50) fills:NO];
    [c attach];
    c.hidden = YES;
    [self layOut];
    XCTAssertEqualWithAccuracy(c.bounds.size.width, 320, 0.01);
    XCTAssertFalse(c.content.hidden);
}

- (void)testOnlyTheBannerTakesTouches
{
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(100, 50) fills:NO];
    [c attach];
    [c setSizeWidth:300 height:50];
    [self layOut];

    XCTAssertEqual([c hitTest:CGPointMake(150, 25) withEvent:nil], c.content);
    XCTAssertNil([c hitTest:CGPointMake(10, 25) withEvent:nil], @"empty container area passes through");
    c.interactionEnabled = NO;
    XCTAssertNil([c hitTest:CGPointMake(150, 25) withEvent:nil]);
}

- (void)testDisposeDetachesAndReleasesTheContent
{
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(320, 50) fills:NO];
    UIView *content = c.content;
    [c attach];
    [c dispose];
    XCTAssertNil(c.superview);
    XCTAssertNil(content.superview);
}

- (void)testBoundsAreReportedOnceInWindowPoints
{
    CBUBannerContainer.callback = RecordEvent;
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(320, 50) fills:NO];
    [c attach];
    [c setAnchor:CBUBannerAnchorTopLeft useSafeArea:NO];
    [self layOut];
    [c setNeedsLayout];
    [self layOut];

    XCTAssertEqual(sEvents.count, 1u, @"an unchanged frame is not re-reported");
    NSArray<NSNumber *> *e = sEvents.firstObject;
    XCTAssertEqual(e[0].intValue, c.containerId);
    XCTAssertEqual(e[1].intValue, CBUBannerContainerEventBounds);
    XCTAssertEqualWithAccuracy(e[4].floatValue, 320, 0.01);
    XCTAssertEqualWithAccuracy(e[5].floatValue, 50, 0.01);
}

- (void)testPlacementDoesNotMirrorInRightToLeftLanguages
{
    _root.semanticContentAttribute = UISemanticContentAttributeForceRightToLeft;
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(100, 50) fills:NO];
    [c attach];
    [c setAnchor:CBUBannerAnchorTopLeft useSafeArea:NO];
    [self layOut];
    XCTAssertEqualWithAccuracy(c.frame.origin.x, 0, 0.01, @"TopLeft stays on the left");

    [c setAbsolutePosition:CGPointMake(10, 20) pivot:CGPointZero];
    [self layOut];
    XCTAssertEqualWithAccuracy(c.frame.origin.x, 10, 0.01, @"absolute x is measured from the left");
}

- (void)testMovesThatKeepTheSizeAreReported
{
    CBUBannerContainer.callback = RecordEvent;
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(100, 50) fills:NO];
    [c attach];
    [c setAbsolutePosition:CGPointMake(10, 20) pivot:CGPointZero];
    [self layOut];
    [c setAbsolutePosition:CGPointMake(30, 40) pivot:CGPointZero];
    [self layOut];

    NSArray<NSNumber *> *last = sEvents.lastObject;
    XCTAssertEqual(last[1].intValue, CBUBannerContainerEventBounds);
    XCTAssertEqualWithAccuracy(last[2].floatValue, 30, 0.01);
    XCTAssertEqualWithAccuracy(last[3].floatValue, 40, 0.01);
}

- (void)testDragClampKeepsWideBannersSixtyPercentOnScreen
{
    CBUBannerContainer *c = [self bannerOfSize:CGSizeMake(728, 90) fills:YES];
    c.clipContent = NO;
    [c attach];
    [c setAnchor:CBUBannerAnchorTopLeft useSafeArea:NO];
    [self layOut];

    CGPoint farLeft = [c clampedOrigin:CGPointMake(-2000, 100) inParent:_root];
    CGPoint farRight = [c clampedOrigin:CGPointMake(2000, 100) inParent:_root];
    // 60% of 728 = 436.8 > 400 wide root, so at least the whole safe width stays covered.
    XCTAssertEqualWithAccuracy(farLeft.x, -(728 - 400), 0.01);
    XCTAssertEqualWithAccuracy(farRight.x, 0, 0.01);
    CBUBannerContainer *small = [self bannerOfSize:CGSizeMake(100, 50) fills:NO];
    [small attach];
    [self layOut];
    CGPoint fitting = [small clampedOrigin:CGPointMake(500, 900) inParent:_root];
    UIEdgeInsets safe = _root.safeAreaInsets; // the simulator window has real insets
    XCTAssertTrue(CGPointEqualToPoint(fitting, CGPointMake(400 - safe.right - 100, 800 - safe.bottom - 50)), @"a fitting banner stays inside the safe area");
}

@end
