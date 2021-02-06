using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.VNCharacter;

namespace VisualNovelFramework.Editor.Serialization
{
    /// <summary>
    ///     Full serialization suite for the VN Character, we're doing this weird shit so we can
    ///     save as without unity exploding.
    /// </summary>
    public static class CharacterSerializer
    {
        public static void InitializeChar(Character character, string charName)
        {
            character.compositor = ScriptableObject.CreateInstance<CharacterCompositor>();
            character.name = charName;
            character.SetCoffeeGUID(Guid.NewGuid().ToString());
        }

        /// <summary>
        ///     Because we clone our assets on save, we must ensure the references for our outfits
        ///     are also updated. Depending on if the user made changes to the file, this could potentially
        ///     fail to serialize certain outfits if they're not valid anymore IE if the layer or pose was
        ///     removed from the character.
        /// </summary>
        private static void ReserializeOutfits(Character character)
        {
            var newOutfits = new List<CharacterOutfit>(character.outfits.Count);
            foreach (var outfit in character.outfits)
            {
                if (outfit == null)
                    continue;

                var clone = OutfitSerializer.UpdateSerializationReferences(character, outfit);
                if (clone != null)
                    newOutfits.Add(clone);
            }

            character.outfits = newOutfits;
        }

        public static Character Serialize(Character character, bool saveAs = false)
        {
            var charAsset = CoffeeAssetDatabase.FindAssetWithCoffeeGUID<Character>(character.GetCoffeeGUID());
            if (saveAs || charAsset == null)
            {
                charAsset = CreateAsAsset(character);
            }
            else
            {
                charAsset = SaveExisting(character, charAsset);
            }

            return charAsset;
        }

        private static Character SaveExisting(Character original, Character characterAsset)
        {
            CoffeeAssetDatabase.CleanAllSubAssets(characterAsset);

            AssetDatabase.StartAssetEditing();
            try
            {
                //Copy the current compositor data to the old asset.
                original.compositor = CompositorSerializer.SerializeRecursive(characterAsset, original.compositor);
                //Updates any outfits 
                ReserializeOutfits(characterAsset);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(original);

            return characterAsset;
        }

        private static Character CreateAsAsset(Character character)
        {
            var clone = CoffeeAssetDatabase.ClonedSaveAs(character);

            AssetDatabase.StartAssetEditing();
            try
            {
                //Clones and saves each item
                CompositorSerializer.SerializeRecursive(clone, character.compositor);

                //Updates any outfits or clear them if this is a clone.
                if (clone.GetCoffeeGUID() == character.GetCoffeeGUID())
                    ReserializeOutfits(clone);
                else
                    clone.outfits.Clear();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(character);
            return clone;
        }
    }
}