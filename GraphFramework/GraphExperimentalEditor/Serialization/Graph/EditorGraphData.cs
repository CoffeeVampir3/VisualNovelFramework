using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.Serialization;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public class EditorGraphData : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField]
        public SerializableType editorWindowType;
        [SerializeField]
        public SerializedGraph targetGraph;
        [SerializeField]
        public string GUID;

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