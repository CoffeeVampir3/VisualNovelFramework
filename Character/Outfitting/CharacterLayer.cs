using System.Collections.Generic;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;

namespace VisualNovelFramework.VNCharacter
{
    public class CharacterLayer : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField]
        public string GUID;
        [SerializeField]
        public bool isMultilayer = false;
        [SerializeField] 
        public List<Texture2D> textures = new List<Texture2D>();

        public Texture2D GetTextureAt(int index)
        {
            if (index < textures.Count)
                return textures[index];

            return null;
        }
        
        public string GetCoffeeGUID()
        {
            return GUID;
        }

        public void SetCoffeeGUID(string newGUID)
        {
            GUID = newGUID;
        }
    }
}