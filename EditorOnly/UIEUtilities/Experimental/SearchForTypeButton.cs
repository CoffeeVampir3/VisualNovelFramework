using System;
using Object = UnityEngine.Object;

namespace VisualNovelFramework.Elements.Utils
{
    public class SearchForTypeButton<T> : DynamicButton where T : Object
    {
        private readonly Action<Object> pickeredAction = null;

        public SearchForTypeButton(Action<Object> onSearchPick, string text)
        {
            pickeredAction = onSearchPick;
            button = DynamicButtonFactory.CreateDefaultDynamicListButton(text);
            button.clicked += OnButtonClicked;
        }

        public void OnButtonClicked()
        {
            SearcherPopup<T> p = new SearcherPopup<T>(pickeredAction);
            
            p.Popup();
        }
    }
}