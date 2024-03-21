#import <Foundation/Foundation.h>
#import <objc/runtime.h>

typedef void (^block)(void);

/// Sends a bloc to be executed on the main thread.
///
/// - Parameter block: block of code to be executed on the main thread.
void toMain(block block);


/// Allocates a C string from a NSString.
///
/// Use to allocate the C String reference when converting from NSString to C string. This reference should be marshalled to Unity so it can be managed and disposed there as needed.
///
/// - Parameter nsString: NSString to use as a base for the C string.
/// - Returns: Allocated C string or Null.
const char * toCStringOrNull(NSString* nsString);

/// Allocates a NSString from a C String.
///
/// Use to allocate a NSString reference when converting from C String.
///
/// - Parameter cString: C String to use as a base for the NString.
/// - Returns: Allocated NString or Empty.
NSString * toNSStringOrEmpty(const char* cString);

/// Allocates a NSString from a C String.
///
/// Use to allocate a NSString reference when converting from C String.
///
/// - Parameter cString: C String to use as a base for the NSString.
/// - Returns: Allocated NSString or Null.
NSString * toNSStringOrNull(const char* cString);


/// Allocates a NSDictionary from a JSON C String.
///
/// Use to allocate a NSDictionary reference when converting from C String.
///
/// - Parameter cString: C String to use as a base for the NSString.
/// - Returns: Allocated NSString or Null.
NSDictionary* toNSDictionary(const char* cString);

/// Allocates a JSON C String from an id
///
/// Use to allocate a C String reference when converting from id serializable.
///
/// - Parameter cString: C String to use as a base for the NSString.
/// - Returns: Allocated JSON C String or Null.
const char * toJSON(id data);
