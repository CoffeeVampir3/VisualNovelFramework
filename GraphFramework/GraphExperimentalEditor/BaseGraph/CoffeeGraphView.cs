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
        [SerializeReference] protected readonly GraphSettings settings;

        protected CoffeeGraphView()
        {
            settings = GraphSettings.CreateOrGetSettings(this);
            styleSheets.Add(settings.graphViewStyle);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());


            //Callback when you copy.
            serializeGraphElements = OnSerializeGraphElements;
            //Backback when you paste.
            unserializeAndPaste = DeserializeElementsOnPaste;
        }

        [Serializable]
        public class SerializedBox
        {
            public List<NodeSerializationData> serializedNodes = new List<NodeSerializationData>();
            public List<StackNodeSerializationData> serializedStacks = new List<StackNodeSerializationData>();
        }

        protected virtual string OnSerializeGraphElements(IEnumerable<GraphElement> selectedItemsToSerialize)
        {
            SerializedBox box = new SerializedBox();
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
                            NodeSerializationData.SerializeFrom(bn, false, true);
                        
                        box.serializedNodes.Add(serialNode);
                        break;
                    }
                    case BaseStackNode sn:
                        StackNodeSerializationData stackSerialData = 
                            StackNodeSerializationData.SerializeFrom(sn, true);
                        
                        box.serializedStacks.Add(stackSerialData);
                        break;
                }
            }

            return JsonUtility.ToJson(box);
        }

        protected virtual void DeserializeElementsOnPaste(string op, string serializationData)
        {
            SerializedBox box = JsonUtility.FromJson<SerializedBox>(serializationData);
            if (box == null)
                return;
            
            List<ISelectable> newSelection = new List<ISelectable>();
            
            //Deserialize each node.
            foreach (var serialNode in box.serializedNodes)
            {
                var node = serialNode.CreateFromSerialization();
                node.SetCoffeeGUID(Guid.NewGuid().ToString());
                newSelection.Add(node);
                AddNode(node);
            }
            
            //Deserialize all stacks and their childrens
            foreach (var serializedStack in box.serializedStacks)
            {
                var stackNode = serializedStack.CreateFromSerialization();
                stackNode.SetCoffeeGUID(Guid.NewGuid().ToString());
                newSelection.Add(stackNode);
                AddStackNode(stackNode);
                foreach (var serialNode in serializedStack.stackedNodes)
                {
                    var node = serialNode.CreateFromSerialization();
                    node.SetCoffeeGUID(Guid.NewGuid().ToString());
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

        /// <summary>
        /// Nodes should be created using AddNoteAt with a rect targeting where on the graph
        /// they should be spawned.
        /// </summary>
        public void AddNode(BaseNode node)
        {
            AddDefaultSettingsToNode(node);
            AddElement(node);
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

        public void AddStackNode(BaseStackNode node)
        {
            AddDefaultSettingsToNode(node);
            AddElement(node);
        }

        /// <summary>
        /// Node is created with on the graph with the given coordinates.
        /// </summary>
        protected void AddNodeAt(BaseNode node, Rect position)
        {
            AddNode(node);
            node.SetPosition(position);
        }

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
    }
}
