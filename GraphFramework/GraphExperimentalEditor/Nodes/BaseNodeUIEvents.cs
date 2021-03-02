using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using VisualNovelFramework.Editor.Elements;

namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
    public abstract partial class BaseNode
    {
        #region UIElements Event Handling
        
        /// <summary>
        /// Handles removing port connection correctly when a node is deleted.
        /// </summary>
        private void OnNodeDeleted(DetachFromPanelEvent panelEvent)
        {
            var ports = this.Query<Port>().ToList();
            var gvParent = panel.visualTree.Q<CoffeeGraphView>();
            if (gvParent == null)
            {
                return;
            }
            //This is a workaround because graphView.DeleteElements throws during event handling...
            foreach (Port port in ports)
            {
                //Important to cast the connections to an array otherwise this will throw
                foreach (Edge edge in port.connections.ToArray())
                {
                    gvParent.OnEdgeDelete(edge);
                    edge.output?.Disconnect(edge);
                    edge.input?.Disconnect(edge);
                    edge.output = null;
                    edge.input = null;
                    edge.parent.Remove(edge);
                }
            }
        }

        private void OnTitleDoubleClicked(PointerDownEvent evt)
        {
            if (evt.clickCount != 2)
                return;
            
            NamerPopup renamerPopup = new NamerPopup(OnTitleRenamed);
            renamerPopup.Popup();
        }

        private void OnTitleRenamed(string newName)
        {
            if (newName == "") 
                return;
            
            title = newName;
            name = newName;
            
            ChangeEvent<string> changeEvent = new ChangeEvent<string>();
            SendEvent(changeEvent);
        }

        #endregion
    }
}