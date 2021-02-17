﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.Attributes;

namespace VisualNovelFramework.GraphFramework.Editor
{
    /// <summary>
    /// Helper class to provide the node search tree for our graph view using RegisterNodeToView
    /// attribute.
    /// </summary>
    public static class CoffeeGraphNodeSearchTreeProvider
    {
        private static List<Type> GetNodesRegisteredToView(Type graphViewType)
        {
            var nodeList = TypeCache.GetTypesWithAttribute<RegisterNodeToView>();

            List<Type> registeredNodes = new List<Type>();
            foreach (var node in nodeList)
            {
                var attr = node.
                    GetCustomAttributes(typeof(RegisterNodeToView), false)[0] 
                    as RegisterNodeToView;

                //Type cache ensures this is never null.
                // ReSharper disable once PossibleNullReferenceException
                if(attr.registeredGraphView == graphViewType)
                    registeredNodes.Add(node);
            }

            return registeredNodes;
        }
        
        #region Node Search Tree Parser

        private static Dictionary<(string directory, int depth), SearchTreeGroupEntry> dirToGroup;
        private static Dictionary<SearchTreeGroupEntry, List<SearchTreeEntry>> groupToEntry;

        private static SearchTreeGroupEntry CreateDirectory(string directory, int depth)
        {
            (string directory, int depth) dDir = (directory, depth);
            if (dirToGroup.TryGetValue(dDir, out var searchGroup)) 
                return searchGroup;
            
            searchGroup = new SearchTreeGroupEntry(new GUIContent(directory), depth);
            dirToGroup.Add(dDir, searchGroup);
            return searchGroup;
        }

        private static void CreateEntry(Type entryNodeType, SearchTreeGroupEntry parent, string entryName, int depth)
        {
            if (!groupToEntry.TryGetValue(parent, out var entryList))
            {
                entryList = new List<SearchTreeEntry>();
                groupToEntry.Add(parent, entryList);
            }

            SearchTreeEntry nEntry = new SearchTreeEntry(new GUIContent(entryName))
            {
                level = depth, userData = entryNodeType
            };

            entryList.Add(nEntry);
        }

        /// <summary>
        /// Returns a search tree of our registered nodes for the given graph view type.
        /// </summary>
        public static List<SearchTreeEntry> CreateNodeSearchTreeFor(Type graphViewType)
        {
            var nodeList = GetNodesRegisteredToView(graphViewType);
            dirToGroup = new Dictionary<(string directory, int depth), SearchTreeGroupEntry>();
            groupToEntry = new Dictionary<SearchTreeGroupEntry, List<SearchTreeEntry>>();

            List<SearchTreeGroupEntry> allGroups = new List<SearchTreeGroupEntry>();
            
            //Create top entry of our search tree.
            SearchTreeGroupEntry top = new SearchTreeGroupEntry(
                new GUIContent("Create Elements"));
            allGroups.Add(top);
            
            //Iterate through each registered node and create the layout infrastructure.
            foreach (var item in nodeList)
            {
                var attr = item.
                    GetCustomAttributes(typeof(RegisterNodeToView), false)[0] as RegisterNodeToView;

                Debug.Assert(attr != null, nameof(attr) + " != null");
                var split = attr.registeredPath.Split('/');
                SearchTreeGroupEntry lastGroup = top;
                for (int i = 0; i < split.Length; i++)
                {
                    string cur = split[i];

                    if (cur == "")
                        break;

                    if (i == split.Length - 1)
                    {
                        CreateEntry(item, lastGroup, cur, i+1);
                        break;
                    }
                    
                    lastGroup = CreateDirectory(cur, i+1);
                    if (!allGroups.Contains(lastGroup))
                    {
                        allGroups.Add(lastGroup);
                    }
                }
            }

            List<SearchTreeEntry> searchTree = new List<SearchTreeEntry> {top};
            //Iterate through each group and create our final search tree.
            foreach (var group in allGroups)
            {
                //Guards against top being added twice.
                if (!searchTree.Contains(group))
                {
                    searchTree.Add(group);
                }
                //For the groups with leafs:
                if(groupToEntry.TryGetValue(group, out var entries))
                {
                    searchTree.AddRange(entries);
                }
            }

            return searchTree;
        }
        
        #endregion
    }
}