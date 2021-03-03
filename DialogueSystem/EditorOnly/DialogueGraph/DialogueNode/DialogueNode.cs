using UnityEngine.UIElements;
using VisualNovelFramework.DialogueGraph;
using VisualNovelFramework.DialogueSystem.Nodes;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Attributes;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    [RegisterNodeToView(typeof(DialogueGraphView), "Dialogue Node")]
    public class DialogueNode : BaseNode<RuntimeDialogueNode>
    {

        protected override void OnNodeCreation()
        {
        }
        
        protected override void InstantiatePorts()
        {
        }
    }
}