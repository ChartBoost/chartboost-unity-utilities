using Chartboost.Editor;
using NUnit.Framework;

namespace Chartboost.Utilities.Tests.Editor
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
