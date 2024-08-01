using Chartboost.Logging;
using Chartboost.Preferences;
using NUnit.Framework;
using UnityEngine;

namespace Chartboost.Tests
{
    public class ApplicationPreferencesTests
    {
        private const string IntKey = "application.preferences.int";
        private const string StringKey = "application.preferences.string";
        
        [SetUp]
        public void Reset()
        {
            LogController.LoggingLevel = LogLevel.Debug;
            // If using default implementation, let's make sure we start clean
            PlayerPrefs.DeleteAll();
        }

        [Test, Order(0)]
        public void GetIntDefault()
        {
            var getInt = ApplicationPreferences.GetInt(IntKey, 10);
            LogController.Log($"Int Default Value: {getInt}", LogLevel.Debug);
            Assert.AreEqual(getInt, 10);
            Reset();
        }

        [Test, Order(0)]
        public void GetStringDefault()
        {
            var getString = ApplicationPreferences.GetString(StringKey, "customValue");
            LogController.Log($"String Default Value: {getString}", LogLevel.Debug);
            Assert.AreEqual(getString, "customValue");
            Reset();
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
    }
}
