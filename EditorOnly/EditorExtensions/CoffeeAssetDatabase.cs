using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VisualNovelFramework.EditorExtensions
{
    public static class CoffeeAssetDatabase
    {
        /// <summary>
        ///     Searches for all assets of type T and returns them as a list
        ///     This search accounts for sub-nested assets.
        /// </summary>
        public static List<T> FindAssetsOfType<T>() where T : Object
        {
            var searchStr = "t:" + typeof(T).Name;
            var charGuids = AssetDatabase.FindAssets(searchStr);
            var items = new List<T>();
            foreach (var chGuid in charGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(chGuid);
                var assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in assetsAtPath)
                {
                    if (!(asset is T item))
                        continue;
                    items.Add(item);
                }
            }

            return items;
        }

        /// <summary>
        ///     Searches for an asset with the provided coffee GUID
        ///     This search accounts for sub-nested assets.
        /// </summary>
        public static T FindAssetWithCoffeeGUID<T>(string coffeeGUID) where T : Object, HasCoffeeGUID
        {
            var searchStr = "t:" + typeof(T).Name;
            var charGuids = AssetDatabase.FindAssets(searchStr);
            foreach (var chGuid in charGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(chGuid);

                var assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in assetsAtPath)
                {
                    if (!(asset is T item))
                        continue;
                    if (item.GetCoffeeGUID() == coffeeGUID) return item;
                }
            }

            return null;
        }

        /// <summary>
        ///     Opens a save-file browser and returns the target save path or empty string on fail.
        /// </summary>
        public static string GetSavePath(string saveTitle, string saveName, string saveMessage)
        {
            string savePath;
            try
            {
                savePath = EditorUtility.SaveFilePanelInProject(
                    saveTitle,
                    saveName,
                    "asset",
                    saveMessage);
            }
            catch
            {
                return "";
            }

            return savePath;
        }

        public static T SaveOverwrite<T>(T original) where T : ScriptableObject, HasCoffeeGUID
        {
            var existing = FindAssetWithCoffeeGUID<T>(original.GetCoffeeGUID());
            string path = "";
            if (existing != null)
            {
                path = AssetDatabase.GetAssetPath(existing);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(existing));
                AssetDatabase.SaveAssets();
            }
            else
            {
                var prettyTypeName = ObjectNames.NicifyVariableName(typeof(T).Name);
                path = GetSavePath("Save " + prettyTypeName, original.name, "Saved!");
            }
            
            AssetDatabase.CreateAsset(original, path);
            AssetDatabase.SaveAssets();
            return original;
        }

        /// <summary>
        ///     Clones the original object, if the GUID already exists in an existing asset it will
        ///     create the as an asset with a new unique GUID, otherwise it will serialize as the
        ///     original's GUID.
        /// </summary>
        public static T ClonedSaveAs<T>(T original) where T : ScriptableObject, HasCoffeeGUID
        {
            var prettyTypeName = ObjectNames.NicifyVariableName(typeof(T).Name);
            var path = GetSavePath("Save " + prettyTypeName, original.name, "Saved!");
            if (path == "")
                return null;

            var clone = ScriptableObject.Instantiate(original);
            clone.name = original.name;
            //Enforces unique GUID.
            if (FindAssetWithCoffeeGUID<T>(original.GetCoffeeGUID()) != null)
                clone.SetCoffeeGUID(Guid.NewGuid().ToString());

            AssetDatabase.CreateAsset(clone, path);
            AssetDatabase.SaveAssets();
            return clone;
        }

        public static T ClonedSaveTo<T>(T original, Object saveTo) where T : ScriptableObject, HasCoffeeGUID
        {
            var clone = ScriptableObject.Instantiate(original);
            clone.name = original.name;
            //Enforces unique GUID.
            if (FindAssetWithCoffeeGUID<T>(original.GetCoffeeGUID()) != null)
                clone.SetCoffeeGUID(Guid.NewGuid().ToString());

            var assetPath = AssetDatabase.GetAssetPath(saveTo);
            if (assetPath == "")
            {
                Debug.LogError("Attempted to save: " + original.name + " to " + saveTo.name +
                               " but it does not exist.");
                return null;
            }

            AssetDatabase.AddObjectToAsset(clone, saveTo);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath);
            return clone;
        }

        public static T SaveTo<T>(T original, Object saveTo) where T : ScriptableObject, HasCoffeeGUID
        {
            var assetPath = AssetDatabase.GetAssetPath(saveTo);
            if (assetPath == "")
            {
                Debug.LogError("Attempted to save: " + original.name + " to " + saveTo.name +
                               " but it does not exist.");
                return null;
            }

            var savedAsset = FindAssetWithCoffeeGUID<T>(original.GetCoffeeGUID());
            if (savedAsset != null)
            {
                AssetDatabase.RemoveObjectFromAsset(savedAsset);
                AssetDatabase.AddObjectToAsset(original, saveTo);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(assetPath);
                return savedAsset;
            }

            AssetDatabase.RemoveObjectFromAsset(original);
            AssetDatabase.AddObjectToAsset(original, saveTo);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath);
            return null;
        }

        /// <summary>
        ///     Removes all sub-assets from an object.
        /// </summary>
        public static void CleanAllSubAssets(Object target)
        {
            var obs =
                AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(target));
            if (obs == null || obs.Length == 0)
                return;

            //Remove all old objects from the old character.
            foreach (var o in obs)
                if (o != target)
                    AssetDatabase.RemoveObjectFromAsset(o);
        }

        /// <summary>
        ///     Removes the target sub-asset, it will linger until the owning object is reimported.
        ///     (Use this only if the parent is unknown for some reason.)
        ///     See DeleteSubAssetFrom
        /// </summary>
        public static T DeleteSubAsset<T>(T asset) where T : Object, HasCoffeeGUID
        {
            var actualAsset = FindAssetWithCoffeeGUID<T>(asset.GetCoffeeGUID());
            if (actualAsset == null)
                return null;

            AssetDatabase.RemoveObjectFromAsset(actualAsset);
            AssetDatabase.SaveAssets();
            return actualAsset;
        }

        /// <summary>
        ///     Removes the target sub-asset from it's parent and reimports the parent cleaning the asset.
        /// </summary>
        public static T DeleteSubAssetFrom<T>(T asset, Object parent) where T : Object, HasCoffeeGUID
        {
            var deletedAsset = DeleteSubAsset(asset);
            var path = AssetDatabase.GetAssetPath(parent);

            if (path != null)
                AssetDatabase.ImportAsset(path);

            return deletedAsset;
        }
    }
}