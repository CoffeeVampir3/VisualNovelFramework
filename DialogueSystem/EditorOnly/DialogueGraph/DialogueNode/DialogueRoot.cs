using UnityEditor.Experimental.GraphView;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class DialogueRoot : BaseNode<RuntimeNode>, IRootNode
    {
        protected override void OnNodeCreation()
        {
        }

        protected override void InstantiatePorts()
        {
            var port = InstantiatePort(Orientation.Horizontal, 
                Direction.Output, Port.Capacity.Single, typeof(string));
            outputPortsContainer.Add(port);
        }
    }
}