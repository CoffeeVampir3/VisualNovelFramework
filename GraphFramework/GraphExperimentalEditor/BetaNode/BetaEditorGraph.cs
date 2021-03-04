using System.Collections.Generic;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;

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