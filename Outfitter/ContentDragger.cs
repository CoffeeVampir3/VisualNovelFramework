using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.Outfitter
{
    /// <summary>
    /// Pans the content container of a Visual Element that implements IHasContentViewContainer.
    /// </summary>
    public class ContentDragger : MouseManipulator
    {
        /// <summary>
        /// Local coordinates of dragged element where it was touched when dragging began.
        /// </summary>
        private Vector2 m_Start;

        /// <summary>
        /// True if target has captured mouse and is being dragged.
        /// </summary>
        protected bool m_Active;

        //public Vector2 panSpeed { get; set; }

        /// <summary>
        /// True if the dragged VisualElement should be clamped to its parent's boundaries. 
        /// </summary>
        public bool clampToParentEdges { get; set; }

        public ContentDragger()
        {
            m_Active = false;
            activators.Add(new ManipulatorActivationFilter
                {button = MouseButton.LeftMouse, modifiers = EventModifiers.Alt});
            activators.Add(new ManipulatorActivationFilter {button = MouseButton.MiddleMouse});
            clampToParentEdges = true;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (m_Active)
            {
                evt.StopImmediatePropagation();
                return;
            }
            if (evt.target == null)
            {
                return;
            }

            var pork = evt.currentTarget as VisualElement;
            var chicken = pork.parent;

            Vector2 dist = pork.transform.position - chicken.transform.position;
            
            m_Start = evt.mousePosition - dist;
            m_Active = true;
            target.CaptureMouse();
            evt.StopImmediatePropagation();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!m_Active)
                return;
            
            if (evt.target == null)
            {
                return;
            }
            
            if ((evt.currentTarget is VisualElement ve))
            {          
                Vector2 scaledPos = evt.mousePosition;
                ve.transform.position = scaledPos - m_Start;
            }

            evt.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            //(evt.currentTarget as VisualElement).transform.position = evt.localMousePosition;

            m_Active = false;
            target.ReleaseMouse();
            evt.StopPropagation();
        }
    }
}