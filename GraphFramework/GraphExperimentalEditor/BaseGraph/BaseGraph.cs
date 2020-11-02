using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public partial class BaseGraph : EditorWindow
    {
        private BaseGraphView graphView;
        
        public static void OpenGraph()
        {
            var window = GetWindow<BaseGraph>();
            window.titleContent = new GUIContent("C0ff33");
            
            window.Focus();
        }

        private void OnEnable()
        {
            graphView = new BaseGraphView
            {
                name = "Coffee Behaviour Graph"
            };
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
            
            graphView.schedule.Execute(ConstructGUIAfterRepaint).StartingIn(100);
        }

        private void ConstructGUIAfterRepaint()
        {
            GenerateToolbar();
            GenerateMiniMap();
            GenerateBlackboard();
        }

        private void OnDisable()
        {
            rootVisualElement.Clear();
        }
    }
    
}
