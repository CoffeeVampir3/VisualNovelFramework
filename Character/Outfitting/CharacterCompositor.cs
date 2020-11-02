using System.Collections.Generic;
using UnityEngine;

namespace VisualNovelFramework.VNCharacter
{
    public class CharacterCompositor : ScriptableObject
    {
        public float layerAspectRatio;
        public List<LayeredPose> layeredPoses = new List<LayeredPose>();
        public List<CharacterLayer> layers = new List<CharacterLayer>();
        public List<CharacterPose> poses = new List<CharacterPose>();

        public CharacterLayer GetPosedLayer(CharacterLayer cl, CharacterPose cp)
        {
            for (var i = 0; i < layeredPoses.Count; i++)
                if (layeredPoses[i].layer == cl && layeredPoses[i].pose == cp)
                    return layeredPoses[i].posedLayer;

            return null;
        }

        public void SetPosedLayer(CharacterLayer cl, CharacterPose cp, CharacterLayer posedLayer)
        {
            for (var i = 0; i < layeredPoses.Count; i++)
                if (layeredPoses[i].layer == cl && layeredPoses[i].pose == cp)
                {
                    layeredPoses[i].posedLayer = posedLayer;
                    return;
                }

            layeredPoses.Add(LayeredPose.Create(cl, cp, posedLayer));
        }
    }
}