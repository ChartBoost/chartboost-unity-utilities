using Chartboost.Constants;
using Chartboost.Preferences;
using UnityEngine;

namespace Chartboost
{
    internal class ApplicationPreferences : IApplicationPreferences
    {
        private static readonly string PreferencesName = $"{Application.identifier}_preferences";
        private const int ContextMode = 0;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInstance()
        {
            if (Application.isEditor)
                return;

            Chartboost.Preferences.ApplicationPreferences.Instance = new ApplicationPreferences();
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            using var sharedPreferences = GetSharedPreferences();
            return sharedPreferences.Call<int>(SharedAndroidConstants.FunctionGetInt, key, defaultValue);
        }

        public string GetString(string key, string defaultValue = "")
        {
            using var sharedPreferences = GetSharedPreferences();
            return sharedPreferences.Call<string>(SharedAndroidConstants.FunctionGetString, key, defaultValue);
        }

        public void SetInt(string key, int value)
        {
            using var sharedPreferences = GetSharedPreferences();
            var sharedPreferencesEditor = sharedPreferences.Call<AndroidJavaObject>(SharedAndroidConstants.FunctionEdit);
            sharedPreferencesEditor.Call<AndroidJavaObject>(SharedAndroidConstants.FunctionPutInt, key, value);
            sharedPreferencesEditor.Call(SharedAndroidConstants.FunctionApply);
        }

        public void SetString(string key, string value)
        {
            using var sharedPreferences = GetSharedPreferences();
            var sharedPreferencesEditor = sharedPreferences.Call<AndroidJavaObject>(SharedAndroidConstants.FunctionEdit);
            sharedPreferencesEditor.Call<AndroidJavaObject>(SharedAndroidConstants.FunctionPutString, key, value);
            sharedPreferencesEditor.Call(SharedAndroidConstants.FunctionApply);
        }

        private static AndroidJavaObject GetSharedPreferences()
        {
            using var currentActivity = SharedAndroidConstants.UnityPlayerCurrentActivity();
            return currentActivity.Call<AndroidJavaObject>(SharedAndroidConstants.FunctionGetSharedPreferences, PreferencesName, ContextMode);
        }
    }
}
