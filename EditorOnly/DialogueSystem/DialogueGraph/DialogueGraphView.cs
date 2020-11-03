using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.EditorOnly.DialogueSystem.Nodes;

namespace VisualNovelFramework.DialogueGraph
{
    public class DialogueGraphView : GraphView
    {
        #region Window Specific

        public DialogueGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //We need this so we don't pollute the blackboard menus.
            if (evt.target is GraphView || evt.target is Node)
            {
                evt.menu.AppendAction("A/Create Node", MenuAddDebugNode);
            }
            
            base.BuildContextualMenu(evt);
        }
        
        private void MenuAddDebugNode(DropdownMenuAction act)
        {
            var pos = GetViewRelativePosition(act.eventInfo.mousePosition,
                new Vector2(50, 75));
            
            var spawnPos = new Rect(pos.x, pos.y, 100, 150);
            var node = new DialogueNode();
            node.Initialize("Dialogue Node");
            node.SetPosition(spawnPos);
            AddElement(node);
        }

        #endregion

        #region Literal Garbage

        private Vector2 GetViewRelativePosition(Vector2 pos, Vector2 offset = default)
        {
            //What the fuck unity. NEGATIVE POSITION???
            Vector2 relPos = new Vector2(
                -viewTransform.position.x + pos.x,
                -viewTransform.position.y + pos.y);

            //Hold the offset as a static value by scaling it in the reverse direction of our scale
            //This way we "undo" the division by scale for only the offset value, scaling everything else.
            relPos -= (offset*scale);
            return relPos/scale;
        }

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
