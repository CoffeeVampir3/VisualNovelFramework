using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.DialogueGraph
{
    public partial class DialogueGraph : EditorWindow
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
                name = "Coffee Dialogue Graph"
            };
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);

            graphView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent e)
        {
            GenerateToolbar();
            graphView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
    }
    
}