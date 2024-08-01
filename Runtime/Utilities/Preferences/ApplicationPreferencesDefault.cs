using UnityEngine;

namespace Chartboost.Preferences
{
    /// <summary>
    /// Default implementation for <see cref="IApplicationPreferences"/>, this does not exactly represent SharedPreferences or User Defaults.
    /// Platform implementation might differ based on Unity's own implementation, this should only be used as a fallback when no custom implementation is available.
    /// </summary>
    public class ApplicationPreferencesDefault : IApplicationPreferences
    {
        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);

        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);

        public int GetInt(string key, int defaultValue = 0) => PlayerPrefs.GetInt(key, defaultValue);

        public string GetString(string key, string defaultValue = "") => PlayerPrefs.GetString(key, defaultValue);
    }
}
