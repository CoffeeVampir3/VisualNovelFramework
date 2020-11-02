using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.DialogueGraph
{
    public class DialogueGraph : EditorWindow
    {
        private DialogueGraphView graphView;

        [MenuItem("VNFramework/Dialogue Graph")]
        public static void OpenGraph()
        {
            var window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent("C0ff33");
            
            window.Focus();
        }

        private void OnEnable()
        {
            graphView = new DialogueGraphView
            {
                name = "Coffee Behaviour Graph"
            };
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void OnDisable()
        {
            rootVisualElement.Clear();
        }
    }
    
}