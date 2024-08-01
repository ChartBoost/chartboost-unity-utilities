using System;
using System.IO;
using System.Linq;
using System.Xml;
using Chartboost.Logging;
using NUnit.Framework;
using UnityEngine;

namespace Chartboost.Editor
{
    public static class VersionCheck
    {
        private const string NuGetVersionXPath = "/package/metadata/version";
        private const string NuGetXmlNamespace = "ns";
        private static readonly string NuGetVersionXPathWithNameSpace = $"/{NuGetXmlNamespace}:package/{NuGetXmlNamespace}:metadata/{NuGetXmlNamespace}:version";

        public static void ValidateVersions(string upmPackageName, string nuGetPackageName, string codeVersion = null)
        {
            var packageLocation = Directory.Exists($"Packages/{upmPackageName}") ?
                // UPM
                $"Packages/{upmPackageName}" :
                // NuGet
                Directory.GetDirectories($"Assets/Packages", $"{nuGetPackageName}*").First();

            var packageJson = Directory.GetFiles(packageLocation, "package.json").First();
            var nuspec = Directory.GetFiles(packageLocation, "*.nuspec").First();
            
            var upmVersion = GetUnityPackageManagerVersion(packageJson);
            var nugetVersion = GetNuGetVersion(nuspec);
            
            LogController.Log($"UPM Version : {upmVersion}", LogLevel.Debug);
            LogController.Log($"NuGet Version : {nugetVersion}", LogLevel.Debug);
            
            if (codeVersion == null)
                Assert.AreEqual(upmVersion, nugetVersion);
            else
            {
                LogController.Log($"Code Version: {codeVersion}", LogLevel.Debug);
                Assert.AreEqual(upmVersion, nugetVersion, codeVersion);
            }
        }

        private static string GetUnityPackageManagerVersion(string filePath)
        {
            LogController.Log($"UPM path : {filePath}", LogLevel.Debug);
            try
            {
                var jsonContent = File.ReadAllText(filePath);
                var jsonData = JsonUtility.FromJson<PackageJsonData>(jsonContent);
                return jsonData.version;
            }
            catch (Exception ex)
            {
                LogController.LogException(ex);
                return null;
            }
        }

        private static string GetNuGetVersion(string filePath)
        {
            LogController.Log($"NuGet path : {filePath}", LogLevel.Debug);
            var xmlDoc = new XmlDocument();
            
            try
            {
                xmlDoc.Load(filePath);
                XmlNode versionNode;
                
                if (!string.IsNullOrEmpty(xmlDoc.DocumentElement?.NamespaceURI))
                {
                    // Create an XmlNamespaceManager to handle namespaces
                    // https://stackoverflow.com/a/1089210
                    var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
                    namespaceManager.AddNamespace(NuGetXmlNamespace, xmlDoc.DocumentElement.NamespaceURI);
                    versionNode = xmlDoc.SelectSingleNode(NuGetVersionXPathWithNameSpace, namespaceManager);
                }
                else
                {
                    versionNode = xmlDoc.SelectSingleNode(NuGetVersionXPath);    
                }

                if (versionNode != null)
                {
                    return versionNode.InnerText.Trim();
                }
                
                LogController.LogException(new Exception("Version not found in the .nuspec metadata section."));
                return null;
            }
            catch (Exception ex)
            {
                LogController.LogException(ex);
                return null;
            }
        }
        
        [Serializable]
        internal class PackageJsonData
        {
            public string version;
            // Add other properties as needed
        }
    }
}
