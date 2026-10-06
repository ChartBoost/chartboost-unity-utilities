using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Chartboost.Testing
{
    /// <summary>
    /// Base for device-only fixtures: ignores them unless they run on Android or iOS. Derived
    /// <c>[OneTimeSetUp]</c> methods run after this one, so they only run on a device.
    /// Shows on-screen progress for each test, counted from the fixture itself.
    /// </summary>
    [Category("Device")]
    public abstract class NativeTestFixture
    {
        private int _testNumber;
        private int _testCount;

        [OneTimeSetUp]
        public void IgnoreOffDevice()
        {
            if (!DevicePlatform.IsNative)
                Assert.Ignore("Native tests only run on Android or iOS.");
            _testNumber = 0;
            // Unity's runner sets its own context here, not NUnit's TestExecutionContext.CurrentContext.
            _testCount = CountTests(TestContext.CurrentTestExecutionContext?.CurrentTest);
            TestProgressTracker.Show();
        }

        [SetUp]
        public void ReportProgress()
            => TestProgressTracker.NotifyTestStart(GetType().Name, ++_testNumber, _testCount, TestContext.CurrentContext.Test.Name);

        [OneTimeTearDown]
        public void ClearProgress()
        {
            if (DevicePlatform.IsNative)
                TestProgressTracker.Clear();
        }

        // Parameterized tests count once per case.
        private static int CountTests(ITest test) => test == null ? 0 : test.HasChildren ? test.Tests.Sum(CountTests) : 1;
    }
}
