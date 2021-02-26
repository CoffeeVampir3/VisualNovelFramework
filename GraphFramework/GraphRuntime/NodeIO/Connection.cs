using System;
using System.Reflection;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO
{
    /// <summary>
    /// Simple class that conducts the key exchange for 
    /// </summary>
    [System.Serializable]
    public class Connection
    {
        [NonSerialized]
        public ValuePort localPort;
        [NonSerialized]
        public ValuePort remotePort;
        [SerializeField]
        public RuntimeNode localNode;
        [SerializeField]
        public RuntimeNode remoteNode;
        [SerializeField]
        public SerializedFieldInfo localPortField;
        [SerializeField]
        public SerializedFieldInfo remotePortField;
        public Connection(
            RuntimeNode localSide, FieldInfo localPortField, 
            RuntimeNode remoteSide, FieldInfo remotePortField)
        {
            localNode = localSide;
            remoteNode = remoteSide;
            this.localPortField = new SerializedFieldInfo(localPortField);
            this.remotePortField = new SerializedFieldInfo(remotePortField);
        }

        public void BindConnection()
        {
            var localPortInfo = localPortField.FieldFromInfo;
            var remotePortInfo = remotePortField.FieldFromInfo;

            localPort = localPortInfo.GetValue(localNode) as ValuePort;
            remotePort = remotePortInfo.GetValue(remoteNode) as ValuePort;
            Debug.Assert(localPort != null, nameof(localPort) + " != null");
            Debug.Assert(remotePort != null, nameof(remotePort) + " != null");
            localPort.valueKey = remotePort;
            remotePort.valueKey = localPort;
        }
    }
}