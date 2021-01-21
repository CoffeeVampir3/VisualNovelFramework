using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.VNCharacter
{
    public class CharacterRuntimeBuilder : MonoBehaviour
    {
        public Character character;
        public CharacterOutfit targetOutfit;
        public List<RawImage> layerImages;
        
        public SceneAction action;
        public Canvas canvas;

        [Button]
        public void TestDisplayOutfit()
        {
            if (character.compositor == null || targetOutfit == null) 
                return;

            DisplayOutfit(targetOutfit);
        }

        private GameObject CreateEmptyGO()
        {
            var newGo = new GameObject();
            newGo.transform.SetParent(transform);
            newGo.transform.localPosition = Vector3.zero;
            newGo.transform.localScale = Vector3.one;
            return newGo;
        }

        private void DisplayOutfit(CharacterOutfit co)
        {
            layerImages.Clear();
            
            var outfitLayers = co.GetOutfitLayers();
            outfitLayers.ForEach( e => DisplayOutfitLayer(co, e));

            RectTransform rt = transform as RectTransform;
            transform.localScale = action.transform.scale;
        }

        private void DisplayOutfitLayer(CharacterOutfit co, CharacterLayer cl)
        {
            var textures= co.GetLayerTextures(cl);
            textures.ForEach((e) => DisplayOutfitTextures(cl, e));
        }

        private void DisplayOutfitTextures(CharacterLayer layer, Texture2D tex)
        {
            var go = CreateEmptyGO();
            go.name = layer.name;
            var raw = go.AddComponent<RawImage>();
            layerImages.Add(raw);
            raw.rectTransform.sizeDelta =
                new Vector2(
                    512,
                    512 * character.compositor.layerAspectRatio);
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