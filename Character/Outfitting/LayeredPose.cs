using UnityEditor;
using UnityEngine;

namespace VisualNovelFramework.Outfitting
{
    [CreateAssetMenu]
    public class LayeredPose : ScriptableObject
    {
        public CharacterLayer layer;
        public CharacterPose pose;
        public CharacterLayer posedLayer;

        public static LayeredPose Create(CharacterLayer cl, CharacterPose pose, CharacterLayer targetLayer)
        {
            LayeredPose lp = CreateInstance<LayeredPose>();
            lp.layer = cl;
            lp.pose = pose;
            lp.posedLayer = targetLayer;
            return lp;
        }
        
        public void SerializeRecursive(CharacterCompositor saveTo)
        {
            var newLayer = saveTo.layerSerializationDict[layer];
            var newPose = saveTo.poseSerializationDict[pose];
             
            var clone = Instantiate(posedLayer);
            clone.name = "pl_" + pose.name + "-" + layer.name;
            AssetDatabase.AddObjectToAsset(clone, saveTo);

            var clonedLayeredPose = Create(newLayer, newPose, clone);
            clonedLayeredPose.name = pose.name + "-" + layer.name;
            AssetDatabase.AddObjectToAsset(clonedLayeredPose, saveTo);
            saveTo.layeredPoses.Add(clonedLayeredPose);

            saveTo.posedLayerSerializationDict.Add(posedLayer, clone);
        }
        
    }
}