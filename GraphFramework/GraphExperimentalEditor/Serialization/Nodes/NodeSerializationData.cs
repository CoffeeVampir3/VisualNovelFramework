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
        public string GUID;
        [SerializeField] 
        public List<SerializedPortData> serializedPorts = new List<SerializedPortData>();
        [SerializeField] 
        public NodeEditorData nodeEditorData;
        [SerializeField]
        public RuntimeNode runtimeNode;
        
        public static NodeSerializationData SerializeFrom(BaseNode node)
        {
            NodeSerializationData serializationData = CreateInstance<NodeSerializationData>();
            
            serializationData.nodeEditorData = node.editorData;
            serializationData.SetCoffeeGUID(node.editorData.GUID);
            
            serializationData.runtimeNode = node.RuntimeData;
            serializationData.runtimeNode.SetCoffeeGUID(node.editorData.GUID);
            
            if (node is IRootNode)
            {
                serializationData.isRoot = true;
            }
            
            return serializationData;
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