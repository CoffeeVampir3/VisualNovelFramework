using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.BetaNode;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Search_Window;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Settings;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public abstract class CoffeeGraphView : GraphView
    {
        public readonly GraphSettings settings;
        protected readonly CoffeeSearchWindow searchWindow;
        public CoffeeGraphWindow parentWindow;
        public BetaEditorGraph editorGraph;
        
        private Dictionary<NodeView, NodeModel> viewToModel =
            new Dictionary<NodeView, NodeModel>();

        private Dictionary<Edge, EdgeModel> edgeToModel = 
            new Dictionary<Edge, EdgeModel>();

        /// <summary>
        /// Called once the graph view is resized to the editor window and all geometry has been
        /// calculated. (Internally, this is called after a GeometryChangedEvent)
        /// </summary>
        public abstract void OnCreateGraphGUI();
        
        //TODO::
        #region DeleteThis

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Debug Node Model", CreateNewNode);
            base.BuildContextualMenu(evt);
        }

        /// TODO::
        private void DEBUG__LOAD_GRAPH()
        {
            editorGraph = CoffeeAssetDatabase.FindAssetsOfType<BetaEditorGraph>().FirstOrDefault();
            Undo.ClearAll();
            BuildGraph();
        }

        private void CreateNewNode(DropdownMenuAction dma)
        {
            var model = NodeModel.InstantiateModel(editorGraph);
            CreateNodeFromModel(model);
            
            Undo.RegisterCreatedObjectUndo(model.RuntimeData, "graphChanges");
            Undo.RecordObject(editorGraph, "graphChanges");
            editorGraph.nodeModels.Add(model);
        }

        #endregion

        protected CoffeeGraphView()
        {
            settings = GraphSettings.CreateOrGetSettings(this);
            styleSheets.Add(settings.graphViewStyle);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            //These callbacks are derived from graphView.
            //Callback on cut/copy.
            //serializeGraphElements = OnSerializeGraphElements;
            //Callback on paste.
            //unserializeAndPaste = DeserializeElementsOnPaste;
            //Callback on "changes" particularly on element delete.

            Undo.undoRedoPerformed += UndoPerformed;

            searchWindow = ScriptableObject.CreateInstance<CoffeeSearchWindow>();
            InitializeSearchWindow();

            DEBUG__LOAD_GRAPH();
            
            graphViewChanged = OnGraphViewChanged;
        }

        private void ClearGraph()
        {
            foreach (var elem in graphElements)
            {
                RemoveElement(elem);
            }

            viewToModel.Clear();
        }
        
        private void CreateNodeFromModel(NodeModel model)
        {
            NodeView nv = model.CreateView();
            model.NodeTitle = "wow!";
            AddElement(nv);
            viewToModel.Add(nv, model);
        }
        
        private void CreateEdgeFromModel(EdgeModel model)
        {
            if (model.inputModel?.View == null || model.outputModel?.View == null)
            {
                return;
            }
            if (!model.inputModel.View.TryGetModelToPort(model.inputPortModel.portGUID, out var inputPort) ||
                !model.outputModel.View.TryGetModelToPort(model.outputPortModel.portGUID, out var outputPort))
            {
                return;
            }
            Edge edge = new Edge {input = inputPort, output = outputPort};
            edge.input.Connect(edge);
            edge.output.Connect(edge);
            edgeToModel.Add(edge, model);
            AddElement(edge);
        }

        private void BuildGraph()
        {
            if (editorGraph.nodeModels == null)
                return;
            
            foreach (var model in editorGraph.nodeModels.ToArray())
            {
                CreateNodeFromModel(model);
            }
            
            foreach (var model in editorGraph.edgeModels.ToArray())
            {
                CreateEdgeFromModel(model);
            }
        }

        private void UndoPerformed()
        {
            //Some notes, the order of operation appears to be very specific/finnicky here
            //The undo stack is a bit weird, but this seems to be the only working arrangement of
            //operations.
            
            ClearGraph();

            //There's some issue with the undo stack rewinding the object state and somehow
            //the editor graph can be null for a moment here. It do be like that.
            if (editorGraph == null) return;
            BuildGraph();
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(editorGraph));
        }

        #region Graph Changes Processing

        private void DeleteNode(NodeModel model)
        {
            //Register the state of the saved assets, because we're about to "implicitly" delete some.
            Undo.RegisterImporterUndo(AssetDatabase.GetAssetPath(model.RuntimeData), "graphChanges");
            Undo.DestroyObjectImmediate(model.RuntimeData);
            editorGraph.nodeModels.Remove(model);
        }

        private void DeleteEdge(EdgeModel model)
        {
            editorGraph.edgeModels.Remove(model);
        }
        
        private void ProcessElementMoves(ref List<GraphElement> elements)
        {
            foreach (var elem in elements)
            {
                if (!(elem is NodeView view)) continue;
                if (!viewToModel.TryGetValue(view, out var model)) continue;
                model.UpdatePosition();
            }
        }

        private void ProcessElementRemovals(ref List<GraphElement> elements)
        {
            //Saves the current undo group.
            var index = Undo.GetCurrentGroup();
            foreach (var elem in elements)
            {
                switch(elem)
                {
                    case NodeView view:
                        if(viewToModel.TryGetValue(view, out var nodeModel))
                        {
                            DeleteNode(nodeModel);
                        }
                        break;
                    case Edge edge:
                        if(edgeToModel.TryGetValue(edge, out var edgeModel))
                        {
                            DeleteEdge(edgeModel);
                        }
                        break;
                }
            }
            //Crushes all the delete operations into one undo operation.
            Undo.CollapseUndoOperations(index);
        }

        private void ProcessEdgesToCreate(ref List<Edge> addedEdges)
        {
            foreach (var edge in addedEdges)
            {
                if (!(edge.input.node is NodeView inView &&
                      edge.output.node is NodeView outView)) continue;
                if (!viewToModel.TryGetValue(inView, out var inModel) ||
                    !viewToModel.TryGetValue(outView, out var outModel)) continue;
                if (!inModel.View.TryGetPortToModel(edge.input, out var inputPort) ||
                    !outModel.View.TryGetPortToModel(edge.output, out var outputPort)) continue;
                EdgeModel modelEdge = new EdgeModel(inModel, inputPort, outModel, outputPort);
                edgeToModel.Add(edge, modelEdge);
                editorGraph.edgeModels.Add(modelEdge);
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
        {
            //Save the state before any changes are made
            Undo.RegisterCompleteObjectUndo(editorGraph, "graphChanges");
            if (changes.movedElements != null)
            {
                ProcessElementMoves(ref changes.movedElements);
            }
            
            //Checks for changes related to our nodes.
            if (changes.elementsToRemove != null)
            {
                ProcessElementRemovals(ref changes.elementsToRemove);
            }

            if (changes.edgesToCreate == null) 
                return changes;
            
            ProcessEdgesToCreate(ref changes.edgesToCreate);
            //Bump up the undo increment so we're not undoing multiple change passes at once.
            Undo.IncrementCurrentGroup();

            return changes;
        }
        
        #endregion
        
        //Thanks @Mert Kirimgeri
        private void InitializeSearchWindow()
        {
            searchWindow.Init(this);
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

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
        
        /*

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

*/
        
        /*
        
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

        */
        
        /*
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
        
        */
        
        
        /*
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
        */
    }
}
