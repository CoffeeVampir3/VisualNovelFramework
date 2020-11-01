using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VisualNovelFramework.Outfitting;

namespace VisualNovelFramework.EditorOnly.CharacterSerializer
{
    public static class CompositorSerializer
    {
        public static Dictionary<CharacterLayer, CharacterLayer> layerSerializationDict 
            = new Dictionary<CharacterLayer, CharacterLayer>();
        
        public static Dictionary<CharacterPose, CharacterPose> poseSerializationDict 
            = new Dictionary<CharacterPose, CharacterPose>();
        
        public static Dictionary<CharacterLayer, CharacterLayer> posedLayerSerializationDict 
            = new Dictionary<CharacterLayer, CharacterLayer>();
        
        public static CharacterCompositor SerializeRecursive(Character saveTo, CharacterCompositor compositor)
        {
            var clone = ScriptableObject.Instantiate(compositor);
            saveTo.compositor = clone;
            clone.name = saveTo.name + "compositor";
            AssetDatabase.AddObjectToAsset(clone, saveTo);

            layerSerializationDict.Clear();
            poseSerializationDict.Clear();
            posedLayerSerializationDict.Clear();

            clone.layers = new List<CharacterLayer>();
            clone.poses = new List<CharacterPose>();
            clone.layeredPoses = new List<LayeredPose>();

            compositor.layers.ForEach(e => LayerSerializer.SerializeRecursive(clone, e));
            compositor.poses.ForEach(e => PoseSerializer.SerializeRecursive(clone, e));
            compositor.layeredPoses.ForEach(e => LayeredPoseSerializer.SerializeRecursive(clone, e));

            return clone;
        }
    }
}