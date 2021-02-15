using System;
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
        private string title;
        [SerializeField] 
        public SerializableType nodeType;

        public static StackNodeSerializationData SerializeFrom(BaseStackNode stackNode)
        {
            StackNodeSerializationData serializationData = CreateInstance<StackNodeSerializationData>();
            
            serializationData.position = stackNode.GetPosition();
            serializationData.nodeType = new SerializableType(stackNode.GetType());
            serializationData.title = stackNode.name;

            serializationData.SetCoffeeGUID(stackNode.GetCoffeeGUID());

            var nodeList = stackNode.nodeList;
            //Serializes all stacked children.
            foreach (var stackedNode in nodeList)
            {
                var serializedStackNode =
                    NodeSerializationData.SerializeFrom(stackedNode, true);
                serializationData.stackedNodes.Add(serializedStackNode);
            }
            
            return serializationData;
        }

        /// <summary>
        /// Creates a new unique copy of the serialized stack with a new GUID.
        /// </summary>
        public BaseStackNode CreateCopyFromSerialization()
        {
            BaseStackNode stackNode = CreateFromSerialization();
            stackNode.SetCoffeeGUID(Guid.NewGuid().ToString());
            return stackNode;
        }

        /// <summary>
        /// Creates a new activated node from the this serialization data.
        /// </summary>
        public BaseStackNode CreateFromSerialization()
        {
            var stackNode = SafeActivatorHelper.LoadArbitrary<BaseStackNode>(nodeType.type);
            stackNode.GUID = GUID;
            stackNode.SetPosition(position);
            stackNode.title = title;
            stackNode.name = title;
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