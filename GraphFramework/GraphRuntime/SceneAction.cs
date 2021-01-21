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
            public Vector2 position = Vector2.zero;
            [SerializeField]
            public Vector3 scale = Vector3.one;

            /// <summary>
            /// Gets the relative position as an appropriate value for UI elements.
            /// (Origin 0,0 top left)
            /// </summary>
            /// <param name="width"></param>
            /// <param name="height"></param>
            /// <returns></returns>
            public Vector2 GetScreenPositionUIE(float width, float height)
            {
                return new Vector2(position.x * width, position.y * height);
            }
            
            public Vector3 ScaleToWindow(float windowWidth, float windowHeight, 
                float targetWidth, float targetHeight)
            {
                var inverseScalingX = windowWidth / targetWidth;
                var inverseScalingY = windowHeight / targetHeight;
            
                var newScale = new Vector3(scale.x * inverseScalingX, 
                    scale.y * inverseScalingY, 1f);

                return newScale;
            }
            
            public Vector3 ScaleFromWindow(Vector3 oldScale, float windowWidth, float windowHeight, 
                float targetWidth, float targetHeight)
            {
                var scalingX = targetWidth / windowWidth;
                var scalingY = targetHeight / windowHeight;
            
                oldScale = new Vector3(oldScale.x * scalingX, 
                    oldScale.y * scalingY, 1f);

                return oldScale;
            }
        }
    }
}