using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class RuntimeDialogueNode : RuntimeNode
    {
        [TextArea(5,50)]
        public string dialogue;
    }
}