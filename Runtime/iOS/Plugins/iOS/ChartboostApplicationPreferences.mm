#import "ChartboostUnityUtilities.h"

static id GetValueFromNSDefaults(const char* key)
{
    NSString* nsKey = [NSString stringWithUTF8String:key];
    return [[NSUserDefaults standardUserDefaults] objectForKey:nsKey];
}

extern "C" {

    int _CBApplicationPreferencesGetInt(const char* key, int defaultValue) {

        if (key == NULL)
            return defaultValue;

        id value = GetValueFromNSDefaults(key);

        if (![value respondsToSelector:@selector(intValue)])
            return defaultValue;

        return [value intValue];
    }

    const char* _CBApplicationPreferencesGetString(const char* key, const char* defaultValue) {

        // Unity frees the returned string, so every path returns a copy.
        if (key == NULL)
            return allocateCString(defaultValue);

        id value = GetValueFromNSDefaults(key);

        // A key holding a number (SetInt) reads as the default, not as a string.
        if (![value isKindOfClass:[NSString class]])
            return allocateCString(defaultValue);

        return toCStringOrNull(value);
    }

    void _CBApplicationPreferencesSetInt(const char* key, int value) {
        if (key == NULL)
            return;

        NSString *nsKey = [NSString stringWithUTF8String:key];
        [[NSUserDefaults standardUserDefaults] setInteger:value forKey:nsKey];
        [[NSUserDefaults standardUserDefaults] synchronize];
    }

    void _CBApplicationPreferencesSetString(const char* key, const char* value) {
        if (key == NULL)
            return;

        // A null value removes the key, like Android's SharedPreferences.Editor.putString(key, null).
        NSString *nsKey = [NSString stringWithUTF8String:key];
        if (value == NULL)
            [[NSUserDefaults standardUserDefaults] removeObjectForKey:nsKey];
        else
            [[NSUserDefaults standardUserDefaults] setObject:[NSString stringWithUTF8String:value] forKey:nsKey];
        [[NSUserDefaults standardUserDefaults] synchronize];
    }
}
