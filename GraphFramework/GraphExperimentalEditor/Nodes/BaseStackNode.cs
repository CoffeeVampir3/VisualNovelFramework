using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
 public class BaseStackNode : StackNode
    {
        protected CoffeeGraphView owner;
        
        /// <inheritdoc />
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            bool accept = base.AcceptsElement(element, ref proposedIndex, maxIndex);
            
            Debug.Log("Stack node accepted " + element.name);
            return accept;
        }
    }
}