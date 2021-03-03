using VisualNovelFramework.GraphFramework.Attributes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.DialogueSystem.Nodes
{
    public class RuntimeRootNode : RuntimeNode
    {
        [Out] 
        private ValuePort<Flow> flowPort = new ValuePort<Flow>();
    }
}