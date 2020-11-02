using UnityEngine;

namespace VisualNovelFramework.Editor.Elements
{
    public class SearchForTypeButton<T> : DynamicButton where T : Object
    {
        private readonly System.Action<Object> pickeredAction = null;

        public SearchForTypeButton(System.Action<Object> onSearchPick, string text)
        {
            pickeredAction = onSearchPick;
            button = DynamicButtonFactory.CreateDefaultDynamicListButton(text);
            button.clicked += OnButtonClicked;
        }

        public void OnButtonClicked()
        {
            var p = new SearcherPopup<T>(pickeredAction);

            p.Popup();
        }
    }
}