using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    //Well doneya!
    public static class GraphLoader
    {
        private static readonly Dictionary<string, BaseNode> guidToNodeDict = new Dictionary<string, BaseNode>();
        private static readonly List<Edge> edges = new List<Edge>();
        
        /// <summary>
        /// Loads a serialized graph (As a collection of SO) from disk.
        /// </summary>
        /// <param name="graphView">Target graph view to load onto</param>
        /// <param name="graphToLoad">Target serialized graph to load</param>
        /// <returns>True if successful.</returns>
        public static bool LoadGraph(CoffeeGraphView graphView, SerializedGraph graphToLoad)
        {
            guidToNodeDict.Clear();
            edges.Clear();

            UnityEngine.Object[] items;
            try
            {
                items =
                    AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(graphToLoad));
            }
            catch
            {
                Debug.LogError("Critical error loading graph asset! Possibly malformed GUID or the asset DB is corrupt.");
                return false;
            }

            var serializedNodes = new List<NodeSerializationData>((items.Length/2)+1);
            
            ClearGraph(graphView);
            
            //First iterate the stack nodes, we iterate twice, once for stack nodes
            //then we iterate the collection again and pick up anything the stack
            //nodes weren't responsible for. Iterating the collection only once
            //would require a different design, this seems like a fine compromise.
            foreach(var obj in items)
            {
                //There's no particular reason for this... But it's kinda funny so I'll keep it around,
                //Z
                switch (obj)
                {
                    case StackNodeSerializationData stackData:
                        LoadSerializedStack(stackData, ref serializedNodes, graphView);
                        break;
                }
            }
            
            foreach(var obj in items)
            {
                switch (obj)
                {
                    case NodeSerializationData serialData:
                    {
                        var node = LoadSerializedNode(serialData, ref serializedNodes, graphView);
                        if(node != null)
                            graphView.AddNode(node);
                        break;
                    }
                }
            }

            LoadEdges(serializedNodes);
            
            edges.ForEach(graphView.AddElement);
            return true;
        }

        private static BaseNode LoadSerializedNode(
            NodeSerializationData serialData, 
            ref List<NodeSerializationData> serializedNodes,
            CoffeeGraphView graphView)
        {
            if (guidToNodeDict.TryGetValue(serialData.GetCoffeeGUID() ?? string.Empty, out _))
                return null;
            
            serializedNodes.Add(serialData);
            BaseNode node = LoadNode(serialData);
            if (node == null)
            {
                Debug.LogError("Attempted to load an invalid/incorrectly serialized graph node.");
                return null;
            }

            if(node is IRootNode root)
            {
                graphView.rootNode = root as BaseNode;
            }

            return node;
        }
        
        private static void LoadSerializedStack(
            StackNodeSerializationData serialData, ref List<NodeSerializationData> serializedNodes,
            CoffeeGraphView graphView)
        {
            BaseStackNode stackNode = LoadStackNode(serialData);
            if (stackNode != null)
            {
                graphView.AddStackNode(stackNode);
            }
            else
            {
                Debug.LogError("Attempted to load an invalid/incorrectly serialized graph stack-node.");
                return;
            }
            
            foreach (var serialNode in serialData.stackedNodes)
            {
                var loadedNode = LoadSerializedNode(serialNode, ref serializedNodes, graphView);
                graphView.AddDefaultSettingsToNode(loadedNode);
                stackNode.AddElement(loadedNode);
            }
        }

        /// <summary>
        /// Clears the graph of any nodes or edges.
        /// </summary>
        private static void ClearGraph(GraphView graphView)
        {
            graphView.nodes.ForEach(graphView.RemoveElement);
            graphView.edges.ForEach(graphView.RemoveElement);
        }

        //"Safe" dynamic activator to instantiate our nodes.
        private static T LoadArbitrary<T>(System.Type arbitraryType)
            where T : Node
        {
            object dynamicNodeActivator;
            try
            {
                dynamicNodeActivator = Activator.CreateInstance(arbitraryType);
            }
            catch
            {
                Debug.Log("Failed to dynamically instantiated node of type: " + 
                          arbitraryType);
                return null;
            }

            if (!(dynamicNodeActivator is T node)) 
                return null;

            return node;
        }

        private static BaseNode LoadNode(NodeSerializationData srd)
        {
            Type nodeType = srd.nodeType.type;
            var node = LoadArbitrary<BaseNode>(nodeType);

            srd.SerializeTo(ref node);
            node.Initialize(srd);
            guidToNodeDict.Add(node.GetCoffeeGUID(), node);
            return node;
        }
        
        private static BaseStackNode LoadStackNode(StackNodeSerializationData srd)
        {
            var node = LoadArbitrary<BaseStackNode>(srd.nodeType.type);
            
            srd.SerializeTo(ref node);
            return node;
        }

        private static readonly Dictionary<string, UQueryBuilder<Port>> inPortDict =
            new Dictionary<string, UQueryBuilder<Port>>();
        private static readonly Dictionary<string, UQueryBuilder<Port>> outPortDict =
            new Dictionary<string, UQueryBuilder<Port>>();
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
            
            //Port queries are cached so they aren't run a bunch for no reason.
            if(!inPortDict.TryGetValue(sEdge.inputNodeGUID, out var inputPortsQ))
            {
                inputPortsQ = inputNode.Query<Port>();
                inPortDict.Add(sEdge.inputNodeGUID, inputPortsQ);
            }
            if(!outPortDict.TryGetValue(sEdge.outputNodeGUID, out var outputPortsQ))
            {
                outputPortsQ = outputNode.Query<Port>();
                outPortDict.Add(sEdge.outputNodeGUID, outputPortsQ);
            }
            var inputPort = inputPortsQ.AtIndex(sEdge.inputPortIndex);
            var outputPort = outputPortsQ.AtIndex(sEdge.outputPortIndex);
            
            Edge nEdge = new Edge {output = outputPort, input = inputPort};
            nEdge.input.Connect(nEdge);
            nEdge.output.Connect(nEdge);
            edges.Add(nEdge);
        }

        //The essence of code.
        private static void LoadEdges(List<NodeSerializationData> serializedNodeData)
        {
            inPortDict.Clear();
            outPortDict.Clear();
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