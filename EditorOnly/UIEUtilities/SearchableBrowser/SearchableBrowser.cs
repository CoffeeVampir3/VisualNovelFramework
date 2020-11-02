using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
namespace VisualNovelFramework.Editor.Elements
{
    public class SearchableBrowser : BindableElement
    {
        private const string SearchableBrowserPath =
            "Assets/VisualNovelFramework/EditorOnly/UIEUtilities/SearchableBrowser/SearchableBrowser.uxml";

        private const string SearchableItemPath =
            "Assets/VisualNovelFramework/EditorOnly/UIEUtilities/Elements/DynamicLabelWithIcon.uxml";

        private readonly Dictionary<Object, VisualElement> objectItemLink =
            new Dictionary<Object, VisualElement>();

        private readonly VisualElement root;
        private readonly ToolbarSearchField searcher;

        private VisualTreeAsset listItemProto;
        private readonly ListView listViewer;

        private readonly List<ScriptableObject> objects = new List<ScriptableObject>();

        private System.Func<Object, Texture2D> textureFactory = null;

        private List<ScriptableObject> workingList = new List<ScriptableObject>();

        public SearchableBrowser()
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
            List();
        }

        private void SetupListView()
        {
            listItemProto =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SearchableItemPath);

            listViewer.reorderable = true;
            listViewer.style.alignContent = new StyleEnum<Align>(Align.Center);
            listViewer.itemHeight = 21;
            listViewer.makeItem = () => listItemProto.Instantiate();
        }

        private void List()
        {
            workingList = objects;
            listViewer.itemsSource = workingList;
            listViewer.bindItem = BindListItem<ScriptableObject>;
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

            if (textureFactory != null)
            {
                var icon = ve.Q<VisualElement>("icon");
                var t = textureFactory.Invoke(targetObj);

                if (t != null) icon.style.backgroundImage = new StyleBackground(t);
            }

            if (!objectItemLink.ContainsKey(targetObj))
            {
                objectItemLink.Remove(targetObj);
                objectItemLink.Add(targetObj, itemLabel);
            }
        }

        private void Refresh()
        {
            listViewer.Refresh();
        }

        private void FilterList(string searchString)
        {
            workingList = new List<ScriptableObject>();
            foreach (var o in objects)
                if (o.name.Contains(searchString))
                    workingList.Add(o);

            listViewer.itemsSource = workingList;
            Refresh();
        }

        private void OnTextChanged(ChangeEvent<string> change)
        {
            FilterList(change.newValue);
        }

        #region UXML

        [Preserve]
        public new class UxmlFactory : UxmlFactory<SearchableBrowser, UxmlTraits>
        {
        }

        [Preserve]
        public new class UxmlTraits : BindableElement.UxmlTraits
        {
        }

        #endregion
    }
}