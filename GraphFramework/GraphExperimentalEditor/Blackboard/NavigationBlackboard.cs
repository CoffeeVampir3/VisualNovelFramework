using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public class NavigationBlackboard : UnityEditor.Experimental.GraphView.Blackboard
    {
        //TODO:: Figure out how tf style sheets work /shrug
        private Foldout hack = new Foldout();
        public NavigationBlackboard(GraphView gv) : base(gv)
        {
            this.AddManipulator(new ContextualMenuManipulator(HandleContextMenu));
            this.Add(hack);
            hack.Q<VisualElement>("unity-checkmark").visible = false;
        }

        private void HandleContextMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target == this)
            {
                evt.menu.AppendAction("Add Category", NewCategoryRequest);
            }
        }
        
        private void NewCategoryRequest(DropdownMenuAction evt)
        {
            Debug.Log("Added");
            BlackboardCategory newCat = new BlackboardCategory("New Category");
            hack.contentContainer.Add(newCat);
        }
    }
}