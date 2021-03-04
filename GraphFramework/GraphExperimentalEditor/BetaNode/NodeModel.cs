using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeExtensions;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.GraphFramework.Serialization;
using VisualNovelFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.BetaNode
{
    [Serializable]
    public class NodeModel<RuntimeNodeType> : NodeModel where RuntimeNodeType : RuntimeNode
    {
        public override RuntimeNode RuntimeData
        {
            get => nodeRuntimeData;
            set => nodeRuntimeData = value as RuntimeNodeType;
        }
 
        [SerializeReference]
        protected RuntimeNodeType nodeRuntimeData;
    }
    
    [Serializable]
    public abstract class NodeModel
    {
        public abstract RuntimeNode RuntimeData { get; set; }
        
        public void OnCreate()
        {
            var thisType = GetType();

            //This magic deserve some explanation:
            //In a declaration like DialogueNode : NodeModel<RuntimeDialogueNode>
            //This code looks at NodeModel<RuntimeDialogueNode>
            //And extracts the type between the <>, RuntimeDialogueNode
            var genericArgs = thisType.GetGenericClassConstructorArguments(typeof(NodeModel<>));
            var finalArg = genericArgs.FirstOrDefault(w => typeof(RuntimeNode).IsAssignableFrom(w));
            RuntimeData = ScriptableObject.CreateInstance(finalArg) as RuntimeNode;
        }

        [NonSerialized] 
        private NodeView currentView;
        public NodeView CreateView()
        {
            currentView = new NodeView(this);
            currentView.Display();
            return currentView;
        }

        [SerializeField] 
        public bool isRoot = false;
        [SerializeField]
        public string GUID;
        
        public string NodeTitle
        {
            get => nodeTitle;
            set => nodeTitle = value;
        }

        public Rect Position
        {
            get => position;
            set => position = value;
        }

        public bool IsExpanded
        {
            get => isExpanded;
            set => isExpanded = value;
        }
        
        
        [SerializeField] 
        private string nodeTitle = "Untitled.";
        [SerializeField] 
        private Rect position = Rect.zero;
        [SerializeField] 
        private bool isExpanded = true;
    }
}