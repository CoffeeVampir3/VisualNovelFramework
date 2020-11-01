using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.Elements.Utils
{
    // This does not inherit DynamicButton because it should not be used in the same circumstances.
    /// <summary>
    /// A dynamic button designed for the ModularList that binds to an object and the modular list's
    /// view.
    /// These buttons are dynamically cloned per-instance and must implement a complete deep-copy.
    /// </summary>
    public abstract class BindingDynamicButton
    {
        protected Object bindingObject;
        protected ListView boundView;
        protected Button button;
        public Button Button => button;
        
        public void Bind(ListView lv, Object o)
        {
            bindingObject = o;
            boundView = lv;
        }

        public abstract BindingDynamicButton DeepCopy();
    }
}