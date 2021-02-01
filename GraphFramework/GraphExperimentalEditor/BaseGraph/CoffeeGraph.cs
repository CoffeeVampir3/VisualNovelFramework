using System;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public abstract class CoffeeGraph : EditorWindow
    {
        protected CoffeeGraphView graphView;
        protected string currentGraphGUID;

        protected void InitializeGraph()
        {
            if (currentGraphGUID.IsNullOrWhitespace())
            {
                currentGraphGUID = Guid.NewGuid().ToString();
            }
            
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
            graphView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        protected abstract void OnGraphGUI();

        private void OnGeometryChanged(GeometryChangedEvent e)
        {
            OnGraphGUI();
            graphView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnDisable()
        {
            rootVisualElement.Clear();
        }
        
        private void SaveGraph()
        {
            GraphSaver.SerializeGraph(graphView);
        }

        private void LoadGraph()
        {
            GraphLoader.LoadGraph(graphView);
        }
    }
    
}
