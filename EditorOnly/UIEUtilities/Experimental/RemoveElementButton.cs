using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.Editor.Elements
{
    public class RemoveElementButton : BindingDynamicButton
    {
        private readonly System.Action<Object> onDeleteCallback;
        private readonly ModularList targetDynamicList;

        public RemoveElementButton(ModularList targetList, System.Action<Object> callback, string text)
        {
            targetDynamicList = targetList;
            button = DynamicButtonFactory.CreateDefaultDynamicListButton(text);
            button.RegisterCallback<ClickEvent>(OnRemovePressed);
            onDeleteCallback = callback;
        }

        private void OnRemovePressed(ClickEvent evt)
        {
            if (bindingObject != null && evt.currentTarget is Button)
            {
                boundView.itemsSource.Remove(bindingObject);
                onDeleteCallback?.Invoke(bindingObject);
                targetDynamicList.RefreshList();
                evt.StopImmediatePropagation();
            }
        }

        public override BindingDynamicButton DeepCopy()
        {
            var rmb = new RemoveElementButton(targetDynamicList, onDeleteCallback, button.text);
            return rmb;
        }
    }
}