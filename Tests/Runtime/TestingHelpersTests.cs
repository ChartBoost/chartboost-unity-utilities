using System.Collections;
using System.Threading;
using Chartboost.Testing;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Chartboost.Tests.Runtime
{
    /// <summary>Covers the shared native-test helpers in <c>Chartboost.Utilities.Testing</c>.</summary>
    public class TestingHelpersTests
    {
        private static readonly string[] NoInventory = { "NO_FILL" };

        [UnityTest]
        public IEnumerator UntilReturnsOnceTheConditionHolds()
        {
            var frames = 0;
            var start = Time.realtimeSinceStartup;

            yield return TestWait.Until(() => ++frames >= 3, 10f);

            Assert.AreEqual(3, frames);
            Assert.Less(Time.realtimeSinceStartup - start, 10f);
        }

        [UnityTest]
        public IEnumerator UntilStopsAtTheTimeout()
        {
            var start = Time.realtimeSinceStartup;

            yield return TestWait.Until(() => false, 0.2f);

            Assert.GreaterOrEqual(Time.realtimeSinceStartup - start, 0.2f);
        }

        [Test]
        public void NoInventoryCodeIsInconclusive()
            => Assert.Throws<InconclusiveException>(
                () => AdInventoryAssert.FailOrInconclusive("NO_FILL", "nothing to show", NoInventory, "load"));

        [TestCase("SERVER_ERROR")]
        [TestCase(null)]
        public void AnyOtherCodeFails(string code)
            => Assert.Throws<AssertionException>(
                () => AdInventoryAssert.FailOrInconclusive(code, "broken", NoInventory, "load"));

        [Test]
        public void CallbackRecorderCapturesSenderValueAndThread()
        {
            var recorder = new CallbackRecorder<string, int>();
            Assert.IsFalse(recorder.Fired);

            recorder.Invoke("ad", 42);

            Assert.IsTrue(recorder.Fired);
            Assert.AreEqual("ad", recorder.Sender);
            Assert.AreEqual(42, recorder.Value);
            Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, recorder.ThreadId);
        }

        [Test]
        public void EditorIsNotANativePlatform()
        {
            if (!Application.isEditor)
                Assert.Ignore("Only meaningful in the Editor.");
            Assert.IsFalse(DevicePlatform.IsNative);
        }
    }
}
