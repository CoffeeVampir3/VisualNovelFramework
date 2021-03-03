using UnityEngine;
using VisualNovelFramework.GraphFramework.Attributes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.DialogueSystem.Nodes
{
    public class RuntimeDialogueNode : RuntimeNode
    {
        [In] 
        private ValuePort<string> stringPort = new ValuePort<string>();
        [In] 
        private ValuePort<Flow> flowPortIn = new ValuePort<Flow>();
        [Out] 
        private ValuePort<Flow> flowPortOut = new ValuePort<Flow>();
        [SerializeField] 
        [TextArea(10,500)]
        public string dialogue;
        
        public override void OnEvaluate()
        {
            Debug.Log("Evaluating.");
            
            foreach (var conn in connections)
            {
                conn.GetRemoteNode().OnEvaluate();
            }
            dialogue = stringPort.Value;
        }
    }
}