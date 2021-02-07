﻿using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.GraphFramework.Serialization;
using VisualNovelFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
    /// <summary>
    /// This class allows us to easily define the runtime data type but still activate it internally
    /// using reflection. I love this so much thanks @MentallyStable on GDL for this one!
    /// </summary>
    public abstract class BaseNode<RuntimeNodeType> : BaseNode where RuntimeNodeType : RuntimeNode
    {
        public override RuntimeNode RuntimeData
        {
            get => nodeRuntimeData;
            set => nodeRuntimeData = value as RuntimeNodeType;
        }

        [SerializeReference]
        protected RuntimeNodeType nodeRuntimeData;
    }
    
    /// <summary>
    /// BaseNode is responsible for the very most basic setup and initialization of any deriving nodes.
    /// This allows us to have a common class we can generate using reflection and an activator,
    /// allowing us to avoid boilerplate code.
    /// </summary>
    public abstract partial class BaseNode : Node
    {
        [SerializeReference]
        public NodeEditorData editorData;
        public abstract RuntimeNode RuntimeData { get; set; }

        #region Node Data Handling
        
        /// <summary>
        /// Deserialization initialization
        /// </summary>
        public void Initialize(NodeSerializationData data)
        {
            LoadNodeData(data);
            OnNodeCreation();
            SetupBaseNodeUI();
            RebuildPortsFromSerialization(data);
        }
        
        /// <summary>
        /// Initial (new node) creation
        /// </summary>
        public void Initialize(string initialName)
        {
            GenerateNewNodeData(initialName);
            OnNodeCreation();
            SetupBaseNodeUI();
            InstantiatePorts();
        }

        /// <summary>
        /// This initialization is performed when the node is spawned, before any UI elements
        /// are generated.
        /// </summary>
        protected abstract void OnNodeCreation();

        /// <summary>
        /// This initialization is called after all UI is generated, allowing you to generate ports
        /// on top of the UI.
        /// </summary>
        protected abstract void InstantiatePorts();

        private void LoadNodeData(NodeSerializationData serializationData)
        {
            RuntimeData = serializationData.runtimeNode;
            RuntimeData.name = serializationData.runtimeNode.name;

            editorData = serializationData.nodeEditorData;
            editorData.name = serializationData.nodeEditorData.name;
            title = editorData.name;
        }

        private System.Type GetGenericRuntimeNodeType()
        {
            var thisType = GetType();

            //This magic deserve some explanation:
            //In a declaration like DialogueNode : BaseNode<RuntimeDialogueNode>
            //This code looks at BaseNode<RuntimeDialogueNode>
            //And extracts the type RuntimeDialogueNode
            var k = thisType.GetGenericClassConstructorArguments(typeof(BaseNode<>));
            return k.FirstOrDefault(w => typeof(RuntimeNode).IsAssignableFrom(w));
        }

        private void GenerateNewNodeData(string initialName)
        {
            editorData = ScriptableObject.CreateInstance<NodeEditorData>();

            var q = GetGenericRuntimeNodeType();
            RuntimeData = ScriptableObject.CreateInstance(q) as RuntimeNode;

            editorData.GUID = Guid.NewGuid().ToString();
            editorData.name = initialName;
            
            Debug.Assert(RuntimeData != null, nameof(RuntimeData) + " != null");
            RuntimeData.SetCoffeeGUID(editorData.GUID);
            editorData.nodeType = new SerializableType(GetType());
            title = editorData.name;
        }

        #endregion
    }
}
