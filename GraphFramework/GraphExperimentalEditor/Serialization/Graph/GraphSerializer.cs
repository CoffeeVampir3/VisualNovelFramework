using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.Serialization;
using Object = UnityEngine.Object;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public static class GraphSerializer
    {
        private static readonly List<Object> savedObjects = new List<Object>();

        private static void OverwriteScriptableObject<T>(T objectBeingOverwritten, 
            T objectToCopyFrom) 
            where T : ScriptableObject
        {
            var l = JsonUtility.ToJson(objectToCopyFrom);
            JsonUtility.FromJsonOverwrite(l, objectBeingOverwritten);
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
                serializedNode.isRoot) 
                graph.rootNode = serializedNode.runtimeNode;

            serializedNode.name = "sNodeData_" + serializedNode.nodeEditorData.name;
            serializedNode.runtimeNode.name = "rtNodeData_" + serializedNode.nodeEditorData.name;

            var nodeGUID = serializedNode.GetCoffeeGUID();

            savedObjects.Add(SaveAssetToGraph(serializedNode,nodeGUID, graph));
            savedObjects.Add(SaveAssetToGraph(serializedNode.nodeEditorData, nodeGUID, graph));
            savedObjects.Add(SaveAssetToGraph(serializedNode.runtimeNode, nodeGUID, graph));
        }

        private static void CreateSerializedStackAsset(SerializedGraph graph, StackNodeSerializationData serializedStack)
        {
            var serializedItem = SaveAssetToGraph(serializedStack,
                serializedStack.GetCoffeeGUID(), graph);
            
            serializedItem.name = serializedItem.GetCoffeeGUID();
            savedObjects.Add(serializedItem);
        }

        public static void WriteSerializedNode(SerializedGraph graph, NodeSerializationData serializedNode)
        {
            CreateSerializedNodeAsset(graph, serializedNode);
        }
        
        public static void WriteSerializedStack(SerializedGraph graph, StackNodeSerializationData serializedStack)
        {
            CreateSerializedStackAsset(graph, serializedStack);

            foreach (var serialNode in serializedStack.stackedNodes)
            {
                CreateSerializedNodeAsset(graph, serialNode);
            }
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
                if (!savedObjects.Contains(o) && !(o is EditorGraphData))
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
        public static SerializedGraph FindOrCreateGraphAsset(string currentGraphGUID, System.Type windowType)
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
            
            var editorGraphAsset = ScriptableObject.CreateInstance<EditorGraphData>();
            editorGraphAsset.name = "GraphEditorData";
            editorGraphAsset.targetGraph = savedGraph;
            editorGraphAsset.editorWindowType = new SerializableType(windowType);
            editorGraphAsset.SetCoffeeGUID(currentGraphGUID);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(savedGraph));
            AssetDatabase.AddObjectToAsset(editorGraphAsset, savedGraph);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(editorGraphAsset));

            return savedGraph;
        }
    }
}