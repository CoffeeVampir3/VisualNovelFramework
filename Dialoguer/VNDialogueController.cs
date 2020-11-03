using Sirenix.OdinInspector;
using UnityEngine;

namespace VisualNovelFramework
{
    public class VNDialogueController : MonoBehaviour
    {
        [TextArea(10, 10)] 
        public string testText = "This is the <b>thing</b> to display.";

        [SerializeField] 
        private TMPShakeParser shakeParser = null;

        [SerializeField] 
        private TMPTeletyper teletyper = null;
        
        [SerializeField] 
        private TMPVertexer vertexManipulator = null;

        public void DisplayString(string tString)
        {
            vertexManipulator.StopShake();

            teletyper.ClearAndSet(tString);
            var shakeRegions = shakeParser.ParseString(tString);
            if (shakeRegions != null) vertexManipulator.SetShake(shakeRegions);

            teletyper.StartTeletype();
        }

        [Button]
        private void TestDisplay()
        {
            DisplayString(testText);
        }
    }
}