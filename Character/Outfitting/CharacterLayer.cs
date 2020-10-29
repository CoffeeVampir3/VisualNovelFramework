using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VisualNovelFramework.Outfitting
{
    [CreateAssetMenu]
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

        public void SerializeRecursive(CharacterCompositor saveTo)
        {
            var clone = Instantiate(this);
            clone.name = this.name;
            AssetDatabase.AddObjectToAsset(clone, saveTo);
            saveTo.layers.Add(clone);

            saveTo.layerSerializationDict.Add(this, clone);
        }
    }
}
