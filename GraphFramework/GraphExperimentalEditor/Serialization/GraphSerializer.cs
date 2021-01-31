using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public static class GraphSerializer
    {
                //TODO:: Rework temporary debug path.
        private const string _DEBUG_assetPath = @"Assets/!TestTrashbin/";
        
        private static List<Object> savedObjects = new List<Object>();

        private static void OverwriteScriptableObject<T>(T objectBeingOverwritten, T objectToCopyFrom) where T : ScriptableObject
        {
            SerializedObject copyingAsset = new SerializedObject(objectToCopyFrom);
            SerializedObject savedAsset = new SerializedObject(objectBeingOverwritten);
            
            var it = copyingAsset.GetIterator();
            if (!it.NextVisible(true)) 
                return;
            //Descends through serialized property children & allows us to edit them.
            do
            {
                if (it.propertyPath == "m_Script" && savedAsset.targetObject != null)
                {
                    continue;
                }

                if (it.propertyPath == "nodeEditorData" || it.propertyPath == "runtimeNode")
                {
                    continue;
                }

                savedAsset.CopyFromSerializedProperty(it);
            }
            while (it.NextVisible(false));

            savedAsset.ApplyModifiedProperties();
            EditorUtility.SetDirty(objectBeingOverwritten);
        }

        private static T SaveAssetToGraph<T>(T item, string GUID, SerializedGraph graph) where T : ScriptableObject, HasCoffeeGUID
        {
            T savedToDiskAsset = CoffeeAssetDatabase.FindAssetWithCoffeeGUID<T>(GUID);
            if (savedToDiskAsset == null)
            {
                savedToDiskAsset = item;
                AssetDatabase.AddObjectToAsset(item, graph);
            }
            else
            {
                OverwriteScriptableObject(savedToDiskAsset, item);
            }

            return savedToDiskAsset;
        }

        public static string _DEBUG_SAVE_PATH => _DEBUG_assetPath + "debug" + ".asset";
        /// <summary>
        /// Serializes the nodes
        /// </summary>
        private static void CreateSerializedNodeAsset(SerializedGraph graph, NodeSerializationData serializedNode)
        {
            if (graph.rootNode == null && 
                serializedNode.nodeEditorData.name.ToLower() == "root node") 
                graph.rootNode = serializedNode.runtimeNode;

            serializedNode.name = "S_" + serializedNode.nodeEditorData.name;
            serializedNode.runtimeNode.name = "R_" + serializedNode.nodeEditorData.name;

            var nodeGUID = serializedNode.GetCoffeeGUID();

            savedObjects.Add(SaveAssetToGraph(serializedNode, nodeGUID, graph));
            savedObjects.Add(SaveAssetToGraph(serializedNode.nodeEditorData, nodeGUID, graph));
            savedObjects.Add(SaveAssetToGraph(serializedNode.runtimeNode, nodeGUID, graph));
        }

        public static void WriteSerializedNode(SerializedGraph graph, NodeSerializationData serializedNode)
        {
            CreateSerializedNodeAsset(graph, serializedNode);
        }

        /// <summary>
        /// Removes all previously added assets do we don't get write errors.
        /// </summary>
        public static void FlushRemovedAssets(SerializedGraph graph)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(_DEBUG_SAVE_PATH);
            savedObjects.Add(graph);
            foreach (var o in assets)
            {
                if (!savedObjects.Contains(o))
                {
                    AssetDatabase.RemoveObjectFromAsset(o);
                }
            }
        }

        public static void ClearSavedAssets()
        {
            savedObjects.Clear();
        }

        public static SerializedGraph CreateGraphDataAsset()
        {
            var graphData = ScriptableObject.CreateInstance<SerializedGraph>();
            graphData.name = "testGraphData";
            AssetDatabase.CreateAsset(graphData, _DEBUG_SAVE_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(graphData));
            AssetDatabase.Refresh();

            return graphData;
        }
    }
}