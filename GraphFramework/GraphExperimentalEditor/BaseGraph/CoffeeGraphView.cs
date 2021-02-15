using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Settings;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.GraphFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public abstract class CoffeeGraphView : GraphView
    {
        [SerializeReference] public BaseNode rootNode;
        [SerializeReference] public readonly GraphSettings settings;
        protected readonly NavigationBlackboard blackboard;

        protected CoffeeGraphView()
        {
            settings = GraphSettings.CreateOrGetSettings(this);
            styleSheets.Add(settings.graphViewStyle);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            blackboard = new NavigationBlackboard(this);
            Add(blackboard);
            
            //These callbacks are derived from graphView.
            //Callback on cut/copy.
            serializeGraphElements = OnSerializeGraphElements;
            //Callback on paste.
            unserializeAndPaste = DeserializeElementsOnPaste;
            //Callback on "changes" particularly on element delete.
            graphViewChanged = OnGraphViewChanged;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
        {
            if (changes.elementsToRemove == null)
                return changes;
            
            //Checks for changes related to our nodes.
            foreach (var elem in changes.elementsToRemove)
            {
                switch (elem)
                {
                    case BaseNode bn:
                        OnNodeDelete(bn);
                        break;
                    case BaseStackNode sn:
                        OnStackDelete(sn);
                        break;
                }
            }

            return changes;
        }

        #region Copy and Paste

        /// <summary>
        /// This makes cut & paste so much easier to implement.
        /// </summary>
        [Serializable]
        protected class CopyAndPasteBox
        {
            public List<NodeSerializationData> serializedNodes = new List<NodeSerializationData>();
            public List<StackNodeSerializationData> serializedStacks = new List<StackNodeSerializationData>();
        }

        protected virtual string OnSerializeGraphElements(IEnumerable<GraphElement> selectedItemsToSerialize)
        {
            CopyAndPasteBox box = new CopyAndPasteBox();
            foreach (var elem in selectedItemsToSerialize)
            {
                switch (elem)
                {
                    case BaseNode bn:
                    {
                        //If the node is stacked, skip it, the stack will serialize it.
                        if (bn.ClassListContains("stack-child-element"))
                            continue;
                        NodeSerializationData serialNode = 
                            NodeSerializationData.SerializeFrom(bn);
                        
                        box.serializedNodes.Add(serialNode);
                        break;
                    }
                    case BaseStackNode sn:
                        StackNodeSerializationData stackSerialData = 
                            StackNodeSerializationData.SerializeFrom(sn);
                        
                        box.serializedStacks.Add(stackSerialData);
                        break;
                }
            }

            return JsonUtility.ToJson(box);
        }

        protected virtual void DeserializeElementsOnPaste(string op, string serializationData)
        {
            CopyAndPasteBox box = JsonUtility.FromJson<CopyAndPasteBox>(serializationData);
            if (box == null)
                return;
            
            List<ISelectable> newSelection = new List<ISelectable>();

            //Deserialize each node.
            foreach (var serialNode in box.serializedNodes)
            {
                var node = serialNode.CreateCopyFromSerialization();
                newSelection.Add(node);
                AddNode(node);
            }
            
            //Deserialize all stacks and their children
            foreach (var serializedStack in box.serializedStacks)
            {
                var stackNode = serializedStack.CreateCopyFromSerialization();
                newSelection.Add(stackNode);
                AddStackNode(stackNode);
                foreach (var serialNode in serializedStack.stackedNodes)
                {
                    var node = serialNode.CreateCopyFromSerialization();
                    AddDefaultSettingsToNode(node);
                    stackNode.AddNode(node);
                }
            }

            ClearSelection();
            foreach (var selectedItem in newSelection)
            {
                AddToSelection(selectedItem);
            }
        }
        
        #endregion

        #region Editor/Debugger link
        
        [SerializeReference]
        private BaseNode lastEvaluatedNode = null;
        /// <summary>
        /// TODO:: Temporary code, should have a faster means of accessing nodes.
        /// </summary>
        public void RuntimeNodeVisited(RuntimeNode node)
        {
            if (!(nodes.ToList().FirstOrDefault(e =>
            {
                if (e is BaseNode bn)
                {
                    return bn.RuntimeData == node;
                }

                return false;
            }) is BaseNode dataNode))
                return;

            lastEvaluatedNode?.OnNodeExited();
            dataNode.OnNodeEntered();
            lastEvaluatedNode = dataNode;
        }
        
        #endregion
        
        #region Nodes
        
        /// <summary>
        /// All "free" BaseNodes (not including stack nodes) in the graph which are not stacked.
        /// </summary>
        public readonly List<BaseNode> freeNodes = new List<BaseNode>();
        
        /// <summary>
        /// All stack nodes in the graph.
        /// </summary>
        public readonly List<BaseStackNode> stackNodes = new List<BaseStackNode>();
        
        /// <summary>
        /// Lookup of StackNode -> Child BaseNodes
        /// </summary>
        private readonly Dictionary<BaseStackNode, List<BaseNode>> stackRelations =
            new Dictionary<BaseStackNode, List<BaseNode>>();

        /// <summary>
        /// Lookup of BaseNode -> Parent BaseStackNode
        /// </summary>
        private readonly Dictionary<BaseNode, BaseStackNode> stackedNodeDictionary =
            new Dictionary<BaseNode, BaseStackNode>();

        public bool TryGetStackNodeChildren(BaseStackNode stack, out List<BaseNode> children)
        {
            return stackRelations.TryGetValue(stack, out children);
        }

        public bool IsNodeStacked(BaseNode bn)
        {
            return stackedNodeDictionary.TryGetValue(bn, out _);
        }

        public List<Node> GetNodes()
        {
            List<Node> listOfNodes = new List<Node>();
            listOfNodes.AddRange(freeNodes);
            listOfNodes.AddRange(stackNodes);
            return listOfNodes;
        }

        public List<Node> GetNodesOrdered()
        {
            List<Node> orderedNodes = new List<Node>();
            orderedNodes.AddRange(freeNodes);
            foreach (var stack in stackNodes)
            {
                orderedNodes.Add(stack);
                if(stackRelations.TryGetValue(stack, out var stackedNodes))
                {
                    orderedNodes.AddRange(stackedNodes);
                }
            }
            return orderedNodes;
        }

        /// <summary>
        /// A callback launched whenever a node in the graph is renamed. Currently used to update
        /// the blackboard so it only repaints when there's a change.
        /// </summary>
        protected virtual void OnNodeNameChanged(ChangeEvent<string> changeEvent)
        {
            blackboard.RequestRefresh();
        }

        protected virtual void OnNodeDelete(BaseNode node)
        {
            if (freeNodes.Contains(node))
            {
                freeNodes.Remove(node);
            }
            else
            {
                RemoveNodeFromStack(node);
            }
            blackboard.RequestRefresh();
        }

        protected virtual void OnStackDelete(BaseStackNode stack)
        {
            if (!stackRelations.TryGetValue(stack, out var relations))
                return;
            
            foreach (var node in relations)
            {
                if(stackedNodeDictionary.TryGetValue(node, out _))
                    stackedNodeDictionary.Remove(node);
            }

            stackRelations.Remove(stack);
            if (stackNodes.Contains(stack))
            {
                stackNodes.Remove(stack);
            }
        }

        public virtual void OnStackChanged(BaseStackNode changedStack, BaseNode changedNode, bool added)
        {
            if (!added)
            {
                RemoveNodeFromStack(changedNode);
                freeNodes.Add(changedNode);
                blackboard.RequestRefresh();
            }
            else
            {
                AddNodeToStack(changedStack, changedNode);
                blackboard.RequestRefresh();
            }
        }

        private void RemoveNodeFromStack(BaseNode bn)
        {
            if (!stackedNodeDictionary.TryGetValue(bn, out var stack))
            {
                return;
            }
            
            if(!stackRelations.TryGetValue(stack, out var stackedNodes))
            {
                stackedNodes = new List<BaseNode>();
                stackRelations[stack] = stackedNodes;
                return;
            }

            if (stackedNodes.Contains(bn))
            {
                stackedNodes.Remove(bn);
                stackedNodeDictionary.Remove(bn);
            }
        }

        private void AddNodeToStack(BaseStackNode stack, BaseNode bn)
        {
            if(!stackRelations.TryGetValue(stack, out var stackedNodes))
            {
                stackedNodes = new List<BaseNode>();
                stackRelations[stack] = stackedNodes;
            }
            stackedNodes.Add(bn);
            stackedNodeDictionary.Add(bn, stack);

            if (freeNodes.Contains(bn))
            {
                freeNodes.Remove(bn);
            }
        }

        /// <summary>
        /// Nodes should be created using AddNoteAt with a rect targeting where on the graph
        /// they should be spawned.
        /// </summary>
        public void AddNode(BaseNode node)
        {
            freeNodes.Add(node);

            AddDefaultSettingsToNode(node);
            AddElement(node);
            
            node.RegisterCallback<ChangeEvent<string>>(OnNodeNameChanged);
            blackboard.RequestRefresh();
        }

        /// <summary>
        /// Node is created on the graph with the given coordinates.
        /// </summary>
        protected void AddNodeAt(BaseNode node, Rect position)
        {
            AddNode(node);
            node.SetPosition(position);
        }
        
        /// <summary>
        /// Adds a stack node to the graph.
        /// </summary>
        public void AddStackNode(BaseStackNode node)
        {
            stackNodes.Add(node);
            AddDefaultSettingsToNode(node);
            AddElement(node);
            node.RegisterCallback<ChangeEvent<string>>(OnNodeNameChanged);
            blackboard.RequestRefresh();
        }

        public void AddDefaultSettingsToNode(Node node)
        {
            switch (node)
            {
                case BaseNode bn:
                    bn.styleSheets.Add(settings.nodeStyle);
                    break;
                case BaseStackNode sn:
                    sn.styleSheets.Add(settings.stackNodeStyle);
                    break;
            }
        }

        #endregion
        
        #region Helper Functions

        public void CenterViewOnAndSelectNode(Node node)
        {
            var halfGraphWidth = resolvedStyle.width / 2;
            var halfGraphHeight = resolvedStyle.height / 2;

            Rect nodePos;
            if (node.ClassListContains("stack-child-element"))
            {
                //If the node is stacked it... for some reason has no position.
                //This is the only method I could find that actually worked to get a real
                //position out of the damn node.
                var stackNodeParent = node.GetFirstAncestorOfType<BaseStackNode>();
                if (stackNodeParent == null)
                    return;
                nodePos = stackNodeParent.GetPosition();
            }
            else
            {
                nodePos = node.GetPosition();
            }

            //Unsure why position is flipped but this is at least consistent across graph view.
            var nodeX = -nodePos.center.x * this.scale;
            var nodeY = -nodePos.center.y * this.scale;
            
            Vector2 centeredPosition = new Vector2(nodeX + halfGraphWidth,
                nodeY + halfGraphHeight);
            
            viewTransform.position = centeredPosition;
        }
        
        protected Vector2 GetViewRelativePosition(Vector2 pos, Vector2 offset = default)
        {
            //What the fuck unity. NEGATIVE POSITION???
            Vector2 relPos = new Vector2(
                -viewTransform.position.x + pos.x,
                -viewTransform.position.y + pos.y);

            //Hold the offset as a static value by scaling it in the reverse direction of our scale
            //This way we "undo" the division by scale for only the offset value, scaling everything else.
            relPos -= (offset * scale);
            return relPos / scale;
        }
        
        #endregion
    }
}
