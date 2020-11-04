using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VisualNovelFramework
{
    public class VNGlobalSettings : ScriptableObject
    {
        public Texture2D previewSceneLayout;

        public static string GetSettingPath()
        {
            var asset = AssetDatabase.FindAssets("t:" + nameof(VNGlobalSettings));

            if (asset.Length == 0)
            {
                Debug.LogError("Unable to find settings.");
            }

            var settingsPathGUID = asset.First();
            var settingsPath = AssetDatabase.GUIDToAssetPath(settingsPathGUID);

            var splits = settingsPath.Split('/');
            var finalString = "";
            for (int i = 0; i < splits.Length-1; i++)
            {
                finalString += splits[i] + '/';
            }
            
            return finalString;
        }
    }
}
