using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public static class GraphSerializer
    {
                //TODO:: Rework temporary debug path.
        private const string _DEBUG_assetPath = @"Assets/!TestTrashbin/";
        
        private static List<Object> savedObjects = new List<Object>();

        public static string _DEBUG_SAVE_PATH => _DEBUG_assetPath + "debug" + ".asset";
        /// <summary>
        /// Serializes the nodes
        /// </summary>
        private static void CreateSerializedNodeAsset(SerializedGraph graph, NodeSerializationData serializedNode)
        {
            if (serializedNode.nodeEditorData.name.ToLower() == "root node") 
                graph.rootNode = serializedNode.runtimeNode;

            serializedNode.name = "S_" + serializedNode.nodeEditorData.name;
            serializedNode.runtimeNode.name = "R_" + serializedNode.nodeEditorData.name;

            if (AssetDatabase.GetAssetPath(serializedNode) == "")
            {
                AssetDatabase.AddObjectToAsset(serializedNode, graph);
            }
            if (AssetDatabase.GetAssetPath(serializedNode.nodeEditorData) == "")
            {
                AssetDatabase.AddObjectToAsset(serializedNode.nodeEditorData, graph);
            }
            if (AssetDatabase.GetAssetPath(serializedNode.runtimeNode) == "")
            {
                AssetDatabase.AddObjectToAsset(serializedNode.runtimeNode, graph);
            }
            
            savedObjects.Add(serializedNode);
            savedObjects.Add(serializedNode.nodeEditorData);
            savedObjects.Add(serializedNode.runtimeNode);
            EditorUtility.SetDirty(serializedNode);
            EditorUtility.SetDirty(serializedNode.nodeEditorData);
            EditorUtility.SetDirty(serializedNode.runtimeNode);
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