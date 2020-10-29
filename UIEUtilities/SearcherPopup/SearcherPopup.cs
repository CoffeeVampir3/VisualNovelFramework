using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace VisualNovelFramework.Elements.Utils
{
    public class SearcherPopup<T> : VisualElement where T : Object
    {
        private SearcherPopupWindow window = null;
        private ModularList searchResults;
        private Action<Object> onItemPickered;
        
        public SearcherPopup(Action<Object> itemPickeredAction)
        {
            onItemPickered = itemPickeredAction;
        }

        public void Popup()
        {
            SearcherPopupWindow.SetObjectPickerType<T>();
            window = EditorWindow.GetWindow<SearcherPopupWindow>();
            window.Reset();
            SearcherPopupWindow.SetPickerCallback(onItemPickered);
            
            Vector2 scrnCenter = new Vector2(Screen.width /2f, Screen.height/2f);
            window.position = new Rect(scrnCenter.x, scrnCenter.y, 0f, 0f);
            window.maxSize = Vector2.zero;
            window.minSize = Vector2.zero;
            window.Show();
            window.Focus();
        }
        
    }
}