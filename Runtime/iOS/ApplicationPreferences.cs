using System.Runtime.InteropServices;
using Chartboost.Constants;
using Chartboost.Preferences;
using UnityEngine;

namespace Chartboost
{
    internal class ApplicationPreferences : IApplicationPreferences
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterInstance()
        {
            if (Application.isEditor)
                return;

            Chartboost.Preferences.ApplicationPreferences.Instance = new ApplicationPreferences();
        }
        
        public int GetInt(string key, int defaultValue = 0) => _CBApplicationPreferencesGetInt(key, defaultValue);
        
        public string GetString(string key, string defaultValue = "") => _CBApplicationPreferencesGetString(key, defaultValue);

        public void SetInt(string key, int value) => _CBApplicationPreferencesSetInt(key, value);

        public void SetString(string key, string value) => _CBApplicationPreferencesSetString(key, value);

        [DllImport(SharedIOSConstants.DLLImport)] private static extern int _CBApplicationPreferencesGetInt(string key, int defaultValue);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern string _CBApplicationPreferencesGetString(string key, string defaultValue);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBApplicationPreferencesSetInt(string key, int value);
        [DllImport(SharedIOSConstants.DLLImport)] private static extern void _CBApplicationPreferencesSetString(string key, string value);
    }
}
