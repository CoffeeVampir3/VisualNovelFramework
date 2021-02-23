using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using VisualNovelFramework.DialogueGraph;
using VisualNovelFramework.DialogueSystem.Nodes;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Attributes;
using VisualNovelFramework.VNCharacter;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    [RegisterNodeToView(typeof(DialogueGraphView), "Dialogue Node")]
    public class DialogueNode : BaseNode<RuntimeDialogueNode>
    {
        private void DynamicPortTest()
        {
            AddDynamicPort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(string));
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
        }
    }
}