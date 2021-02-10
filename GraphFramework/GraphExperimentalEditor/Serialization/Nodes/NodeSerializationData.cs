using System.Collections.Generic;
using UnityEngine;
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
        public SerializableType nodeType = null;
        [SerializeField] 
        public List<SerializedPortData> serializedPorts = new List<SerializedPortData>();
        [SerializeField]
        public RuntimeNode runtimeNode;
        
        public static NodeSerializationData SerializeFrom(BaseNode node)
        {
            NodeSerializationData serializationData = CreateInstance<NodeSerializationData>();
            
            serializationData.SetCoffeeGUID(node.GetCoffeeGUID());
            serializationData.nodeTitle = node.title;
            serializationData.position = node.GetPosition();
            serializationData.nodeType = new SerializableType(node.GetType());
            serializationData.runtimeNode = node.RuntimeData;
            serializationData.runtimeNode.SetCoffeeGUID(node.GetCoffeeGUID());
            
            serializationData.name = "sNodeData_" + serializationData.nodeTitle;
            serializationData.runtimeNode.name = serializationData.nodeTitle;
            
            serializationData.runtimeNode.outputConnections.Clear();
            serializationData.runtimeNode.inputConnections.Clear();
            
            if (node is IRootNode)
            {
                serializationData.isRoot = true;
            }
            
            return serializationData;
        }

        /// <summary>
        /// We use ref here so we do not need to cast the returned serialized node.
        /// </summary>
        public void SerializeTo(ref BaseNode node)
        {
            node.SetCoffeeGUID(GetCoffeeGUID());
            node.title = nodeTitle;
            node.SetPosition(position);
            node.RuntimeData = runtimeNode;
            node.RuntimeData.name = nodeTitle;
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