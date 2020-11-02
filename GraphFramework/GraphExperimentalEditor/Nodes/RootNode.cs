using UnityEditor.Experimental.GraphView;

namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
    public class RootNode : BaseNode
    {
        private void InstantiatePorts()
        {
            var port = InstantiatePort(Orientation.Horizontal, 
                Direction.Output, Port.Capacity.Multi, typeof(int));
            outputPortsContainer.Add(port);
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