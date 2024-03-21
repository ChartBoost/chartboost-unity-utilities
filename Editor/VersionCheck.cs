using System;
using System.IO;
using System.Linq;
using System.Xml;
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

            var packageJson = Directory.GetFiles(packageLocation, "*.json").First();
            var nuspec = Directory.GetFiles(packageLocation, "*.nuspec").First();
            
            var upmVersion = GetUnityPackageManagerVersion(packageJson);
            var nugetVersion = GetNuGetVersion(nuspec);
            
            Debug.Log($"UPM Version : {upmVersion}");
            Debug.Log($"NuGet Version : {nugetVersion}");
            
            if (codeVersion == null)
                Assert.AreEqual(upmVersion, nugetVersion);
            else
            {
                Debug.Log($"Code Version: {codeVersion}");
                Assert.AreEqual(upmVersion, nugetVersion, codeVersion);
            }
        }

        private static string GetUnityPackageManagerVersion(string filePath)
        {
            Debug.Log($"UPM path : {filePath}");
            try
            {
                var jsonContent = System.IO.File.ReadAllText(filePath);
                var jsonData = JsonUtility.FromJson<PackageJsonData>(jsonContent);
                return jsonData.version;
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred while reading the package.json file: {ex.Message}");
                return null;
            }
        }

        private static string GetNuGetVersion(string filePath)
        {
            Debug.Log($"NuGet path : {filePath}");
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
                
                Debug.LogError("Version not found in the .nuspec metadata section.");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred while reading the .nuspec file: {ex.Message}");
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
