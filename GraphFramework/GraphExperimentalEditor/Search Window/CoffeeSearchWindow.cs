using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Search_Window
{
    public class CoffeeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        public CoffeeGraphView graphView;

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