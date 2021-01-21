using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.VNCharacter;

namespace VisualNovelFramework.Editor.Elements
{
    public class OutfitPreviewer : VisualElement
    {
        public OutfitPreviewer()
        {
            style.alignContent = new StyleEnum<Align>(Align.Center);
            style.justifyContent = new StyleEnum<Justify>(Justify.Center);
            style.position = new StyleEnum<Position>(Position.Absolute);
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

                style.minHeight = item.height;
                style.minWidth = item.width;
                transform.scale = new Vector3(resizeRatio, resizeRatio, 1f);
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