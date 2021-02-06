using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.GraphFramework.Attributes;
using VisualNovelFramework.DialogueSystem.VNScene;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.VNCharacter;

namespace VisualNovelFramework.DialogueSystem.Nodes
{
    public class RuntimeCharacterNode : RuntimeNode
    {
        [SerializeField]
        public Character swag;
        [SerializeField, ReadonlyField]
        public CharacterOutfit outfit;
        
        //HideInInspector appears to be bugged, but this is a fine workaround.
        [SerializeField, HideInInspector]
        public Vector2 spawnPosition;
        [SerializeField, HideInInspector]
        public Vector3 spawnScale = Vector3.one;
        
        private VisualElement spawnTarget;
        private void CreateCharacter(VisualElement spawnAsChildOf)
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
            if (cd == null)
                return;
            
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
            var doc = FindObjectOfType<UIDocument>();
            var templateContainer = doc.rootVisualElement;
            var sceneCont = templateContainer.Q<VisualElement>("sceneView");

            CreateCharacter(sceneCont);
        }
    }
}