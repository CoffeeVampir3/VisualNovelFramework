using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public class NavBlackboardNodeItem : VisualElement
    {
        public readonly Foldout foldout;
        private NavBlackboardNodeItem nodeItemParent = null;
        public Node targetNode;
        
        public NavBlackboardNodeItem()
        {
            foldout = new Foldout();
            foldout.SetValueWithoutNotify(false);

            this.AddManipulator(new ContextualMenuManipulator(HandleContextMenu));
            Add(foldout);
        }

        public NavBlackboardNodeItem(string catName) : this()
        {
            name = catName;
            foldout.text = catName;
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
            nodeItemParent?.ChildDeleted();
        }

        private void ChildDeleted()
        {
            int catCount = contentContainer.Query<NavBlackboardNodeItem>().
                ToList().Count;
            
            if (catCount <= 1)
            {
                RemoveFromClassList("has-children");
            }
        }
        
        private void NewCategoryRequest(DropdownMenuAction evt)
        {
            NavBlackboardNodeItem newCat = new NavBlackboardNodeItem("New Category");
            AddToClassList("has-children");
            foldout.contentContainer.Add(newCat);
            foldout.SetValueWithoutNotify(true);
            newCat.nodeItemParent = this;
        }
    }
}