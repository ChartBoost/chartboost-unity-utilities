namespace Chartboost.Preferences
{
    /// <summary>
    /// Stores local data related to Chartboost SDKs. Data is stored in local registries without encryption.
    /// Defaults to `PlayerPrefs` if native implementation does not exist.
    /// </summary>
    public static class ApplicationPreferences
    {
        private static readonly IApplicationPreferences Default = new ApplicationPreferencesDefault();
        
        internal static IApplicationPreferences Instance = Default;

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        public static int GetInt(string key, int defaultValue = default) => Instance.GetInt(key, defaultValue);

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        public static string GetString(string key, string defaultValue = "") => Instance.GetString(key, defaultValue);
        
        /// <summary>
        /// Sets a single <see cref="int"/> value for the preference identified by the given key. You can use <see cref="ApplicationPreferences.GetInt"/> to retrieve this value.
        /// </summary>
        public static void SetInt(string key, int value) => Instance.SetInt(key, value);
        
        /// <summary>
        /// Sets a single <see cref="string"/> value for the preference identified by the given key. You can use <see cref="ApplicationPreferences.GetString"/> to retrieve this value.
        /// </summary>
        public static void SetString(string key, string value) => Instance.SetString(key, value);
    }
}
