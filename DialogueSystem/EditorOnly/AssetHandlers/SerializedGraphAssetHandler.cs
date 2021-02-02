using UnityEditor;
using UnityEditor.Callbacks;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.AssetHandlers
{
    public class SerializedGraphAssetHandler
    {
        [OnOpenAsset(1)]
        public static bool OnAnyGraphAssetOpened(int instanceID, int line)
        {
            var graphs = CoffeeAssetDatabase.FindAssetsOfType<SerializedGraph>();

            foreach (var graph in graphs)
            {
                if (graph.GetInstanceID() == instanceID)
                {
                    var window = EditorWindow.GetWindow<DialogueGraph.DialogueGraph>();
                    window.LoadGraph(graph);
                    window.Show();
                    return true;
                }
            }
            return false;
        }
    }
}