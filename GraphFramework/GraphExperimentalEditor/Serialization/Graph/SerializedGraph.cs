using UnityEditor;
using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public class SerializedGraph : ScriptableObject
    {
        //TODO:: Rework temporary debug path.
        private const string _DEBUG_assetPath = @"Assets/!TestTrashbin/";
 
        public static string _DEBUG_SAVE_PATH => _DEBUG_assetPath + "debug" + ".asset";
        private void CreateSerializedNodeAsset(NodeSerializationData serializedNode)
        {
            //Copy data.
            NodeSerializationData serializationCopy = Instantiate(serializedNode);
            NodeEditorData editorDataCopy = Instantiate(serializedNode.nodeEditorData);
            NodeRuntimeData runtimeDataCopy = Instantiate(serializedNode.nodeRuntimeData);
            
            //Rename data
            editorDataCopy.name = serializedNode.nodeEditorData.name;
            serializationCopy.nodeEditorData = editorDataCopy;
            serializationCopy.name = "S_" + editorDataCopy.name;
            serializationCopy.nodeRuntimeData = runtimeDataCopy;
            runtimeDataCopy.name = "R_" + editorDataCopy.name;
            
            AssetDatabase.AddObjectToAsset(serializationCopy, this);
            AssetDatabase.AddObjectToAsset(serializationCopy.nodeEditorData, this);
            AssetDatabase.AddObjectToAsset(serializationCopy.nodeRuntimeData, this);
            EditorUtility.SetDirty(serializationCopy);
            EditorUtility.SetDirty(serializationCopy.nodeEditorData);
            EditorUtility.SetDirty(serializationCopy.nodeRuntimeData);
        }
        
        public void WriteSerializedNode(NodeSerializationData serializedNode)
        {
            CreateSerializedNodeAsset(serializedNode);
        }

        /// <summary>
        /// Removes all previously added assets do we don't get write errors.
        /// </summary>
        public void FlushSavedAssets()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(_DEBUG_SAVE_PATH);
            foreach (var o in assets)
            {
                if (o != this)
                {
                    AssetDatabase.RemoveObjectFromAsset(o);
                }
            }
        }

        public static SerializedGraph CreateGraphDataAsset()
        {
            var graphData = CreateInstance<SerializedGraph>();
            graphData.name = "testGraphData";
            AssetDatabase.CreateAsset(graphData, _DEBUG_SAVE_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(graphData));
            AssetDatabase.Refresh();

            return graphData;
        }
    }
}