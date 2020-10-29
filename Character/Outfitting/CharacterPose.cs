using UnityEditor;
using UnityEngine;

namespace VisualNovelFramework.Outfitting
{
    /// <summary>
    /// Effectively a dynamic enumeration.
    /// </summary>
    [CreateAssetMenu]
    public class CharacterPose : ScriptableObject
    {
        public void SerializeRecursive(CharacterCompositor saveTo)
        {
            var clone = Instantiate(this);
            clone.name = this.name;
            AssetDatabase.AddObjectToAsset(clone, saveTo);
            saveTo.poses.Add(clone);
            
            saveTo.poseSerializationDict.Add(this, clone);
        }
    }
}