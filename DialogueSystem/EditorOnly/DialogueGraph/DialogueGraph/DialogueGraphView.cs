using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelFramework.EditorOnly.DialogueSystem.Nodes;
using VisualNovelFramework.GraphFramework.Editor;

namespace VisualNovelFramework.DialogueGraph
{
    public class DialogueGraphView : CoffeeGraphView
    {
        private void CreateGrid()
        {
            var grid = new GridBackground();
            Insert(0, grid);
        }

        public override void OnCreateGraphGUI()
        {
            CreateGrid();
            SpawnRootNode();
        }

        private void SpawnRootNode()
        {
            if (rootNode != null) 
                return;
            
            rootNode = new DialogueRoot();
            var width = worldBound.width;
            var height = worldBound.height;
            
            var spawnWidth = (width - 150) / 5;
            var spawnHeight = (height - 75) / 2;
            
            rootNode.Initialize("Root Node");
            AddNodeAt(rootNode, new Rect(spawnWidth, spawnHeight, 150, 150));
        }
    }
}
