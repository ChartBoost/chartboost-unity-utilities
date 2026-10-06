using System;
using System.Collections;
using Chartboost.Logging;
using NUnit.Framework;

namespace Chartboost.Testing
{
    /// <summary>
    /// The checks every Mediation adapter and Core consent module facade shares. In the Editor each runs its
    /// Default implementation, whose answers are known exactly; on a device the native adapter answers.
    /// </summary>
    public static class AdapterContract
    {
        /// <summary>
        /// The partner id (the same on every platform), and in the Editor the Default's display name and versions
        /// (the adapter's Unity version). Native adapters word their display names their own way.
        /// </summary>
        public static void AssertIdentity(string expectedIdentifier, string expectedDisplayName, string unityVersion,
            string identifier, string displayName, string nativeVersion, string partnerSdkVersion)
        {
            Assert.AreEqual(expectedIdentifier, identifier, "partner identifier");
            AssertVersion(unityVersion, "AdapterUnityVersion");
            if (DevicePlatform.IsNative)
            {
                Assert.IsNotEmpty(displayName, "partner display name");
                Assert.IsNotEmpty(nativeVersion, "native adapter version");
                Assert.IsNotEmpty(partnerSdkVersion, "partner SDK version");
                return;
            }
            Assert.AreEqual(expectedDisplayName, displayName, "partner display name");
            Assert.AreEqual(unityVersion, nativeVersion, "the Default reports the Unity version as the native adapter version");
            Assert.AreEqual(unityVersion, partnerSdkVersion, "the Default reports the Unity version as the partner SDK version");
        }

        /// <summary>A Core module's id, and a version (rewritten by release automation, so only its form is pinned).</summary>
        public static void AssertModuleIdentity(string expectedModuleId, string moduleId, string moduleVersion)
        {
            Assert.AreEqual(expectedModuleId, moduleId, "module id");
            AssertVersion(moduleVersion, "module version");
        }

        /// <summary>
        /// A setting keeps each value written to it; the original value is restored afterwards. In the Editor a
        /// flag, optional or list must also start unset (false, null or empty), as every Default does.
        /// </summary>
        public static void AssertKeeps<T>(Func<T> get, Action<T> set, params T[] values)
        {
            var original = get();
            if (!DevicePlatform.IsNative)
                AssertUnset(original);
            try
            {
                foreach (var value in values)
                {
                    set(value);
                    Assert.AreEqual(value, get());
                }
            }
            finally
            {
                set(original);
            }
        }

        /// <summary>Writing null to a list setting reads back an empty list; the original is restored.</summary>
        public static void AssertNullReadsEmpty<T>(Func<T> get, Action<T> set) where T : class, IEnumerable
        {
            var original = get();
            try
            {
                set(null);
                var value = get();
                Assert.IsNotNull(value, "null read back as null");
                Assert.IsFalse(value.GetEnumerator().MoveNext(), "null read back as a non-empty list");
            }
            finally
            {
                set(original);
            }
        }

        /// <summary>
        /// A setter the Default can't apply logs "<paramref name="method"/> does nothing on <paramref name="defaultClass"/>",
        /// so a misnamed log (HB-12134) fails here. Editor-only: on a device it would change real, unreadable consent state.
        /// </summary>
        public static void AssertDefaultSetterLogs(string method, string defaultClass, Action call)
        {
            if (DevicePlatform.IsNative)
                Assert.Ignore("The Default's log only exists in the Editor.");

            var expected = $"{method} does nothing on {defaultClass}";
            var logged = 0;
            void Count(string message, LogLevel level)
            {
                if (message.Contains(expected))
                    logged++;
            }

            var level = LogController.LoggingLevel;
            LogController.LoggingLevel = LogLevel.Info;
            LogController.MessageLogged += Count;
            try
            {
                call();
            }
            finally
            {
                LogController.MessageLogged -= Count;
                LogController.LoggingLevel = level;
            }
            Assert.AreEqual(1, logged, $"expected one \"{expected}\" log");
        }

        private static void AssertVersion(string version, string label)
            => Assert.That(version, Does.Match(@"^\d+\.\d+\.\d+"), $"{label} isn't a version");

        // Value types (nullables included) start at their default, lists empty; other references (e.g. a listener) may be set.
        private static void AssertUnset<T>(T value)
        {
            if (value is IEnumerable list && !(value is string))
                Assert.IsFalse(list.GetEnumerator().MoveNext(), "the Default should start with an empty list");
            else if (typeof(T).IsValueType)
                Assert.AreEqual(default(T), value, "the Default should start unset");
        }
    }
}
