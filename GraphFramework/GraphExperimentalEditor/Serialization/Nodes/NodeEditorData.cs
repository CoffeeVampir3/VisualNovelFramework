using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    //Strong separation of runtime/editor time.
    //[Serializable]
    public class NodeEditorData : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField]
        public string GUID;
        [SerializeField] 
        public Rect position;
        [SerializeField] 
        public SerializableType nodeType;

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