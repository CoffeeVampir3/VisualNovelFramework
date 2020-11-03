using UnityEditor.Experimental.GraphView;
using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class DialogueRoot : BaseNode
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