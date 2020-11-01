using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using VisualNovelFramework.Outfitting;
using Object = UnityEngine.Object;

namespace VisualNovelFramework.Elements.Utils
{
    public class SearchableBrowser : BindableElement
    {
        private const string SearchableBrowserPath =
            "Assets/VisualNovelFramework/EditorOnly/UIEUtilities/SearchableBrowser/SearchableBrowser.uxml";
        private const string SearchableItemPath =
            "Assets/VisualNovelFramework/EditorOnly/UIEUtilities/Elements/DynamicLabelWithIcon.uxml";
        
        private readonly VisualElement root;
        private readonly ToolbarSearchField searcher;
        private ListView listViewer;
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
            DebugList();
            List();
        }

        private VisualTreeAsset listItemProto;
        private void SetupListView()
        {
            listItemProto =
                    AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SearchableItemPath);
            
            listViewer.reorderable = true;
            listViewer.style.alignContent = new StyleEnum<Align>(Align.Center);
            listViewer.itemHeight = 21;
            listViewer.makeItem = () => listItemProto.Instantiate();
        }

        private Func<Object, Texture2D> textureFactory = null;
        public void BindIconFactory(Func<Object, Texture2D> texFac)
        {
            textureFactory = texFac;
        }

        private void DebugList()
        {
            var stuff = AssetDatabase.FindAssets("t:CharacterOutfit");

            foreach (var pathGUID in stuff)
            {
                var path = AssetDatabase.GUIDToAssetPath(pathGUID);
                var o = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var m in o)
                {
                    if (m is CharacterOutfit q)
                    {
                        objects.Add(q);
                    }
                }
            }
            Refresh();
        }

        private List<ScriptableObject> objects = new List<ScriptableObject>();
        private void List()
        {
            workingList = objects;
            listViewer.itemsSource = workingList;
            listViewer.bindItem = BindListItem<ScriptableObject>;
            Refresh();
        }

        private readonly Dictionary<Object, VisualElement> objectItemLink = 
            new Dictionary<Object, VisualElement>();
        private void BindListItem<T>(VisualElement e, int i) where T : Object
        {
            VisualElement ve = e;
            Label itemLabel = ve.Q<Label>("itemLabel");

            T targetObj = listViewer.itemsSource[i] as T;
            SerializedObject so = new SerializedObject(targetObj);
            itemLabel.text = targetObj.name;
            itemLabel.Bind(so);

            if (textureFactory != null)
            {
                VisualElement icon = ve.Q<VisualElement>("icon");
                Texture2D t = textureFactory.Invoke(targetObj);

                if (t != null)
                {
                    icon.style.backgroundImage = new StyleBackground(t);
                }
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

        private List<ScriptableObject> workingList = new List<ScriptableObject>();
        private void FilterList(string searchString)
        {
            workingList = new List<ScriptableObject>();
            foreach (ScriptableObject o in objects)
            {
                if (o.name.Contains(searchString))
                {
                    workingList.Add(o);
                }
            }

            listViewer.itemsSource = workingList;
            Refresh();
        }

        private void OnTextChanged(ChangeEvent<string> change)
        {
            FilterList(change.newValue);
        }
        
        #region UXML
        
        [Preserve]
        public new class UxmlFactory : UxmlFactory<SearchableBrowser, UxmlTraits> { }
   
        [Preserve]
        public new class UxmlTraits : BindableElement.UxmlTraits
        {
        }

        #endregion
    }
}