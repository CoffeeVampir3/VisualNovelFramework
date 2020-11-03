using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.VNCharacter;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class DialogueNodeData : NodeRuntimeData
    {
        public Character character;
        [TextArea(5,50)]
        public string dialogue;
    }
}