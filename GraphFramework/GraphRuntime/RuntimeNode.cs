using System.Collections.Generic;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.Attributes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO;

namespace VisualNovelFramework.GraphFramework.GraphRuntime
{
    public class RuntimeNode : ScriptableObject
    {
        [SerializeField]
        public List<DEPRECATED_RuntimeConnection> connections = new List<DEPRECATED_RuntimeConnection>();
        public virtual RuntimeNode OnEvaluate()
        {
            Debug.Log(this.name);
            return null;
        }
    }
}