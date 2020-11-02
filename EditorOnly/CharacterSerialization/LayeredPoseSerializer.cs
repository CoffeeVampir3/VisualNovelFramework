using UnityEditor;
using UnityEngine;
using VisualNovelFramework.VNCharacter;

namespace VisualNovelFramework.Editor.Serialization
{
    public static class LayeredPoseSerializer
    {
        public static void SerializeRecursive(CharacterCompositor saveTo, LayeredPose lp)
        {
            //Discard if the layer was deleted.
            if (!CompositorSerializer.layerSerializationDict.TryGetValue(lp.layer, out var newLayer)) 
                return;
            if (!CompositorSerializer.poseSerializationDict.TryGetValue(lp.pose, out var newPose)) 
                return;

            var clone = ScriptableObject.Instantiate(lp.posedLayer);
            clone.name = "Pose: " + lp.pose.name + " Layer: " + lp.layer.name;
            AssetDatabase.AddObjectToAsset(clone, saveTo);

            var clonedLayeredPose = LayeredPose.Create(newLayer, newPose, clone);
            clonedLayeredPose.name = lp.pose.name + "-" + lp.layer.name;
            AssetDatabase.AddObjectToAsset(clonedLayeredPose, saveTo);
            saveTo.layeredPoses.Add(clonedLayeredPose);

            CompositorSerializer.posedLayerSerializationDict.Add(lp.posedLayer, clone);
        }
    }
}