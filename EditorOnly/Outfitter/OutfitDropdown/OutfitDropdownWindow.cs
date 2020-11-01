using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.Elements.Utils;
using VisualNovelFramework.Outfitting;

namespace VisualNovelFramework.Outfitter
{
    public class OutfitDropdownWindow : EditorWindow
    {
        private const string windowXMLPath =
            "Assets/VisualNovelFramework/EditorOnly/Outfitter/OutfitDropdown/OutfitDropdownWindow.uxml";

        public PreviewSearchableBrowser browser;

        public static void ShowExample()
        {
            var wnd = GetWindow<OutfitDropdownWindow>();
            wnd.titleContent = new GUIContent("dropdownWindow");
        }

        public void OnEnable()
        {
            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(windowXMLPath);
            VisualElement labelFromUXML = visualTree.Instantiate();
            root.Add(labelFromUXML);

            browser = root.Q<PreviewSearchableBrowser>();
            browser.BindPreviewFactory(100, 100, OutfitPreviewFactory);
        }

        private List<Texture2D> OutfitPreviewFactory(Object o)
        {
            if (o != null && o is CharacterOutfit co) return co.GetOutfitTextures();
            return null;
        }
    }
}