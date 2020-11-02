using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.Editor.Elements
{
    public class SearcherPopup<T> : VisualElement where T : Object
    {
        private readonly System.Action<Object> onItemPickered;
        private ModularList searchResults;
        private SearcherPopupWindow window = null;

        public SearcherPopup(System.Action<Object> itemPickeredAction)
        {
            onItemPickered = itemPickeredAction;
        }

        public void Popup()
        {
            SearcherPopupWindow.SetObjectPickerType<T>();
            window = EditorWindow.GetWindow<SearcherPopupWindow>();
            window.Reset();
            SearcherPopupWindow.SetPickerCallback(onItemPickered);

            var scrnCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            window.position = new Rect(scrnCenter.x, scrnCenter.y, 0f, 0f);
            window.maxSize = Vector2.zero;
            window.minSize = Vector2.zero;
            window.Show();
            window.Focus();
        }
    }
}