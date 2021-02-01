using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor;
using VisualNovelFramework.GraphFramework.GraphRuntime;

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

        [SerializeField]
        private StyleSheet defautGraphStyle = null;
        private void OnEnable()
        {
            graphView = new DialogueGraphView(defautGraphStyle)
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