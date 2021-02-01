using UnityEngine;
using VisualNovelFramework.EditorExtensions;

namespace VisualNovelFramework.VNCharacter
{
    /// <summary>
    ///     Effectively a dynamic enumeration.
    /// </summary>
    public class CharacterPose : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField] 
        private string GUID;
        
        public string GetCoffeeGUID()
        {
            return GUID;
        }

        public void SetCoffeeGUID(string newGUID)
        {
            GUID = newGUID;
        }
    }
}