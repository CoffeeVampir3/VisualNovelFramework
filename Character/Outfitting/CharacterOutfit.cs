using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace VisualNovelFramework.Outfitting
{
    /// <summary>
    /// Serialization Note::
    /// Items are going to be serialized as Textures! This means the character layer can drift
    /// out of synch with the character outfit. When deserializing the outfit we must ensure
    /// each texture actually exists within the character layer, and, if not, warn the user!
    /// </summary>
    public class CharacterOutfit : SerializedScriptableObject
    {
        [OdinSerialize]
        public Dictionary<CharacterLayer, List<Texture2D>> outfitDictionary = 
            new Dictionary<CharacterLayer,List<Texture2D>>();
        public HashSet<CharacterLayer> utilizedLayers = new HashSet<CharacterLayer>();
        public CharacterPose outfitPose = null;

        public void ResetOutfit()
        {
            outfitDictionary.Clear();
            utilizedLayers.Clear();
            outfitPose = null;
        }

        public Texture2D GetRandomPreviewTexture()
        {
            var layer = utilizedLayers.ElementAt(0);
            var ggd = outfitDictionary[layer];
            return ggd[0];
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

        public void AddLayerItem(CharacterLayer layer, Texture2D targetTex)
        {
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

        public List<Texture2D> GetLayerItems(CharacterLayer layer)
        {
            return outfitDictionary[layer];
        }

        public CharacterOutfit UpdateSerializationReferences(Character saveTo)
        {
            var newUtilLayers = new HashSet<CharacterLayer>();
            var newLayerDict = new Dictionary<CharacterLayer, List<Texture2D>>();
            CharacterCompositor compositor = saveTo.compositor;

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
            utilizedLayers = newUtilLayers;

            AssetDatabase.RemoveObjectFromAsset(this);
            AssetDatabase.AddObjectToAsset(this, saveTo);
            return this;
        }

        public void SerializeToCharacter(Character saveTo)
        {
            var clone = Instantiate(this);
            saveTo.outfits.Add(clone);
            AssetDatabase.AddObjectToAsset(clone, saveTo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}