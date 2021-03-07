using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO
{
    /// <summary>
    /// Reflection that allows us to save the relationship between actual value field and value port.
    /// </summary>
    [System.Serializable]
    internal class BindingConnection
    {
        [SerializeReference]
        public RuntimeNode localNode;
        [SerializeReference]
        public RuntimeNode remoteNode;
        [SerializeReference]
        private SerializedFieldInfo localPortField;
        [SerializeReference]
        private SerializedFieldInfo remotePortField;

        internal BindingConnection(RuntimeNode local, RuntimeNode remote,
            SerializedFieldInfo localField, SerializedFieldInfo remoteField)
        {
            localNode = local;
            remoteNode = remote;
            localPortField = localField;
            remotePortField = remoteField;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValuePort BindLocal()
        {
            var localPortInfo = localPortField.FieldFromInfo;
            ValuePort port = localPortInfo.GetValue(localNode) as ValuePort;

#if UNITY_EDITOR
            if (localPortInfo == null || port == null) {
                throw new ArgumentException("Unable to instantiate a port from field named: " + localPortField.FieldName + "" +
                                            " . Field was likely renamed or removed.");
            }
#endif
            return port;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValuePort BindRemote()
        {
            var remotePortInfo = remotePortField.FieldFromInfo;
            ValuePort port = remotePortInfo.GetValue(remoteNode) as ValuePort;

#if UNITY_EDITOR
            if (remotePortInfo == null || port == null) {
                throw new ArgumentException("Unable to instantiate a port from field named: " + remotePortField.FieldName + "" +
                                            " . Field was likely renamed or removed.");
            }
#endif
            return port;
        }
    }
    
    /// <summary>
    /// Simple class that conducts the key exchange for graph "ports".
    /// </summary>
    [System.Serializable]
    public class Connection
    {
        [NonSerialized]
        public ValuePort localPort;
        [NonSerialized]
        public ValuePort remotePort;
        [SerializeReference] 
        private BindingConnection connectionBinder;
        [SerializeReference] 
        public string GUID;

        public Connection(
            RuntimeNode localSide, SerializedFieldInfo localPortField, 
            RuntimeNode remoteSide, SerializedFieldInfo remotePortField)
        {
            connectionBinder = new BindingConnection(
                localSide, 
                remoteSide,
                localPortField,
                remotePortField);
            GUID = Guid.NewGuid().ToString();
        }

        public RuntimeNode GetRemoteNode()
        {
            return connectionBinder.remoteNode;
        }

        public void BindConnection()
        {
            localPort = connectionBinder.BindLocal();
            remotePort = connectionBinder.BindRemote();
            
            localPort.valueKey = remotePort;
            remotePort.valueKey = localPort;
        }
    }
}