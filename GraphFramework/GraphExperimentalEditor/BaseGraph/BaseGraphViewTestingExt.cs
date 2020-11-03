using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public partial class BaseGraphView
    {
        public NavigationBlackboard blackboard;
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            
            //We need this so we don't pollute the blackboard menus.
            if (evt.target is GraphView || evt.target is Node)
            {
                evt.menu.AppendAction("A/Create Node", MenuAddDebugNode);
            }
        }

        private void SpawnRootNode()
        {
            //var root = new RootNode();
            //root.Initialize("Root Node");
            //root.SetPosition(new Rect(550, 350, 150, 150));
            //AddElement(root); 
        }

        private void CreateGrid()
        {
            var grid = new GridBackground();
            Insert(0, grid);
        }

        private void MenuAddDebugNode(DropdownMenuAction act)
        {
            //var pos = GetViewRelativePosition(act.eventInfo.mousePosition,
                //new Vector2(50, 75));
            
            //var spawnPos = new Rect(pos.x, pos.y, 100, 150);
            //var node = new CupNode();
            //node.Initialize("Cup Node");
            //node.SetPosition(spawnPos);
            //AddElement(node);
        }

    }
}