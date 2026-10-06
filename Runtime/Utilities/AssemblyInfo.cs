using System.Runtime.CompilerServices;
using Chartboost;

[assembly: InternalsVisibleTo(AssemblyInfo.UtilitiesAssemblyAndroid)]
[assembly: InternalsVisibleTo(AssemblyInfo.UtilitiesAssemblyIOS)]
[assembly: InternalsVisibleTo(AssemblyInfo.UtilitiesRuntimeTestsAssembly)]
[assembly: InternalsVisibleTo(AssemblyInfo.MediationRuntimeTestsAssembly)]

namespace Chartboost
{
    internal class AssemblyInfo
    {
        public const string UtilitiesAssemblyAndroid = "Chartboost.Utilities.Android";
        public const string UtilitiesAssemblyIOS = "Chartboost.Utilities.IOS";
        // Test assemblies that exercise shared utilities internals (e.g. AdCache<T> test seams).
        public const string UtilitiesRuntimeTestsAssembly = "Chartboost.Utilities.Tests.Runtime";
        public const string MediationRuntimeTestsAssembly = "Chartboost.Mediation.Tests.Runtime";
    }
}
