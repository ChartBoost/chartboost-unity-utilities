#import <XCTest/XCTest.h>

extern "C" {
    int _CBApplicationPreferencesGetInt(const char *key, int defaultValue);
    const char *_CBApplicationPreferencesGetString(const char *key, const char *defaultValue);
    void _CBApplicationPreferencesSetInt(const char *key, int value);
    void _CBApplicationPreferencesSetString(const char *key, const char *value);
}

// Unity's NSUserDefaults bridge. Strings returned to Unity are freed by the marshaller, so they must be malloc'd copies.
@interface ChartboostApplicationPreferencesTests : XCTestCase
@end

@implementation ChartboostApplicationPreferencesTests

static const char *kIntKey = "cb.tests.prefs.int";
static const char *kStringKey = "cb.tests.prefs.string";

- (void)setUp {
    [self clearKeys];
}

- (void)tearDown {
    [self clearKeys];
    [super tearDown];
}

- (void)clearKeys {
    [[NSUserDefaults standardUserDefaults] removeObjectForKey:@(kIntKey)];
    [[NSUserDefaults standardUserDefaults] removeObjectForKey:@(kStringKey)];
}

- (void)testIntFallsBackToTheDefault {
    XCTAssertEqual(_CBApplicationPreferencesGetInt(kIntKey, 42), 42);
    XCTAssertEqual(_CBApplicationPreferencesGetInt(NULL, 7), 7);
}

- (void)testIntRoundTrips {
    _CBApplicationPreferencesSetInt(kIntKey, -5);
    XCTAssertEqual(_CBApplicationPreferencesGetInt(kIntKey, 42), -5);
}

- (void)testStringRoundTripsAsAFreshCopy {
    _CBApplicationPreferencesSetString(kStringKey, "héllo");
    const char *value = _CBApplicationPreferencesGetString(kStringKey, "fallback");
    XCTAssertTrue(value != NULL);
    XCTAssertEqualObjects(@(value), @"héllo");
    free((void *)value);
}

- (void)testMissingStringReturnsACopyOfTheDefault {
    const char *fallback = "fallback";
    const char *value = _CBApplicationPreferencesGetString(kStringKey, fallback);
    XCTAssertTrue(value != NULL);
    XCTAssertEqualObjects(@(value), @"fallback");
    XCTAssertTrue(value != fallback, @"Unity frees the result, so it must not be the caller's pointer");
    free((void *)value);
}

- (void)testNullKeyReturnsACopyOfTheDefault {
    const char *fallback = "fallback";
    const char *value = _CBApplicationPreferencesGetString(NULL, fallback);
    XCTAssertTrue(value != NULL);
    XCTAssertEqualObjects(@(value), @"fallback");
    XCTAssertTrue(value != fallback);
    free((void *)value);
}

- (void)testNullDefaultStaysNull {
    XCTAssertTrue(_CBApplicationPreferencesGetString(kStringKey, NULL) == NULL);
}

- (void)testNullKeyIsIgnoredBySetters {
    _CBApplicationPreferencesSetInt(NULL, 1);
    _CBApplicationPreferencesSetString(NULL, "x");
}

- (void)testNullStringValueRemovesTheKey {
    _CBApplicationPreferencesSetString(kStringKey, "old");
    _CBApplicationPreferencesSetString(kStringKey, NULL);
    const char *value = _CBApplicationPreferencesGetString(kStringKey, "fallback");
    XCTAssertEqualObjects(@(value), @"fallback");
    free((void *)value);
}

- (void)testMismatchedTypesReadAsTheDefault {
    _CBApplicationPreferencesSetInt(kIntKey, 5);
    const char *asString = _CBApplicationPreferencesGetString(kIntKey, "fallback");
    XCTAssertEqualObjects(@(asString), @"fallback");
    free((void *)asString);
}

@end
