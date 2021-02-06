using System;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.GraphFramework.Serialization;
using Object = UnityEngine.Object;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public abstract class CoffeeGraph : EditorWindow
    {
        [SerializeReference]
        protected CoffeeGraphView graphView;
        [SerializeField]
        public string currentGraphGUID;

        protected void InitializeGraph()
        {
            if (currentGraphGUID.IsNullOrWhitespace())
            {
                currentGraphGUID = Guid.NewGuid().ToString();
            }
            
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
            graphView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            OnGeometryChanged(null);
        }

        protected abstract void OnGraphGUI();

        private void OnGeometryChanged(GeometryChangedEvent e)
        {
            GenerateToolbar();
            OnGraphGUI();
            graphView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnDisable()
        {
            rootVisualElement.Clear();
        }
        
        protected void SaveGraph()
        {
            GraphSaver.SerializeGraph(graphView, currentGraphGUID);
        }
        
        public void LoadGraph(SerializedGraph graph)
        {
            if (GraphLoader.LoadGraph(graphView, graph))
            {
                serializedGraphSelector.SetValueWithoutNotify(graph);
                currentGraphGUID = graph.GetCoffeeGUID();
            }
        }

        private void LoadGraphEvent(ChangeEvent<Object> evt)
        {
            var graph = evt.newValue as SerializedGraph;

            if (graph == null)
            {
                return;
            }
            LoadGraph(graph);
        }

        protected void RevertGraphToVersionOnDisk()
        {
            if (serializedGraphSelector == null)
                return;

            var currentGraph = serializedGraphSelector.value as SerializedGraph;
            if (currentGraph == null) 
                return;
            
            if (GraphLoader.LoadGraph(graphView, currentGraph))
            {
                currentGraphGUID = currentGraph.GetCoffeeGUID();
            }
        }

        /// <summary>
        /// TODO:: Test Code:
        /// </summary>
        protected void DuplicateGraph()
        {
            var currentGraph = serializedGraphSelector.value as SerializedGraph;
            if (currentGraph == null) 
                return;

            var p = AssetDatabase.GetAssetPath(currentGraph);
            var nP = p.Replace(".asset", "");
            nP += "2.asset";
            AssetDatabase.CopyAsset(p, nP);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(nP);

            var q= AssetDatabase.LoadAllAssetsAtPath(nP);

            foreach (var k in q)
            {
                if (k is HasCoffeeGUID cguid)
                {
                    cguid.SetCoffeeGUID(Guid.NewGuid().ToString());
                }
            }
        }

        private ObjectField serializedGraphSelector = null;
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            
            serializedGraphSelector = new ObjectField {objectType = typeof(SerializedGraph)};
            serializedGraphSelector.RegisterValueChangedCallback(LoadGraphEvent);

            toolbar.Add(new Button( SaveGraph ) {text = "Save"});
            toolbar.Add(new Button( DuplicateGraph ) {text = "Duplicate Test"});
            toolbar.Add(new Button( RevertGraphToVersionOnDisk ) {text = "Revert To Saved Version"});

            toolbar.Add(serializedGraphSelector);
            rootVisualElement.Add(toolbar);
        }
    }
    
}
