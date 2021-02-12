using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;
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
    /// Inherit from BaseNode<RuntimeNodeType>, not this class.
    /// BaseNode is responsible for the very most basic setup and initialization of any deriving nodes.
    /// This allows us to have a common class we can generate using reflection and an activator,
    /// allowing us to avoid boilerplate code.
    /// </summary>
    [Serializable]
    public abstract partial class BaseNode : Node, HasCoffeeGUID
    {
        public string GUID;
        /// <summary>
        /// This is an auto property which is overriden in BaseNode<T> allowing us to use
        /// a more-specific type for RuntimeData instead of the less-specific RuntimeNode.
        /// </summary>
        public abstract RuntimeNode RuntimeData { get; set; }

        /// <summary>
        /// Called when this node is visited by the graph while it is
        /// open in the editor window.
        /// </summary>
        public virtual void OnNodeEntered()
        {
            Debug.Log("Current: " + title);
            AddToClassList("currentNode");
        }

        /// <summary>
        /// Called when this node is visited by the graph while it is
        /// open in the editor window.
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
        /// Deserialization initialization
        /// </summary>
        public void Initialize(NodeSerializationData data)
        {
            OnNodeCreation();
            SetupBaseNodeUI();
            RebuildPortsFromSerialization(data);
        }
        
        /// <summary>
        /// Initial (new node) creation
        /// </summary>
        public void Initialize(string initialName)
        {
            GenerateNewNodeData(initialName);
            OnNodeCreation();
            SetupBaseNodeUI();
            InstantiatePorts();
        }

        private System.Type GetGenericRuntimeNodeType()
        {
            var thisType = GetType();

            //This magic deserve some explanation:
            //In a declaration like DialogueNode : BaseNode<RuntimeDialogueNode>
            //This code looks at BaseNode<RuntimeDialogueNode>
            //And extracts the type RuntimeDialogueNode
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
    }
}
