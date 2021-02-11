using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using VisualNovelFramework.DialogueSystem.Nodes;
using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class DialogueNode : BaseNode<RuntimeDialogueNode>
    {
        private void DynamicPortTest()
        {
            var port = CreatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(string));

            outputPortsContainer.Add(port);
        }

        private void DynamicPortRemover()
        {
            int count = outputPortsContainer.childCount;
            if(count > 0)
                outputPortsContainer.RemoveAt(count-1);
        }
        
        protected override void OnNodeCreation()
        {
            var addButton = new Button(DynamicPortTest) {text = "Add Port"};
            titleButtonContainer.Add(addButton);
            var removeButton = new Button(DynamicPortRemover) {text = "Remove Port"};
            titleButtonContainer.Add(removeButton);
        }
        
        protected override void InstantiatePorts()
        {
            var port = CreatePort(Orientation.Horizontal, 
                Direction.Input, Port.Capacity.Multi, typeof(string));
            inputPortsContainer.Add(port);
        }
    }
}