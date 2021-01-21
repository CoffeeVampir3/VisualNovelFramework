using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.VNCharacter;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class RuntimeDialogueNode : RuntimeNode
    {
        public Character swag;
        
        [TextArea(5,50)]
        public string dialogue;
    }
}