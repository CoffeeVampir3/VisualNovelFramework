using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public static class SafeActivatorHelper
    {
        /// <summary>
        /// A wrapper around dynamic activator so it's "safer"
        /// </summary>
        public static T LoadArbitrary<T>(System.Type arbitraryType)
            where T : Node
        {
            object dynamicNodeActivator;
            try
            {
                dynamicNodeActivator = Activator.CreateInstance(arbitraryType);
            }
            catch
            {
                Debug.Log("Failed to dynamically instantiated graph element of type: " + 
                          arbitraryType);
                return null;
            }

            if (!(dynamicNodeActivator is T node)) 
                return null;

            return node;
        }
    }
}