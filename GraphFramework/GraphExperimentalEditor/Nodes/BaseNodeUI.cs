﻿using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
    public abstract partial class BaseNode
    {
        protected readonly VisualElement outputPortsContainer = new VisualElement();
        protected readonly VisualElement inputPortsContainer = new VisualElement();
        public SerializedObject serializedNode = null; //Allows us to write the runtime data correctly in the graph serializer.

        #region Node UI Construction

        protected virtual VisualElement CreateNodeGUI()
        {
            return CreateEditorFromNodeData();
        }
        
        private VisualElement CreateEditorFromNodeData()
        {
            serializedNode = new SerializedObject(RuntimeData);
            var container = new VisualElement();
            
            var it = serializedNode.GetIterator();
            if (!it.NextVisible(true)) 
                return container;
            
            //Descends through serialized property children & allows us to edit them.
            do
            {
                var propertyField = new PropertyField(it.Copy()) 
                    { name = "PropertyField:" + it.propertyPath };

                //Bind the property so we can edit the values.
                propertyField.Bind(serializedNode);
                    
                //This ignores the label name field, it's ugly.
                if (it.propertyPath == "m_Script" && serializedNode.targetObject != null)
                {
                    propertyField.SetEnabled(false);
                    propertyField.visible = false;
                }
                    
                container.Add(propertyField);
            }
            while (it.NextVisible(false));

            return container;
        }

        private void SetupBaseNodeUI()
        {
            //Add port containers
            inputContainer.Add(inputPortsContainer);
            outputContainer.Add(outputPortsContainer);
            
            //Add inline data editor
            outputContainer.Add(CreateNodeGUI());

            SetPosition(editorData.position);
            Repaint();
        }

        private void Repaint()
        {
            RefreshExpandedState();
            RefreshPorts();
        }

        private void RebuildPortFromSerialization(SerializedPortData serializedPort)
        {
            Port p = InstantiatePort(serializedPort.orientation, 
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