using UnityEngine;
using VisualNovelFramework.EditorExtensions;

namespace VisualNovelFramework.DialogueSystem.VNScene
{
    public class DialogueCoordinate : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField] 
        public Vector2 screenRelPosition;
        [SerializeField] 
        private string dialogueGUID = "";
        public string GetCoffeeGUID()
        {
            return dialogueGUID;
        }

        public void SetCoffeeGUID(string GUID)
        {
            dialogueGUID = GUID;
        }
    }
}