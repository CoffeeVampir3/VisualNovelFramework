using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Editor.Nodes;
using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public abstract class CoffeeGraphView : GraphView
    {
        [SerializeReference]
        public BaseNode rootNode;

        public CoffeeGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            graphViewChanged = OnGraphViewChanged;
        }

        //TODO:: Bad temporary code
        private const string edgeStylePath =
            "Assets/VisualNovelFramework/GraphFramework/GraphExperimentalEditor/UITK/Styles/BasePortDefaultStyle.uss";

        private StyleSheet edgeStyleSheet = null;
        GraphViewChange OnGraphViewChanged(GraphViewChange changes)
        {
            if (changes.edgesToCreate != null)
            {
                foreach (var edge in changes.edgesToCreate)
                {
                    edge.styleSheets.Clear();
                    if (edgeStyleSheet == null)
                    {
                        edgeStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(edgeStylePath);
                    }
                    edge.styleSheets.Add(edgeStyleSheet);
                }
            }

            return default;
        }

        protected Vector2 GetViewRelativePosition(Vector2 pos, Vector2 offset = default)
        {
            //What the fuck unity. NEGATIVE POSITION???
            Vector2 relPos = new Vector2(
                -viewTransform.position.x + pos.x,
                -viewTransform.position.y + pos.y);

            //Hold the offset as a static value by scaling it in the reverse direction of our scale
            //This way we "undo" the division by scale for only the offset value, scaling everything else.
            relPos -= (offset*scale);
            return relPos/scale;
        }

        [SerializeReference]
        private BaseNode lastEvaluatedNode = null;
        /// <summary>
        /// TODO:: Temporary code, should have a faster means of accessing nodes.
        /// </summary>
        public void RuntimeNodeVisited(RuntimeNode node)
        {
            if (!(nodes.ToList().FirstOrDefault(e =>
            {
                if (e is BaseNode bn)
                {
                    return bn.RuntimeData == node;
                }

                return false;
            }) is BaseNode dataNode))
                return;

            lastEvaluatedNode?.OnNodeExited();
            dataNode.OnNodeEntered();
            lastEvaluatedNode = dataNode;
        }
    }
}
