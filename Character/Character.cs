using System.Collections.Generic;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;

namespace VisualNovelFramework.VNCharacter
{
    //Characters maintain their refs and they're fine to directly reference.
    public class Character : ScriptableObject, HasCoffeeGUID
    {
        [SerializeField] 
        private string characterGUID = "";
        public CharacterCompositor compositor;
        [SerializeField] 
        public List<CharacterOutfit> outfits = new List<CharacterOutfit>();

        public string GetCoffeeGUID()
        {
            return characterGUID;
        }

        public void SetCoffeeGUID(string GUID)
        {
            characterGUID = GUID;
        }
    }
}