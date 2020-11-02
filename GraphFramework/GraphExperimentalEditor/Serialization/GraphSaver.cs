using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor;
using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public static class GraphSaver
    {
        private static readonly Dictionary<Port, List<Edge>> outputPortToEdges = 
            new Dictionary<Port, List<Edge>>();
        private static readonly List<NodeSerializationData> serializedNodeData = 
            new List<NodeSerializationData>();
        
        private static SerializedGraph serializedGraph;

        public static void SerializeGraph(BaseGraphView graphView)
        {
            outputPortToEdges.Clear();
            serializedNodeData.Clear();
            EnumerateEdges(graphView);
            WalkNodes(graphView);

            serializedGraph = FindGraphAsset();

            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var nodeData in serializedNodeData)
                {
                    serializedGraph.WriteSerializedNode(nodeData);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private static SerializedGraph FindGraphAsset()
        {
            var graph = AssetDatabase.LoadAssetAtPath<SerializedGraph>(SerializedGraph._DEBUG_SAVE_PATH);
            if (graph == null)
            {
                return serializedGraph = SerializedGraph.CreateGraphDataAsset();
            }
            
            graph.FlushSavedAssets();
            return graph;
        }

        /// <summary>
        /// Output ports hold the edge information, input ports do not.
        /// </summary>
        private static SerializedPortData SerializeOutputPort(Port port)
        {
            SerializedPortData serializedPort = new SerializedPortData(port);
            //If the output port has edges.
            if (outputPortToEdges.TryGetValue(port, out var gottenEdges))
            {
                //Enumerate the edges and serialize them to the output port edge list.
                foreach (var edge in gottenEdges)
                {
                    serializedPort.serializedEdges.Add(new SerializedEdgeData(edge));
                }
            }

            return serializedPort;
        }

        private static SerializedPortData SerializeInputPort(Port port)
        {
            SerializedPortData serializedPort = new SerializedPortData(port);
            return serializedPort;
        }

        private static void SerializeNode(BaseNode node)
        {
            var ports = node.Query<Port>().ToList();

            NodeSerializationData serializationData =
                ScriptableObject.CreateInstance<NodeSerializationData>();
            
            serializationData.nodeEditorData = node.editorData;
            serializedNodeData.Add(serializationData);
            
            foreach (var port in ports)
            {
                switch (port.direction)
                {
                    case Direction.Output:
                        serializationData.serializedPorts.
                            Add(SerializeOutputPort(port));
                        continue;
                    case Direction.Input:
                        serializationData.serializedPorts.
                            Add(SerializeInputPort(port));
                        continue;
                }
            }
        }

        /// <summary>
        /// Walks each edge and creates a dictionary of Output Port -> List of Edges
        /// </summary>
        private static void EnumerateEdges(BaseGraphView graphView)
        {
            var edges = graphView.edges.ToList();
            foreach (Edge edge in edges)
            {
                if (!outputPortToEdges.TryGetValue(edge.output, out var listOfEdges))
                {
                    listOfEdges = new List<Edge> {edge};
                    outputPortToEdges.Add(edge.output, listOfEdges);
                }
                listOfEdges.Add(edge);
            }
        }

        private static void WalkNodes(BaseGraphView graphView)
        {
            var enumerationOfNodes = graphView.nodes.ToList();
            foreach (var node in enumerationOfNodes)
            {
                SerializeNode(node as BaseNode);
            }
        }
        
    }
}