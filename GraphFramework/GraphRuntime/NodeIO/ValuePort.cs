using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO
{
    /// <summary>
    /// An I/O port for the coffee graph. Internally this works as a simple dictionary.
    /// Connections with other nodes use a simple "pointer" key exchange,
    /// so retrieving a value is always one single dictionary lookup with no other overhead.
    /// </summary>
    [Serializable]
    public abstract class ValuePort
    {
        [SerializeReference] 
        public List<Connection> connections = new List<Connection>();
        public ValuePort valueKey;
    }

    [Serializable]
    public class ValuePort<T> : ValuePort
    {
        [SerializeReference]
        public T portValue = default;
        public T Value => (valueKey as ValuePort<T>).portValue;
    }
}