using VisualNovelFramework.GraphFramework.Attributes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO;

namespace VisualNovelFramework.GraphFramework.GraphRuntime
{
    public class ModelTester : RuntimeNode
    {
        [In]
        public ValuePort<string> stringValue = new ValuePort<string>();
        [In] 
        private ValuePort<Flow> flowPortIn = new ValuePort<Flow>();
        [Out]
        public ValuePort<string> stringValue2 = new ValuePort<string>();
        [Out] 
        private ValuePort<Flow> flowPortOut = new ValuePort<Flow>();

        public int myInt = 5;
    }
}