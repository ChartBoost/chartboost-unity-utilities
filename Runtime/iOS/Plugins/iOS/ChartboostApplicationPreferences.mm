#import "ChartboostUnityUtilities.h"

static NSString* GetValueFromNSDefaults(const char* key)
{
    NSString* nsKey = [NSString stringWithUTF8String:key];
    return [[NSUserDefaults standardUserDefaults] objectForKey:nsKey];
}

extern "C" {

    int _CBApplicationPreferencesGetInt(const char* key, int defaultValue) {

        if (key == NULL)
            return defaultValue;

        NSString* value = GetValueFromNSDefaults(key);

        if (value == nil)
            return defaultValue;

        return [value intValue];
    }

    const char* _CBApplicationPreferencesGetString(const char* key, const char* defaultValue) {

        if (key == NULL)
            return defaultValue;

        NSString* value = GetValueFromNSDefaults(key);

        if (value == nil)
            return allocateCString(defaultValue);

        return toCStringOrNull(value);
    }

    void _CBApplicationPreferencesSetInt(const char* key, int value) {
        NSString *nsKey = [NSString stringWithUTF8String:key];
        [[NSUserDefaults standardUserDefaults] setInteger:value forKey:nsKey];
        [[NSUserDefaults standardUserDefaults] synchronize];
    }

    void _CBApplicationPreferencesSetString(const char* key, const char* value) {
        NSString *nsKey = [NSString stringWithUTF8String:key];
        NSString *nsValue = [NSString stringWithUTF8String:value];
        [[NSUserDefaults standardUserDefaults] setObject:nsValue forKey:nsKey];
        [[NSUserDefaults standardUserDefaults] synchronize];
    }
}
