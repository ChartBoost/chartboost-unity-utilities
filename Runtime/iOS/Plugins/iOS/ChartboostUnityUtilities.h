#import <Foundation/Foundation.h>
#import <objc/runtime.h>

typedef void (^block)(void);

/// Sends a bloc to be executed on the main thread.
///
/// - Parameter block: block of code to be executed on the main thread.
void toMain(block block);

/// Creates an id from a C String
///
/// - Parameter jsonString: Json string to create id from.
id toObjectFromJson(const char * jsonString);

/// Creates a null terminated C string on the heap so that our string's memory isn't wiped out right after method's return
/// 
/// - Parameter target: Target C string to allocate
const char * allocateCString(const char * target);

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


/// Allocates a NSMutableArray from a C array..
///
/// Use to allocate a NSMutableArray reference when converting from C Array.
///
/// - Parameter cArray: C Array reference to use as a base for the NSMutableArray.
/// - Parameter cArrayCount: C Array count.
/// - Returns: Allocated NSMutableArray or Null.
NSMutableArray* toNSMutableArray(const char** cArray, int cArrayCount);

/// Allocates a JSON C String from an id
///
/// Use to allocate a C String reference when converting from id serializable.
///
/// - Parameter data: C String to use as a base for the NSString.
/// - Returns: Allocated JSON C String or Null.
const char * toJSON(id data);


/// Allocates a JSON  NSString from an id
///
/// Use to allocate a NSString reference when converting from id serializable.
///
/// - Parameter data: NString to use as a base for the NSString.
/// - Returns: Allocated JSON NSString or Null.
NSString * toJSONNSString(id data);

/// Generates a int hashCode from a NSString
///
///  Use to obtain a Java like hashCode from a NSString
///
///  - Parameter value: Target NSString.
///  - Returns: int Java like hashCode
int hashCode(NSString* value);
