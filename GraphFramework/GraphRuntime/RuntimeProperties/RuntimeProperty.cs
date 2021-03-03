using UnityEngine;
using VisualNovelFramework.GraphFramework.Attributes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Properties
{
    public class RuntimeProperty<T> : RuntimeNode
    {
        [Out] 
        protected ValuePort<T> propertyPortValue = new ValuePort<T>();

        [SerializeField]
        private T propertyValue;
        
        public override void OnEvaluate()
        {
            ValuePort.SetPortValue(propertyPortValue, propertyValue);
        }
    }
}