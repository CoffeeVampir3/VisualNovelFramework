using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Properties
{
    //Forwarding class for type differentiation, but is still a BaseNode.
    public abstract class BasePropertyNode<RuntimeNodeType> : BaseNode<RuntimeNodeType> 
        where RuntimeNodeType : RuntimeNode
    {
        protected override void OnNodeCreation()
        {
        }

        protected override void InstantiatePorts()
        {
        }
    }
}