using VisualNovelFramework.DialogueGraph;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Attributes;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    [RegisterNodeToView(typeof(DialogueGraphView), "Flow/Sequence Node")]
    public class SequenceStackNode : BaseStackNode
    {
        public SequenceStackNode()
        {
            AddCompatibleElement(typeof(CharacterNode));
            AddCompatibleElement(typeof(DialogueNode));
        }
    }
}