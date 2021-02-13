using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor.Nodes;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public class NavigationBlackboard : UnityEditor.Experimental.GraphView.Blackboard
    {
        private readonly StyleSheet categoryStyle;
        //Necessary for some stupid reason, without this the alignment is fucked.
        private readonly ListView listBase = new ListView();
        public NavigationBlackboard(CoffeeGraphView gv) : base(gv)
        {
            this.AddManipulator(new ContextualMenuManipulator(HandleContextMenu));
            styleSheets.Add(gv.settings.blackboardStyle);
            categoryStyle = gv.settings.blackboardCategoryStyle;
            listBase.AddToClassList("list-base");
            Add(listBase);
            SetupListView();
        }

        private void SetupListView()
        {
            listBase.reorderable = false;
            listBase.makeItem = () => new NavBlackboardNodeItem();
            listBase.bindItem = BindNodeToList;
            listBase.itemHeight = 22;
        }
        
        private void BindNodeToList(VisualElement ele, int index)
        {
            if (!(ele is NavBlackboardNodeItem bc)) 
                return;
            bc.styleSheets.Add(categoryStyle);
            if (listBase.itemsSource[index] is Node node)
            {
                bc.foldout.text = node.name;
                bc.targetNode = node;
                bc.RegisterCallback<ClickEvent>(OnLabelClicked);
            }
        }

        private void OnLabelClicked(ClickEvent evt)
        {
            if (!(evt.currentTarget is NavBlackboardNodeItem bbItem))
            {
                return;
            }
            
            Node targetNode = bbItem.targetNode;
            
            ((CoffeeGraphView) graphView).CenterViewOnAndSelectNode(targetNode);

            graphView.ClearSelection();
            graphView.AddToSelection(targetNode);
        }

        private List<Node> listItemNodes = new List<Node>();
        private void DelayedRefresh()
        {
            refreshingImminent = false;
            listBase.itemsSource = null;
            listItemNodes = graphView.nodes.ToList();
            listBase.itemsSource = listItemNodes;
            listBase.Refresh();
        }

        //This setup lets us refresh only once every 100MS at most, so we arent constantly
        //building and rebuilding the blackboard during say, a copy-paste operation
        //or during loading, but it's still quick enough to not be perceivable to the user.
        private bool refreshingImminent = false;
        public void RequestRefresh()
        {
            if (!refreshingImminent)
            {
                schedule.Execute(DelayedRefresh).StartingIn(100);
                refreshingImminent = true;
            }
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
        }
    }
}