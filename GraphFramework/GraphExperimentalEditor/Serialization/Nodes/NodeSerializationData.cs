﻿using System.Collections.Generic;
using UnityEngine;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public class NodeSerializationData : ScriptableObject
    {
        [SerializeField] 
        public List<SerializedPortData> serializedPorts = new List<SerializedPortData>();
        [SerializeField] 
        public NodeEditorData nodeEditorData;
    }
}