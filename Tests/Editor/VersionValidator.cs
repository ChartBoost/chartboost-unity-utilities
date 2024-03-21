using NUnit.Framework;
using Chartboost.Editor;

namespace Chartboost.Utilities
{
    public class VersionValidator
    {
        private const string UnityPackageManagerPackageName = "com.chartboost.unity.utilities";
        private const string NuGetPackageName = "Chartboost.CSharp.Utilities.Unity";
        
        [Test]
        public void ValidateVersion() 
            => VersionCheck.ValidateVersions(UnityPackageManagerPackageName, NuGetPackageName);
    }
}
