using System.Collections.Generic;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public class NodeSerializationData : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField]
        public string GUID;
        [SerializeField] 
        public List<SerializedPortData> serializedPorts = new List<SerializedPortData>();
        [SerializeField] 
        public NodeEditorData nodeEditorData;
        [SerializeField]
        public RuntimeNode runtimeNode;
        
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