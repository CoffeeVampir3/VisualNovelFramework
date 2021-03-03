using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Search_Window;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Settings;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.GraphFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public abstract partial class CoffeeGraphView : GraphView
    {
        [SerializeReference] public BaseNode rootNode;
        [SerializeReference] public readonly GraphSettings settings;
        protected readonly NavigationBlackboard navBlackboard;
        protected readonly PropertyBlackboard propBlackboard;
        protected readonly CoffeeSearchWindow searchWindow;
        public CoffeeGraphWindow parentWindow;

        protected CoffeeGraphView()
        {
            settings = GraphSettings.CreateOrGetSettings(this);
            styleSheets.Add(settings.graphViewStyle);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            navBlackboard = new NavigationBlackboard(this);
            //Add(navBlackboard);

            propBlackboard = new PropertyBlackboard(this);
            Add(propBlackboard);

            //These callbacks are derived from graphView.
            //Callback on cut/copy.
            serializeGraphElements = OnSerializeGraphElements;
            //Callback on paste.
            unserializeAndPaste = DeserializeElementsOnPaste;
            //Callback on "changes" particularly on element delete.
            graphViewChanged = OnGraphViewChanged;

            searchWindow = ScriptableObject.CreateInstance<CoffeeSearchWindow>();
            InitializeSearchWindow();
        }
        
        /// <summary>
        /// Called once the graph view is resized to the editor window and all geometry has been
        /// calculated. (Internally, this is called after a GeometryChangedEvent)
        /// </summary>
        public abstract void OnCreateGraphGUI();

        private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
        {
            //Checks for changes related to our nodes.
            if (changes.elementsToRemove != null)
            {
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
                        case Edge e:
                            OnEdgeDelete(e);
                            break;
                    }
                }
            }

            if (changes.edgesToCreate == null) 
                return changes;

            foreach (var edge in changes.edgesToCreate)
            {
                CreateConnectionFromEdge(edge);
            }

            return changes;
        }
        
        //Thanks @Mert Kirimgeri
        private void InitializeSearchWindow()
        {
            searchWindow.Init(this);
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
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
            navBlackboard.RequestRefresh();
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
            navBlackboard.RequestRefresh();
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
                navBlackboard.RequestRefresh();
            }
            else
            {
                AddNodeToStack(changedStack, changedNode);
                navBlackboard.RequestRefresh();
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
            navBlackboard.RequestRefresh();
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
            navBlackboard.RequestRefresh();
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

        public void CreateNode(Type nodeType, Vector2 contextSpawnPos)
        {
            //Thanks to Mert Kirimgeri "UNITY DIALOGUE GRAPH TUTORIAL - Variables and Search Window"
            //https://www.youtube.com/watch?v=F4cTWOxMjMY
            Vector2 spawnPosition = parentWindow.rootVisualElement.ChangeCoordinatesTo(
                parentWindow.rootVisualElement.parent,
                contextSpawnPos - parentWindow.position.position);

            spawnPosition = contentViewContainer.WorldToLocal(spawnPosition);

            Vector2 nodeSize;
            if (typeof(BaseNode).IsAssignableFrom(nodeType))
            {
                var node = SafeActivatorHelper.LoadArbitrary<BaseNode>(nodeType);
                node.Initialize(nodeType.Name);
                nodeSize = new Vector2(300, 300);
                AddNodeAt(node, new Rect(spawnPosition.x - (nodeSize.x * scale / 2), 
                    spawnPosition.y - (nodeSize.y * scale / 2), 
                    nodeSize.x, nodeSize.y));
            }
            else if (typeof(BaseStackNode).IsAssignableFrom(nodeType))
            {
                BaseStackNode stack = SafeActivatorHelper.LoadArbitrary<BaseStackNode>(nodeType);
                stack.Initialize(nodeType.Name);
                nodeSize = new Vector2(150, 150);
                stack.SetPosition(
                    new Rect(spawnPosition.x - (nodeSize.x * scale / 2), 
                        spawnPosition.y - (nodeSize.y * scale / 2), 
                        nodeSize.x, nodeSize.y));
                AddStackNode(stack);
            }
        }

        #endregion
        
        #region Edges
        
        private void CreateConnectionFromEdge(Edge edge)
        {
            if (edge.input.node is BaseNode inputSide && 
                edge.output.node is BaseNode outputSide)
            {
                inputSide.ConnectPortTo(edge.input, outputSide, edge.output);
            }
        }

        public void OnEdgeDelete(Edge edge)
        {
            if (edge.input.node is BaseNode inputSide && 
                edge.output.node is BaseNode outputSide)
            {
                inputSide.DisconnectPortFrom(edge.input, outputSide, edge.output);
            }
        }
        
        #endregion
        
        #region Helper Functions

        /// <summary>
        /// Centers the graph view onto target node.
        /// </summary>
        /// <param name="node"></param>
        public void LookAtNode(Node node)
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

        #region Event Handling

        public override void HandleEvent(EventBase evt)
        {
            //Prevents the root node from being copied/deleted/weird shit
            if (evt is ExecuteCommandEvent)
            {
                if (this.selection.Contains(rootNode))
                {
                    this.selection.Remove(rootNode);
                }
            }
            base.HandleEvent(evt);
        }
        
        #endregion
        
        #region Default Connection Edge Rules
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compPorts = new List<Port>();

            foreach (var port in ports)
            {
                if (startPort == port || startPort.node == port.node) continue;
                if (startPort.portType != port.portType) continue;
                compPorts.Add(port);
            }

            return compPorts;
        }
        
        #endregion
    }
}
