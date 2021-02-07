using Sirenix.OdinInspector;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphExecutor;

namespace VisualNovelFramework.DialogueSystem.Runtime.DialogueGraph.GraphWalker
{
    public class DialogueGraphWalker : MonoBehaviour
    {
        [SerializeField] 
        private GraphExecutor executor;

        [Button]
        public void WalkGraphNextNode()
        {
            executor.WalkNode();
        }
    }
}