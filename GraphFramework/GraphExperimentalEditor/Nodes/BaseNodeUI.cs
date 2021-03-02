using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoffeeExtensions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Attributes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO;
using VisualNovelFramework.GraphFramework.GraphRuntime;
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

                //This ignores the label name field, it's ugly.
                if (it.propertyPath == "m_Script" && 
                    serializedNode.targetObject != null) 
                {
                    propertyField.SetEnabled(false);
                    propertyField.visible = false;
                    continue;
                }

                DisableReadonlyField(it, propertyField);
                    
                extensionContainer.Add(propertyField);
            }
            while (it.NextVisible(false));
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
        }
        private void CreateDirectionalPortsFromList(List<FieldInfo> fields, Direction dir)
        {
            foreach (var field in fields)
            {
                var portValueType = field.FieldType.GetGenericClassConstructorArguments(typeof(ValuePort<>));
                var port = AddPort(Orientation.Horizontal, 
                    dir, Port.Capacity.Single, portValueType.FirstOrDefault());

                SerializedFieldInfo serializedFieldInfo = new SerializedFieldInfo(field);
                portValueBindings.Add(port, serializedFieldInfo);
            }
        }

        protected void CreatePortsFromReflection()
        {
            var oFields = RuntimeData.GetType().GetLocalFieldsWithAttribute<Out>();
            var iFields = RuntimeData.GetType().GetLocalFieldsWithAttribute<In>();
            CreateDirectionalPortsFromList(oFields, Direction.Output);
            CreateDirectionalPortsFromList(iFields, Direction.Input);
        }

        protected Port AddDynamicPort(Orientation orientation,
            Direction direction,
            Port.Capacity capacity,
            System.Type type)
        {
            throw new System.NotImplementedException();
            
            //TODO:: Need a way to reference dynamic ports from runtime node.
            var genericValuePort = typeof(ValuePort<>).MakeGenericType(type);
            var valuePort = Activator.CreateInstance(genericValuePort) as ValuePort;
            var port = AddPort(orientation, direction, capacity, type);
            return port;
        }

        private Port AddPort(Orientation orientation,
            Direction direction,
            Port.Capacity capacity,
            System.Type type)
        {
            var port = InstantiatePort(orientation, direction, capacity, type);
            switch (direction)
            {
                case Direction.Input:
                    inputPortsContainer.Add(port);
                    break;
                case Direction.Output:
                    outputPortsContainer.Add(port);
                    break;
            }
            Repaint();

            return port;
        }
        
        private Port AddSerializedPort(Orientation orientation, 
            Direction direction,
            Port.Capacity capacity,
            System.Type type,
            SerializedFieldInfo valueFieldInfo, 
            List<string> connectionGuids)
        {
            var port = AddPort(orientation, direction, capacity, type);
            if (valueFieldInfo != null)
            {
                portValueBindings.Add(port, valueFieldInfo);
            }
            if (connectionGuids != null)
            {
                portConnectionGuids.Add(port, connectionGuids);
            }

            return port;
        }

        /// <summary>
        /// Repaints this node, repaint will automatically hide unused containers.
        /// </summary>
        protected void Repaint()
        {
            //Hide unused
            inputContainer.style.display = 
                inputPortsContainer.childCount == 0 
                    ? DisplayStyle.None 
                    : DisplayStyle.Flex;
            outputContainer.style.display = 
                outputPortsContainer.childCount == 0 
                    ? DisplayStyle.None 
                    : DisplayStyle.Flex;
            extensionContainer.style.display = 
                extensionContainer.childCount == 0 
                    ? DisplayStyle.None 
                    : DisplayStyle.Flex;
            titleContainer.style.display = title == "" 
                    ? DisplayStyle.None 
                    : DisplayStyle.Flex;

            RefreshExpandedState();
            RefreshPorts();
        }

        private void RebuildPortFromSerialization(SerializedPortData serializedPort)
        {
            AddSerializedPort(serializedPort.orientation,
                serializedPort.direction, serializedPort.capacity,
                serializedPort.portValueType.type,
                serializedPort.serializedValueFieldInfo,
                serializedPort.portConnectionGuids);
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