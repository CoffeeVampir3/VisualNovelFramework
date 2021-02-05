using System;
using UnityEditor;
using UnityEngine;

namespace VisualNovelFramework.EditorExtensions
{
    public abstract class OpenWindowOnAssetClickedHandler<Item, Window> 
        where Item : ScriptableObject, HasCoffeeGUID
        where Window : EditorWindow
    {
        public static Window IsOpenedAssetTargetType(int instanceID, out Item openedItem, params Type[] desiredDockNextTo)
        {
            var targetItems = CoffeeAssetDatabase.FindAssetsOfType<Item>();

            openedItem = null;
            foreach (var targetItem in targetItems)
            {
                if (targetItem.GetInstanceID() == instanceID)
                {
                    openedItem = targetItem;
                    return EditorWindow.GetWindow<Window>(desiredDockNextTo);
                }
            }

            return null;
        }
    }
}