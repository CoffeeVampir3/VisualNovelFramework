using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.DialogueGraph
{
    public class DialogueGraphView : GraphView
    {
        #region Window Specific
        private const string assetDir =
            @"Assets/GraphFramework/GraphExperimentalEditor/UITK/";
        
        public DialogueGraphView()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                assetDir + "BehaviourGraph.uss");

            styleSheets.Add(styleSheet);
            
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        #endregion

        #region Literal Garbage

        private Vector2 GetViewRelativePosition(Vector2 pos, Vector2 offset = default)
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

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compPorts = new List<Port>();
            
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node)
                {
                    compPorts.Add(port);
                }
            });

            return compPorts;
        }

        #endregion
    }
}
