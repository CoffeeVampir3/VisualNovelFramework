using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace VisualNovelFramework.UIEUtilities.Elements
{
    public class MultitexturePreviewer : VisualElement
    {
        private const string MultitextureCssClass = "--multitextured-image";

        public MultitexturePreviewer()
        {
            style.alignSelf = new StyleEnum<Align>(Align.Center);
            style.alignContent = new StyleEnum<Align>(Align.Center);
            style.justifyContent = new StyleEnum<Justify>(Justify.Center);
        }

        public void DisplayTextures(List<Texture2D> textures, float width, float height)
        {
            Clear();
            foreach (var item in textures)
            {
                var cTex = new Texture2D(item.width, item.height, item.format, true);
                Graphics.CopyTexture(item, cTex);

                var inverseAspect = height / width;
                TextureScaler.scale(cTex, (int) (width * inverseAspect), (int) height);
                var img = new Image {image = cTex};
                img.style.width = width;
                img.style.height = height;
                img.style.maxWidth = width;
                img.style.maxWidth = height;
                img.style.minHeight = width;
                img.style.minWidth = height;

                img.AddToClassList(MultitextureCssClass);
                Add(img);
            }
        }

        #region UXML

        [Preserve]
        public new class UxmlFactory : UxmlFactory<MultitexturePreviewer, UxmlTraits>
        {
        }

        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        #endregion
    }
}