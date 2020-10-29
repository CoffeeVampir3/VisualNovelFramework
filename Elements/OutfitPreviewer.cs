using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.Outfitting;

namespace VisualNovelFramework.Elements
{
    public class OutfitPreviewer : VisualElement
    {
        public OutfitPreviewer()
        {
            style.alignContent = new StyleEnum<Align>(Align.Center);
            style.justifyContent = new StyleEnum<Justify>(Justify.Center);
        }
        
        private void DisplayOutfitLayer(CharacterOutfit outfit, CharacterLayer layer, float scalingSize)
        {
            if (!outfit.outfitDictionary.TryGetValue(layer, out var currentLayerItems) 
                || currentLayerItems.Count <= 0 )
                return;
            
            //Single layers can be displayed using the same method because they have only one index.
            foreach (var item in currentLayerItems)
            {
                Image img = new Image {image = item};

                transform.scale = new Vector3(scalingSize, scalingSize, 1f);
                img.AddToClassList("charImgStyle");
                this.Add(img);
            }
        }

        public void DisplayOutfit(CharacterOutfit outfit, float scalingSize = 1f)
        {
            this.Clear();
            foreach (var layer in outfit.utilizedLayers)
            {
                DisplayOutfitLayer(outfit, layer, scalingSize);
            }
        }
    }
}