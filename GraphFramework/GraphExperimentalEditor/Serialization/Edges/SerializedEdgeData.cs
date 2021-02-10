using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    [Serializable]
    public struct SerializedEdgeData
    {
        [SerializeField, HideInInspector] 
        public string outputNodeGUID;
        [SerializeField, HideInInspector] 
        public int outputPortIndex;
        [SerializeField, HideInInspector] 
        public string inputNodeGUID;
        [SerializeField, HideInInspector] 
        public int inputPortIndex;

        public SerializedEdgeData(Edge edge)
        {
            var inputNode = edge.input.node as BaseNode;
            var inputPort = edge.input;
            var outputNode = edge.output.node as BaseNode;
            var outputPort = edge.output;
            if (inputNode != null && outputNode != null)
            {
                outputNodeGUID = outputNode.GetCoffeeGUID();
                outputPortIndex = outputNode.Query<Port>().ToList().IndexOf(outputPort);
                inputNodeGUID = inputNode.GetCoffeeGUID();
                inputPortIndex = inputNode.Query<Port>().ToList().IndexOf(inputPort);
            }
            else
            {
                inputPortIndex = -1;
                inputNodeGUID = "";
                outputPortIndex = -1;
                outputNodeGUID = "";
                Debug.LogError("Bug with serializing edge, edge did not have input parent.");
            }
        }
    }
}