using UnityEditor;
using UnityEngine;

using VisualNovelFramework.GraphFramework.Editor;

namespace VisualNovelFramework.DialogueGraph
{
    public partial class DialogueGraphWindow : CoffeeGraphWindow
    {
        [MenuItem("VNFramework/Dialogue Graph")]
        public static void OpenGraph()
        {
            var window = GetWindow<DialogueGraphWindow>();
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
    }
    
}