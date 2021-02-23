using System.Collections.Generic;
using UnityEditor;
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

        private static readonly Dictionary<NodeSerializationData, NodeSerializationData> 
            stackNodeRefTracker = new Dictionary<NodeSerializationData, NodeSerializationData>();
        
        private static void OverwriteScriptableObject<T>(T objectBeingOverwritten, 
            T objectToCopyFrom) 
            where T : ScriptableObject
        {
            var l = JsonUtility.ToJson(objectToCopyFrom);
            JsonUtility.FromJsonOverwrite(l, objectBeingOverwritten);
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

            EditorUtility.SetDirty(savedToDiskAsset);
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
            
            var nodeGUID = serializedNode.GetCoffeeGUID();

            var savedSn = SaveAssetToGraph(serializedNode, nodeGUID, graph);
            savedObjects.Add(savedSn);
            savedObjects.Add(SaveAssetToGraph(serializedNode.runtimeNode, nodeGUID, graph));
            stackNodeRefTracker.Add(serializedNode, savedSn);
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

        //Matches the references for the serialized stack to the nodes on disk,
        //which are potentially different objects.
        private static void UpdateStackRefs(StackNodeSerializationData serializedStack)
        {
            for (int i = 0; i < serializedStack.stackedNodes.Count; i++)
            {
                var node = serializedStack.stackedNodes[i];
                if (stackNodeRefTracker.TryGetValue(node, out var savedNode))
                {
                    serializedStack.stackedNodes[i] = savedNode;
                }
            }
        }
        
        public static void WriteSerializedStack(SerializedGraph graph, StackNodeSerializationData serializedStack)
        {
            foreach (var serialNode in serializedStack.stackedNodes)
            {
                CreateSerializedNodeAsset(graph, serialNode);
            }

            UpdateStackRefs(serializedStack);
            CreateSerializedStackAsset(graph, serializedStack);
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
            stackNodeRefTracker.Clear();
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