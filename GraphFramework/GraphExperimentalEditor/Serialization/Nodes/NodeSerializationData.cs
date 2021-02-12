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
        public static NodeSerializationData SerializeFrom(BaseNode node, bool isStacked = false, bool createRuntimeDataCopy = false)
        {
            NodeSerializationData serializationData = CreateInstance<NodeSerializationData>();
            
            serializationData.SetCoffeeGUID(node.GetCoffeeGUID());
            serializationData.nodeTitle = node.title;
            serializationData.position = node.GetPosition();
            serializationData.isStacked = isStacked;
            serializationData.isExpanded = node.expanded;
            serializationData.nodeType = new SerializableType(node.GetType());

            //Create a copy of the runtime data if requested.
            serializationData.runtimeNode = createRuntimeDataCopy ? 
                Instantiate(node.RuntimeData) : 
                node.RuntimeData;
            
            serializationData.runtimeNode.SetCoffeeGUID(node.GetCoffeeGUID());
            serializationData.name = "sNodeData_" + serializationData.nodeTitle;
            serializationData.runtimeNode.name = serializationData.nodeTitle;
            
            serializationData.runtimeNode.outputConnections.Clear();
            serializationData.runtimeNode.inputConnections.Clear();
            
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
        /// Creates a new activated node from the this serialization data.
        /// </summary>
        public BaseNode CreateFromSerialization()
        {
            var node = SafeActivatorHelper.LoadArbitrary<BaseNode>(nodeType.type);
            node.SetCoffeeGUID(GetCoffeeGUID());
            node.title = nodeTitle;
            
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
                if(edge.input.node is BaseNode bn)
                    AddOutputLink(runtimeData, bn.RuntimeData);
                serializedPort.serializedEdges.Add(new SerializedEdgeData(edge));
            }

            return serializedPort;
        }

        private static SerializedPortData SerializeInputPort(Port port, RuntimeNode runtimeData)
        {
            SerializedPortData serializedPort = new SerializedPortData(port);
            foreach (var edge in port.connections)
            {
                if(edge.output.node is BaseNode bn)
                    AddInputLink(runtimeData, bn.RuntimeData);
            }
            
            return serializedPort;
        }

        /// <summary>
        /// This is the actual connection information that we're serializing, not just
        /// the edge data. So the node actually connects to another runtime node.
        /// </summary>
        private static void AddOutputLink(RuntimeNode addingTo, RuntimeNode connection)
        {
            addingTo.outputConnections.Add(connection);
        }
        
        /// <summary>
        /// This is the actual connection information that we're serializing, not just
        /// the edge data. So the node actually connects to another runtime node.
        /// </summary>
        private static void AddInputLink(RuntimeNode addingTo, RuntimeNode connection)
        {
            addingTo.inputConnections.Add(connection);
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