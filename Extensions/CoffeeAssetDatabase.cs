using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VisualNovelFramework.GenericInterfaces;

public static class CoffeeAssetDatabase
{
    /// <summary>
    /// Searches for all assets of type T and returns them as a list
    /// This search accounts for sub-nested assets.
    /// </summary>
    public static List<T> FindAssetsOfType<T>() where T : Object
    {
        string searchStr = "t:" + typeof(T).Name;
        var charGuids = AssetDatabase.FindAssets(searchStr);
        List<T> items = new List<T>();
        foreach(var chGuid in charGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(chGuid);
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
    /// Searches for an asset with the provided coffee GUID
    /// This search accounts for sub-nested assets.
    /// </summary>
    public static T FindAssetWithCoffeeGUID<T>(string coffeeGUID) where T : Object, HasCoffeeGUID
    {
        string searchStr = "t:" + typeof(T).Name;
        var charGuids = AssetDatabase.FindAssets(searchStr);
        foreach(var chGuid in charGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(chGuid);

            var assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in assetsAtPath)
            {
                if (!(asset is T item)) 
                    continue;
                if (item.GetCoffeeGUID() == coffeeGUID)
                {
                    return item;
                }
            }
        }
        return null;
    }
}
