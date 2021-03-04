﻿using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.BetaNode
{
    public class NodeView : Node
    {
        private readonly NodeModel nodeModel;
        private Dictionary<Port, PortModel> portToModel = new Dictionary<Port, PortModel>();
        //Lookup via string because undo/redo creates a different copy.
        private Dictionary<string, Port> modelToPort = new Dictionary<string, Port>();

        public NodeView(NodeModel model)
        {
            nodeModel = model;
        }
        
        #region Ports

        /// <summary>
        /// Takes a port and spits out it's related model.
        /// </summary>
        public bool TryGetPortToModel(Port p, out PortModel model)
        {
            return portToModel.TryGetValue(p, out model);
        }
        
        /// <summary>
        /// Takes a model GUID and spits out it's related port.
        /// Lookup is via GUID because undo/redo creates a different copy.
        /// </summary>
        public bool TryGetModelToPort(string modelGUID, out Port p)
        {
            return modelToPort.TryGetValue(modelGUID, out p);
        }
        
        private void CreatePortsFromModel()
        {
            foreach (var portModel in nodeModel.inputPorts)
            {
                Port p = AddPort(portModel.orientation, portModel.direction, 
                    portModel.capacity, portModel.portValueType.type);

                portToModel.Add(p, portModel);
                modelToPort.Add(portModel.portGUID, p);
            }
            foreach (var portModel in nodeModel.outputPorts)
            {
                Port p = AddPort(portModel.orientation, portModel.direction, 
                    portModel.capacity, portModel.portValueType.type);
                
                portToModel.Add(p, portModel);
                modelToPort.Add(portModel.portGUID, p);
            }
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
                    inputContainer.Add(port);
                    break;
                case Direction.Output:
                    outputContainer.Add(port);
                    break;
            }

            return port;
        }
        
        #endregion 

        private void CreateEditorFromNodeData()
        {
            var serializedNode = new SerializedObject(nodeModel.RuntimeData);
            var it = serializedNode.GetIterator();
            if (!it.NextVisible(true))
                return;
            
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

                extensionContainer.Add(propertyField);
            }
            while (it.NextVisible(false));
        }

        public void OnDirty()
        {
            title = nodeModel.NodeTitle;
            expanded = nodeModel.IsExpanded;
            SetPosition(nodeModel.Position);
            RefreshExpandedState();
            RefreshPorts();
        }

        public void Display()
        {
            CreatePortsFromModel();
            CreateEditorFromNodeData();
            
            OnDirty();
        }
    }
}