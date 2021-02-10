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

        #endregion
    }
}