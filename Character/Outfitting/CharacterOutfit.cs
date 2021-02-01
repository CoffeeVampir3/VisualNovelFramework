using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;

namespace VisualNovelFramework.VNCharacter
{
    /// <summary>
    ///     Serialization Note::
    ///     Items are going to be serialized as Textures! This means the character layer can drift
    ///     out of synch with the character outfit. When deserializing the outfit we must ensure
    ///     each texture actually exists within the character layer, and, if not, warn the user!
    /// </summary>
    /// Outfits maintain their refs and it's fine to directly ref or access VIA guid.
    public class CharacterOutfit : SerializedScriptableObject, HasCoffeeGUID
    {
        [SerializeField] 
        private string outfitGUID;
        
        [OdinSerialize] 
        public Dictionary<CharacterLayer, List<Texture2D>> outfitDictionary =
            new Dictionary<CharacterLayer, List<Texture2D>>();

        public CharacterPose outfitPose = null;

        [OdinSerialize] 
        public Dictionary<CharacterPose, HashSet<CharacterLayer>> poseToUtilized =
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

        public List<Texture2D> GetOutfitTextures()
        {
            var previewTextures = new List<Texture2D>();
            if (!poseToUtilized.TryGetValue(outfitPose, out var utilizedLayers)) 
                return null;

            foreach (var layer in utilizedLayers)
                if (outfitDictionary.TryGetValue(layer, out var texs))
                    previewTextures.AddRange(texs);

            return previewTextures;
        }

        public IEnumerable<CharacterLayer> GetOutfitLayers()
        {
            if (!poseToUtilized.TryGetValue(outfitPose, out var utilizedLayers)) 
                return null;

            return utilizedLayers;
        }

        public List<Texture2D> GetLayerTextures(CharacterLayer cl)
        {
            if (!poseToUtilized.TryGetValue(outfitPose, out var utilizedLayers)) 
                return null;

            foreach (var layer in utilizedLayers)
            {
                if (layer == cl)
                {
                    if (outfitDictionary.TryGetValue(layer, out var texs))
                    {
                        return texs;
                    }
                }
            }

            return null;
        }

        public void SetLayerDefault(CharacterPose inPose, CharacterLayer layer)
        {
            var tex = layer.GetTextureAt(0);
            outfitPose = inPose;
            if (layer.GetTextureAt(0) != null) AddLayerItem(layer, tex);
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

                if (setDefaults)
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
                if (!layer.isMultilayer)
                    layerItemList.Clear();

                if (!layerItemList.Contains(targetTex))
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
        ///     True if added false if removed.
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

        public List<Texture2D> GetLayerIfNotEmpty(CharacterLayer layer)
        {
            if (!outfitDictionary.TryGetValue(layer, out var layerItems)) return null;

            if (layerItems.Count == 0)
                return null;

            return layerItems;
        }

        public HashSet<CharacterLayer> GetCurrentUtilizedLayers()
        {
            if (!poseToUtilized.TryGetValue(outfitPose, out var utilizedLayers)) return null;
            return utilizedLayers;
        }
    }
}