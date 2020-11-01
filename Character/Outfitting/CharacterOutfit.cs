using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;

namespace VisualNovelFramework.Outfitting
{
    /// <summary>
    /// Serialization Note::
    /// Items are going to be serialized as Textures! This means the character layer can drift
    /// out of synch with the character outfit. When deserializing the outfit we must ensure
    /// each texture actually exists within the character layer, and, if not, warn the user!
    /// </summary>
    public class CharacterOutfit : SerializedScriptableObject, HasCoffeeGUID
    {
        public CharacterPose outfitPose = null;
        [SerializeField]
        private string outfitGUID;

        [OdinSerialize] private Dictionary<CharacterLayer, List<Texture2D>> outfitDictionary =
            new Dictionary<CharacterLayer, List<Texture2D>>();

        [OdinSerialize] private Dictionary<CharacterPose, HashSet<CharacterLayer>> poseToUtilized =
            new Dictionary<CharacterPose, HashSet<CharacterLayer>>();

        public string GetCoffeeGUID()
        {
            return outfitGUID;
        }
        
        public void SetCoffeeGUID(string GUID)
        {
            outfitGUID = GUID;
        }
        
        public void Initialize(string charName)
        {
            name = charName;
            outfitGUID = Guid.NewGuid().ToString();
        }

        public List<Texture2D> GetPreviewTextures()
        {
            List<Texture2D> previewTextures = new List<Texture2D>();
            if (!poseToUtilized.TryGetValue(outfitPose, out var utilizedLayers))
            {
                return null;
            }

            foreach (var layer in utilizedLayers)
            {
                if (outfitDictionary.TryGetValue(layer, out var texs))
                {
                    previewTextures.AddRange(texs);
                }
            }

            return previewTextures;
        }

        public void SetLayerDefault(CharacterPose inPose, CharacterLayer layer)
        {
            var tex = layer.GetTextureAt(0);
            outfitPose = inPose;
            if (layer.GetTextureAt(0) != null)
            {
                AddLayerItem(layer, tex);
            }
        }

        private List<int> FindPoseUnusedDefaultable(CharacterCompositor compositor, bool setDefaults)
        {
            var unusedLayers = new List<int>();
            for (var index = 0; index < compositor.layers.Count; index++)
            {
                var cl = compositor.layers[index];
                var posedLayer = compositor.GetPosedLayer(cl, outfitPose);
                if (posedLayer == null || posedLayer.textures.Count == 0)
                {
                    unusedLayers.Add(index);
                    continue;
                }
                
                if (posedLayer.isMultilayer)
                    continue;
                
                if(setDefaults)
                    SetLayerDefault(outfitPose, posedLayer);
            }

            return unusedLayers;
        }

        public List<int> SwitchPose(CharacterPose inPose, CharacterCompositor compositor)
        {
            outfitPose = inPose;
            
            if (!poseToUtilized.TryGetValue(outfitPose, out var utilizedLayers))
            {
                utilizedLayers = new HashSet<CharacterLayer>();
                poseToUtilized.Add(outfitPose, utilizedLayers);
                return FindPoseUnusedDefaultable(compositor, true);
            }

            return FindPoseUnusedDefaultable(compositor, false);
        }

        private void AddLayerItem(CharacterLayer layer, Texture2D targetTex)
        {
            if (!poseToUtilized.TryGetValue(outfitPose, out var utilizedLayers))
            {
                utilizedLayers = new HashSet<CharacterLayer>();
                poseToUtilized.Add(outfitPose, utilizedLayers);
            }
            
            if (outfitDictionary.TryGetValue(layer, out var layerItemList))
            {
                if(!layer.isMultilayer)
                    layerItemList.Clear();
                
                if(!layerItemList.Contains(targetTex))
                    layerItemList.Add(targetTex);
            }
            else
            {
                outfitDictionary[layer] = new List<Texture2D> {targetTex};
                utilizedLayers.Add(layer);
            }
        }

        public bool AddOrRemoveExistingItem(CharacterLayer layer, int index)
        {
            var tex = layer.GetTextureAt(index);
            if (tex != null)
                return AddOrRemoveExistingItem(layer, tex);
            return false;
        }

        /// <summary>
        /// True if added false if removed.
        /// </summary>
        /// <returns></returns>
        public bool AddOrRemoveExistingItem(CharacterLayer layer, Texture2D targetTex)
        {
            if (!poseToUtilized.TryGetValue(outfitPose, out var utilizedLayers))
            {
                utilizedLayers = new HashSet<CharacterLayer>();
                poseToUtilized.Add(outfitPose, utilizedLayers);
            }
            
            if (outfitDictionary.TryGetValue(layer, out var layerItemList))
            {
                if (!layer.isMultilayer)
                {
                    layerItemList.Clear();
                    
                    if (!layerItemList.Contains(targetTex))
                        layerItemList.Add(targetTex);
                }
                else
                {
                    if (layerItemList.Contains(targetTex))
                    {
                        layerItemList.Remove(targetTex);
                        return false;
                    }
 
                    layerItemList.Add(targetTex);
                }
            }
            else
            {
                outfitDictionary[layer] = new List<Texture2D> {targetTex};
                utilizedLayers.Add(layer);
            }

            return true;
        }

        public CharacterOutfit UpdateSerializationReferences(Character saveTo)
        {
            var newUtilLayers = new HashSet<CharacterLayer>();
            var newLayerDict = new Dictionary<CharacterLayer, List<Texture2D>>();
            CharacterCompositor compositor = saveTo.compositor;
            
            if (!poseToUtilized.TryGetValue(outfitPose, out var utilizedLayers))
            {
                utilizedLayers = new HashSet<CharacterLayer>();
                poseToUtilized.Add(outfitPose, utilizedLayers);
            }

            foreach (var cl in utilizedLayers)
            {
                if(compositor.posedLayerSerializationDict.TryGetValue(cl, out var newLayer))
                {
                    var textures = outfitDictionary[cl];
                    newUtilLayers.Add(newLayer);
                    newLayerDict.Add(newLayer, textures);
                }
            }

            if (newUtilLayers.Count == 0)
                return null;
            
            if(compositor.poseSerializationDict.TryGetValue(outfitPose, out var newPose))
            {
                outfitPose = newPose;
            }
            
            outfitDictionary = newLayerDict;
            poseToUtilized.Clear();
            poseToUtilized.Add(outfitPose, newUtilLayers);

            AssetDatabase.RemoveObjectFromAsset(this);
            AssetDatabase.AddObjectToAsset(this, saveTo);
            return this;
        }

        public void DeleteFromCharacter(Character character)
        {
            var outfitAsset = CoffeeAssetDatabase.
                DeleteSubAssetFrom(this, character);

            character.outfits.Remove(outfitAsset);
        }
        
        public List<Texture2D> GetLayerIfNotEmpty(CharacterLayer layer)
        {
            if(!outfitDictionary.TryGetValue(layer, out var layerItems))
            {
                return null;
            }

            if (layerItems.Count == 0)
                return null;

            return layerItems;
        }

        public HashSet<CharacterLayer> GetCurrentUtilizedLayers()
        {
            if (!poseToUtilized.TryGetValue(outfitPose, out var utilizedLayers))
            {
                return null;
            }
            return utilizedLayers;
        }

        public void SerializeToCharacter(Character saveTo)
        {
            var clone = CoffeeAssetDatabase.ClonedSave(this, saveTo);
            saveTo.outfits.Add(clone);
        }
    }
}