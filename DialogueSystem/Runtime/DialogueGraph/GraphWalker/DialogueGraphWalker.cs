using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.DialogueSystem.Runtime.DialogueGraph.GraphWalker
{
    public class DialogueGraphWalker : MonoBehaviour
    {
        [SerializeField]
        private RuntimeNode currentNode = null;
        [SerializeField]
        private SerializedGraph testGraph = null;
        
        [Button]
        public RuntimeNode WalkGraphNextNode()
        {
            if (currentNode == null)
            {
                currentNode = testGraph.rootNode;
            }

            currentNode.OnEvaluate();
            currentNode = currentNode.outputConnections.FirstOrDefault();
            return currentNode;
        }

        public void OnEnable()
        {
            currentNode = testGraph.rootNode;
            while (currentNode != null)
            {
                currentNode = WalkGraphNextNode();
            }
        }
    }
}