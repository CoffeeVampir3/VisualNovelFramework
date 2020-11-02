using UnityEditor;
using UnityEngine;

namespace VisualNovelFramework.GraphFramework.Serialization
{
    public class SerializedGraph : ScriptableObject
    {
        //TODO:: Rework temporary debug path.
        private const string _DEBUG_assetPath = @"Assets/GraphFramework/GraphExperimentalEditor/TestTrashbin/";
 
        public static string _DEBUG_SAVE_PATH => _DEBUG_assetPath + "debug" + ".asset";
        private void CreateSerializedNodeAsset(NodeSerializationData serializedNode)
        {
            //Copy data.
            NodeEditorData editorDataCopy = Instantiate(serializedNode.nodeEditorData);
            NodeSerializationData serializationCopy = Instantiate(serializedNode);
            serializationCopy.nodeEditorData = editorDataCopy;
            serializationCopy.name = "S_" + editorDataCopy.name;

            Debug.Log(serializationCopy.name);
            AssetDatabase.AddObjectToAsset(serializationCopy, this);
            AssetDatabase.AddObjectToAsset(serializationCopy.nodeEditorData, this);
            EditorUtility.SetDirty(serializationCopy);
            EditorUtility.SetDirty(serializationCopy.nodeEditorData);
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