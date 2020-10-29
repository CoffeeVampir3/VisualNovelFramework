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
            style.alignContent = new StyleEnum<Align>(Align.Center);
            style.justifyContent = new StyleEnum<Justify>(Justify.Center);
        }

        public void DisplayTextures(List<Texture2D> textures, float scalingSize = 1f)
        {
            this.Clear();
            foreach (var item in textures)
            {
                Image img = new Image {image = item};
                transform.scale = new Vector3(scalingSize, scalingSize, 1f);
                img.AddToClassList(MultitextureCssClass);
                this.Add(img);
            }
        }
        
        #region UXML
        
        [Preserve]
        public new class UxmlFactory : UxmlFactory<MultitexturePreviewer, UxmlTraits> { }
   
        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        #endregion
    }
}