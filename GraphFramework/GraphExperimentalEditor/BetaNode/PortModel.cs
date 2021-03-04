using System;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.BetaNode
{
    [Serializable]
    public class PortModel
    {
        [SerializeField]
        public Orientation orientation;
        [SerializeField]
        public Direction direction;
        [SerializeField]
        public Port.Capacity capacity;
        [SerializeField] 
        public SerializableType portValueType = null;
        [SerializeField] 
        public SerializedFieldInfo serializedValueFieldInfo;
        //Lookup is done via GUID because undo/redo creates a different copy.
        [SerializeField] 
        public string portGUID;
        
        public PortModel(Orientation orientation, 
            Direction direction, 
            Port.Capacity capacity, 
            Type portType, 
            FieldInfo fieldInfo)
        {
            this.orientation = orientation;
            this.direction = direction;
            this.capacity = capacity;
            this.portValueType = new SerializableType(portType);
            this.serializedValueFieldInfo = new SerializedFieldInfo(fieldInfo);
            portGUID = Guid.NewGuid().ToString();
        }
    }
}