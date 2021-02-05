using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.Editor.Elements;
using VisualNovelFramework.Editor.Serialization;
using VisualNovelFramework.VNCharacter;
using Object = UnityEngine.Object;

namespace VisualNovelFramework.Editor.Outfitter
{
    //Why the fuck didin't I make this a state machine???
    /// <summary>
    ///     Event Paths:
    ///     Initialization Events
    ///     SetupOutfitDropdown()    -> outfitDropdown -> OnOutfitClicked
    ///     SetupCharacterSelector() -> charSelectorOnValueChanged -> LoadCharacter
    ///     SetupLayerListView()     -> OnLayerItemClicked
    ///     File Menu
    ///     SetupFileMenu() -> LoadCharacterMenu -> CharSelectorSynthesis /w Object Field Click
    ///     SetupFileMenu() -> NewOutfitMenu -> CreateNamedOutfit -> LoadCharacterDefault
    ///     -> LoadNewPoseToWorking -> DisplayOutfit
    ///     SetupFileMenu() -> SaveOutfitMenu
    ///     SetupFileMenu() -> DeleteOutfitMenu
    ///     Operational Events
    ///     OnPoseItemSelected 	-> LoadNewPoseToWorking
    ///     -> OnCompositorItemSelect -> LoadPosedLayerTextureList
    ///     OnLayerItemSelected	-> OnCompositorItemSelect -> LoadPosedLayerTextureList
    ///     OnTextureItemClicked -> DisplayOutfit
    /// </summary>
    public partial class Outfitter : EditorWindow
    {
        private readonly List<Texture2D> images = new List<Texture2D>();
        private readonly Dictionary<Image, int> imageToIndex = new Dictionary<Image, int>();
        private ObjectField charSelector = null;
        private Character currentCharacter = null;
        private CharacterCompositor currentCompositor = null;
        private CharacterLayer currentLayer = null;
        private CharacterPose currentPose = null;
        private CharacterLayer currentPosedLayer = null;
        private ListView layerImageLister;
        private ModularList layerList = null;
        private Label outfitLabel;
        private ModularList poseList = null;
        private CharacterOutfit workingOutfit = null;

        #region FileMenu

        private void LoadCharacterMenu(DropdownMenuAction dma)
        {
            if (charSelector == null)
                return;

            var btnQuery = charSelector.Query<VisualElement>(null, "unity-object-field__selector");
            var objectFieldSelector = btnQuery.First();
            if (objectFieldSelector == null)
                return;

            var clickEvent = new MouseDownEvent {target = objectFieldSelector};

            objectFieldSelector.SendEvent(clickEvent);
        }

        private void SaveOutfitMenu(DropdownMenuAction dma)
        {
            if (workingOutfit == null || currentCharacter == null)
                return;

            Debug.Log("Saved.");
            OutfitSerializer.SerializeToCharacter(currentCharacter, workingOutfit);
        }

        private Action<Character, CharacterOutfit> postRenameAction = null;

        private void SaveOutfitAsMenu(DropdownMenuAction dma)
        {
            if (workingOutfit == null || currentCharacter == null)
                return;

            postRenameAction = OutfitSerializer.SerializeToCharacter;
            RenameOutfitMenu(null);
        }

        private void DeleteOutfitMenu(DropdownMenuAction dma)
        {
            if (workingOutfit == null || currentCharacter == null)
                return;

            OutfitSerializer.DeleteFromCharacter(currentCharacter, workingOutfit);
        }

        private void NewOutfitMenu(DropdownMenuAction dma)
        {
            if (currentCharacter == null)
            {
                Debug.LogError("Must select a character before you can create an outfit!");
                return;
            }

            var popup = new NamerPopup(CreateNamedOutfit);
            popup.Popup();
        }

        private void RenameOutfitMenu(DropdownMenuAction dma)
        {
            if (workingOutfit == null || currentCharacter == null)
                return;

            var popup = new NamerPopup(RenameCurrentOutfit);
            popup.Popup();
        }

        #endregion

        #region Outfit

        private void RenameCurrentOutfit(string newName)
        {
            if (workingOutfit != null)
            {
                workingOutfit.name = newName;
                outfitLabel.text = newName;
                postRenameAction?.Invoke(currentCharacter, workingOutfit);
                postRenameAction = null;
            }
        }

        private void CreateNamedOutfit(string charName)
        {
            if (charName == "")
                if (workingOutfit == null)
                    DisableOutfitFrame();

            workingOutfit = CreateInstance<CharacterOutfit>();
            workingOutfit.Initialize(charName);
            OnOutfitLoaded();
            LoadCharacterDefault(currentCharacter.compositor);
        }

