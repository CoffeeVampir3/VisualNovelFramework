using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VisualNovelFramework.Outfitting
{
    [CreateAssetMenu]
    public class CharacterCompositor : ScriptableObject
    {
        public float layerAspectRatio;
        public List<CharacterLayer> layers = new List<CharacterLayer>();
        public List<CharacterPose> poses = new List<CharacterPose>();
        public List<LayeredPose> layeredPoses = new List<LayeredPose>();

        public CharacterLayer GetPosedLayer(CharacterLayer cl, CharacterPose cp)
        {
            for (int i = 0; i < layeredPoses.Count; i++)
            {
                if (layeredPoses[i].layer == cl && layeredPoses[i].pose == cp)
                {
                    return layeredPoses[i].posedLayer;
                }
            }

            return null;
        }

        public void SetPosedLayer(CharacterLayer cl, CharacterPose cp, CharacterLayer posedLayer)
        {
            for (int i = 0; i < layeredPoses.Count; i++)
            {
                if (layeredPoses[i].layer == cl && layeredPoses[i].pose == cp)
                {
                    layeredPoses[i].posedLayer = posedLayer;
                    return;
                }
            }
            
            layeredPoses.Add(LayeredPose.Create(cl, cp, posedLayer));
        }

        public Dictionary<CharacterLayer, CharacterLayer> layerSerializationDict 
            = new Dictionary<CharacterLayer, CharacterLayer>();
        
        public Dictionary<CharacterPose, CharacterPose> poseSerializationDict 
            = new Dictionary<CharacterPose, CharacterPose>();
        
        public Dictionary<CharacterLayer, CharacterLayer> posedLayerSerializationDict 
            = new Dictionary<CharacterLayer, CharacterLayer>();
        public void SerializeRecursive(Character saveTo)
        {
            var clone = Instantiate(this);
            saveTo.compositor = clone;
            clone.name = saveTo.name + "compositor";
            AssetDatabase.AddObjectToAsset(clone, saveTo);

            clone.layerSerializationDict.Clear();
            clone.poseSerializationDict.Clear();
            clone.posedLayerSerializationDict.Clear();

            clone.layers = new List<CharacterLayer>();
            clone.poses = new List<CharacterPose>();
            clone.layeredPoses = new List<LayeredPose>();

            layers.ForEach(e => e.SerializeRecursive(clone));
            poses.ForEach(e => e.SerializeRecursive(clone));
            layeredPoses.ForEach(e => e.SerializeRecursive(clone));
        }
    }
}