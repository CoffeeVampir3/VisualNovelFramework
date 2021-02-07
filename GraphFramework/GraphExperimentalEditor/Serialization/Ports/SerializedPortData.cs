using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
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
        [SerializeField] 
        public string portName;
        [SerializeField, HideInInspector] 
        public List<SerializedEdgeData> serializedEdges = new List<SerializedEdgeData>();

        public SerializedPortData(Port p)
        {
            portValueType = new SerializableType(p.portType);
            orientation = p.orientation;
            direction = p.direction;
            capacity = p.capacity;
            portName = p.portName;
        }
    }
}