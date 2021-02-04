using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.VNCharacter;

namespace VisualNovelFramework.DialogueSystem.VNScene
{
    public class CharacterDisplayer : VisualElement
    {
        public CharacterDisplayer()
        {
            AddToClassList("characterDisplay");
        }
        
        private void DisplayOutfitLayer(CharacterOutfit outfit, CharacterLayer layer, bool resize, float targetSize)
        {
            var currentLayerItems = outfit.GetLayerIfNotEmpty(layer);
            if (currentLayerItems == null) return;

            //Single layers can be displayed using the same method because they have only one index.
            foreach (var item in currentLayerItems)
            {
                var img = new Image {image = item};
                
                var resizeRatio = 1f;
                if (resize) resizeRatio = targetSize / item.height;
                
                transform.scale = new Vector3(resizeRatio, resizeRatio, 1f);
                //from CharacterImageStyle.css
                img.AddToClassList("charImgStyle");
                Add(img);
            }
        }

        public void DisplayOutfit(CharacterOutfit outfit, bool resize = false, float targetSize = 0f)
        {
            Clear();

            var utilLayers = outfit.GetCurrentUtilizedLayers();
            if (utilLayers.Count > 0)
            {
                foreach (var layer in outfit.GetCurrentUtilizedLayers())
                    DisplayOutfitLayer(outfit, layer, resize, targetSize);
            }
        }
    }
}