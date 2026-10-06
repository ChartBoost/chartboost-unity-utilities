using Chartboost.Logging;
using Chartboost.Preferences;
using Chartboost.Testing;
using NUnit.Framework;
using UnityEngine;

namespace Chartboost.Tests
{
    public class ApplicationPreferencesTests : DebugLogLevelFixture
    {
        private const string IntKey = "application.preferences.int";
        private const string StringKey = "application.preferences.string";
        
        [SetUp]
        public void SetUp() => Reset();

        [TearDown]
        public void TearDown() => Reset();

        // Only our keys: DeleteAll would wipe the project's (or the Editor's) other preferences.
        private static void Reset()
        {
            PlayerPrefs.DeleteKey(IntKey);
            PlayerPrefs.DeleteKey(StringKey);
        }

        [Test, Order(0)]
        public void GetIntDefault()
        {
            var getInt = ApplicationPreferences.GetInt(IntKey, 10);
            LogController.Log($"Int Default Value: {getInt}", LogLevel.Debug);
            Assert.AreEqual(getInt, 10);
        }

        [Test, Order(0)]
        public void GetStringDefault()
        {
            var getString = ApplicationPreferences.GetString(StringKey, "customValue");
            LogController.Log($"String Default Value: {getString}", LogLevel.Debug);
            Assert.AreEqual(getString, "customValue");
        }

        [Test, Order(1)]
        public void GetIntNoSet()
        {
            var getInt = ApplicationPreferences.GetInt(IntKey);
            Assert.AreEqual(0, getInt);
        }

        [Test, Order(1)]
        public void GetStringNoSet()
        {
            var getString = ApplicationPreferences.GetString(StringKey);
            Assert.AreEqual(string.Empty, getString);
        }

        // Through the PlayerPrefs-backed default, so Reset() can clean up on device too.
        [Test]
        public void SetValuesAreReadBack()
        {
            var original = ApplicationPreferences.Instance;
            ApplicationPreferences.Instance = new ApplicationPreferencesDefault();
            try
            {
                ApplicationPreferences.SetInt(IntKey, 42);
                ApplicationPreferences.SetString(StringKey, "stored");

                Assert.AreEqual(42, ApplicationPreferences.GetInt(IntKey, 10));
                Assert.AreEqual("stored", ApplicationPreferences.GetString(StringKey, "fallback"));
            }
            finally
            {
                ApplicationPreferences.Instance = original;
            }
        }
    }
}
