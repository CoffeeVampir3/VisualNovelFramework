using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.Outfitting;

namespace VisualNovelFramework.EditorOnly.CharacterSerializer
{
    public static class OutfitSerializer
    {
        public static void SerializeToCharacter(Character saveTo, CharacterOutfit outfit)
        {
            var oldAsset = CoffeeAssetDatabase.SaveTo(outfit, saveTo);
            if (saveTo.outfits.Contains(oldAsset))
                saveTo.outfits.Remove(oldAsset);
            if (!saveTo.outfits.Contains(outfit))
                saveTo.outfits.Add(outfit);

            EditorUtility.SetDirty(outfit);
            EditorUtility.SetDirty(saveTo);
        }

        public static void DeleteFromCharacter(Character character, CharacterOutfit outfit)
        {
            var outfitAsset = CoffeeAssetDatabase.DeleteSubAssetFrom(outfit, character);

            character.outfits.Remove(outfitAsset);
        }

        public static CharacterOutfit UpdateSerializationReferences(Character saveTo, CharacterOutfit outfit)
        {
            var newUtilLayers = new HashSet<CharacterLayer>();
            var newLayerDict = new Dictionary<CharacterLayer, List<Texture2D>>();
            var compositor = saveTo.compositor;

            if (!outfit.poseToUtilized.TryGetValue(outfit.outfitPose, out var utilizedLayers))
            {
                utilizedLayers = new HashSet<CharacterLayer>();
                outfit.poseToUtilized.Add(outfit.outfitPose, utilizedLayers);
            }

            foreach (var cl in utilizedLayers)
                if (CompositorSerializer.posedLayerSerializationDict.TryGetValue(cl, out var newLayer))
                {
                    var textures = outfit.outfitDictionary[cl];
                    newUtilLayers.Add(newLayer);
                    newLayerDict.Add(newLayer, textures);
                }

            if (newUtilLayers.Count == 0)
                return null;

            if (CompositorSerializer.poseSerializationDict.TryGetValue(outfit.outfitPose, out var newPose))
                outfit.outfitPose = newPose;

            outfit.outfitDictionary = newLayerDict;
            outfit.poseToUtilized.Clear();
            outfit.poseToUtilized.Add(outfit.outfitPose, newUtilLayers);

            AssetDatabase.RemoveObjectFromAsset(outfit);
            AssetDatabase.AddObjectToAsset(outfit, saveTo);

            EditorUtility.SetDirty(outfit);
            EditorUtility.SetDirty(saveTo);

            return outfit;
        }
    }
}