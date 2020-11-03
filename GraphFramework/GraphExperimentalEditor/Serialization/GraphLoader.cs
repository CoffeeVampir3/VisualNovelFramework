using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    //Well doneya!
    public static class GraphLoader
    {
        private static readonly Dictionary<string, BaseNode> guidToNodeDict = new Dictionary<string, BaseNode>();
        private static readonly List<Edge> edges = new List<Edge>();
        
        public static void LoadGraph(GraphView graphView)
        {
            guidToNodeDict.Clear();
            edges.Clear();
            
            var items = AssetDatabase.LoadAllAssetsAtPath(GraphSerializer._DEBUG_SAVE_PATH);
            var serializedNodes = new List<NodeSerializationData>((items.Length/2)+1);
            
            ClearGraph(graphView);
            foreach(var obj in items)
            {
                if (!(obj is NodeSerializationData serialData)) 
                    continue;
                
                serializedNodes.Add(serialData);
                BaseNode node = LoadNode(serialData);
                if (node != null)
                {
                    graphView.AddElement(node);
                }
                else
                {
                    Debug.LogError("Attempted to load an invalid/incorrectly serialized graph node.");
                }
            }

            LoadEdges(serializedNodes);
            
            edges.ForEach(graphView.AddElement);
        }

        /// <summary>
        /// Clears the graph of any nodes or edges.
        /// </summary>
        private static void ClearGraph(GraphView graphView)
        {
            graphView.nodes.ForEach(graphView.RemoveElement);
            graphView.edges.ForEach(graphView.RemoveElement);
        }

        private static BaseNode LoadNode(NodeSerializationData srd)
        {
            Type nodeType = srd.nodeEditorData.nodeType.type;
            object dynamicNodeActivator;
            try
            {
                dynamicNodeActivator = Activator.CreateInstance(nodeType);
            }
            catch
            {
                Debug.Log("Failed to dynamically instantiated node: " + 
                          srd?.name + srd?.nodeEditorData?.GUID);
                return null;
            }

            if (dynamicNodeActivator == null || !(dynamicNodeActivator is BaseNode node)) 
                return null;
 
            node.Initialize(srd);
            guidToNodeDict.Add(node.editorData.GUID, node);
            return node;
        }

        private static void LoadEdge(SerializedEdgeData sEdge)
        {
            if (!guidToNodeDict.TryGetValue(sEdge.outputNodeGUID, out var outputNode) ||
                !guidToNodeDict.TryGetValue(sEdge.inputNodeGUID, out var inputNode))
            {
                Debug.LogError("Failed to load an edge because the node GUID did not match existing nodes.");
                Debug.Log("Input: " + sEdge.inputNodeGUID);
                Debug.Log("Output: " + sEdge.outputNodeGUID);
                return;
            }
            
            Port inputPort = inputNode.Query<Port>().AtIndex(sEdge.inputPortIndex);
            Port outputPort = outputNode.Query<Port>().AtIndex(sEdge.outputPortIndex);
            
            Edge nEdge = new Edge {output = outputPort, input = inputPort};
            nEdge.input.Connect(nEdge);
            nEdge.output.Connect(nEdge);
            edges.Add(nEdge);
        }

        //The essence of code.
        private static void LoadEdges(List<NodeSerializationData> serializedNodeData)
        {
            foreach (
                var sEdge in from node in serializedNodeData 
                from sPort in node.serializedPorts 
                from sEdge in sPort.serializedEdges 
                select sEdge)
            {
                LoadEdge(sEdge);
            }
        }
    }
}