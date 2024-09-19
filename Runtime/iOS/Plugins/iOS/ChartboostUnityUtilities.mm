#import "ChartboostUnityUtilities.h"

void toMain(block block) {
    dispatch_async(dispatch_get_main_queue(), block);
}

id toObjectFromJson(const char * jsonString) {
    NSData* jsonData = [[NSString stringWithUTF8String:jsonString] dataUsingEncoding:NSUTF8StringEncoding];
    NSError* error = nil;
    id arr = [NSJSONSerialization JSONObjectWithData:jsonData options:0 error:&error];

    if (error != nil)
        return nil;

    return arr;
}

const char * allocateCString(const char* target)
{
    if (target == NULL)
        return NULL;

    char* cString = (char*)malloc(strlen(target) + 1);
    strcpy(cString, target);
    return cString;
}

const char * toCStringOrNull(NSString* nsString) {
    if (nsString == NULL)
        return NULL;

    return allocateCString([nsString UTF8String]);
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
        return [NSDictionary dictionary];

    return dict;
}

NSMutableArray* toNSMutableArray(const char** cArray, int cArrayCount){
    NSMutableArray *retArray = [NSMutableArray new];

    if (cArrayCount > 0) {
        for (int x = 0; x < cArrayCount; x++)
        {
            if(strlen(cArray[x]) > 0)
                [retArray addObject:toNSStringOrNull(cArray[x])];
        }

        return retArray;
    }

    return retArray;
}

const char * toJSON(id data)
{
    return toCStringOrNull(toJSONNSString(data));
}


NSString * toJSONNSString(id data)
{
    if (data == nil)
        return NULL;

    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:data options:0 error:&error];
    if (!jsonData) {
        NSLog(@"%s: error: %@", __func__, error.localizedDescription);
        return NULL;
     }
    return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
}

int hashCode(NSString* value) {
    int h = 0;

    for (int i = 0; i < (int)value.length; i++) {
        h = (31 * h) + [value characterAtIndex:i];
    }

    return h;
}
