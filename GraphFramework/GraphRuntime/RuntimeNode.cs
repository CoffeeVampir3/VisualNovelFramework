using System.Collections.Generic;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;

namespace VisualNovelFramework.GraphFramework.GraphRuntime
{
    public class RuntimeNode : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField, HideInInspector]
        public string GUID;
        [SerializeField, HideInInspector]
        public List<RuntimeNode> inputConnections = new List<RuntimeNode>();
        [SerializeField, HideInInspector]
        public List<RuntimeNode> outputConnections = new List<RuntimeNode>();

        public virtual RuntimeNode GetNextNode()
        {
            return null;
        }

        public virtual void OnEvaluate()
        {
            Debug.Log(this.name);
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