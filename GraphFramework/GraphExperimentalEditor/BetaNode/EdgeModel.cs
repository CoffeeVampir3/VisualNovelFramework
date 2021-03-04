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
        public PortModel inputPortModel;
        [SerializeField] 
        public PortModel outputPortModel;

        public EdgeModel(NodeModel inputNode, PortModel inputPort, NodeModel outputNode, PortModel outputPort)
        {
            inputModel = inputNode;
            outputModel = outputNode;
            inputPortModel = inputPort;
            outputPortModel = outputPort;
        }
    }
}