using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;

namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
 public class BaseStackNode : StackNode, HasCoffeeGUID
    {
        [SerializeField]
        public string GUID;
        protected CoffeeGraphView owner;

        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            bool accept = base.AcceptsElement(element, ref proposedIndex, maxIndex);
            return accept;
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