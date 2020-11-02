using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelFramework.Editor.Serialization;
using VisualNovelFramework.GraphFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
    public abstract partial class BaseNode : Node
    {
        public NodeEditorData editorData;

        //TODO::Debugging RT data.

        #region Node Data Handling

        private void LoadNodeData(NodeEditorData nodeEditorData)
        {
            var copy = ScriptableObject.Instantiate(nodeEditorData);
            editorData = copy;
            editorData.name = nodeEditorData.name;
            title = editorData.name;
        }

        protected abstract void OnNodeUnserialized();

        protected abstract void OnNodeCreation();

        /// <summary>
        /// Deserialization initialization
        /// </summary>
        public void Initialize(NodeSerializationData data)
        {
            LoadNodeData(data.nodeEditorData);
            OnNodeUnserialized();
            RebuildPortsFromSerialization(data);
        }

        protected void GenerateNewNodeData(string initialName)
        {
            editorData = ScriptableObject.CreateInstance<NodeEditorData>();
            editorData.GUID = Guid.NewGuid().ToString();
            editorData.name = initialName;
            editorData.nodeType = new SerializableType(GetType());
            title = editorData.name;
        }

        //TODO:: Temporary shit.
        /// <summary>
        /// Initial (new node) creation
        /// </summary>
        public void Initialize(string initialName)
        {
            GenerateNewNodeData(initialName);
            SetupBaseNodeUI();
            OnNodeCreation();
        }

        #endregion
    }
}
