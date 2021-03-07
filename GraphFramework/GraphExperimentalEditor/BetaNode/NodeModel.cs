using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoffeeExtensions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelFramework.GraphFramework.Attributes;
using VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.BetaNode
{
    [Serializable]
    public class NodeModel
    {
        [SerializeReference]
        public RuntimeNode RuntimeData;
        [SerializeReference]
        public List<PortModel> inputPorts = new List<PortModel>();
        [SerializeReference]
        public List<PortModel> outputPorts = new List<PortModel>();

        public static NodeModel InstantiateModel(BetaEditorGraph editorGraph)
        {
            var model = new NodeModel();
            model.CreateRuntimeData(editorGraph, typeof(ModelTester));
            model.CreatePortModels();
            return model;
        }

        public NodeModel Clone(BetaEditorGraph editorGraph)
        {
            NodeModel model = new NodeModel();
            model.CreateRuntimeDataClone(editorGraph, RuntimeData);
            model.RuntimeData.name = RuntimeData.name;
            model.CreatePortModels(true);
            return model;
        }

        public void CreateRuntimeData(BetaEditorGraph editorGraph, Type runtimeDataType)
        {
            RuntimeData = ScriptableObject.CreateInstance(runtimeDataType) as RuntimeNode;
            AssetDatabase.AddObjectToAsset(RuntimeData, editorGraph);
            EditorUtility.SetDirty(editorGraph);
        }
        
        public void CreateRuntimeDataClone(BetaEditorGraph editorGraph, RuntimeNode toCopy)
        {
            RuntimeData = ScriptableObject.Instantiate(toCopy);
            AssetDatabase.AddObjectToAsset(RuntimeData, editorGraph);
            EditorUtility.SetDirty(editorGraph);
        }

        public PortModel CreatePortModel(FieldInfo field, Direction dir)
        {
            var portValueType = field.FieldType.
                GetGenericClassConstructorArguments(typeof(ValuePort<>));
            return new PortModel(Orientation.Horizontal, dir, 
                Port.Capacity.Multi, portValueType.FirstOrDefault(), field);
        }

        public void CreatePortModels(bool clearCopy = false)
        {
            var oFields = RuntimeData.GetType().GetLocalFieldsWithAttribute<Out>();
            var iFields = RuntimeData.GetType().GetLocalFieldsWithAttribute<In>();

            foreach (var field in iFields)
            {
                PortModel p = CreatePortModel(field, Direction.Input);
                inputPorts.Add(p);
                if (!clearCopy) continue;
                if (field.GetValue(RuntimeData) is ValuePort vp)
                {
                    vp.connections.Clear();
                }
            }
            foreach (var field in oFields)
            {
                PortModel p = CreatePortModel(field, Direction.Output);
                outputPorts.Add(p);
                if (!clearCopy) continue;
                if (field.GetValue(RuntimeData) is ValuePort vp)
                {
                    vp.connections.Clear();
                }
            }
        }

        [SerializeField] 
        public bool isRoot = false;
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