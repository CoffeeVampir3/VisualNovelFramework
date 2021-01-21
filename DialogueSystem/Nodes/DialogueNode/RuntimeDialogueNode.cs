using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class RuntimeDialogueNode : RuntimeNode
    {
        [TextArea(10,500)]
        public string dialogue;
    }
}