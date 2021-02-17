using UnityEditor.Experimental.GraphView;
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
            var port = CreatePort(Orientation.Horizontal, 
                Direction.Output, Port.Capacity.Single, typeof(string));
            outputPortsContainer.Add(port);
        }
    }
}