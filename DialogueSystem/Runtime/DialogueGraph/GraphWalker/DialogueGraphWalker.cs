using System.Linq;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.DialogueSystem.Runtime.DialogueGraph.GraphWalker
{
    public class DialogueGraphWalker
    {
        private RuntimeNode currentNode = null;
        private SerializedGraph testGraph = null;
        
        public RuntimeNode WalkGraphNextNode()
        {
            if (currentNode == null)
            {
                currentNode = testGraph.rootNode;
            }

            currentNode = currentNode.outputConnections.FirstOrDefault();
            return currentNode;
        }
        
        
    }
}