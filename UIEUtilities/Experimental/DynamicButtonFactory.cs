using UnityEngine.UIElements;

namespace VisualNovelFramework.Elements.Utils
{
    /// <summary>
    /// Helper functions to create consistent dynamic UI
    /// </summary>
    public static class DynamicButtonFactory
    {
        /// <summary>
        /// Creates the default style of dynamic list button.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Button CreateDefaultDynamicListButton(string text)
        {
            Button btn = new Button();
            btn.AddToClassList("--element-button");
            btn.text = text;

            return btn;
        }
    }
}