        private void LoadOutfit(CharacterOutfit outfit)
        {
            currentLayer = null;
            currentPose = null;
            workingOutfit = null;

            if (outfit == null)
                return;

            workingOutfit = outfit;
            OnOutfitLoaded();
            currentPose = workingOutfit.outfitPose;
            currentLayer = currentCharacter.compositor.layers[0];
            outfitPreviewer.DisplayOutfit(workingOutfit);

            poseList.HighlightItem(currentPose);
            layerList.HighlightItem(currentLayer);
        }

        private void OnOutfitLoaded()
        {
            outfitLabel.text = workingOutfit.name;
            EnableOutfitFrame();
        }

        #endregion

        #region Frame Primary

        private void LoadCharacterDefault(CharacterCompositor compositor)
        {
            if (compositor.layers.Count == 0 || compositor.poses.Count == 0)
            {
                Debug.LogError("Character needs at least one pose and one layer!");
                return;
            }

            //Selects and sets as current 
            OnLayerItemSelected(compositor.layers[0]);
            OnPoseItemSelected(compositor.poses[0]);
            LoadNewPoseToWorking(compositor);
        }

        private void LoadNewPoseToWorking(CharacterCompositor compositor)
        {
            var unusedLayers = workingOutfit.SwitchPose(currentPose, compositor);
            for (var index = 0; index < compositor.layers.Count; index++)
                layerList.SetItemEnabled(index, !unusedLayers.Contains(index));

            outfitPreviewer.DisplayOutfit(workingOutfit);
        }

        private void LoadCharacter(Character character)
        {
            if (character == null)
            {
                currentCharacter = null;
                DisableOutfitFrame();
                return;
            }

            currentCharacter = character;
            var compositor = character.compositor;
            if (compositor == null)
            {
                Debug.LogError(
                    "This character file is malformed and does not have a compositor, it's not able to be loaded!");
                return;
            }

            LoadCompositor(compositor);
        }

        private void LoadCharacter(ChangeEvent<Object> evt)
        {
            var character = evt.newValue as Character;

            LoadCharacter(character);
        }

        private void LoadCompositor(CharacterCompositor compositor)
        {
            currentCompositor = compositor;
            layerList.BindToList(compositor.layers, OnLayerItemSelected);
            poseList.BindToList(compositor.poses, OnPoseItemSelected);

            DisableOutfitFrame();
        }

        private void OnCompositorItemSelect(CharacterLayer cl, CharacterPose cp)
        {
            if (currentPose != null && currentLayer != null)
            {
                var posedLayer = currentCompositor.GetPosedLayer(cl, cp);
                if (posedLayer == null)
                    return;

                LoadPosedLayerTextureList(posedLayer);
            }
        }

        private void LoadPosedLayerTextureList(CharacterLayer posedLayer)
        {
            if (workingOutfit == null)
                return;

            images.Clear();
            currentPosedLayer = posedLayer;
            for (var i = 0; i < posedLayer.textures.Count; i++) images.Add(posedLayer.GetTextureAt(i));

            layerImageLister.ClearSelection();
            layerImageLister.Refresh();
            layerImageLister.ScrollToItem(0);
            layerImageLister.SetSelection(0);
        }

        #endregion

        #region List Controls

        private void OnLayerItemSelected(Object targetItem)
        {
            if (!(targetItem is CharacterLayer cl))
                return;

            if (cl == currentLayer)
                return;

            layerList.HighlightItem(targetItem);
            currentLayer = cl;
            OnCompositorItemSelect(currentLayer, currentPose);
        }

        private void OnPoseItemSelected(Object targetItem)
        {
            if (!(targetItem is CharacterPose cp))
                return;

            if (cp == currentPose)
                return;

            poseList.HighlightItem(targetItem);
            currentPose = cp;
            LoadNewPoseToWorking(currentCompositor);
            OnCompositorItemSelect(currentLayer, currentPose);
        }

        private void OnTextureItemClicked(ClickEvent e)
        {
            if (currentPosedLayer == null)
                return;

            if (!(e.currentTarget is Image img))
                return;

            if (!imageToIndex.TryGetValue(img, out var index))
                return;

            workingOutfit.AddOrRemoveExistingItem(currentPosedLayer, index);
            outfitPreviewer.DisplayOutfit(workingOutfit);
        }

        #endregion

        #region FrameHelpers

        private void EnableOutfitFrame()
        {
            layerList.SetEnabled(true);
            poseList.SetEnabled(true);
            outfitLabel.SetEnabled(true);
            layerImageLister.SetEnabled(true);
        }

        private void DisableOutfitFrame()
        {
            outfitPreviewer.Clear();
            layerList.SetEnabled(false);
            poseList.SetEnabled(false);
            outfitLabel.SetEnabled(false);
            layerImageLister.SetEnabled(false);
            workingOutfit = null;
        }

        #endregion
    }
}