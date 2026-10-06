using System;
using System.Collections;
using UnityEngine;

namespace Chartboost.Testing
{
    public static class TestWait
    {
        /// <summary>Yields frames until <paramref name="condition"/> holds or <paramref name="timeoutSeconds"/> pass.</summary>
        /// <remarks>Doesn't fail on timeout: assert the condition afterwards, with a message about what didn't happen.</remarks>
        public static IEnumerator Until(Func<bool> condition, float timeoutSeconds)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition() && Time.realtimeSinceStartup < deadline)
                yield return null;
        }
    }
}
