﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.BetaNode;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Search_Window;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Settings;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public abstract class CoffeeGraphView : GraphView
    {
        protected readonly GraphSettings settings;
        protected readonly CoffeeSearchWindow searchWindow;
        protected BetaEditorGraph editorGraph;

        //Keeps track of all NodeView's and their relation to their model.
        private readonly Dictionary<NodeView, NodeModel> viewToModel =
            new Dictionary<NodeView, NodeModel>();
        //Keeps track of all edges and their relation to their model.
        private readonly Dictionary<Edge, EdgeModel> edgeToModel =
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
        
        #region Copy and Paste 
        
        
        
        #endregion

        private void ClearGraph()
        {
            foreach (var elem in graphElements)
            {
                RemoveElement(elem);
            }

            viewToModel.Clear();
            edgeToModel.Clear();
        }

        #region Graph Building

        private void CreateNodeFromModel(NodeModel model)
        {
            NodeView nv = model.CreateView();
            model.NodeTitle = "wow!";
            AddElement(nv);
            viewToModel.Add(nv, model);
        }

        private void CreateEdgeFromModel(EdgeModel model)
        {
            if (model.inputModel?.View == null || model.outputModel?.View == null ||
                !model.inputModel.View.TryGetModelToPort(model.inputPortModel.portGUID, out var inputPort) ||
                !model.outputModel.View.TryGetModelToPort(model.outputPortModel.portGUID, out var outputPort))
            {
                editorGraph.edgeModels.Remove(model);
                return;
            }

            Edge edge = new Edge {input = inputPort, output = outputPort};
            edge.input.Connect(edge);
            edge.output.Connect(edge);
            AddElement(edge);
            edgeToModel.Add(edge, model);
        }

        private void BindConnections()
        {
            foreach (var conn in editorGraph.connections)
            {
                conn.BindConnection();
            }
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

            BindConnections();
        }

        #endregion

        #region Undo-specific

        /// <summary>
        /// This is a hack to restore the state of ValuePort connections after an undo,
        /// long story short the undo system does not preserve their state, so we need
        /// to essentially rebuild them to match the current graph state which was undone.
        /// This operation is quite expensive, but this ensures their can be no synchronization
        /// loss between the graph and the ValuePorts.
        /// </summary>
        // NOTE:: It may be possible to cheapen this operation significantly by keeping track
        // of dirtied nodes, but the undo system makes this very difficult.
        private void PostUndoSyncNodePortConnections()
        {
            //Map guid->connection since we're going to be doing lots of lookups and
            //this is a more efficient data format.
            var graphKnownGuidToConnection = new Dictionary<string, Connection>();
            List<Connection> untraversedConnections = new List<Connection>(editorGraph.connections);
            bool anyConnectionsRemoved = false;
            foreach (var conn in editorGraph.connections)
            {
                graphKnownGuidToConnection.Add(conn.GUID, conn);
            }

            foreach (var node in editorGraph.nodeModels)
            {
                void DeleteUndoneConnections(PortModel port)
                {
                    var localPortInfo = port.serializedValueFieldInfo.FieldFromInfo;
                    if (!(localPortInfo.GetValue(node.RuntimeData) is ValuePort valuePort))
                        return;
                    for (int i = valuePort.connections.Count - 1; i >= 0; i--)
                    {
                        var conn = valuePort.connections[i];
                        if (graphKnownGuidToConnection.ContainsKey(conn.GUID))
                        {
                            //We found a port containing this connection, so we mark it traversed.
                            untraversedConnections.Remove(conn);
                            continue;
                        }

                        //The graph doesn't know about this connection, so it's undone. Remove it
                        //from the value port.
                        valuePort.connections.Remove(conn);
                        anyConnectionsRemoved = true;
                    }
                }

                foreach (var port in node.inputPorts)
                {
                    DeleteUndoneConnections(port);
                }

                foreach (var port in node.outputPorts)
                {
                    DeleteUndoneConnections(port);
                }
            }

            if (anyConnectionsRemoved || untraversedConnections.Count <= 0)
                return;

            //If we didin't remove any connections, we're going to probe for restored connections
            //by checking to see if there's any connections we didin't traverse. If any exist,
            //those connections are the "redone" connections.
            foreach (var conn in untraversedConnections)
            {
                //Their should be no side effects to binding more than once.
                conn.BindConnection();
                bool existsAlready = false;
                //We need to be careful not to add the same connection twice
                foreach (var localConn in conn.localPort.connections)
                {
                    if (localConn.GUID == conn.GUID)
                        existsAlready = true;
                }

                if (existsAlready) continue;
                conn.localPort.connections.Add(conn);
            }
        }

        private void UndoPerformed()
        {
            //The undo stack is VERY finnicky, so this order of operations is important.
            ClearGraph();

            //There's some issue with the undo stack rewinding the object state and somehow
            //the editor graph can be null for a moment here. It do be like it is sometimes.
            if (editorGraph == null) return;
            PostUndoSyncNodePortConnections();
            BuildGraph();
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(editorGraph));
        }

        #endregion

        #region Graph Changes Processing

        private void DeleteNode(NodeModel model)
        {
            //Register the state of the saved assets, because we're about to "implicitly" delete some.
            Undo.RegisterImporterUndo(AssetDatabase.GetAssetPath(model.RuntimeData), "graphChanges");
            Undo.DestroyObjectImmediate(model.RuntimeData);
            editorGraph.nodeModels.Remove(model);
        }

        private void DeletePortConnectionByGuid(ValuePort valuePort, string guid)
        {
            //Delete value port connection
            for (int i = valuePort.connections.Count - 1; i >= 0; i--)
            {
                Connection currentConnection = valuePort.connections[i];
                if (currentConnection.GUID != guid) continue;
                valuePort.connections.Remove(currentConnection);
                break;
            }

            //Delete graph connection
            for (int j = editorGraph.connections.Count - 1; j >= 0; j--)
            {
                Connection currentConnection = editorGraph.connections[j];
                if (currentConnection.GUID != guid) continue;
                editorGraph.connections.Remove(currentConnection);
                return;
            }
        }

        private void DeleteEdge(Edge edge, EdgeModel model)
        {
            editorGraph.edgeModels.Remove(model);
            if (!ResolveEdge(edge, out var inModel, out var outModel,
                out var inputPort, out var outputPort))
                return;
            if (TryResolveValuePortFromModels(inModel, inputPort, out var inputValuePort))
            {
                DeletePortConnectionByGuid(inputValuePort, model.inputConnectionGuid);
            }

            if (TryResolveValuePortFromModels(outModel, outputPort, out var outputValuePort))
            {
                DeletePortConnectionByGuid(outputValuePort, model.outputConnectionGuid);
            }
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
                switch (elem)
                {
                    case NodeView view:
                        if (viewToModel.TryGetValue(view, out var nodeModel))
                            DeleteNode(nodeModel);
                        break;
                    case Edge edge:
                        if (edgeToModel.TryGetValue(edge, out var edgeModel))
                            DeleteEdge(edge, edgeModel);
                        break;
                }
            }

            //Crushes all the delete operations into one undo operation.
            Undo.CollapseUndoOperations(index);
        }

        private bool TryCreateConnection(Edge edge,
            NodeModel inModel, NodeModel outModel,
            PortModel inputPort, PortModel outputPort)
        {
            if (!TryResolveValuePortFromModels(inModel, inputPort, out var inputValuePort) ||
                !TryResolveValuePortFromModels(outModel, outputPort, out var outputValuePort))
                return false;

            var localConnection = new Connection(inModel.RuntimeData, inputPort.serializedValueFieldInfo,
                outModel.RuntimeData, outputPort.serializedValueFieldInfo);
            var remoteConnection = new Connection(outModel.RuntimeData, outputPort.serializedValueFieldInfo,
                inModel.RuntimeData, inputPort.serializedValueFieldInfo);

            inputValuePort.connections.Add(localConnection);
            outputValuePort.connections.Add(remoteConnection);
            localConnection.BindConnection();
            remoteConnection.BindConnection();

            var modelEdge = new EdgeModel(inModel, inputPort,
                outModel, outputPort,
                localConnection.GUID,
                remoteConnection.GUID);

            edgeToModel.Add(edge, modelEdge);
            editorGraph.edgeModels.Add(modelEdge);
            editorGraph.connections.Add(localConnection);
            editorGraph.connections.Add(remoteConnection);
            return true;
        }

        /// <summary>
        /// Resolves an edge connection to its related models
        /// </summary>
        private bool ResolveEdge(Edge edge,
            out NodeModel inModel, out NodeModel outModel,
            out PortModel inputPort, out PortModel outputPort)
        {
            if (edge.input.node is NodeView inView &&
                edge.output.node is NodeView outView &&
                viewToModel.TryGetValue(inView, out inModel) &&
                viewToModel.TryGetValue(outView, out outModel) &&
                inModel.View.TryGetPortToModel(edge.input, out inputPort) &&
                outModel.View.TryGetPortToModel(edge.output, out outputPort))
                return true;

            inModel = null;
            outModel = null;
            inputPort = null;
            outputPort = null;
            return false;
        }

        private static bool TryResolveValuePortFromModels(NodeModel nodeModel, PortModel portModel,
            out ValuePort valuePort)
        {
            valuePort = null;
            try
            {
                var inputPortInfo = portModel.serializedValueFieldInfo.FieldFromInfo;
                valuePort = inputPortInfo.GetValue(nodeModel.RuntimeData) as ValuePort;
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void ProcessEdgesToCreate(ref List<Edge> addedEdges)
        {
            for (int i = addedEdges.Count - 1; i >= 0; i--)
            {
                Edge edge = addedEdges[i];

                //(NodeView) Input Node -> (NodeModel) In Model -> (PortModel) Input Port
                //(NodeView) Output Node -> (NodeModel) Out Model -> (PortModel) Output Port
                if (!ResolveEdge(edge, out var inModel, out var outModel,
                        out var inputPort, out var outputPort) ||
                    !TryCreateConnection(edge, inModel, outModel, inputPort, outputPort))
                {
                    //We failed to create a connection so discard this edge, otherwise it's confusing
                    //to the user if an edge is created when a connection isin't.
                    addedEdges.Remove(edge);
                }
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
    }
}