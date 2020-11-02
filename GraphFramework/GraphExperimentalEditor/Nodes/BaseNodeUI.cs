using UnityEditor;
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

        #region Node UI Construction

        private VisualElement CreateEditorFromNodeData()
        {
            SerializedObject soEditor = new UnityEditor.SerializedObject(editorData);
            var container = new VisualElement();
            
            var it = soEditor.GetIterator();
            if (!it.NextVisible(true)) 
                return container;
            
            //Descends through serialized property children & allows us to edit them.
            do
            {
                var propertyField = new PropertyField(it.Copy()) 
                    { name = "PropertyField:" + it.propertyPath };

                //Bind the property so we can edit the values.
                propertyField.Bind(soEditor);
                    
                //This ignores the label name field, it's ugly.
                if (it.propertyPath == "m_Script" && soEditor.targetObject != null)
                {
                    propertyField.SetEnabled(false);
                    propertyField.visible = false;
                }
                    
                container.Add(propertyField);
            }
            while (it.NextVisible(false));

            return container;
        }
        
        protected void SetupBaseNodeUI()
        {
            //Add port containers
            inputContainer.Add(inputPortsContainer);
            outputContainer.Add(outputPortsContainer);
            
            //Add inline data editor
            outputContainer.Add(CreateEditorFromNodeData());

            SetPosition(editorData.position);
            Repaint();
        }

        protected void Repaint()
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