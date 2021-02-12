using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelFramework.GraphFramework.Editor;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public static class GraphSaver
    {
        private static readonly List<NodeSerializationData> serializedUnstackedNodeData = 
            new List<NodeSerializationData>();
        
        private static readonly List<StackNodeSerializationData> serializedStackedNodeData = 
            new List<StackNodeSerializationData>();

        private static SerializedGraph serializedGraph;

        /// <summary>
        /// Serializes the target CoffeeGraphView to disk as ScriptableObjects.
        /// </summary>
        /// <param name="graphView">The target graph view.</param>
        /// <param name="currentGraphGUID">The GUID you want to serialize the graph as.</param>
        /// <param name="windowType">Editor window type.</param>
        public static void SerializeGraph(CoffeeGraphView graphView, string currentGraphGUID, System.Type windowType)
        {
            serializedUnstackedNodeData.Clear();
            serializedStackedNodeData.Clear();
            WalkNodes(graphView);

            serializedGraph = GraphSerializer.FindOrCreateGraphAsset(currentGraphGUID, windowType);
            if (serializedGraph == null)
                return;
            
            Debug.Log(AssetDatabase.GetAssetPath(serializedGraph));
            GraphSerializer.ClearSavedAssets();
            try
            {
                AssetDatabase.StartAssetEditing();
                
                foreach (var stackedNodeData in serializedStackedNodeData)
                {
                    GraphSerializer.WriteSerializedStack(serializedGraph, stackedNodeData);
                }
                
                foreach (var nodeData in serializedUnstackedNodeData)
                {
                    GraphSerializer.WriteSerializedNode(serializedGraph, nodeData);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            
            GraphSerializer.CleanDeletedItemsFromGraphAsset(serializedGraph);
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(serializedGraph);
        }

        private static NodeSerializationData SerializeNode(BaseNode node)
        {
            NodeSerializationData serializationData =
                NodeSerializationData.SerializeFrom(node);
            
            return serializationData;
        }
        
        /// <summary>
        /// Serializes an entire stack, stack nodes are serialized together with
        /// anything stacked in them. This makes for easy deserialization later.
        /// </summary>
        private static StackNodeSerializationData SerializeStack(BaseStackNode stack)
        {
            var stackSerialData = StackNodeSerializationData.SerializeFrom(stack);
            
            return stackSerialData;
        }
        
        /// <summary>
        /// Walks all nodes in the graph, including stack nodes.
        /// </summary>
        private static void WalkNodes(GraphView graphView)
        {
            var enumerationOfNodes = graphView.nodes.ToList();
            
            //Iterating backwards no longer necessary, but still very cool.
            for (int i = enumerationOfNodes.Count-1; i >= 0; i--)
            {
                var node = enumerationOfNodes[i];
                //Stack nodes serialize all their children themselves.
                if (node is BaseStackNode sn)
                {
                    serializedStackedNodeData.Add(SerializeStack(sn));
                }

                //This serializes any free hanging children that aren't stacked.
                if (node is BaseNode bn && !(bn.ClassListContains("stack-child-element")))
                {
                    var serializationData = SerializeNode(bn);
                    if (serializationData == null)
                        continue;
                    serializedUnstackedNodeData.Add(serializationData);
                }
            }
        }
        
    }
}