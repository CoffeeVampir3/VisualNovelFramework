using UnityEngine;

namespace VisualNovelFramework.GraphFramework.GraphRuntime
{
    public class RuntimeNode : ScriptableObject
    {
        public virtual RuntimeNode OnEvaluate()
        {
            Debug.Log(this.name);
            return null;
        }
    }
}