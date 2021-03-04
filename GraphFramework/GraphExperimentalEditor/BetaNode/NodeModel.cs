using System;
using UnityEditor;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.BetaNode
{
    [Serializable]
    public class NodeModel
    {
        [SerializeReference]
        public RuntimeNode RuntimeData;

        public void CreateRuntimeData(BetaEditorGraph editorGraph, Type runtimeDataType)
        {
            //This magic deserve some explanation:
            //In a declaration like DialogueNode : NodeModel<RuntimeDialogueNode>
            //This code looks at NodeModel<RuntimeDialogueNode>
            //And extracts the type between the <>, RuntimeDialogueNode
            RuntimeData = ScriptableObject.CreateInstance(runtimeDataType) as RuntimeNode;
            AssetDatabase.AddObjectToAsset(RuntimeData, editorGraph);
            EditorUtility.SetDirty(editorGraph);
        }

        [SerializeField] 
        public bool isRoot = false;
        [SerializeField]
        public string GUID;
        [SerializeField] 
        private string nodeTitle = "Untitled.";
        [SerializeField] 
        private Rect position = Rect.zero;
        [SerializeField] 
        private bool isExpanded = true;
        
        [NonSerialized] 
        private NodeView view;

        public NodeView View => view;
        
        public NodeView CreateView()
        {
            view = new NodeView(this);
            view.Display();
            return view;
        }

        public void UpdatePosition()
        {
            position = view.GetPosition();
        }
        
        public string NodeTitle
        {
            get => nodeTitle;

            set
            {
                nodeTitle = value;
                view.OnDirty();
            }
        }

        public Rect Position
        {
            get => position;
            set
            {
                position = value;
                view.OnDirty();
            }
        }

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                view.OnDirty();
            }
        }
    }
}