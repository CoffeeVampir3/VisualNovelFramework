using UnityEngine;

namespace VisualNovelFramework.Outfitting
{
    public class LayeredPose : ScriptableObject
    {
        public CharacterLayer layer;
        public CharacterPose pose;
        public CharacterLayer posedLayer;

        public static LayeredPose Create(CharacterLayer cl, CharacterPose pose, CharacterLayer targetLayer)
        {
            var lp = CreateInstance<LayeredPose>();
            lp.layer = cl;
            lp.pose = pose;
            lp.posedLayer = targetLayer;
            return lp;
        }
    }
}