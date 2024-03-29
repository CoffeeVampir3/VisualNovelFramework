﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VisualNovelFramework
{
    public class TMPVertexer : MonoBehaviour
    {
        private bool shake = false;
        private List<TMPShakeParser.ShakeRegion> shakeRegions;
        public TMP_Text target;

        public void SetShake(List<TMPShakeParser.ShakeRegion> regions)
        {
            shakeRegions = regions;
            shake = true;
        }

        public void StopShake()
        {
            shakeRegions?.Clear();
            shake = false;
        }

        public void LateUpdate()
        {
            if (!shake) return;

            target.ForceMeshUpdate();
            var textInfo = target.textInfo;

            foreach (var region in shakeRegions)
                for (var i = region.start; i < region.end; i++)
                {
                    var charInfo = textInfo.characterInfo[i];

                    if (!charInfo.isVisible)
                        continue;

                    var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                    for (var j = 0; j < 4; j++)
                    {
                        var orig = verts[charInfo.vertexIndex + j];
                        verts[charInfo.vertexIndex + j] =
                            orig + new Vector3(Mathf.Cos(Time.time * 2f * orig.y * 0.01f) * 5f,
                                Mathf.Sin(Time.time * 2f * orig.x * 0.01f) * 5f, 0);
                    }
                }

            for (var i = 0; i < textInfo.meshInfo.Length; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                target.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }
}