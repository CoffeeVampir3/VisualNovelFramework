using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace VisualNovelFramework.Elements.Utils
{
    public class SearchableBrowser : BindableElement
    {
        private readonly VisualElement root;
        private readonly ToolbarSearchField searcher;
        private ListView listViewer;
        public SearchableBrowser()
        {
            var listUxml =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/VisualNovelFramework/UIEUtilities/SearchableBrowser/SearchableBrowser.uxml");


            root = listUxml.Instantiate();
            Add(root);

            var tc = root.Q<TemplateContainer>();

            tc.style.flexGrow = 1f;
            tc.style.flexShrink = 1f;

            searcher = root.Q<ToolbarSearchField>("searcher");
            listViewer = root.Q<ListView>("listView");

            searcher.RegisterValueChangedCallback(OnTextChanged);

            Test();
            ListStrings(stronks);
        }

        private List<string> stronks = new List<string>();
        private void Test()
        {
            stronks.Add("yey");
            stronks.Add("hey");
        }

        private void ListStrings(List<string> strings)
        {
            listViewer.itemsSource = strings;
            listViewer.makeItem = () => new TextField();
            listViewer.itemHeight = 21;
            listViewer.bindItem = (e, i) =>
            {
                TextField tf = e as TextField;
                tf.SetValueWithoutNotify(listViewer.itemsSource[i] as string);
            };
            Refresh();
        }

        private void Refresh()
        {
            listViewer.Refresh();
        }

        private void OnTextChanged(ChangeEvent<string> change)
        {
            stronks.Add(change.newValue);
            Refresh();
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