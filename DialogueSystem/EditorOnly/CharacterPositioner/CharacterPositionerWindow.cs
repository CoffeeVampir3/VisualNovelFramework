using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.DialogueSystem.Nodes;
using VisualNovelFramework.DialogueSystem.VNScene;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.VNCharacter;

public class CharacterPositionerWindow : EditorWindow
{
    [SerializeField]
    private RuntimeCharacterNode rtCharNode;

    [MenuItem("VNFramework/Test Window")]
    private static void ShowWindow()
    {
        var wnd = GetWindow<CharacterPositionerWindow>();
        wnd.titleContent = new GUIContent("Tester Window");
    }

    private void LoadCharacterNode()
    {
        var character = rtCharNode.swag;
        
        CharacterDisplayer cd = new CharacterDisplayer();
        cd.DisplayOutfit(rtCharNode.outfit);

        templateContainer.Q<VisualElement>("sceneContainer").Add(cd);
        cd.SendToBack();
        cd.RegisterCallback<GeometryChangedEvent>(Reposition);
    }

    private void Reposition(GeometryChangedEvent geoChange)
    {
        var cd = geoChange.currentTarget as CharacterDisplayer;
        var sceneCont = templateContainer.Q<VisualElement>("sceneContainer");
        var width = sceneCont.layout.width;
        var height = sceneCont.layout.height;

        var posX = rtCharNode.spawnPosition.x * width;
        var posY = rtCharNode.spawnPosition.y * height;

        cd.transform.position = new Vector2(posX, posY);
        cd.AddManipulator(new CharacterDragManipulator());
        cd.AddManipulator(new CharacterWheelResizer(cd));
    }

    private VisualElement templateContainer;
    private void OnInitializationGeo(GeometryChangedEvent geoChange)
    {
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UIETester/testerWindow.uxml");
        rootVisualElement.Add(visualTree.Instantiate());

        templateContainer = rootVisualElement.Children().FirstOrDefault();
        Debug.Assert(templateContainer != null, nameof(templateContainer) + " != null");
        templateContainer.style.flexGrow = 1;
        templateContainer.style.flexShrink = 1;
        templateContainer.style.flexBasis = new StyleLength(100f);

        var btn = rootVisualElement.Q<ToolbarButton>("dbgBtn");

        LoadCharacterNode();

        rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnInitializationGeo);
    }

    public void OnEnable()
    {
        rootVisualElement.StretchToParentSize();
        rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnInitializationGeo);
    }
}