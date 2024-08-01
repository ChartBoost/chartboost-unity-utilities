using System.IO;
using UnityEditor;

namespace Chartboost.Editor
{
    public static class EditorConstants
    {
        public static string AssetsDirectory => "Assets";
        public static string EditorDefaultResourcesDirectory => "Editor Default Resources";

        public static string PluginsDirectory => "Plugins";

        public static string AndroidDirectory => "Android";

        public static string AndroidManifestFile => "AndroidManifest.xml";

        public static string PathToAndroidManifest = Path.Combine(AssetsDirectory, PluginsDirectory, AndroidDirectory, AndroidManifestFile);
        
        public static string PathToEditorDefaultResources => Path.Combine(AssetsDirectory, EditorDefaultResourcesDirectory);
        
        public static void ValidateEditorResourcesDirectories()
        {
            if(AssetDatabase.IsValidFolder(Path.Combine(AssetsDirectory, EditorDefaultResourcesDirectory)))
               return;
            AssetDatabase.CreateFolder(AssetsDirectory, EditorDefaultResourcesDirectory);
            AssetDatabase.Refresh();
        }
    }
}
