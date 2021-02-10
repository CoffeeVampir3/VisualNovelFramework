using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
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

        private static readonly List<BaseNode> stackedNodes = new List<BaseNode>();
        
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
            stackedNodes.Clear();
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

        /// <summary>
        /// Output ports hold the edge information, input ports do not.
        /// </summary>
        private static SerializedPortData SerializeOutputPort(Port port, RuntimeNode runtimeData)
        {
            SerializedPortData serializedPort = new SerializedPortData(port);
            foreach (var edge in port.connections)
            {
                if(edge.input.node is BaseNode bn)
                    AddOutputLink(runtimeData, bn.RuntimeData);
                serializedPort.serializedEdges.Add(new SerializedEdgeData(edge));
            }

            return serializedPort;
        }

        private static SerializedPortData SerializeInputPort(Port port, RuntimeNode runtimeData)
        {
            SerializedPortData serializedPort = new SerializedPortData(port);
            foreach (var edge in port.connections)
            {
                if(edge.output.node is BaseNode bn)
                    AddInputLink(runtimeData, bn.RuntimeData);
            }
            
            return serializedPort;
        }

        /// <summary>
        /// This is the actual connection information that we're serializing, not just
        /// the edge data. So the node actually connects to another runtime node.
        /// </summary>
        private static void AddOutputLink(RuntimeNode addingTo, RuntimeNode connection)
        {
            addingTo.outputConnections.Add(connection);
        }
        
        /// <summary>
        /// This is the actual connection information that we're serializing, not just
        /// the edge data. So the node actually connects to another runtime node.
        /// </summary>
        private static void AddInputLink(RuntimeNode addingTo, RuntimeNode connection)
        {
            addingTo.inputConnections.Add(connection);
        }

        private static NodeSerializationData SerializeNode(BaseNode node)
        {
            if (stackedNodes.Contains(node))
                return null;
            
            var ports = node.Query<Port>().ToList();

            NodeSerializationData serializationData =
                NodeSerializationData.SerializeFrom(node);
            
            serializationData.runtimeNode.outputConnections.Clear();
            serializationData.runtimeNode.inputConnections.Clear();

            foreach(var port in ports)
            {
                switch (port.direction)
                {
                    case Direction.Output:
                        serializationData.serializedPorts.
                            Add(SerializeOutputPort(port, serializationData.runtimeNode));
                        continue;
                    case Direction.Input:
                        serializationData.serializedPorts.
                            Add(SerializeInputPort(port, serializationData.runtimeNode));
                        continue;
                }
            }

            return serializationData;
        }
        
        /// <summary>
        /// Serializes an entire stack, stack nodes are serialized together with
        /// anything stacked in them. This makes for easy deserialization later.
        /// </summary>
        private static StackNodeSerializationData SerializeStack(BaseStackNode stack)
        {
            var nodeList = stack.Query<BaseNode>().ToList();

            var stackSerialData = StackNodeSerializationData.SerializeFrom(stack);

            //Serializes each stacked node.
            foreach (var stackedNode in nodeList)
            {
                var serialData = SerializeNode(stackedNode);
                stackSerialData.stackedNodes.Add(serialData);
                stackedNodes.Add(stackedNode);
            }
            
            return stackSerialData;
        }
        
        /// <summary>
        /// Walks all nodes in the graph, including stack nodes.
        /// </summary>
        private static void WalkNodes(GraphView graphView)
        {
            var enumerationOfNodes = graphView.nodes.ToList();
            
            //Iterate backwards through the nodes for all stack nodes.
            //This will serialize them and any nodes stacked under them.
            //This step happens first because the stack node is "responsible"
            //for serializing its children, but the node will be serialized twice
            //if we aren't careful.
            for (int i = enumerationOfNodes.Count-1; i >= 0; i--)
            {
                var node = enumerationOfNodes[i];
                if (!(node is BaseStackNode sn)) 
                    continue;
                
                serializedStackedNodeData.Add(SerializeStack(sn));
                enumerationOfNodes.RemoveAt(i);
            }

            //Serialize all free-hanging nodes left.
            foreach (var node in enumerationOfNodes)
            {
                //This guard clause is "technically" not possible but /shrug.
                if (!(node is BaseNode bn)) 
                    continue;
                
                var serializationData = SerializeNode(bn);
                if (serializationData == null)
                    continue;
                serializedUnstackedNodeData.Add(serializationData);
            }

        }
        
    }
}