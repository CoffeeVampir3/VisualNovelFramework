using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.Editor.Outfitter
{
    public class ZoomManipulator : Manipulator
    {
        public static readonly float DefaultReferenceScale = 1f;
        public static readonly float DefaultMinScale = 0.05f;
        public static readonly float DefaultMaxScale = 2.5f;
        public static readonly float DefaultScaleStep = 0.1f;

        /// <summary>
        ///     Scale that should be computed when scroll wheel offset is at zero.
        /// </summary>
        public float referenceScale { get; set; } = DefaultReferenceScale;

        public float minScale { get; set; } = DefaultMinScale;
        public float maxScale { get; set; } = DefaultMaxScale;

        /// <summary>
        ///     Relative scale change when zooming in/out (e.g. For 15%, use 0.15).
        /// </summary>
        /// <remarks>
        ///     Depending on the values of <c>minScale</c>, <c>maxScale</c> and <c>scaleStep</c>, it is not guaranteed that
        ///     the first and last two scale steps will correspond exactly to the value specified in <c>scaleStep</c>.
        /// </remarks>
        public float scaleStep { get; set; } = DefaultScaleStep;

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<WheelEvent>(OnWheel);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<WheelEvent>(OnWheel);
        }

        private void OnWheel(WheelEvent evt)
        {
            var ve = evt.currentTarget as VisualElement;
            if (ve == null)
                return;

            var panel = ve.panel;
            if (panel.GetCapturingElement(PointerId.mousePointerId) != null
            ) //if something else is already capturing mouse, return
                return;

            var position = ve.transform.position;
            var scale = ve.transform.scale;

            // TODO: augment the data to have the position as well, so we don't have to read in data from the target.
            // 0-1 ranged center relative to size
            var zoomCenter = ve.ChangeCoordinatesTo(ve, evt.localMousePosition);
            var x = zoomCenter.x + ve.layout.x;
            var y = zoomCenter.y + ve.layout.y;

            position += Vector3.Scale(new Vector3(x, y, 0), scale);

            // Apply the new zoom.
            var zoom = CalculateNewZoom(scale.y, -evt.delta.y, scaleStep, referenceScale, minScale, maxScale);
            scale.x = zoom;
            scale.y = zoom;
            scale.z = 1;

            position -= Vector3.Scale(new Vector3(x, y, 0), scale);

            ve.transform.position = position;
            ve.transform.scale = scale;

            evt.StopPropagation();
        }

        // Compute the parameters of our exponential model:
        // z(w) = (1 + s) ^ (w + a) + b
        // Where
        // z: calculated zoom level
        // w: accumulated wheel deltas (1 unit = 1 mouse notch)
        // s: zoom step
        //
        // The factors a and b are calculated in order to satisfy the conditions:
        // z(0) = referenceZoom
        // z(1) = referenceZoom * (1 + zoomStep)
        //https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/GraphViewEditor/Manipulators/Zoomer.cs
        private static float CalculateNewZoom(float currentZoom, float wheelDelta, float zoomStep, float referenceZoom,
            float minZoom, float maxZoom)
        {
            if (minZoom <= 0 || referenceZoom < minZoom || referenceZoom > maxZoom || zoomStep < 0)
            {
                Debug.Log("One of zooming constraints is violated.");
                return currentZoom;
            }

            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            if (Mathf.Approximately(wheelDelta, 0)) return currentZoom;

            // Calculate the factors of our model:
            var a = Math.Log(referenceZoom, 1 + zoomStep);
            var b = referenceZoom - Math.Pow(1 + zoomStep, a);

            // Convert zoom levels to scroll wheel values.
            var minWheel = Math.Log(minZoom - b, 1 + zoomStep) - a;
            var maxWheel = Math.Log(maxZoom - b, 1 + zoomStep) - a;
            var currentWheel = Math.Log(currentZoom - b, 1 + zoomStep) - a;

            wheelDelta = Math.Sign(wheelDelta);
            currentWheel += wheelDelta;

            // Assimilate to the boundary when it is nearby.
            if (currentWheel > maxWheel - 0.5) return maxZoom;

            if (currentWheel < minWheel + 0.5) return minZoom;

            // Snap the wheel to the unit grid.
            currentWheel = Math.Round(currentWheel);

            // Do not assimilate again. Otherwise, points as far as 1.5 units away could be stuck to the boundary
            // because the wheel delta is either +1 or -1.

            // Calculate the corresponding zoom level.
            return (float) (Math.Pow(1 + zoomStep, currentWheel + a) + b);
        }
    }
}