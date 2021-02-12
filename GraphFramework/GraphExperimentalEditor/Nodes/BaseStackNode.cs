using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.EditorExtensions;

namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
 public class BaseStackNode : StackNode, HasCoffeeGUID
    {
        public string GUID;
        protected CoffeeGraphView owner;
        
        public List<BaseNode> nodeList => this.Query<BaseNode>().ToList();

        private readonly Dictionary<System.Type, bool> acceptedElementDictionary
            = new Dictionary<System.Type, bool>();
        
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            return acceptedElementDictionary.TryGetValue(element.GetType(), out _);
        }

        /// <summary>
        /// Keeps track of the first and last node in the stack whenever the
        /// stack order is changed.
        /// </summary>
        private void OnStackOrderChanged(GeometryChangedEvent geoChange)
        {
            var nodesInStack = nodeList;

            if (!nodesInStack.Any())
                return;

            foreach (var node in nodesInStack)
            {
                node.RemoveFromClassList("firstInStack");
                node.RemoveFromClassList("lastInStack");
            }

            var firstNode = nodesInStack.First();
            var lastNode = nodesInStack.Last();
            
            firstNode.AddToClassList("firstInStack");
            lastNode.AddToClassList("lastInStack");
            UnregisterCallback<GeometryChangedEvent>(OnStackOrderChanged);
        }

        public void AddNode(BaseNode node)
        {
            AddElement(node);
            RegisterCallback<GeometryChangedEvent>(OnStackOrderChanged);
        }

        /// <summary>
        /// Called when an item is added to our stack node.
        /// </summary>
        public override bool DragPerform(DragPerformEvent evt, IEnumerable<ISelectable> selection, IDropTarget dropTarget, ISelection dragSource)
        {
            var selectables = selection as ISelectable[] ?? selection.ToArray();
            foreach (var s in selectables)
            {
                if (s is BaseNode bn)
                {
                    Debug.Log("Added element: " + bn.name);
                }
            }

            RegisterCallback<GeometryChangedEvent>(OnStackOrderChanged);
            return base.DragPerform(evt, selectables, dropTarget, dragSource);
        }

        /// <summary>
        /// Called when an item is removed from our stack node.
        /// </summary>
        public override void OnStartDragging(GraphElement ge)
        {
            RegisterCallback<GeometryChangedEvent>(OnStackOrderChanged);
            ge.RemoveFromClassList("firstInStack");
            ge.RemoveFromClassList("lastInStack");
            base.OnStartDragging(ge);
        }

        /// <summary>
        /// Potentially wire this to an attribute instead.
        /// </summary>
        protected void AddCompatibleElement(System.Type type)
        {
            if (!acceptedElementDictionary.TryGetValue(type, out _))
            {
                acceptedElementDictionary.Add(type, true);
            }
        }

        public string GetCoffeeGUID()
        {
            return GUID;
        }

        public void SetCoffeeGUID(string newGuid)
        {
            GUID = newGuid;
        }
    }
}