using UnityEngine;

namespace VisualNovelFramework.Editor.Elements
{
    public class CreateNamedItemButton : DynamicButton
    {
        private readonly System.Func<Object> factoryFunction = null;
        private readonly ModularList targetDynamicList;
        private readonly System.Action<Object> onCreateCallback = null;

        public CreateNamedItemButton(ModularList targetList, System.Func<Object> creationFunction,
            System.Action<Object> callback, string text)
        {
            factoryFunction = creationFunction;
            targetDynamicList = targetList;

            onCreateCallback = callback;
            button = DynamicButtonFactory.CreateDefaultDynamicListButton(text);
            button.clicked += OnButtonClicked;
        }

        public void OnButtonClicked()
        {
            if (factoryFunction == null)
                return;

            NameItemPopup();
        }

        private void NameItemPopup()
        {
            var popup = new NamerPopup(CreateNewNamedItem);

            popup.Popup();
        }

        private void CreateNewNamedItem(string newName)
        {
            var newItem = factoryFunction.Invoke();
            if (newItem == null)
            {
                Debug.LogError("Unable to instantiate a pretty list item as the activator function returned null.");
                return;
            }

            newItem.name = newName;

            targetDynamicList.listViewer.itemsSource.Add(newItem);
            targetDynamicList.RefreshList();
            onCreateCallback?.Invoke(newItem);
        }
    }
}