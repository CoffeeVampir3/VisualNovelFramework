using System;
using System.Linq;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.GraphExecutor
{
    /// <summary>
    /// This class is responsible for executing the graph at runtime, during edit/play
    /// mode this class also creates our editor/graph link.
    /// </summary>
    [Serializable]
    public partial class GraphExecutor
    {
        [SerializeField]
        public SerializedGraph targetGraph;
        [SerializeField]
        public RuntimeNode currentNode = null;

        private Func<bool> isEditorLinkedStub = null;
        private Action<RuntimeNode> runtimeNodeVisitedEditor = null;
        private GraphExecutor()
        {
            #if UNITY_EDITOR
            isEditorLinkedStub = IsEditorLinkedToGraphWindow;
            runtimeNodeVisitedEditor = EditorLinkedRuntimeNodeVisited;
            #endif
        }

        public void WalkNode()
        {
            if (currentNode == null)
            {
                currentNode = targetGraph.rootNode;
            }
            
            if (isEditorLinkedStub != null && isEditorLinkedStub.Invoke())
            {
                runtimeNodeVisitedEditor.Invoke(currentNode);
            }
            
            //TODO::
            //currentNode = currentNode.outputConnections.FirstOrDefault();
        }
    }
}