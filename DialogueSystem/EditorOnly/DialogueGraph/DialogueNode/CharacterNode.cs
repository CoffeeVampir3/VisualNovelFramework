using UnityEditor.Experimental.GraphView;
using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class CharacterNode : BaseNode
    {
        public new RuntimeCharacterNode runtimeData;
        
        protected override void OnNodeCreation()
        {
            //Empty
        }
        
        protected override void InstantiatePorts()
        {
            var iPort = InstantiatePort(Orientation.Horizontal, 
                Direction.Input, Port.Capacity.Single, typeof(string));
            inputPortsContainer.Add(iPort);
            var oPort = InstantiatePort(Orientation.Horizontal, 
                Direction.Output, Port.Capacity.Single, typeof(string));
            outputPortsContainer.Add(oPort);
        }
    }
}