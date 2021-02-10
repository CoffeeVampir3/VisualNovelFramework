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
        public string GUID;
        [SerializeField] 
        public List<NodeSerializationData> stackedNodes = new List<NodeSerializationData>();
        [SerializeField] 
        public Rect position;
        [SerializeField] 
        public SerializableType nodeType;

        public static StackNodeSerializationData SerializeFrom(BaseStackNode stackNode)
        {
            StackNodeSerializationData serializationData = CreateInstance<StackNodeSerializationData>();
            
            serializationData.position = stackNode.GetPosition();
            serializationData.nodeType = new SerializableType(stackNode.GetType());
            serializationData.SetCoffeeGUID(stackNode.GetCoffeeGUID());
            return serializationData;
        }

        public void SerializeTo(ref BaseStackNode stackNode)
        {
            stackNode.GUID = GUID;
            stackNode.SetPosition(position);
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