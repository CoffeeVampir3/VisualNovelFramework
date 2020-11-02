using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.DialogueGraph
{
    public class DialogueGraph : EditorWindow
    {
        private DialogueGraphView graphView;
        private const string assetDir =
            @"Assets/VisualNovelFramework/EditorOnly/DialogueSystem/DialogueGraph/";

        [MenuItem("VNFramework/Dialogue Graph")]
        public static void OpenGraph()
        {
            var window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent("C0ff33");
            
            window.Focus();
        }

        private void OnEnable()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                assetDir + "DialogueGraphWindow.uxml");

            var instance = tree.Instantiate();
            rootVisualElement.Add(instance);

            var flexer = instance.Q<VisualElement>("flexContainer");
            
            graphView = new DialogueGraphView
            {
                name = "Coffee Dialogue Graph"
            };
            flexer.Add(graphView);
            graphView.StretchToParentSize();
        }

        private void OnDisable()
        {
            rootVisualElement.Clear();
        }
    }
    
}