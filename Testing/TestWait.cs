using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace Chartboost.Testing
{
    public static class TestWait
    {
        public const float DefaultTimeoutSeconds = 10f;

        /// <summary>Yields frames until <paramref name="condition"/> holds or <paramref name="timeoutSeconds"/> pass.</summary>
        /// <remarks>Doesn't fail on timeout: assert the condition afterwards, or use the overload that takes <c>what</c>.</remarks>
        public static IEnumerator Until(Func<bool> condition, float timeoutSeconds)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition() && Time.realtimeSinceStartup < deadline)
                yield return null;
        }

        /// <summary>Like <see cref="Until(Func{bool}, float)"/>, but fails the test on timeout with "<paramref name="what"/> didn't happen in Ns".</summary>
        public static IEnumerator Until(Func<bool> condition, string what, float timeoutSeconds = DefaultTimeoutSeconds)
        {
            yield return Until(condition, timeoutSeconds);
            Assert.IsTrue(condition(), $"{what} didn't happen in {timeoutSeconds}s");
        }
    }
}
