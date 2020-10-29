using UnityEditor;
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
            LayeredPose lp = CreateInstance<LayeredPose>();
            lp.layer = cl;
            lp.pose = pose;
            lp.posedLayer = targetLayer;
            return lp;
        }
        
        public void SerializeRecursive(CharacterCompositor saveTo)
        {
            //Discard if the layer was deleted.
            if (!saveTo.layerSerializationDict.TryGetValue(layer, out var newLayer))
            {
                return;
            }
            if (!saveTo.poseSerializationDict.TryGetValue(pose, out var newPose))
            {
                return;
            }
             
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