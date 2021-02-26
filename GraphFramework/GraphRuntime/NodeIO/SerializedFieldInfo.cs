using System;
using System.Reflection;
using UnityEngine;
using VisualNovelFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.GraphRuntime
{
    [Serializable]
    public class SerializedFieldInfo
    {
        [SerializeField]
        private string fieldName;
        [SerializeField]
        private SerializableType declaringType;
        
        public FieldInfo FieldFromInfo => declaringType.type.GetField(fieldName, 
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public SerializedFieldInfo(FieldInfo info)
        {
            fieldName = info.Name;
            declaringType = new SerializableType(info.DeclaringType);
        }
    }
}