using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace VisualNovelFramework.Editor.Elements
{
    public class PureImage : VisualElement
    {
        #region UXML

        [Preserve]
        public new class UxmlFactory : UxmlFactory<PureImage, UxmlTraits>
        {
        }

        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        #endregion
    }
}