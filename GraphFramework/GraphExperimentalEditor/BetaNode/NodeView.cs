using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoffeeExtensions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using VisualNovelFramework.GraphFramework.Attributes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.BetaNode
{
    public class NodeView : Node
    {
        private readonly NodeModel nodeModel;
        
        public NodeView(NodeModel model)
        {
            nodeModel = model;
        }
        
        #region Ports
        
        private void CreateDirectionalPortsFromList(List<FieldInfo> fields, Direction dir)
        {
            foreach (var field in fields)
            {
                var portValueType = field.FieldType.GetGenericClassConstructorArguments(typeof(ValuePort<>));
                var port = AddPort(Orientation.Horizontal, 
                    dir, Port.Capacity.Single, portValueType.FirstOrDefault());

                //SerializedFieldInfo serializedFieldInfo = new SerializedFieldInfo(field);
                //portValueBindings.Add(port, serializedFieldInfo);
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
            //Repaint();

            return port;
        }

        private void CreatePortsFromViewDataReflection()
        {
            var oFields = nodeModel.RuntimeData.GetType().GetLocalFieldsWithAttribute<Out>();
            var iFields = nodeModel.RuntimeData.GetType().GetLocalFieldsWithAttribute<In>();
            CreateDirectionalPortsFromList(oFields, Direction.Output);
            CreateDirectionalPortsFromList(iFields, Direction.Input);
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

            //From node
            RefreshExpandedState();
            RefreshPorts();
        }
        
        public void Display()
        {
            CreatePortsFromViewDataReflection();
            CreateEditorFromNodeData();
            
            OnDirty();
        }
    }
}