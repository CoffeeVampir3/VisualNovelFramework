using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelFramework.GraphFramework.Editor;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Search_Window
{
    public class CoffeeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private CoffeeGraphView graphView;

        public void Init(CoffeeGraphView parentGraphView)
        {
            graphView = parentGraphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = CoffeeGraphNodeSearchTreeProvider.
                CreateNodeSearchTreeFor(graphView.GetType());

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            graphView.CreateNode(SearchTreeEntry.userData as Type, context.screenMousePosition);
            return true;
        }
    }
}