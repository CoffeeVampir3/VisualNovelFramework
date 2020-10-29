using System;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace VisualNovelFramework.Elements.Utils
{
    public class RemoveElementButton : BindingDynamicButton
    {
        private readonly ModularList targetDynamicList;
        private readonly Action<Object> onDeleteCallback;

        public RemoveElementButton(ModularList targetList, Action<Object> callback, string text)
        {
            targetDynamicList = targetList;
            button = DynamicButtonFactory.CreateDefaultDynamicListButton(text);
            button.RegisterCallback<ClickEvent>(OnRemovePressed);
            onDeleteCallback = callback;
        }

        private void OnRemovePressed(ClickEvent evt)
        {
            if(bindingObject != null && evt.currentTarget is Button) {
                boundView.itemsSource.Remove(bindingObject);
                onDeleteCallback?.Invoke(bindingObject);
                targetDynamicList.RefreshList();
                evt.StopImmediatePropagation();
            }
        }

        public override BindingDynamicButton DeepCopy()
        {
            RemoveElementButton rmb = new RemoveElementButton(targetDynamicList, onDeleteCallback, button.text);
            return rmb;
        }
    }
}