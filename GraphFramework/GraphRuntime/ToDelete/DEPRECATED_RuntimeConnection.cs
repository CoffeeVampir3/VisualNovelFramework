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
    internal class __BindingConnection
    {
        [SerializeField]
        public RuntimeNode localNode;
        [SerializeField]
        public RuntimeNode remoteNode;
        [SerializeField]
        private SerializedFieldInfo localPortField;
        [SerializeField]
        private SerializedFieldInfo remotePortField;

        internal __BindingConnection(RuntimeNode local, RuntimeNode remote,
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
    public class DEPRECATED_RuntimeConnection
    {
        [NonSerialized]
        public ValuePort localPort;
        [NonSerialized]
        public ValuePort remotePort;
        [SerializeField] 
        private __BindingConnection connectionBinder;
        [SerializeField] 
        public string GUID;
        public DEPRECATED_RuntimeConnection(
            RuntimeNode localSide, SerializedFieldInfo localPortField, 
            RuntimeNode remoteSide, SerializedFieldInfo remotePortField)
        {
            connectionBinder = new __BindingConnection(
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
            Debug.Log("Binding connection: " + GUID);
            localPort = connectionBinder.BindLocal();
            remotePort = connectionBinder.BindRemote();
            
            localPort.valueKey = remotePort;
            remotePort.valueKey = localPort;
        }
    }
}