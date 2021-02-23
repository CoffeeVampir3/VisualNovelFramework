using VisualNovelFramework.GraphFramework.Attributes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Properties
{
    public class BoolProperty : RuntimeNode
    {
        [Out] 
        protected ValuePort<bool> boolPort = new ValuePort<bool>();
        [Out] 
        protected ValuePort<int> intPort = new ValuePort<int>();
        [In] 
        protected ValuePort<string> stronk = new ValuePort<string>();
        [In] 
        protected ValuePort<string> bonk = new ValuePort<string>();
    }
}