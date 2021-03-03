using VisualNovelFramework.DialogueGraph;
using VisualNovelFramework.DialogueSystem.Nodes;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Attributes;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    [RegisterNodeToView(typeof(DialogueGraphView))]
    public class DialogueRoot : BaseNode<RuntimeRootNode>, IRootNode
    {
        protected override void OnNodeCreation()
        {
        }

        protected override void InstantiatePorts()
        {
        }
    }
}