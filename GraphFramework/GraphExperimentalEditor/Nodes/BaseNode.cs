using System;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelFramework.Editor.Serialization;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.GraphFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
    /// <summary>
    /// Classes inheriting from BaseNode MUST implement a new runtimeData
    /// public new NodeRuntimeData runtimeData;
    /// where NodeRuntimeData can be a class inheriting from it
    /// </summary>
    public abstract partial class BaseNode : Node
    {
        public NodeEditorData editorData;
        public RuntimeNode runtimeData;

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
            ReflectAndLoadRuntimeData(serializationData);

            editorData = ScriptableObject.Instantiate(serializationData.nodeEditorData);
            editorData.name = serializationData.nodeEditorData.name;
            title = editorData.name;
        }

        private void ReflectAndLoadRuntimeData(NodeSerializationData serializationData)
        {
            var runtimeField = GetOverridenRuntimeDataField();
            runtimeData = ScriptableObject.Instantiate(serializationData.runtimeNode);
            runtimeData.name = serializationData.runtimeNode.name;

            if (runtimeField == null) 
                return;
            
            runtimeField.SetValue(this, runtimeData);
        }
        
        private FieldInfo GetOverridenRuntimeDataField()
        {
            var k = GetType().
                GetFields( BindingFlags.CreateInstance |
                           BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase | 
                           BindingFlags.InvokeMethod | BindingFlags.NonPublic | 
                           BindingFlags.Instance | BindingFlags.Public);
            
            foreach (var field in k)
            {
                if (typeof(RuntimeNode).IsAssignableFrom(field.FieldType))
                {
                    return field;
                }
            }

            return null;
        }
        
        private void GenerateNewNodeData(string initialName)
        {
            editorData = ScriptableObject.CreateInstance<NodeEditorData>();
            var runtimeField = GetOverridenRuntimeDataField();
            
            if (runtimeField == null)
            {
                runtimeData = ScriptableObject.CreateInstance<RuntimeNode>();
            }
            else
            {
                runtimeData = ScriptableObject.CreateInstance(runtimeField.FieldType) as RuntimeNode;
                runtimeField.SetValue(this, runtimeData);
            }

            editorData.GUID = Guid.NewGuid().ToString();
            editorData.name = initialName;
            
            Debug.Assert(runtimeData != null, nameof(runtimeData) + " != null");
            runtimeData.SetCoffeeGUID(editorData.GUID);
            editorData.nodeType = new SerializableType(GetType());
            title = editorData.name;
        }

        #endregion
    }
}
