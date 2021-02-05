using UnityEditor.Callbacks;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.AssetHandlers
{
    public class SerializedGraphAssetHandler : OpenWindowOnAssetClickedHandler<SerializedGraph, DialogueGraph.DialogueGraph>
    {
        [OnOpenAsset]
        public static bool OnSerializedGraphOpened(int instanceID, int line)
        {
            var window = IsOpenedAssetTargetType(instanceID, out var graph);
            if (window != null)
            {
                window.LoadGraph(graph);
                return true;
            }
            return false;
        }
    }
}