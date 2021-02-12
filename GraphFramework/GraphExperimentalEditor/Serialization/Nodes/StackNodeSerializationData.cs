using System.Collections.Generic;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public class StackNodeSerializationData : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField]
        private string GUID;
        [SerializeField] 
        public List<NodeSerializationData> stackedNodes = new List<NodeSerializationData>();
        [SerializeField] 
        private Rect position;
        [SerializeField] 
        public SerializableType nodeType;

        public static StackNodeSerializationData SerializeFrom(BaseStackNode stackNode, bool createRuntimeDataCopy = false)
        {
            StackNodeSerializationData serializationData = CreateInstance<StackNodeSerializationData>();
            
            serializationData.position = stackNode.GetPosition();
            serializationData.nodeType = new SerializableType(stackNode.GetType());
            serializationData.SetCoffeeGUID(stackNode.GetCoffeeGUID());
            
            var nodeList = stackNode.nodeList;
            //Serializes all stacked children.
            foreach (var stackedNode in nodeList)
            {
                var serializedStackNode =
                    NodeSerializationData.SerializeFrom(stackedNode, true, createRuntimeDataCopy);
                serializationData.stackedNodes.Add(serializedStackNode);
            }
            
            return serializationData;
        }

        /// <summary>
        /// We use ref here so we do not need to cast the returned serialized node.
        /// </summary>
        public BaseStackNode CreateFromSerialization()
        {
            var stackNode = SafeActivatorHelper.LoadArbitrary<BaseStackNode>(nodeType.type);
            stackNode.GUID = GUID;
            stackNode.SetPosition(position);
            return stackNode;
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