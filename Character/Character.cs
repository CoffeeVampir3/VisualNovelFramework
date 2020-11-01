using System.Collections.Generic;
using UnityEngine;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.Outfitting;

namespace VisualNovelFramework
{
    //Characters maintain their refs and they're fine to directly reference.
    public class Character : ScriptableObject, HasCoffeeGUID
    {
        public CharacterCompositor compositor;
        [SerializeField]
        public List<CharacterOutfit> outfits = new List<CharacterOutfit>();
        [SerializeField]
        private string characterGUID = "";

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