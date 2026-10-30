#import <XCTest/XCTest.h>
#import "ChartboostUnityUtilities.h"

// The C-string / JSON marshaling every iOS bridge uses. Strings handed to Unity are malloc'd; free them here.
@interface ChartboostUnityUtilitiesTests : XCTestCase
@end

@implementation ChartboostUnityUtilitiesTests

- (void)testNullInputsAreSafe
{
    XCTAssertTrue(allocateCString(NULL) == NULL);
    XCTAssertTrue(toCStringOrNull(nil) == NULL);
    XCTAssertEqualObjects(toNSStringOrEmpty(NULL), @"");
    XCTAssertNil(toNSStringOrNull(NULL));
    XCTAssertNil(toNSStringOrNull(""), @"empty reads as null");
    XCTAssertNil(toObjectFromJson(NULL));
    XCTAssertEqualObjects(toNSDictionary(NULL), @{});
    XCTAssertTrue(toJSON(nil) == NULL);
}

- (void)testCStringsAreCopiedAndRoundTripUtf8
{
    const char *copy = toCStringOrNull(@"héllo 🙂");
    XCTAssertEqualObjects(toNSStringOrNull(copy), @"héllo 🙂");
    free((void *)copy);
}

- (void)testJsonRoundTripKeepsTypes
{
    NSDictionary *input = @{@"s": @"x", @"n": @3, @"b": @YES, @"a": @[@1, @2], @"d": @{@"k": @"v"}};
    const char *json = toJSON(input);
    XCTAssertEqualObjects(toNSDictionary(json), input);
    XCTAssertEqualObjects(toObjectFromJson(json), input);
    free((void *)json);
}

- (void)testInvalidJsonDegradesInsteadOfThrowing
{
    XCTAssertEqualObjects(toNSDictionary("{not json"), @{});
    XCTAssertNil(toObjectFromJson("{not json"));
    XCTAssertNoThrow(toJSONNSString(@{@"date": [NSDate date]}));
    XCTAssertNil(toJSONNSString(@{@"date": [NSDate date]}), @"non-JSON values give null, not an exception");
}

- (void)testArraysSkipEmptyAndNullEntries
{
    const char *items[] = {"a", "", NULL, "b"};
    XCTAssertEqualObjects(toNSMutableArray(items, 4), (@[@"a", @"b"]));
    XCTAssertEqualObjects(toNSMutableArray(NULL, 0), @[]);
}

- (void)testHashCodeMatchesJava
{
    // Values from Java's String.hashCode(): cross-platform ids depend on them matching.
    XCTAssertEqual(hashCode(@""), 0);
    XCTAssertEqual(hashCode(@"hello"), 99162322);
    XCTAssertEqual(hashCode(@"Chartboost Mediation banner"), -1033580657, @"overflow wraps like Java's 32-bit int");
}

@end
