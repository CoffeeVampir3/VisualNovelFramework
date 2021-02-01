using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using Object = UnityEngine.Object;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public static class GraphSerializer
    {
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

        /// <summary>
        /// Adds the item to the graph object.
        /// </summary>
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
        
        /// <summary>
        /// Serializes the nodes
        /// </summary>
        private static void CreateSerializedNodeAsset(SerializedGraph graph, NodeSerializationData serializedNode)
        {
            if (graph.rootNode == null && 
                serializedNode.nodeEditorData.name.ToLower() == "root node") 
                graph.rootNode = serializedNode.runtimeNode;

            serializedNode.name = "sNodeData_" + serializedNode.nodeEditorData.name;
            serializedNode.runtimeNode.name = "rtNodeData_" + serializedNode.nodeEditorData.name;

            var nodeGUID = serializedNode.GetCoffeeGUID();

            savedObjects.Add(SaveAssetToGraph(serializedNode,nodeGUID, graph));
            savedObjects.Add(SaveAssetToGraph(serializedNode.nodeEditorData, nodeGUID, graph));
            savedObjects.Add(SaveAssetToGraph(serializedNode.runtimeNode, nodeGUID, graph));
        }

        public static void WriteSerializedNode(SerializedGraph graph, NodeSerializationData serializedNode)
        {
            CreateSerializedNodeAsset(graph, serializedNode);
        }

        /// <summary>
        /// Cleans any deleted items from our graph asset so there's no garbage files piling up.
        /// </summary>
        public static void CleanDeletedItemsFromGraphAsset(SerializedGraph graph)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(graph));
            savedObjects.Add(graph);
            foreach (var o in assets)
            {
                if (!savedObjects.Contains(o))
                {
                    AssetDatabase.RemoveObjectFromAsset(o);
                }
            }
        }

        /// <summary>
        /// Cleans the saved object cache, call this before writing starts or the cleanup code
        /// might not clean correctly.
        /// </summary>
        public static void ClearSavedAssets()
        {
            savedObjects.Clear();
        }

        /// <summary>
        /// Finds a graph based on the provided GUID or creates one if none exist.
        /// </summary>
        public static SerializedGraph FindOrCreateGraphAsset(string currentGraphGUID)
        {
            var existingGraph =
                CoffeeAssetDatabase.FindAssetWithCoffeeGUID<SerializedGraph>(currentGraphGUID);
            if (existingGraph != null)
            {
                return existingGraph;
            }

            var newGraph = ScriptableObject.CreateInstance<SerializedGraph>();
            newGraph.SetCoffeeGUID(currentGraphGUID);
            newGraph.name = "testingGraph";
            var savedGraph = CoffeeAssetDatabase.SaveAs(newGraph);
            if (savedGraph == null)
            {
                Debug.Log("Unable to save graph!");
                return null;
            }
            
            return savedGraph;
        }
    }
}