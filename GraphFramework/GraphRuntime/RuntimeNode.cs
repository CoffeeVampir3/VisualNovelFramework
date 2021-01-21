using System.Collections.Generic;
using UnityEngine;

namespace VisualNovelFramework.GraphFramework.GraphRuntime
{
    public class RuntimeNode : ScriptableObject
    {
        [SerializeField, HideInInspector]
        public List<RuntimeNode> outputConnections = new List<RuntimeNode>();
        [SerializeField, HideInInspector]
        public List<RuntimeNode> inputConnections = new List<RuntimeNode>();

        public virtual RuntimeNode GetNextNode()
        {
            
            
            return null;
        }
    }
}