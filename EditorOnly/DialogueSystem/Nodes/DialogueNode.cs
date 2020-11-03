using UnityEditor.Experimental.GraphView;
using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class DialogueNode : BaseNode
    {
        protected override void InstantiatePorts()
        {
            var port = InstantiatePort(Orientation.Horizontal, 
                Direction.Input, Port.Capacity.Single, typeof(string));
            inputPortsContainer.Add(port);
            
            port = InstantiatePort(Orientation.Horizontal, 
                Direction.Input, Port.Capacity.Single, typeof(string));
            inputPortsContainer.Add(port);
        }

        protected override void OnNodeCreation()
        {
        }
    }
}