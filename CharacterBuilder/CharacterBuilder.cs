using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.Elements.Utils;
using VisualNovelFramework.Outfitting;
using Button = UnityEngine.UIElements.Button;
using Object = UnityEngine.Object;
using Toggle = UnityEngine.UIElements.Toggle;

namespace VisualNovelFramework.CharacterBuilder
{
    /// <summary>
    /// Event Paths:
    /// Window:
    /// SetupFileMenu -> New -> CreateNewCharacterMenu
    /// SetupFileMenu -> Load -> LoadCharacterMenu
    /// SetupFileMenu -> Rename -> RenameCharacterMenu
    /// SetupFileMenu -> Save -> SaveCharacterMenu
    /// SetupFileMenu -> Save As -> SaveCharacterAsMenu
    /// 
    /// Character:
    /// SetupCharSelector -> LoadCharacter -> LoadCompositor
    /// SetupNewCharBtn -> NewCharBtnClicked -> CreateNamedCharacter
    /// aspectRatioField -> OnValueChanged (initialization implicit)
    /// SetupCharSelector -> OnCharacterFieldClicked -> RenameCurrentCharacter
    ///
    /// Compositor:
    /// LoadCompositor -> OnCompositorItemSelect -> SetupLayerInspector
    ///
    /// Layers:
    /// SetupLayerInspector -> OnLayerItemSelected
    /// LayerListSearcher -> AddPickedItemToLayerList
    /// </summary>
    public partial class CharacterBuilder : EditorWindow
    {
        private VisualElement previewer;
        private Character currentCharacter = null;
        private CharacterCompositor currentCompositor = null;
        private CharacterLayer currentLayer = null;
        private ModularList textureList = null;
        private ModularList poseList = null;
        private FloatField aspectRatioField;
        private ObjectField charSelector = null;
        private ModularList layerList;
        private CharacterPose currentPose;
        private CharacterLayer currentWorkingLayer;
        private Toggle multilayerToggle;
        
        #region Window
        
        private void SaveCharacterMenu(DropdownMenuAction dma)
        {
            if (currentCharacter != null)
            {
                var c = currentCharacter.Serialize();
                if(c != null)
                    LoadCharacter(c);
            }
        }
        
        private void SaveCharacterAsMenu(DropdownMenuAction dma)
        {
            if (currentCharacter != null)
            {
                var c = currentCharacter.Serialize(true);
                if(c != null)
                    LoadCharacter(c);
            }
        }

        /// <summary>
        /// Synthesizes a mouse click on the object field selector.
        /// </summary>
        private void LoadCharacterMenu(DropdownMenuAction dma)
        {
            if (charSelector == null) 
                return;
            
            var btnQuery = charSelector.Query<VisualElement>(null, "unity-object-field__selector");
            var objectFieldSelector = btnQuery.First();
            if (objectFieldSelector == null)
                return;

            var clickEvent = new MouseDownEvent();
            clickEvent.target = objectFieldSelector;
            
            objectFieldSelector.SendEvent(clickEvent);
        }

        private void RenameCharacterMenu(DropdownMenuAction dma)
        {
            if (currentCharacter != null)
            {
                NamerPopup renamerPopup = new NamerPopup(RenameCurrentCharacter);
                renamerPopup.Popup();
            }
        }

        #endregion

        #region Character

        private void LoadCharacter(Character character)
        {
            currentCharacter = null;
            currentCompositor = null;
            currentLayer = null;
            currentPose = null;
            currentWorkingLayer = null;
            
            if (character != null)
            {
                currentCharacter = character;
                CharacterCompositor compositor = character.compositor;
                if (compositor == null)
                {
                    Debug.LogError("Null compositor!");
                    return;
                }

                LoadCompositor(compositor);
            }
            else
            {
                HideCompositorFrame();
                ClearLayerInspector();
            }
        }

        private void LoadCharacter(ChangeEvent<Object> evt)
        {
            Character character = evt.newValue as Character;

            LoadCharacter(character);
        }

        private void CreateNewCharacterMenu(DropdownMenuAction dma)
        {
            NamerPopup popup = new NamerPopup(CreateNamedCharacter);

            popup.Popup();
        }

        private void CreateNamedCharacter(string charName)
        {
            ClearLayerInspector();

            currentCharacter = null;
            currentCompositor = null;
            currentLayer = null;
            currentPose = null;
            currentWorkingLayer = null;

            Character nChar = CreateInstance<Character>();
            
            nChar.InitializeChar(charName);
            nChar.compositor.layerAspectRatio = 1.0f;
            SerializedObject so = new SerializedObject(nChar);
            charSelector.Bind(so);
            charSelector.value = nChar;
        }
        
        private void OnCharacterFieldClicked(ClickEvent evt)
        {
            if (evt.target is Button 
                || !(evt.currentTarget is ObjectField of) 
                || currentCharacter == null 
                || of.value != currentCharacter)
                return;
            
            if (evt.clickCount == 2)
            {
                //Rename (double click)
                NamerPopup renamerPopup = new NamerPopup(RenameCurrentCharacter);
                renamerPopup.Popup();
                evt.StopImmediatePropagation();
            }
        }

