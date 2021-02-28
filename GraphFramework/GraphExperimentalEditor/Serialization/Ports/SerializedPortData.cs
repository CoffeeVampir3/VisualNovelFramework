using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    [Serializable]
    public class SerializedPortData
    {
        [SerializeField, HideInInspector]
        public Orientation orientation;
        [SerializeField, HideInInspector]
        public Direction direction;
        [SerializeField, HideInInspector]
        public Port.Capacity capacity;
        [SerializeField, HideInInspector] 
        public SerializableType portValueType = null;
        [SerializeField, HideInInspector] 
        public List<SerializedEdgeData> serializedEdges = new List<SerializedEdgeData>();
        [SerializeField] 
        public List<RuntimeConnection> portConnections;
        [SerializeField] 
        public SerializedFieldInfo serializedValueFieldInfo;

        public SerializedPortData(Port p)
        {
            portValueType = new SerializableType(p.portType);
            orientation = p.orientation;
            direction = p.direction;
            capacity = p.capacity;
        }
    }
}