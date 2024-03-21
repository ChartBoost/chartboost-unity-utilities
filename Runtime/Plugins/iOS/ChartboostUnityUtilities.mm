#import "ChartboostUnityUtilities.h"

void toMain(block block) {
    dispatch_async(dispatch_get_main_queue(), block);
}

const char * toCStringOrNull(NSString* nsString) {
    if (nsString == NULL)
        return NULL;

    const char* nsStringUtf8 = [nsString UTF8String];
    //create a null terminated C string on the heap so that our string's memory isn't wiped out right after method's return
    char* cString = (char*)malloc(strlen(nsStringUtf8) + 1);
    strcpy(cString, nsStringUtf8);
    return cString;
}

NSString * toNSStringOrEmpty(const char* cString) {
    return cString != NULL ? [NSString stringWithUTF8String:cString] : @"";
}

NSString * toNSStringOrNull(const char* cString) {
    return (cString != NULL && strlen(cString)) ? [NSString stringWithUTF8String:cString] : nil;
}

NSDictionary* toNSDictionary(const char* cString)
{
    if (cString == nil)
        return [NSDictionary dictionary];

    NSError *error;
    NSData* jsonData = [[NSString stringWithUTF8String:cString] dataUsingEncoding:NSUTF8StringEncoding];

    NSDictionary* dict = [NSJSONSerialization JSONObjectWithData:jsonData options:0 error:&error];

    if (error != nil)
        return nil;

    return dict;
}

const char * toJSON(id _Nonnull data)
{
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:data options:0 error:&error];
    if (! jsonData) {
        NSLog(@"%s: error: %@", __func__, error.localizedDescription);
        return "";
     }
    NSString *json = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    return toCStringOrNull(json);
}
