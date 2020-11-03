using System.Collections.Generic;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public class NodeSerializationData : ScriptableObject
    {
        [SerializeField] 
        public List<SerializedPortData> serializedPorts = new List<SerializedPortData>();
        [SerializeField] 
        public NodeEditorData nodeEditorData;
        [SerializeField]
        public NodeRuntimeData nodeRuntimeData;
    }
}