﻿using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
    public class CupNode : BaseNode
    {
        #region Temporary/Debugging

        private void DynamicPortTest()
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(int));

            outputPortsContainer.Add(port);
        }

        private void DynamicPortRemover()
        {
            int count = outputPortsContainer.childCount;
            if(count > 0)
                outputPortsContainer.RemoveAt(count-1);
        }
        
        #endregion
        
        private void InstantiatePorts()
        {
            var port = InstantiatePort(Orientation.Horizontal, 
                Direction.Input, Port.Capacity.Single, typeof(int));
            inputPortsContainer.Add(port);
            
            port = InstantiatePort(Orientation.Horizontal, 
                Direction.Input, Port.Capacity.Single, typeof(int));
            inputPortsContainer.Add(port);
        }

        protected override void OnNodeUnserialized()
        {
            SetupBaseNodeUI();
            var addButton = new Button(DynamicPortTest) {text = "Add Port"};
            titleButtonContainer.Add(addButton);
            var removeButton = new Button(DynamicPortRemover) {text = "Remove Port"};
            titleButtonContainer.Add(removeButton);
        }

        protected override void OnNodeCreation()
        {
            InstantiatePorts();
            var addButton = new Button(DynamicPortTest) {text = "Add Port"};
            titleButtonContainer.Add(addButton);
            var removeButton = new Button(DynamicPortRemover) {text = "Remove Port"};
            titleButtonContainer.Add(removeButton);
        }
    }
}