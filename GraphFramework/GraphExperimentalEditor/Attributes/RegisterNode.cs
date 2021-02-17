using System;
using UnityEngine;
using VisualNovelFramework.GraphFramework.Editor;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterNodeToView : Attribute
    {
        public Type registeredGraphView = null;
        public string registeredPath = null;
        public RegisterNodeToView(System.Type registerToGraphView, string nodePath)
        {
            if (!typeof(CoffeeGraphView).IsAssignableFrom(registerToGraphView))
            {
                Debug.LogError("Must register node to a coffee graph type!");
                return;
            }
            registeredGraphView = registerToGraphView;
            registeredPath = nodePath;
        }
        public RegisterNodeToView(System.Type registerToGraphView)
        {
            if (!typeof(CoffeeGraphView).IsAssignableFrom(registerToGraphView))
            {
                Debug.LogError("Must register node to a coffee graph type!");
                return;
            }
            registeredGraphView = registerToGraphView;
            registeredPath = "";
        }
    }
}