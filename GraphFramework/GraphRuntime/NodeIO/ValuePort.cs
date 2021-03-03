using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO
{
    /// <summary>
    /// An I/O port for the coffee graph. Internally this works as a simple dictionary.
    /// Connections with other nodes use a simple "pointer" key exchange,
    /// so retrieving a value is always one single dictionary lookup with no other overhead.
    /// </summary>
    [Serializable]
    public class ValuePort
    {
        public ValuePort valueKey;
        protected static Dictionary<ValuePort, object> ioValue =
            new Dictionary<ValuePort, object>();
        
        public static void SetPortValue(ValuePort valueKey, object value)
        {
            ioValue[valueKey] = value;
        }

        public static object GetPortValue(ValuePort port)
        {
            return ioValue[port.valueKey];
        }

        public static ValuePort FromField(FieldInfo field, RuntimeNode fromNode)
        {
            ValuePort valuePort = field.GetValue(fromNode) as ValuePort;
            SetPortValue(valuePort, true);
            return valuePort;
        }
    }

    public class ValuePort<T> : ValuePort
    {
        public ValuePort()
        {
            ioValue[this] = default(T);
        }

        public T Value => (T) ioValue[valueKey];
    }
}