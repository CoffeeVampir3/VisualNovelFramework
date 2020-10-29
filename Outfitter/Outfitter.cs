using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.Outfitting;
using Object = UnityEngine.Object;

namespace VisualNovelFramework.Outfitter
{
    public partial class Outfitter : EditorWindow
    {
        private Character currentCharacter = null;
        private CharacterCompositor currentCompositor = null;
        private CharacterLayer currentLayer = null;
        private CharacterPose currentPose = null;
        private CharacterLayer currentPosedLayer = null;
        private ListView layerImageLister;
        private readonly Dictionary<Image, int> imageToIndex = new Dictionary<Image, int>();
        private readonly List<Texture2D> images = new List<Texture2D>();
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

            var clickEvent = new MouseDownEvent();
            clickEvent.target = objectFieldSelector;
            
            objectFieldSelector.SendEvent(clickEvent);
        }
        
        private void SaveOutfitMenu(DropdownMenuAction dma)
        {
            if (workingOutfit == null || currentCharacter == null)
                return;

            workingOutfit.name = "outfitTester";
            workingOutfit.SerializeToCharacter(currentCharacter);
        }
        
        #endregion
        
        private void LoadNewPoseToWorking(CharacterCompositor compositor)
        {
            workingOutfit.ResetOutfit();
            for (var index = 0; index < compositor.layers.Count; index++)
            {
                var cl = compositor.layers[index];
                var posedLayer = compositor.GetPosedLayer(cl, currentPose);
                if (posedLayer == null || posedLayer.textures.Count == 0)
                {
                    //Disables layers with no content.
                    layerList.SetItemEnabled(index, false);
                    continue;
                }
                
                layerList.SetItemEnabled(index, true);

                if (posedLayer.isMultilayer)
                    continue;

                workingOutfit.SetLayerDefault(currentPose, posedLayer);
            }

            outfitPreviewer.DisplayOutfit(workingOutfit);
        }

        private void LoadCharacterDefault(CharacterCompositor compositor)
        {
            workingOutfit = CreateInstance<CharacterOutfit>();
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
        
        private void LoadCharacter(ChangeEvent<Object> evt)
        {
            Character character = evt.newValue as Character;

            if (character == null) 
                return;
            
            currentCharacter = character;
            CharacterCompositor compositor = character.compositor;
            if (compositor == null)
            {
                Debug.LogError("This character file is malformed and does not have a compositor, it's not able to be loaded!");
                return;
            }

            LoadCompositor(compositor);
        }
        
        private void LoadCompositor(CharacterCompositor compositor)
        {
            currentCompositor = compositor;
            layerList.BindToList(compositor.layers, OnLayerItemSelected);
            poseList.BindToList(compositor.poses, OnPoseItemSelected);

            layerList.SetEnabled(true);
            poseList.SetEnabled(true);

            LoadCharacterDefault(compositor);
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
                workingOutfit = CreateInstance<CharacterOutfit>();

            images.Clear();
            currentPosedLayer = posedLayer;
            layerImageLister.itemHeight = (int)(currentCompositor.layerAspectRatio * 200f);
            for (int i = 0; i < posedLayer.textures.Count; i++)
            {
                images.Add(posedLayer.GetTextureAt(i));
            }

            layerImageLister.Refresh();
            layerImageLister.ClearSelection();
        }
        
        private void OnLayerItemSelected(Object targetItem)
        {
            if (!(targetItem is CharacterLayer cl)) 
                return;

            layerList.HighlightItem(targetItem);
            currentLayer = cl;
            OnCompositorItemSelect(currentLayer, currentPose);
        }

        private void OnPoseItemSelected(Object targetItem)
        {
            if (!(targetItem is CharacterPose cp)) 
                return;

            poseList.HighlightItem(targetItem);
            currentPose = cp;
            LoadNewPoseToWorking(currentCompositor);
            OnCompositorItemSelect(currentLayer, currentPose);
        }
        
        private void OnLayerItemClicked(ClickEvent e)
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

    }
}