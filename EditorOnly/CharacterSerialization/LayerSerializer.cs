using UnityEditor;
using UnityEngine;
using VisualNovelFramework.Outfitting;

namespace VisualNovelFramework.EditorOnly.CharacterSerializer
{
    public static class LayerSerializer
    {
        public static void SerializeRecursive(CharacterCompositor saveTo, CharacterLayer layer)
        {
            var clone = ScriptableObject.Instantiate(layer);
            clone.name = layer.name;
            AssetDatabase.AddObjectToAsset(clone, saveTo);
            saveTo.layers.Add(clone);

            CompositorSerializer.layerSerializationDict.Add(layer, clone);
        }
    }
}