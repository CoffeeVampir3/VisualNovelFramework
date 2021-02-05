using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.DialogueSystem.Nodes;
using VisualNovelFramework.Editor.Outfitter;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.VNCharacter;

namespace VisualNovelFramework.EditorOnly.DialogueSystem.Nodes
{
    public class CharacterNode : BaseNode<RuntimeCharacterNode>
    {
        protected override void OnNodeCreation()
        {
            var addButton = new Button(LaunchCharacterPositionerWindowTab) {text = "Position Character"};
            titleButtonContainer.Add(addButton);

            SetupOutfitDropdown();
        }

        private OutfitDropdownWindow outfitDropdown = null;
        private void SetupOutfitDropdown()
        {
            var charDropdownThing = new Button {text = "Change Outfits"};
            extensionContainer.Add(charDropdownThing);
            charDropdownThing.RegisterCallback<ClickEvent>(e =>
            {
                if (nodeRuntimeData == null)
                    return;

                var m = EditorWindow.GetWindow<DialogueGraph.DialogueGraph>();
                var newPos = new Rect(m.position.xMin + charDropdownThing.worldBound.position.x,
                    m.position.yMin + charDropdownThing.worldBound.position.y, 200, 400);
                outfitDropdown = EditorWindow.CreateInstance<OutfitDropdownWindow>();
                outfitDropdown.ShowAsDropDown(newPos, new Vector2(200, 400));
                
                outfitDropdown.browser.BindToList(nodeRuntimeData.swag.outfits, OnOutfitItemClicked);
            });
        }
        
        private void OnOutfitItemClicked(Object target)
        {
            if (target == null || !(target is CharacterOutfit co))
                return;
            
            SerializedObject so = new SerializedObject(nodeRuntimeData);
            so.FindProperty(nameof(nodeRuntimeData.outfit)).objectReferenceValue = co;
            so.ApplyModifiedProperties();
            if (outfitDropdown != null)
                outfitDropdown.Close();
        }

        private void LaunchCharacterPositionerWindowTab()
        {
            var wnd = EditorWindow.GetWindow<CharacterPositionerWindow>();
            CharacterPositionerWindow.SetupWindow(wnd);
            
            if(nodeRuntimeData == null)
                Debug.Log("Null!");
            
            wnd.rtCharNode = nodeRuntimeData;
            wnd.ShowTab(); 
        }
        
        protected override void InstantiatePorts()
        {
            var iPort = InstantiatePort(Orientation.Horizontal, 
                Direction.Input, Port.Capacity.Multi, typeof(string));
            inputPortsContainer.Add(iPort);
            var oPort = InstantiatePort(Orientation.Horizontal, 
                Direction.Output, Port.Capacity.Single, typeof(string));
            outputPortsContainer.Add(oPort);
        }
    }
}