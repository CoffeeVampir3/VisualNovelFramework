using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Properties;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public abstract partial class CoffeeGraphView
    {
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Debug Property", CreateProperty);
            evt.menu.AppendSeparator();
            
            base.BuildContextualMenu(evt);
        }

        public void CreateProperty(DropdownMenuAction dma)
        {
            BasePropertyNode bpNode = new BasePropertyNode();
            bpNode.Initialize("woo");
            AddNodeAt(bpNode, new Rect(0, 0, 300, 300));
        }
    }
}