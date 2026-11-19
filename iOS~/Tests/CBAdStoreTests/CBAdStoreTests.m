#import <XCTest/XCTest.h>
#import "CBAdStore.h"

// The retention primitive under both wrappers' ad stores: ARC must not free an ad while Unity holds its handle.
@interface CBAdStoreTests : XCTestCase
@end

@implementation CBAdStoreTests

- (void)testTrackAndLookUp {
    CBAdStore *store = [[CBAdStore alloc] init];
    NSObject *ad = [NSObject new];
    [store trackAd:ad forKey:7];
    XCTAssertIdentical([store adForKey:7], ad);
    XCTAssertEqual(store.count, 1u);
}

- (void)testUnknownKeyIsNil {
    CBAdStore *store = [[CBAdStore alloc] init];
    XCTAssertNil([store adForKey:1]);
    XCTAssertNil([store removeKey:1]);
}

- (void)testTrackNilIsANoOp {
    CBAdStore *store = [[CBAdStore alloc] init];
    [store trackAd:(id _Nonnull)nil forKey:3];
    XCTAssertEqual(store.count, 0u);
}

- (void)testTrackingTheSameKeyReplacesTheAd {
    CBAdStore *store = [[CBAdStore alloc] init];
    NSObject *first = [NSObject new];
    NSObject *second = [NSObject new];
    [store trackAd:first forKey:5];
    [store trackAd:second forKey:5];
    XCTAssertIdentical([store adForKey:5], second);
    XCTAssertEqual(store.count, 1u);
}

- (void)testRemoveReturnsTheAdAndForgetsIt {
    CBAdStore *store = [[CBAdStore alloc] init];
    NSObject *ad = [NSObject new];
    [store trackAd:ad forKey:9];
    XCTAssertIdentical([store removeKey:9], ad);
    XCTAssertNil([store adForKey:9]);
    XCTAssertEqual(store.count, 0u);
}

- (void)testStoreRetainsUntilRemoved {
    CBAdStore *store = [[CBAdStore alloc] init];
    __weak NSObject *weakAd;
    @autoreleasepool {
        NSObject *ad = [NSObject new];
        weakAd = ad;
        [store trackAd:ad forKey:1];
    }
    XCTAssertNotNil(weakAd);
    @autoreleasepool { [store removeKey:1]; }
    XCTAssertNil(weakAd);
}

- (void)testClearReleasesEverything {
    CBAdStore *store = [[CBAdStore alloc] init];
    __weak NSObject *weakAd;
    @autoreleasepool {
        NSObject *ad = [NSObject new];
        weakAd = ad;
        [store trackAd:ad forKey:1];
        [store trackAd:[NSObject new] forKey:2];
    }
    [store clear];
    XCTAssertNil(weakAd);
    XCTAssertEqual(store.count, 0u);
}

- (void)testStoresAreIsolated {
    CBAdStore *a = [[CBAdStore alloc] init];
    CBAdStore *b = [[CBAdStore alloc] init];
    [a trackAd:[NSObject new] forKey:1];
    XCTAssertNil([b adForKey:1]);
    XCTAssertEqual(b.count, 0u);
}

- (void)testConcurrentAccessIsSafe {
    CBAdStore *store = [[CBAdStore alloc] init];
    NSMutableArray *ads = [NSMutableArray array];
    for (int i = 0; i < 500; i++) [ads addObject:[NSObject new]];
    dispatch_apply(ads.count, dispatch_get_global_queue(QOS_CLASS_USER_INITIATED, 0), ^(size_t i) {
        [store trackAd:ads[i] forKey:(long)i];
        (void)[store adForKey:(long)i];
        if (i % 2 == 1) [store removeKey:(long)i];
    });
    XCTAssertEqual(store.count, 250u);
}

@end
