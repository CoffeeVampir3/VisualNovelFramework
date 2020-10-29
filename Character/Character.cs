using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VisualNovelFramework.Outfitting;

namespace VisualNovelFramework
{
    [CreateAssetMenu]
    public class Character : ScriptableObject
    {
        public CharacterCompositor compositor;
        public List<CharacterOutfit> outfits = new List<CharacterOutfit>();

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
                
                newOutfits.Add(outfit.UpdateSerializationReferences(this));
            }
            
            outfits = newOutfits;
        }

        public void CreateAsAsset()
        {
            string savePath;
            try
            {
                savePath = EditorUtility.SaveFilePanelInProject(
                    "Save Character",
                    name,
                    "asset", 
                    "Saved Character!");
            }
            catch
            {
                return;
            }

            if (savePath == "")
            {
                return;
            }

            var clone = Instantiate(this);
            AssetDatabase.CreateAsset(clone, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
            AssetDatabase.Refresh();
        }
    }
}