using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.DialogueGraph
{
    public partial class DialogueGraph : EditorWindow
    {
        private DialogueGraphView graphView;
        private string currentGraphGUID = "";

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

            if (currentGraphGUID == "")
            {
                currentGraphGUID = Guid.NewGuid().ToString();
            }

            graphView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent e)
        {
            GenerateToolbar();
            graphView.OnGeometryResizeInitialization();
            graphView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
    }
    
}