using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class DialogueNode : BaseNode
    {
        private void InstantiatePorts()
        {
            var port = InstantiatePort(Orientation.Horizontal, 
                Direction.Input, Port.Capacity.Single, typeof(int));
            inputPortsContainer.Add(port);
            
            port = InstantiatePort(Orientation.Horizontal, 
                Direction.Input, Port.Capacity.Single, typeof(int));
            inputPortsContainer.Add(port);
        }
        
        protected override void OnNodeUnserialized()
        {
            SetupBaseNodeUI();
        }

        protected override void OnNodeCreation()
        {
            InstantiatePorts();
        }
    }
}