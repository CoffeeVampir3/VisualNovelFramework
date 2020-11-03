using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class DialogueNode : BaseNode
    {
        private void DynamicPortTest()
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(string));

            outputPortsContainer.Add(port);
        }

        private void DynamicPortRemover()
        {
            int count = outputPortsContainer.childCount;
            if(count > 0)
                outputPortsContainer.RemoveAt(count-1);
        }
        
        protected override void InstantiatePorts()
        {
            var port = InstantiatePort(Orientation.Horizontal, 
                Direction.Input, Port.Capacity.Single, typeof(string));
            inputPortsContainer.Add(port);
            
            port = InstantiatePort(Orientation.Horizontal, 
                Direction.Output, Port.Capacity.Single, typeof(string));
            outputPortsContainer.Add(port);
        }

        protected override void OnNodeCreation()
        {
            if(runtimeData == null)
                runtimeData = ScriptableObject.CreateInstance<DialogueNodeData>();
            
            var addButton = new Button(DynamicPortTest) {text = "Add Port"};
            titleButtonContainer.Add(addButton);
            var removeButton = new Button(DynamicPortRemover) {text = "Remove Port"};
            titleButtonContainer.Add(removeButton);
        }
    }
}