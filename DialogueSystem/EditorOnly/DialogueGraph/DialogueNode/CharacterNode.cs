using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.DialogueSystem.Nodes;
using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class CharacterNode : BaseNode<RuntimeCharacterNode>
    {
        protected override void OnNodeCreation()
        {
            var addButton = new Button(LaunchCharacterPositionerWindowTab) {text = "Position Character"};
            titleButtonContainer.Add(addButton);
        }

        private void LaunchCharacterPositionerWindowTab()
        {
            var wnd = EditorWindow.GetWindow<CharacterPositionerWindow>();
            
            if(nodeRuntimeData == null)
                Debug.Log("Null!");
            
            Debug.Log(nodeRuntimeData.outfit.name);
            wnd.rtCharNode = nodeRuntimeData;
            wnd.ShowTab();
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