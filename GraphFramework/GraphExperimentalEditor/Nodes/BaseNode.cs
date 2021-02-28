using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoffeeExtensions;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.GraphFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
    /// <summary>
    /// This class allows us to easily define the runtime data type but still activate it internally
    /// using reflection. I love this so much thanks @MentallyStable on GDL for this one!
    /// </summary>
    public abstract class BaseNode<RuntimeNodeType> : BaseNode where RuntimeNodeType : RuntimeNode
    {
        public override RuntimeNode RuntimeData
        {
            get => nodeRuntimeData;
            set => nodeRuntimeData = value as RuntimeNodeType;
        }
 
        [SerializeReference]
        protected RuntimeNodeType nodeRuntimeData;
    }
    
    /// <summary>
    /// Inherit using the generic version with a RuntimeNodeType.
    /// BaseNode is responsible for the very most basic setup and initialization of any deriving nodes.
    /// This allows us to have a common class we can generate using reflection and an activator,
    /// allowing us to avoid boilerplate code.
    /// </summary>
    [Serializable]
    public abstract partial class BaseNode : Node, HasCoffeeGUID
    {
        public string GUID;
        /// <summary>
        /// This is an auto property which is overriden in BaseNode allowing us to use
        /// a more-specific type for RuntimeData instead of the less-specific RuntimeNode.
        /// </summary>
        public abstract RuntimeNode RuntimeData { get; set; }
        
        /// <summary>
        /// The value binding dictionary for Port to ValuePort
        /// </summary>
        public Dictionary<Port, FieldInfo> portValueBindings = new Dictionary<Port, FieldInfo>();
        
        /// <summary>
        /// The value binding dictionary for Port to Connection
        /// </summary>
        public Dictionary<Port, List<RuntimeConnection>> portConnectionBindings = 
            new Dictionary<Port, List<RuntimeConnection>>();
        
        /// <summary>
        /// Editor Only Callback:
        /// Called when this node is visited by the graph while it is open in an editor window.
        /// </summary>
        public virtual void OnNodeEntered()
        {
            Debug.Log("Current: " + title);
            AddToClassList("currentNode");
        }

        /// <summary>
        /// Editor Only Callback:
        /// Called when this node is visited by the graph while it is open in an editor window.
        /// </summary>
        public virtual void OnNodeExited()
        {
            RemoveFromClassList("currentNode");
        }
        
        public string GetCoffeeGUID()
        {
            return GUID;
        }

        public void SetCoffeeGUID(string newGuid)
        {
            GUID = newGuid;
        }

        #region Initialization
        
        /// <summary>
        /// Initializes 
        /// </summary>
        public void Initialize(NodeSerializationData data)
        {
            OnNodeCreation();
            SetupBaseNodeUI();
            RebuildPortsFromSerialization(data);
            Repaint();
        }
        
        /// <summary>
        /// Initializes a new node and it's UI from scratch with the given name.
        /// </summary>
        public void Initialize(string initialName)
        {
            GenerateNewNodeData(initialName);
            OnNodeCreation();
            SetupBaseNodeUI();
            CreatePortsFromReflection();
            Repaint();
        }

        private System.Type GetGenericRuntimeNodeType()
        {
            var thisType = GetType();

            //This magic deserve some explanation:
            //In a declaration like DialogueNode : BaseNode<RuntimeDialogueNode>
            //This code looks at BaseNode<RuntimeDialogueNode>
            //And extracts the type between the <>, RuntimeDialogueNode
            var k = thisType.GetGenericClassConstructorArguments(typeof(BaseNode<>));
            return k.FirstOrDefault(w => typeof(RuntimeNode).IsAssignableFrom(w));
        }

        private void GenerateNewNodeData(string initialName)
        {
            var q = GetGenericRuntimeNodeType();
            RuntimeData = ScriptableObject.CreateInstance(q) as RuntimeNode;

            SetCoffeeGUID(Guid.NewGuid().ToString());
            name = initialName;
            title = initialName;
            
            Debug.Assert(RuntimeData != null, nameof(RuntimeData) + " != null");
            RuntimeData.SetCoffeeGUID(GetCoffeeGUID());
        }

        #endregion
        
        #region Port Binding

        public void AddPortConnection(RuntimeConnection connection, Port port)
        {
            if(!portConnectionBindings.TryGetValue(port, out var localConnections))
            {
                localConnections = new List<RuntimeConnection>();
                portConnectionBindings.Add(port, localConnections);
            }
            localConnections.Add(connection);
        }

        public RuntimeConnection ConnectPortTo(Port localPort, BaseNode connectingTo, Port remotePort)
        {
            FieldInfo localValuePortField = portValueBindings[localPort];
            FieldInfo remoteValuePortField = connectingTo.portValueBindings[remotePort];

            RuntimeConnection connection = new RuntimeConnection(
                RuntimeData, localValuePortField, 
                connectingTo.RuntimeData, remoteValuePortField);
            RuntimeData.connections.Add(connection);
            AddPortConnection(connection, localPort);
            connectingTo.AddPortConnection(connection, remotePort);

            Debug.Log("Created Connection from: " + this.name + " to " + connectingTo.name);
            
            connection.BindConnection();
            return connection;
        }

        public void DisconnectPortFrom(Port localPort, BaseNode connectingTo, Port remotePort)
        {
            if (!portConnectionBindings.TryGetValue(localPort, 
                out var localConnections)) return;
            if (!connectingTo.portConnectionBindings.TryGetValue(remotePort, 
                out var remoteConnections)) return;
            
            //This could be optimized via dictionary search, but this makes serialization much
            //simpler and this operation will never be very expensive.
            foreach (var lCon 
                in localConnections.Where(lCon => 
                    remoteConnections.Contains(lCon)))
            {
                localConnections.Remove(lCon);
                remoteConnections.Remove(lCon);

                if (!RuntimeData.connections.Contains(lCon)) return;
                
                RuntimeData.connections.Remove(lCon);
                Debug.Log("Disconnecting port from: " + this.name + " to " + connectingTo.name);
                return;
            }
        }
        
        #endregion 
    }
}
