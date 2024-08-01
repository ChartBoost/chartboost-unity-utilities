namespace Chartboost.Preferences
{
    public interface IApplicationPreferences
    {
        int GetInt(string key, int defaultValue = 0);

        string GetString(string key, string defaultValue = "");

        void SetInt(string key, int value);
        void SetString(string key, string value);
    }
}
