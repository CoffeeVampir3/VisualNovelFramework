using System.Collections.Generic;
using UnityEngine;

namespace VisualNovelFramework.Outfitting
{
    public class CharacterLayer : ScriptableObject
    {
        [SerializeField] public List<Texture2D> textures = new List<Texture2D>();
        public bool isMultilayer = false;

        public Texture2D GetTextureAt(int index)
        {
            if (index < textures.Count)
                return textures[index];

            return null;
        }
    }
}
