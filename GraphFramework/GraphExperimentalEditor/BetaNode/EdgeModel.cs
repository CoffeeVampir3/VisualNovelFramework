using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.BetaNode
{
    [Serializable]
    public class EdgeModel
    {
        [SerializeReference] 
        public NodeModel inputModel;
        [SerializeReference] 
        public NodeModel outputModel;
        [SerializeField] 
        public int outputPortIndex;
        [SerializeField] 
        public int inputPortIndex;

        public EdgeModel(Edge edge, NodeModel input, NodeModel output)
        {
            var inputNode = edge.input.node;
            var inputPort = edge.input;
            var outputNode = edge.output.node;
            var outputPort = edge.output;
            outputPortIndex = outputNode.Query<Port>().ToList().IndexOf(outputPort);
            inputPortIndex = inputNode.Query<Port>().ToList().IndexOf(inputPort);

            inputModel = input;
            outputModel = output;
        }
    }
}