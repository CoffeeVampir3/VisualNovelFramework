using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.DialogueSystem.VNScene;
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
        [SerializeField]
        public Vector3 spawnScale = Vector3.one;

        private VisualElement spawnTarget;
        public void CreateCharacter(VisualElement spawnAsChildOf)
        {
            spawnTarget = spawnAsChildOf;
            CharacterDisplayer cd = new CharacterDisplayer(); 
            cd.DisplayOutfit(outfit);
            spawnAsChildOf.Add(cd);
            cd.SendToBack();
            cd.RegisterCallback<GeometryChangedEvent>(Reposition);
        }

        private void Reposition(GeometryChangedEvent geoChange)
        {
            var cd = geoChange.currentTarget as CharacterDisplayer;
            var sceneCont = spawnTarget;
            var width = sceneCont.layout.width;
            var height = sceneCont.layout.height;

            var posX = spawnPosition.x * width;
            var posY = spawnPosition.y * height;

            cd.transform.position = new Vector2(posX, posY);
            cd.transform.scale = spawnScale;
            cd.UnregisterCallback<GeometryChangedEvent>(Reposition);
        }
        
        public override void OnEvaluate()
        {
            var doc = GameObject.FindObjectOfType<UIDocument>();
            var templateContainer = doc.rootVisualElement;
            var sceneCont = templateContainer.Q<VisualElement>("sceneView");

            CreateCharacter(sceneCont);
        }
    }
}