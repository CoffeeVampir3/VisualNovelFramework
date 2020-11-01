using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace VisualNovelFramework.Outfitting
{
    public class CharacterRuntimeBuilder : MonoBehaviour
    {
        public CharacterCompositor compositor;
        public RawImage[] layerImages;

        [Button]
        public void TestCompositor()
        {
            if (compositor == null) 
                return;
            
            layerImages = new RawImage[compositor.layers.Count];
            for (var i = 0; i < compositor.layers.Count; i++)
            {
                var layer = compositor.layers[i];
                var newGo = new GameObject();
                newGo.transform.SetParent(transform);
                newGo.transform.localPosition = Vector3.zero;
                newGo.transform.localScale = Vector3.one;

                layerImages[i] = newGo.AddComponent<RawImage>();
                layerImages[i].rectTransform.sizeDelta =
                    new Vector2(
                        300,
                        300 * compositor.layerAspectRatio);

                layerImages[i].texture = layer.GetTextureAt(0);
            }
        }

        [Button]
        public void UpdateCompositorLayers()
        {
            for (var i = 0; i < compositor.layers.Count; i++)
            {
                var layer = compositor.layers[i];
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