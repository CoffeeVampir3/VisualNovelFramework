using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public class BlackboardCategory : VisualElement
    {
        private readonly Foldout foldout;
        private readonly VisualElement foldoutIcon;
        private BlackboardCategory categoryParent = null;

        public BlackboardCategory(string catName)
        {
            name = catName;
            foldout = new Foldout {text = catName};
            foldout.SetValueWithoutNotify(false);
            foldoutIcon = foldout.Q<VisualElement>("unity-checkmark");
            foldoutIcon.visible = false;

            this.AddManipulator(new ContextualMenuManipulator(HandleContextMenu));
            Add(foldout);
        }
        
        private void HandleContextMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target != this) 
                return;
            
            evt.menu.AppendAction("Add Sub-Category", NewCategoryRequest);
            evt.menu.AppendAction("Delete", DeleteCategoryRequest);
        }
        
        private void DeleteCategoryRequest(DropdownMenuAction evt)
        {
            parent.Remove(this);
            categoryParent?.ChildDeleted();
        }

        private void ChildDeleted()
        {
            int catCount = this.contentContainer.Query<BlackboardCategory>().
                ToList().Count;
            
            if (catCount <= 1)
            {
                foldoutIcon.visible = false;
            }
        }
        
        private void NewCategoryRequest(DropdownMenuAction evt)
        {
            Debug.Log("Added");
            BlackboardCategory newCat = new BlackboardCategory("New Category");
            foldoutIcon.visible = true;
            foldout.contentContainer.Add(newCat);
            foldout.SetValueWithoutNotify(true);
            
            Texture2D icon = Texture2D.whiteTexture;
            foldout.style.backgroundImage = new StyleBackground(icon);
            newCat.categoryParent = this;
        }
    }
}