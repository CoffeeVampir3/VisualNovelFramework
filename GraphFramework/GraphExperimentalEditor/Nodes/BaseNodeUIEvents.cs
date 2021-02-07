using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
    public abstract partial class BaseNode
    {
        #region UIElements Event Handling
        
        /// <summary>
        /// Handles removing port connection correctly when a node is deleted.
        /// </summary>
        private void OnNodeDeleted()
        {
            var ports = this.Query<Port>().ToList();
            foreach (Port port in ports)
            {
                var tempConnections = port.connections.ToArray();
                foreach (Edge e in tempConnections)
                {
                    e.input.Disconnect(e);
                    e.output.Disconnect(e);
                    e.parent.Remove(e);
                }
            }
        }

        //Not using geometry change event because it's called too often and causes lag.
        private void UpdateNodePosition()
        {
            editorData.position = GetPosition();
            schedule.Execute(UpdateNodePosition).StartingIn(125);
        }
        
        public override void HandleEvent(EventBase evt)
        {
            //This is called when a node is added to the panel (Instantiation event, basically.)
            if (evt is AttachToPanelEvent)
            {
                //Update our position after a short delay, it will recur automagically.
                var k = schedule.Execute(UpdateNodePosition);
                k.StartingIn(400);
            }
            //This is called when a node is deleted, or "removed" from the panel.
            else if (evt is DetachFromPanelEvent)
            {
                OnNodeDeleted();
            }

            base.HandleEvent(evt);
        }
        
        #endregion
    }
}