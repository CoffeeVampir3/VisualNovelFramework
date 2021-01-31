using UnityEngine;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.VNCharacter;

namespace VisualNovelFramework.DialogueSystem.Nodes
{
    public class RuntimeCharacterNode : RuntimeNode
    {
        [SerializeField]
        public Character swag;
        [SerializeField]
        public CharacterOutfit outfit;
        [SerializeField]
        public Vector2 spawnPosition;
    }
}