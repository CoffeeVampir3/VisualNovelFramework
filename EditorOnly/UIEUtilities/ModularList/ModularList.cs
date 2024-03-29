﻿using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace VisualNovelFramework.Editor.Elements
{
    /// <summary>
    ///     Brief functionality overview:
    ///     Binds to a list (BindToList
    ///     <T>
    ///         )
    ///         Creates an item using the method provided to AddItemObjectActivator
    ///         Updates the list only when RefreshList() is called
    ///         Doesn't synch serialized properties, must be supported by save/load.
    ///         Bounds items can be double-clicked to be renamed.
    ///         Added items are automatically prompted to be named.
    ///         Any number of Dynamic Buttons can be added to the Foldout
    ///         Any number of Binding Dynamic Buttons can be added to the list view items.
    /// </summary>
    //TODO:: Support binding when unity adds support for it.
    public class ModularList : BindableElement
    {
        private const string ModularListPath =
            "Assets/VisualNovelFramework/EditorOnly/UIEUtilities/ModularList/ModularList.uxml";

        private const string ModularListItemPath =
            "Assets/VisualNovelFramework/EditorOnly/UIEUtilities/Elements/DynamicLabelWithIcon.uxml";

        private const string highlightUssClassName = "--highlighted";
        private readonly VisualElement buttonContainer;
        private readonly List<BindingDynamicButton> dynamicListItemButtons = new List<BindingDynamicButton>();
        private readonly VisualElement root;

        public ListView listViewer;

        #region Creation

        public ModularList()
        {
            var listUxml =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModularListPath);

            root = listUxml.Instantiate();
            Add(root);

            var tc = root.Q<TemplateContainer>();
            buttonContainer = root.Q<VisualElement>("buttonContainer");

            tc.style.flexGrow = 1f;
            tc.style.flexShrink = 1f;
            foldout = root.Q<Foldout>("ListFoldout");

            SetupListView();
        }

        public void AddFoldoutDynamicButton(DynamicButton dynBtn)
        {
            if (!buttonContainer.Contains(dynBtn.Button)) buttonContainer.Add(dynBtn.Button);
        }

        public void AddListItemDynamicButton(BindingDynamicButton dynBtn)
        {
            dynamicListItemButtons.Add(dynBtn);
        }

        private VisualTreeAsset listItemProto = null;

        private void SetupListView()
        {
            if (listItemProto == null)
                listItemProto =
                    AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModularListItemPath);

            listViewer = root.Q<ListView>("listViewer");
            listViewer.reorderable = true;
            listViewer.style.alignContent = new StyleEnum<Align>(Align.Center);
            listViewer.makeItem = () => listItemProto.Instantiate();
        }

        #endregion

        #region ListManagement

        private System.Action<Object> onItemSelect;

        public void BindToList<T>(List<T> listOfObjects, System.Action<Object> onSelectAction = null) where T : Object
        {
            listViewer.itemsSource = listOfObjects;
            onItemSelect = onSelectAction;
            objectItemLink.Clear();
            labelClickLink.Clear();
            listViewer.bindItem = BindListItem<T>;
            RefreshList();
        }

        public void RefreshList()
        {
            objectItemLink.Clear();
            labelClickLink.Clear();
            listViewer.Refresh();
            foldout.Q<VisualElement>("unity-content").style.minHeight =
                listViewer.itemHeight * listViewer.itemsSource.Count;
        }

        private readonly Dictionary<Object, VisualElement> objectItemLink =
            new Dictionary<Object, VisualElement>();

        private readonly Dictionary<Label, Object> labelClickLink =
            new Dictionary<Label, Object>();

        private void BindListItem<T>(VisualElement e, int i) where T : Object
        {
            var ve = e;
            var itemLabel = ve.Q<Label>("itemLabel");

            var itemContainer = ve.Q<VisualElement>("ItemContainer");

            var targetObj = listViewer.itemsSource[i] as T;
            var so = new SerializedObject(targetObj);
            itemLabel.text = targetObj.name;
            itemLabel.Bind(so);

            if (labelClickLink.ContainsKey(itemLabel)) labelClickLink.Remove(itemLabel);
            labelClickLink.Add(itemLabel, targetObj);

            if (objectItemLink.ContainsKey(targetObj)) objectItemLink.Remove(targetObj);
            objectItemLink.Add(targetObj, itemContainer);

            itemLabel.RegisterCallback<ClickEvent>(OnLabelClicked);

            var itemButtonContainer = itemContainer.Q<VisualElement>("buttonContainer");
            itemButtonContainer.Clear();
            foreach (var btn in dynamicListItemButtons)
            {
                var newBtn = btn.DeepCopy();
                newBtn.Bind(listViewer, targetObj);
                itemButtonContainer.Add(newBtn.Button);
            }
        }

        private VisualElement lastHighlitField = null;

        public void HighlightItem(Object targetItem, bool unhighlightOld = true)
        {
            if (!objectItemLink.TryGetValue(targetItem, out var ve))
                return;

            if (unhighlightOld && lastHighlitField != null)
            {
                lastHighlitField.RemoveFromClassList(highlightUssClassName);
                foreach (var child in lastHighlitField.Children()) child.RemoveFromClassList(highlightUssClassName);
            }

            lastHighlitField = ve;
            ve.AddToClassList(highlightUssClassName);
            foreach (var child in ve.Children()) child.AddToClassList(highlightUssClassName);
        }

        public void SetItemEnabled(Object targetItem, bool enabled)
        {
            if (!objectItemLink.TryGetValue(targetItem, out var ve))
                return;

            ve.SetEnabled(enabled);
        }

        public void SetItemEnabled(int index, bool enabled)
        {
            if (index > listViewer.itemsSource.Count)
                return;

            var m = listViewer.itemsSource[index] as Object;
            SetItemEnabled(m, enabled);
        }

        #endregion

        #region Events

        private Object targetRenameObject = null;

        private void OnLabelClicked(ClickEvent evt)
        {
            if (evt.target is Button
                || !(evt.currentTarget is Label label)
                || !labelClickLink.TryGetValue(label, out var obj))
                return;

            if (evt.clickCount == 1)
            {
                //Select (single click)
                onItemSelect?.Invoke(obj);
            }
            else
            {
                //Rename (double click)
                targetRenameObject = obj;
                var renamerPopup = new NamerPopup(RenameTargetObject);
                renamerPopup.Popup();
                evt.StopImmediatePropagation();
            }
        }

        private void RenameTargetObject(string newName)
        {
            if (targetRenameObject == null)
                return;

            targetRenameObject.name = newName;
            targetRenameObject = null;
            RefreshList();
        }

        #endregion

        #region UXML

        [Preserve]
        public new class UxmlFactory : UxmlFactory<ModularList, UxmlTraits>
        {
        }

        [Preserve]
        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription foldout = new UxmlStringAttributeDescription
                {name = "FoldoutText", defaultValue = "Foldout"};

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as ModularList;

                ate.FoldoutText = foldout.GetValueFromBag(bag, cc);
            }
        }

        private readonly Foldout foldout;

        public string FoldoutText
        {
            get => this.Q<Foldout>("ListFoldout").text;
            set => this.Q<Foldout>("ListFoldout").text = value;
        }

        #endregion
    }
}