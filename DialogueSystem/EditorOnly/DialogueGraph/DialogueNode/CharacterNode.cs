using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.DialogueSystem.Nodes;
using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class CharacterNode : BaseNode
    {
        public new RuntimeCharacterNode runtimeData;
        
        protected override void OnNodeCreation()
        {
            var addButton = new Button(LaunchCharacterPositionerWindowTab) {text = "Position Character"};
            titleButtonContainer.Add(addButton);
        }

        private void LaunchCharacterPositionerWindowTab()
        {
            var wnd = EditorWindow.GetWindow<CharacterPositionerWindow>();
            
            if(runtimeData == null)
                Debug.Log("Null!");
            
            Debug.Log(runtimeData.outfit.name);
            wnd.rtCharNode = runtimeData;
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