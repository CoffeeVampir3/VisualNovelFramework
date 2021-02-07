using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public static class GraphSaver
    {
        private static readonly List<NodeSerializationData> serializedNodeData = 
            new List<NodeSerializationData>();
        
        private static SerializedGraph serializedGraph;

        public static void SerializeGraph(GraphView graphView, string currentGraphGUID, System.Type windowType)
        {
            serializedNodeData.Clear();
            WalkNodes(graphView);
            
            serializedGraph = GraphSerializer.FindOrCreateGraphAsset(currentGraphGUID, windowType);
            if (serializedGraph == null)
                return;
            
            Debug.Log(AssetDatabase.GetAssetPath(serializedGraph));
            GraphSerializer.ClearSavedAssets();
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var nodeData in serializedNodeData)
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
                    AddOutputRuntimeLink(runtimeData, bn.RuntimeData);
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
                    AddInputRuntimeLink(runtimeData, bn.RuntimeData);
            }
            
            return serializedPort;
        }

        private static void AddOutputRuntimeLink(RuntimeNode addingTo, RuntimeNode connection)
        {
            addingTo.outputConnections.Add(connection);
        }
        
        private static void AddInputRuntimeLink(RuntimeNode addingTo, RuntimeNode connection)
        {
            addingTo.inputConnections.Add(connection);
        }

        private static void SerializeNode(BaseNode node)
        {
            var ports = node.Query<Port>().ToList();

            NodeSerializationData serializationData =
                ScriptableObject.CreateInstance<NodeSerializationData>();

            if (node.editorData == null || node.RuntimeData == null)
            {
                Debug.Log("Problem.");
            }

            serializationData.nodeEditorData = node.editorData;
            serializationData.SetCoffeeGUID(node.editorData.GUID);
            
            serializationData.runtimeNode = node.RuntimeData;
            serializationData.runtimeNode.SetCoffeeGUID(node.editorData.GUID);
            
            serializedNodeData.Add(serializationData);

            if (node is IRootNode)
            {
                serializationData.isRoot = true;
            }
            
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
        }

        private static List<Node> enumerationOfNodes;
        private static void WalkNodes(GraphView graphView)
        {
            enumerationOfNodes = graphView.nodes.ToList();
            foreach (var node in enumerationOfNodes)
            {
                SerializeNode(node as BaseNode);
            }
        }
        
    }
}