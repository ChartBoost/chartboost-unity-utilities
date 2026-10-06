using System.Collections.Generic;
using NUnit.Framework;

namespace Chartboost.Testing
{
    /// <summary>
    /// Ad tests run against live demand: a lack of ads is not a code problem, so it marks the test Inconclusive
    /// rather than failing it. Each SDK passes the error codes that mean "no inventory" for its own errors.
    /// </summary>
    public static class AdInventoryAssert
    {
        /// <summary>Inconclusive when <paramref name="code"/> is a no-inventory code; fails the test otherwise.</summary>
        public static void FailOrInconclusive(string code, string message, ICollection<string> noInventoryCodes, string context)
        {
            if (code != null && noInventoryCodes.Contains(code))
                Assert.Inconclusive($"[Ad Inventory - {code}] {context}: {message}. A lack of ad inventory, not a code issue.");
            Assert.Fail($"{context}: {code ?? "unknown"} - {message}");
        }
    }
}
