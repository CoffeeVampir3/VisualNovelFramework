﻿using System.Collections.Generic;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO;

namespace VisualNovelFramework.GraphFramework.GraphRuntime
{
    public class RuntimeNode : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField, HideInInspector]
        public string GUID;
        [SerializeField] 
        public List<Connection> connections = new List<Connection>();

        public virtual RuntimeNode GetNextNode()
        {
            return null;
        }

        public virtual void OnEvaluate()
        {
            Debug.Log(this.name);
        }

        public string GetCoffeeGUID()
        {
            return GUID;
        }

        public void SetCoffeeGUID(string newGuid)
        {
            GUID = newGuid;
        }
    }
}