using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace VisualNovelFramework.Editor.Elements
{
    public class PreviewSearchableBrowser : BindableElement
    {
        private const string SearchableBrowserPath =
            "Assets/VisualNovelFramework/EditorOnly/UIEUtilities/PreviewedSearchableBrowser/PreviewSearchableBrowser.uxml";

        private const string SearchableItemPath =
            "Assets/VisualNovelFramework/EditorOnly/UIEUtilities/PreviewedSearchableBrowser/PreviewedLabelItem.uxml";

        private const float DefaultTexturePreviewSize = 100f;

        private readonly Dictionary<VisualElement, Object> itemClickLink =
            new Dictionary<VisualElement, Object>();

        private readonly VisualElement root;
        private readonly ToolbarSearchField searcher;

        private VisualTreeAsset listItemProto;

        private IList listReference;
        private readonly ListView listViewer;

        private System.Action<Object> onItemSelected;
        private System.Func<Object, List<Texture2D>> textureFactory = null;
        private float textureHeight = DefaultTexturePreviewSize;
        private float textureWidth = DefaultTexturePreviewSize;
        private IList workingList;

        public PreviewSearchableBrowser()
        {
            var listUxml =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SearchableBrowserPath);

            root = listUxml.Instantiate();
            Add(root);

            var tc = root.Q<TemplateContainer>();
            tc.style.flexGrow = 1f;
            tc.style.flexShrink = 1f; //effectively adding flex: 1

            searcher = root.Q<ToolbarSearchField>("searcher");
            listViewer = root.Q<ListView>("listView");

            searcher.RegisterValueChangedCallback(OnTextChanged);

            SetupListView();
        }

        private void SetupListView()
        {
            listItemProto =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SearchableItemPath);

            listViewer.reorderable = true;
            listViewer.style.alignContent = new StyleEnum<Align>(Align.Center);
            listViewer.itemHeight = 100;
            listViewer.makeItem = () => listItemProto.Instantiate();
        }

        public void BindPreviewFactory(int width, int height, System.Func<Object, List<Texture2D>> texFac)
        {
            textureFactory = texFac;
            textureWidth = width;
            textureHeight = height;

            listViewer.itemHeight = height;
        }

        public void BindToList<T>(List<T> listOfObjects, System.Action<Object> onSelectAction = null) where T : Object
        {
            listReference = listOfObjects;
            workingList = listReference;
            listViewer.itemsSource = workingList;
            onItemSelected = onSelectAction;
            listViewer.bindItem = BindListItem<T>;
            Refresh();
        }

        private void BindListItem<T>(VisualElement e, int i) where T : Object
        {
            var ve = e;
            var itemLabel = ve.Q<Label>("itemLabel");

            var targetObj = listViewer.itemsSource[i] as T;
            var so = new SerializedObject(targetObj);
            itemLabel.text = targetObj.name;
            itemLabel.Bind(so);

            if (itemClickLink.ContainsKey(ve)) itemClickLink.Remove(ve);
            itemClickLink.Add(ve, targetObj);

            var textures = textureFactory?.Invoke(targetObj);
            if (textures == null)
                return;

            ve.RegisterCallback<ClickEvent>(OnItemClicked);

            var previewer = ve.Q<MultitexturePreviewer>("mtexPreviewer");
            previewer.DisplayTextures(textures, textureWidth, textureHeight);
        }

        private void OnItemClicked(ClickEvent evt)
        {
            if (!(evt.currentTarget is VisualElement ve)
                || !itemClickLink.TryGetValue(ve, out var obj))
                return;

            if (evt.clickCount == 1) onItemSelected?.Invoke(obj);
        }

        public void Refresh()
        {
            listViewer.Refresh();
        }

        private void FilterList(string searchString)
        {
            workingList = new List<Object>();
            searchString = searchString.ToLower();
            foreach (Object o in listReference)
                if (o.name.ToLower().Contains(searchString))
                    workingList.Add(o);

            if (listViewer.itemsSource.Count == workingList.Count)
                return;

            listViewer.itemsSource = workingList;
            Refresh();
        }

        private void OnTextChanged(ChangeEvent<string> change)
        {
            FilterList(change.newValue);
        }

        #region UXML

        [Preserve]
        public new class UxmlFactory : UxmlFactory<PreviewSearchableBrowser, UxmlTraits>
        {
        }

        [Preserve]
        public new class UxmlTraits : BindableElement.UxmlTraits
        {
        }

        #endregion
    }
}