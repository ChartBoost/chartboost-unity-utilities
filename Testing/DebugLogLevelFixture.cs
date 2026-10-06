using Chartboost.Logging;
using NUnit.Framework;

namespace Chartboost.Testing
{
    /// <summary>Runs each test at <see cref="LogLevel.Debug"/> and restores the previous level afterwards.</summary>
    public abstract class DebugLogLevelFixture
    {
        private LogLevel _initialLogLevel;

        [SetUp]
        public void SetDebugLogLevel()
        {
            _initialLogLevel = LogController.LoggingLevel;
            LogController.LoggingLevel = LogLevel.Debug;
        }

        [TearDown]
        public void RestoreLogLevel() => LogController.LoggingLevel = _initialLogLevel;
    }
}
