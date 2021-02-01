using UnityEngine;
using VisualNovelFramework.EditorExtensions;

namespace VisualNovelFramework.VNCharacter
{
    public class LayeredPose : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField] 
        private string GUID;
        [SerializeField] 
        public CharacterLayer layer;
        [SerializeField] 
        public CharacterPose pose;
        [SerializeField] 
        public CharacterLayer posedLayer;

        public static LayeredPose Create(CharacterLayer cl, CharacterPose pose, CharacterLayer targetLayer)
        {
            var lp = CreateInstance<LayeredPose>();
            lp.layer = cl;
            lp.pose = pose;
            lp.posedLayer = targetLayer;
            return lp;
        }
        
        public string GetCoffeeGUID()
        {
            return GUID;
        }

        public void SetCoffeeGUID(string newGUID)
        {
            GUID = newGUID;
        }
    }
}