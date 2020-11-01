using UnityEditor;
using UnityEngine;
using VisualNovelFramework.Outfitting;

namespace VisualNovelFramework.EditorOnly.CharacterSerializer
{
    public static class PoseSerializer
    {
        public static void SerializeRecursive(CharacterCompositor saveTo, CharacterPose pose)
        {
            var clone = ScriptableObject.Instantiate(pose);
            clone.name = pose.name;
            AssetDatabase.AddObjectToAsset(clone, saveTo);
            saveTo.poses.Add(clone);

            CompositorSerializer.poseSerializationDict.Add(pose, clone);
        }
    }
}