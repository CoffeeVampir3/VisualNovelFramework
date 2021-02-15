
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Attributes;
using VisualNovelFramework.GraphFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
    public abstract partial class BaseNode
    {
        [SerializeReference]
        protected readonly VisualElement outputPortsContainer = new VisualElement();
        [SerializeReference]
        protected readonly VisualElement inputPortsContainer = new VisualElement();
        #region Node UI Construction

        /// <summary>
        /// Override this if you want more fine-grain control over your nodes GUI.
        /// </summary>
        protected virtual void CreateNodeGUI()
        {
            CreateEditorFromNodeData();
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
        
        #region Readonly Attribute Handler
        
        private readonly Dictionary<string, FieldInfo> nameToReadonlyField
            = new Dictionary<string,FieldInfo>();
        private void DisableReadonlyField(SerializedProperty it, PropertyField propertyField)
        {
            if (nameToReadonlyField.TryGetValue(it.propertyPath, out _))
            {
                propertyField.SetEnabled(false);
            }
        }

        private void CacheReadonlyProperties()
        {
            nameToReadonlyField.Clear();
            var fields = RuntimeData.GetType().
                GetFields(BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.NonPublic);
            
            foreach (var field in fields)
            {
                if (field.GetCustomAttributes<ReadonlyField>().Any())
                {
                    nameToReadonlyField.Add(field.Name, field);
                }
            }
        }
        
        #endregion

        protected void CreateEditorFromNodeData()
        {
            var serializedNode = new SerializedObject(RuntimeData);
            VisualElement container = new VisualElement();

            var it = serializedNode.GetIterator();
            if (!it.NextVisible(true))
                return;
            
            CacheReadonlyProperties();
            //Descends through serialized property children & allows us to edit them.
            do
            {
                var propertyField = new PropertyField(it.Copy()) 
                    { name = it.propertyPath };

                //Bind the property so we can edit the values.
                propertyField.Bind(serializedNode);

                container.Add(propertyField);
                
                //This ignores the label name field, it's ugly.
                if (it.propertyPath == "m_Script" && 
                    serializedNode.targetObject != null) 
                {
                    propertyField.SetEnabled(false);
                    propertyField.visible = false;
                }

                DisableReadonlyField(it, propertyField);
                    
                container.Add(propertyField);
            }
            while (it.NextVisible(false));

            extensionContainer.Add(container);
        }

        private void SetupBaseNodeUI()
        {
            //Add port containers
            inputContainer.Add(inputPortsContainer);
            outputContainer.Add(outputPortsContainer);
            CreateNodeGUI();

            titleContainer.pickingMode = PickingMode.Position;
            titleContainer.RegisterCallback<PointerDownEvent>(OnTitleDoubleClicked);
            RegisterCallback<DetachFromPanelEvent>(OnNodeDeleted);

            Repaint();
        }
        
        protected Port CreatePort(Orientation orientation, 
            Direction direction,
            Port.Capacity capacity,
            System.Type type)
        {
            var port = InstantiatePort(orientation, direction, capacity, type);
            return port;
        }

        private void Repaint()
        {
            RefreshExpandedState();
            RefreshPorts();
        }

        private void RebuildPortFromSerialization(SerializedPortData serializedPort)
        {
            Port p = CreatePort(serializedPort.orientation, 
                serializedPort.direction, serializedPort.capacity, 
                serializedPort.portValueType.type);

            p.portName = serializedPort.portName;
            switch (serializedPort.direction)
            {
                case Direction.Input:
                    inputPortsContainer.Add(p);
                    break;
                case Direction.Output:
                    outputPortsContainer.Add(p);
                    break;
            }
        }

        private void RebuildPortsFromSerialization(NodeSerializationData serializedNode)
        {
            foreach (var sPort in serializedNode.serializedPorts)
            {
                RebuildPortFromSerialization(sPort);
            }
        }
        
        #endregion
    }
}