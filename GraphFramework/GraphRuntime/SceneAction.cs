using System;
using UnityEngine;

namespace VisualNovelFramework.GraphFramework.GraphRuntime
{
    [CreateAssetMenu]
    public class SceneAction : ScriptableObject
    {
        public Transform transform = new Transform();
        
        [Serializable]
        public class Transform
        {
            [SerializeField]
            public Vector2 anchorPosition = Vector2.zero;
            [SerializeField]
            public Vector3 scale = Vector3.one;

            public Vector2 GetScreenPositionUIE(float width, float height)
            {
                return new Vector2(anchorPosition.x * width, anchorPosition.y * height);
            }
            
            public Vector2 GetScreenPositionUnity(float width, float height)
            {
                return new Vector2(-.5f + (anchorPosition.x * width),
                    .5f - (anchorPosition.y * height));
            }
        }
    }
}