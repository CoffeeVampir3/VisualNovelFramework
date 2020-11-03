using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelFramework.Editor.Serialization;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.GraphFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
    public abstract partial class BaseNode : Node
    {
        public NodeEditorData editorData;
        public NodeRuntimeData runtimeData;

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

        protected abstract void OnNodeCreation();

        protected abstract void InstantiatePorts();

        private void LoadNodeData(NodeSerializationData serializationData)
        {
            runtimeData = ScriptableObject.Instantiate(serializationData.nodeRuntimeData);
            runtimeData.name = serializationData.nodeRuntimeData.name;

            editorData = ScriptableObject.Instantiate(serializationData.nodeEditorData);
            editorData.name = serializationData.nodeEditorData.name;
            title = editorData.name;
        }
        
        private void GenerateNewNodeData(string initialName)
        {
            editorData = ScriptableObject.CreateInstance<NodeEditorData>();
            editorData.GUID = Guid.NewGuid().ToString();
            editorData.name = initialName;
            editorData.nodeType = new SerializableType(GetType());
            title = editorData.name;
        }

        #endregion
    }
}
