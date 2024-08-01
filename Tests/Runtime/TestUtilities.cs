using System;
using Chartboost.Logging;
using NUnit.Framework;

namespace Chartboost.Tests.Runtime
{
    public static class TestUtilities 
    {
        public static void TestBooleanAccessor(Func<bool> get, Func<bool, bool> set)
        {
            var initial = get();
            LogController.Log(initial, LogLevel.Debug);
            Assert.False(initial);
            var change = set(true);
            LogController.Log(change, LogLevel.Debug);
            Assert.True(change);
            set(false);
            Assert.False(get());
        }

        public static void TestStringGetter(Func<string> get)
        {
            var value = get();
            LogController.Log(value, LogLevel.Debug);
            Assert.IsNotEmpty(value);
            Assert.IsNotNull(value);
        }
    }
}
