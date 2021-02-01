using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Serialization;

namespace VisualNovelFramework.DialogueGraph
{
    public partial class DialogueGraph
    {
        private string debugFileName = "default";
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            
            var fileName = new TextField("File Name: ");
            fileName.SetValueWithoutNotify(currentGraphGUID);
            fileName.MarkDirtyRepaint();
            
            fileName.RegisterValueChangedCallback(
                evt => debugFileName = evt.newValue);
            
            toolbar.Add(new Button( SaveGraph ) {text = "Save"});
            toolbar.Add(new Button( LoadGraph ) {text = "Load"});
            
            toolbar.Add(fileName);
            rootVisualElement.Add(toolbar);
        }
        
        private void SaveGraph()
        {
            GraphSaver.SerializeGraph(graphView);
        }

        private void LoadGraph()
        {
            GraphLoader.LoadGraph(graphView);
        }
        
        private void OnDisable()
        {
            rootVisualElement.Clear();
        }
    }
}