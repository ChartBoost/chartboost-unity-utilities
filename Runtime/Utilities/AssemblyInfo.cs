using System.Runtime.CompilerServices;
using Chartboost;

[assembly: InternalsVisibleTo(AssemblyInfo.UtilitiesAssemblyAndroid)]
[assembly: InternalsVisibleTo(AssemblyInfo.UtilitiesAssemblyIOS)]

namespace Chartboost
{
    internal class AssemblyInfo
    {
        public const string UtilitiesAssemblyAndroid = "Chartboost.Utilities.Android";
        public const string UtilitiesAssemblyIOS = "Chartboost.Utilities.IOS";
    }
}