        private void RenameCurrentCharacter(string newName)
        {
            if (currentCharacter == null) 
                return;
            
            currentCharacter.name = newName;
            
            string path = AssetDatabase.GetAssetPath(currentCharacter);
            if (path != "")
            {
                AssetDatabase.RenameAsset(path, newName);
                path = AssetDatabase.GetAssetPath(currentCharacter);
                AssetDatabase.ImportAsset(path);
                AssetDatabase.Refresh();
            }

            //Dunno why this is necessary but this will not rename correctly without the null.
            charSelector.value = null;
            charSelector.value = currentCharacter;
        }

        #endregion

        #region Compositor

        private void LoadCompositor(CharacterCompositor compositor)
        {
            aspectRatioField.value = compositor.layerAspectRatio;

            
            currentLayer = null;
            currentPose = null;
            
            currentCompositor = compositor;
            layerList.BindToList(compositor.layers, OnLayerItemSelected);
            poseList.BindToList(compositor.poses, OnPoseItemSelected);

            layerList.SetEnabled(true);
            poseList.SetEnabled(true);

            LoadCharacterDefaults();
        }

        private void LoadCharacterDefaults()
        {
            if (currentCompositor.layers != null && currentCompositor.layers.Count > 0)
            {
                OnLayerItemSelected(currentCompositor.layers[0]);
            }

            if (currentCompositor.poses != null && currentCompositor.poses.Count > 0)
            {
                OnPoseItemSelected(currentCompositor.poses[0]);
            }
        }

        private void HideCompositorFrame()
        {
            layerList.SetEnabled(false);
            poseList.SetEnabled(false);
        }

        private void OnCompositorItemSelect()
        {
            if (currentPose != null && currentLayer != null)
            {
                var posedLayer = currentCompositor.GetPosedLayer(currentLayer, currentPose);
                if (posedLayer == null)
                {
                    posedLayer = CreateInstance<CharacterLayer>();
                    posedLayer.name = currentPose.name + "-" + currentLayer.name;
                    currentCompositor.SetPosedLayer(currentLayer, currentPose, posedLayer);
                }

                DisplayLayerSelector(posedLayer);
            }
        }

        private void OnLayerItemSelected(Object targetItem)
        {
            if (targetItem is CharacterLayer cl)
            {
                layerList.HighlightItem(targetItem);
                currentLayer = cl;
                OnCompositorItemSelect();
            }
        }

        private void OnLayerItemDelete(Object targetItem)
        {
            if (currentLayer != null && targetItem is CharacterLayer cl && cl == currentLayer)
            {
                textureList?.SetEnabled(false);
                currentLayer = null;
            }
        }

        private void OnPoseItemSelected(Object targetItem)
        {
            if (targetItem is CharacterPose cp)
            {
                poseList.HighlightItem(targetItem);
                currentPose = cp;
                OnCompositorItemSelect();
            }
        }

        private void OnPoseItemDelete(Object targetItem)
        {
            if (currentPose != null && targetItem is CharacterPose cp && cp == currentPose)
            {
                textureList?.SetEnabled(false);
                currentPose = null;
            }
        }

        #endregion

        #region Textures

        private void DisplayLayerSelector(CharacterLayer layer)
        {
            if (layer.textures == null)
            {
                layer.textures = new List<Texture2D>();
            }

            currentWorkingLayer = layer;

            textureList.BindToList(layer.textures, OnTextureSelected);
            
            textureList.FoldoutText = currentPose.name + "-" + currentLayer.name;
            textureList.visible = true;
            textureList.SetEnabled(true);
            multilayerToggle.visible = true;
            multilayerToggle.SetEnabled(true);
            multilayerToggle.value = currentWorkingLayer.isMultilayer;
            
            //Default Select
            if (currentCompositor.layeredPoses.Count > 0)
            {
                var posedLayer = currentCompositor.GetPosedLayer(currentLayer, currentPose);
                OnTextureSelected(posedLayer.GetTextureAt(0));
            }
        }

        private void ClearLayerInspector()
        {
            if (textureList != null)
            {
                textureList.SetEnabled(false);
                multilayerToggle.SetEnabled(false);
                previewer.style.backgroundImage = null;
            }
        }

        private void OnTextureSelected(Object targetObj)
        {
            if (targetObj is Texture2D tex)
            {
                previewer.style.backgroundImage = tex;
                textureList.HighlightItem(targetObj);
            }
        }

        private void AddPickedItemToLayerList(Object targetObj)
        {
            if (!(targetObj is Texture2D tex))
                return;

            if (currentWorkingLayer.textures.Contains(tex))
                return;

            currentWorkingLayer.textures.Add(tex);
            previewer.style.backgroundImage = tex;
            textureList.RefreshList();

            textureList.HighlightItem(targetObj);
        }

        #endregion

    }
}