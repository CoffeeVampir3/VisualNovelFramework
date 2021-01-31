using System.Collections.Generic;
using Sirenix.Utilities;
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

        public static void SerializeGraph(GraphView graphView)
        {
            serializedNodeData.Clear();
            WalkNodes(graphView);
            
            serializedGraph = FindGraphAsset();

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
            
            GraphSerializer.FlushRemovedAssets(serializedGraph);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(serializedGraph));
            EditorUtility.SetDirty(serializedGraph);
        }

        private static SerializedGraph FindGraphAsset()
        {
            var graph = AssetDatabase.LoadAssetAtPath<SerializedGraph>(GraphSerializer._DEBUG_SAVE_PATH);
            if (graph == null)
            {
                return GraphSerializer.CreateGraphDataAsset();
            }
            
            return graph;
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
                    AddOutputRuntimeLink(runtimeData, bn.runtimeData);
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
                    AddInputRuntimeLink(runtimeData, bn.runtimeData);
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

            //Write any serialized changes.
            node.serializedNode.ApplyModifiedPropertiesWithoutUndo();
            
            serializationData.nodeEditorData = node.editorData;
            serializationData.SetCoffeeGUID(node.editorData.GUID);
            
            serializationData.runtimeNode = node.runtimeData;
            serializationData.runtimeNode.SetCoffeeGUID(node.editorData.GUID);
            
            serializedNodeData.Add(serializationData);
            
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