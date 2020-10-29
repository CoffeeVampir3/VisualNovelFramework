using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VisualNovelFramework.Elements.Utils
{
    public class CreateNamedItemButton : DynamicButton
    {
        private readonly ModularList targetDynamicList;
        private readonly Func<Object> factoryFunction = null;
        private Action<Object> onCreateCallback = null;

        public CreateNamedItemButton(ModularList targetList, Func<Object> creationFunction, Action<Object> callback, string text)
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
            NamerPopup popup = new NamerPopup(CreateNewNamedItem);
            
            popup.Popup();
        }

        private void CreateNewNamedItem(string newName)
        {
            Object newItem = factoryFunction.Invoke();
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