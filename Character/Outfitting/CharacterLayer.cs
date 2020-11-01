using System.Collections.Generic;
using UnityEngine;

namespace VisualNovelFramework.Outfitting
{
    public class CharacterLayer : ScriptableObject
    {
        public bool isMultilayer = false;
        [SerializeField] public List<Texture2D> textures = new List<Texture2D>();

        public Texture2D GetTextureAt(int index)
        {
            if (index < textures.Count)
                return textures[index];

            return null;
        }
    }
}