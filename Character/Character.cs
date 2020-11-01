using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.Outfitting;

namespace VisualNovelFramework
{
    public class Character : ScriptableObject, HasCoffeeGUID
    {
        public CharacterCompositor compositor;
        public List<CharacterOutfit> outfits = new List<CharacterOutfit>();
        [SerializeField]
        private string characterGUID = "";

        public string GetCoffeeGUID()
        {
            return characterGUID;
        }

        public void SetCoffeeGUID(string GUID)
        {
            characterGUID = GUID;
        }

        public void InitializeChar(string charName)
        {
            compositor = CreateInstance<CharacterCompositor>();
            name = charName;
            characterGUID = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Because we clone our assets on save, we must ensure the references for our outfits
        /// are also updated. Depending on if the user made changes to the file, this could potentially
        /// fail to serialize certain outfits if they're not valid anymore IE if the layer or pose was
        /// removed from the character.
        /// </summary>
        private void ReserializeOutfits()
        {
            var newOutfits = new List<CharacterOutfit>(outfits.Count);
            foreach (var outfit in outfits)
            {
                if (outfit == null)
                    continue;

                var clone = outfit.UpdateSerializationReferences(this);
                if(clone != null)
                    newOutfits.Add(clone);
            }
            
            outfits = newOutfits;
        }

        public Character Serialize(bool saveAs = false)
        {
            var charAsset = CoffeeAssetDatabase.
                FindAssetWithCoffeeGUID<Character>(this.characterGUID);
            if(saveAs || charAsset == null) 
                return CreateAsAsset();
            
            return SaveExisting(charAsset);
        }

        private Character SaveExisting(Character characterAsset)
        {
            CoffeeAssetDatabase.CleanAllSubAssets(characterAsset);
            
            AssetDatabase.StartAssetEditing();
            try
            {
                //Copy the current compositor data to the old asset.
                this.compositor = compositor.SerializeRecursive(characterAsset);
                //Updates any outfits 
                characterAsset.ReserializeOutfits();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            AssetDatabase.SaveAssets();

            return characterAsset;
        }

        private Character CreateAsAsset()
        {
            var clone = CoffeeAssetDatabase.ClonedSaveAs(this);

            AssetDatabase.StartAssetEditing();
            try
            {
                //Clones and saves each item
                compositor.SerializeRecursive(clone);
                //Updates any outfits 
                clone.ReserializeOutfits();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            AssetDatabase.SaveAssets();
            return clone;
        }
        
    }
}