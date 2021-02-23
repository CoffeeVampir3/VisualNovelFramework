using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public class NodeSerializationData : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField] 
        public bool isRoot = false;
        [SerializeField]
        private string GUID;
        [SerializeField] 
        private string nodeTitle;
        [SerializeField] 
        private Rect position;
        [SerializeField] 
        private bool isExpanded;
        [SerializeField] 
        public SerializableType nodeType = null;
        [SerializeField] 
        public List<SerializedPortData> serializedPorts = new List<SerializedPortData>();
        [SerializeField]
        public RuntimeNode runtimeNode;
        [SerializeField] 
        public bool isStacked;
        
        /// <summary>
        /// Serializes the given nodes data into a new serialization data scriptable object.
        /// </summary>
        public static NodeSerializationData SerializeFrom(BaseNode node, bool isStacked = false)
        {
            NodeSerializationData serializationData = CreateInstance<NodeSerializationData>();
            
            serializationData.SetCoffeeGUID(node.GetCoffeeGUID());
            serializationData.nodeTitle = node.title;
            serializationData.position = node.GetPosition();
            serializationData.isStacked = isStacked;
            serializationData.isExpanded = node.expanded;
            serializationData.nodeType = new SerializableType(node.GetType());
            serializationData.runtimeNode = node.RuntimeData;
            serializationData.runtimeNode.SetCoffeeGUID(node.GetCoffeeGUID());
            serializationData.name = "sNodeData_" + serializationData.nodeTitle;
            serializationData.runtimeNode.name = serializationData.nodeTitle;
            
            serializationData.runtimeNode.connections.Clear();
            
            if (node is IRootNode)
            {
                serializationData.isRoot = true;
            }
            
            var ports = node.Query<Port>().ToList();
            foreach(var port in ports)
            {
                switch (port.direction)
                {
                    case Direction.Output:
                        serializationData.serializedPorts.
                            Add(SerializeOutputPort(port, serializationData.runtimeNode));
                        continue;
                    case Direction.Input:
                        serializationData.serializedPorts.
                            Add(SerializeInputPort(port, serializationData.runtimeNode));
                        continue;
                }
            }
            
            return serializationData;
        }

        /// <summary>
        /// Creates a new unique copy of the serialized node with a different RTD and GUID.
        /// </summary>
        public BaseNode CreateCopyFromSerialization()
        {
            BaseNode bn = CreateFromSerialization();
            bn.RuntimeData = Instantiate(bn.RuntimeData);
            bn.SetCoffeeGUID(Guid.NewGuid().ToString());
            bn.RuntimeData.SetCoffeeGUID(bn.GetCoffeeGUID());
            return bn;
        }

        /// <summary>
        /// Creates a new activated node from the this serialization data.
        /// </summary>
        public BaseNode CreateFromSerialization()
        {
            var node = SafeActivatorHelper.LoadArbitrary<BaseNode>(nodeType.type);
            node.SetCoffeeGUID(GetCoffeeGUID());
            node.title = nodeTitle;
            node.name = nodeTitle;
            
            if(!(position == default))
                node.SetPosition(position);
            
            node.RuntimeData = runtimeNode;
            node.RuntimeData.name = nodeTitle;
            node.expanded = isExpanded;
            node.Initialize(this);
            return node;
        }
        
        /// <summary>
        /// Output ports hold the edge information, input ports do not.
        /// </summary>
        private static SerializedPortData SerializeOutputPort(Port port, RuntimeNode runtimeData)
        {
            SerializedPortData serializedPort = new SerializedPortData(port);
            foreach (var edge in port.connections)
            {
                if (edge.input.node is BaseNode bn)
                {
                    //TODO::
                    //AddOutputLink(runtimeData, bn.RuntimeData);
                }
                serializedPort.serializedEdges.Add(new SerializedEdgeData(edge));
            }

            return serializedPort;
        }

        private static SerializedPortData SerializeInputPort(Port port, RuntimeNode runtimeData)
        {
            SerializedPortData serializedPort = new SerializedPortData(port);
            foreach (var edge in port.connections)
            {
                if (edge.output.node is BaseNode bn)
                {
                    //TODO::
                    //AddInputLink(runtimeData, bn.RuntimeData);
                }
            }
            
            return serializedPort;
        }

        public string GetCoffeeGUID()
        {
            return GUID;
        }

        public void SetCoffeeGUID(string newGuid)
        {
            GUID = newGuid;
        }
    }
}