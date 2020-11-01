using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace VisualNovelFramework.Outfitting
{
    public class CharacterRuntimeBuilder : MonoBehaviour
    {
        public Character character;
        public CharacterOutfit targetOutfit;
        public List<RawImage> layerImages;

        [Button]
        public void TestDisplayOutfit()
        {
            if (character.compositor == null || targetOutfit == null) 
                return;

            DisplayOutfit(targetOutfit);
        }

        public GameObject CreateEmptyGO()
        {
            var newGo = new GameObject();
            newGo.transform.SetParent(transform);
            newGo.transform.localPosition = Vector3.zero;
            newGo.transform.localScale = Vector3.one;
            return newGo;
        }

        private void DisplayOutfit(CharacterOutfit co)
        {
            var outfitLayers = co.GetOutfitLayers();
            outfitLayers.ForEach( e => DisplayOutfitLayer(co, e));
        }

        private void DisplayOutfitLayer(CharacterOutfit co, CharacterLayer cl)
        {
            var textures= co.GetLayerTextures(cl);
            textures.ForEach((e) => DisplayOutfitTextures(cl, e));
        }

        private void DisplayOutfitTextures(CharacterLayer layer, Texture2D tex)
        {
            var go = CreateEmptyGO();
            go.name = ObjectNames.NicifyVariableName(layer.name);
            var raw = go.AddComponent<RawImage>();
            layerImages.Add(raw);
            raw.rectTransform.sizeDelta =
                new Vector2(
                    300,
                    300 * character.compositor.layerAspectRatio);
            raw.texture = tex;
        }

        [Button]
        public void UpdateCompositorLayers()
        {
            for (var i = 0; i < character.compositor.layers.Count; i++)
            {
                var layer = character.compositor.layers[i];
                var currentTex = layer.GetTextureAt(0);
                if (layerImages[i].texture != currentTex)
                {
                    Debug.Log("Updating texture for layer: " + layer.name);
                    layerImages[i].texture = currentTex;
                }
            }
        }
    }
}