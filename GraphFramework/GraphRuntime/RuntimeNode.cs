using System.Collections.Generic;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;

namespace VisualNovelFramework.GraphFramework.GraphRuntime
{
    public class RuntimeNode : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField]
        public string GUID;
        [SerializeField]
        public List<RuntimeNode> outputConnections = new List<RuntimeNode>();
        [SerializeField]
        public List<RuntimeNode> inputConnections = new List<RuntimeNode>();

        public virtual RuntimeNode GetNextNode()
        {
            return null;
        }
        
        public string GetCoffeeGUID()
        {
            return GUID;
        }

        public void SetCoffeeGUID(string newGuid)
        {
            GUID = newGuid;
        }
    }
}