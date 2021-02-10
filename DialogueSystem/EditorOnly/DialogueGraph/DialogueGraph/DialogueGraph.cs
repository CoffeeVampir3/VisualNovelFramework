using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor;

namespace VisualNovelFramework.DialogueGraph
{
    public partial class DialogueGraph : CoffeeGraph
    {
        [MenuItem("VNFramework/Dialogue Graph")]
        public static void OpenGraph()
        {
            var window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent("C0ff33");
            
            window.Focus();
        }
        
        private void OnEnable()
        {
            if (graphView != null)
            {
                return;
            }
            
            graphView = new DialogueGraphView
            {
                name = "Coffee Dialogue Graph"
            };
            InitializeGraph();
        }

        protected override void OnGraphGUI()
        {
            (graphView as DialogueGraphView).OnGeometryResizeInitialization();
        }
    }
    
}