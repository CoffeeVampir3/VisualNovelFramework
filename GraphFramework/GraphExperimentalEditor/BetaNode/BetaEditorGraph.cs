using System.Collections.Generic;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.BetaNode
{
    [CreateAssetMenu]
    public class BetaEditorGraph : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField]
        private string coffeeGuid;
        [SerializeReference] 
        public List<NodeModel> nodeModels = new List<NodeModel>();
        [SerializeReference] 
        public List<EdgeModel> edgeModels = new List<EdgeModel>();
        [SerializeReference] 
        public List<Connection> connections = new List<Connection>();
        
        public string GetCoffeeGUID()
        {
            return coffeeGuid;
        }

        public void SetCoffeeGUID(string GUID)
        {
            coffeeGuid = GUID;
        }
    }
}