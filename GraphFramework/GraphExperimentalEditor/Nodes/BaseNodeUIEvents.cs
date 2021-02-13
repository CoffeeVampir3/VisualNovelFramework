using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
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

        private void OnTitleDoubleClicked(PointerDownEvent evt)
        {
            Debug.Log("Clik");
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