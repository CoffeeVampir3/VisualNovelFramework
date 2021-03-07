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
        [SerializeReference] 
        public PortModel inputPortModel;
        [SerializeReference] 
        public PortModel outputPortModel;
        [SerializeReference]
        public string inputConnectionGuid;
        [SerializeReference]
        public string outputConnectionGuid;

        public EdgeModel(NodeModel inputNode, PortModel inputPort, 
            NodeModel outputNode, PortModel outputPort,
            string inputConnectionGuid, string outputConnectionGuid)
        {
            this.inputModel = inputNode;
            this.outputModel = outputNode;
            this.inputPortModel = inputPort;
            this.outputPortModel = outputPort;
            this.inputConnectionGuid = inputConnectionGuid;
            this.outputConnectionGuid = outputConnectionGuid;
        }
    }
}