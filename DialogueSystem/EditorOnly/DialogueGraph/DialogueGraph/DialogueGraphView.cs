using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.EditorOnly.DialogueSystem.Nodes;
using VisualNovelFramework.GraphFramework.Editor;
using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.DialogueGraph
{
    public class DialogueGraphView : CoffeeGraphView
    {
        private void CreateGrid()
        {
            var grid = new GridBackground();
            Insert(0, grid);
        }

        public void OnGeometryResizeInitialization()
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
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //We need this so we don't pollute the blackboard menus.
            if (evt.target is GraphView || evt.target is Node)
            {
                evt.menu.AppendAction("Nodes/Character", MenuAddCharacterNode);
                evt.menu.AppendAction("Nodes/Dialogue", MenuAddDialogueNode);
                evt.menu.AppendAction("Nodes/Debug", MenuAddDebugNode);
            }
            
            base.BuildContextualMenu(evt);
        }

        private void MenuAddDialogueNode(DropdownMenuAction act)
        {
            var pos = GetViewRelativePosition(act.eventInfo.mousePosition,
                new Vector2(50, 75));
            
            var spawnPos = new Rect(pos.x, pos.y, 100, 150);
            var node = new DialogueNode();
            node.Initialize("Dialogue Node");
            AddNodeAt(node, spawnPos);
        }

        private void MenuAddDebugNode(DropdownMenuAction act)
        {
            var pos = GetViewRelativePosition(act.eventInfo.mousePosition,
                new Vector2(50, 75));
            
            var spawnPos = new Rect(pos.x, pos.y, 100, 150);
            var node = new BaseStackNode();
            node.SetPosition(spawnPos);
            AddElement(node);
        }

        private void MenuAddCharacterNode(DropdownMenuAction act)
        {
            var pos = GetViewRelativePosition(act.eventInfo.mousePosition,
                new Vector2(50, 75));
            
            var spawnPos = new Rect(pos.x, pos.y, 100, 150);
            var node = new CharacterNode();
            node.Initialize("Character Node");
            AddNodeAt(node, spawnPos);
        }

        #region Events
        
        public override void HandleEvent(EventBase evt)
        {
            //Prevents the root node from being copied/deleted/weird shit
            if (evt is ExecuteCommandEvent)
            {
                if (this.selection.Contains(rootNode))
                {
                    this.selection.Remove(rootNode);
                }
            }
            base.HandleEvent(evt);
        }
        
        #endregion

        #region Literal Garbage

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compPorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node)
                {
                    compPorts.Add(port);
                }
            });
            
            return compPorts;
        }

        #endregion
    }
}
