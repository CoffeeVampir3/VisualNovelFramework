using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using VisualNovelFramework.DialogueGraph;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Attributes;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    [RegisterNodeToView(typeof(DialogueGraphView))]
    public class DialogueRoot : BaseNode<RuntimeNode>, IRootNode
    {
        protected override void OnNodeCreation()
        {
        }

        protected override void InstantiatePorts()
        {
        }
    }
}