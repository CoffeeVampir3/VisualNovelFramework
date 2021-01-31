using UnityEngine;
using VisualNovelFramework.Editor.Serialization;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.Editor;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    //TODO:: This information isin't needed at runtime?
    //Strong seperation of runtime/editor time.
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