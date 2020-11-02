using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public partial class BaseGraph
    {
        private string deubgFileName = "default";
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            
            var fileName = new TextField("File Name: ");
            fileName.SetValueWithoutNotify(deubgFileName);
            fileName.MarkDirtyRepaint();
            
            fileName.RegisterValueChangedCallback(
                evt => deubgFileName = evt.newValue);
            
            toolbar.Add(new Button( SaveGraph ) {text = "Save"});
            toolbar.Add(new Button( LoadGraph ) {text = "Load"});
            
            toolbar.Add(fileName);
            rootVisualElement.Add(toolbar);
        }

        private void GenerateBlackboard()
        {
            var blackboard = new NavigationBlackboard(graphView);
            blackboard.SetPosition(new Rect(10, 30, 200, 300));

            graphView.blackboard = blackboard;
            graphView.Add(blackboard);
        }

        private void GenerateMiniMap()
        {
            var miniMap = new MiniMap{anchored = true};
            var spawnCoord = graphView.contentViewContainer.WorldToLocal(
                new Vector2( graphView.contentViewContainer.layout.width - 20, 45));
            miniMap.SetPosition(new Rect(spawnCoord.x, spawnCoord.y, 200, 140));
            graphView.Add(miniMap);
        }

        private void SaveGraph()
        {
            GraphSaver.SerializeGraph(graphView);
        }

        private void LoadGraph()
        {
            GraphLoader.LoadGraph(graphView);
        }
    }
}