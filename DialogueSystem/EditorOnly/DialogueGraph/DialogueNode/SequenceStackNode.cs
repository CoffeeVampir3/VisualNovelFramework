using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class SequenceStackNode : BaseStackNode
    {
        public SequenceStackNode()
        {
            AddCompatibleElement(typeof(CharacterNode));
            AddCompatibleElement(typeof(DialogueNode));
        }
    }
}