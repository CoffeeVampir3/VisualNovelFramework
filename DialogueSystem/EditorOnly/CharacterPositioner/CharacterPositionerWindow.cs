using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.DialogueSystem.Nodes;
using VisualNovelFramework.DialogueSystem.VNScene;

public class CharacterPositionerWindow : EditorWindow
{
    [SerializeField]
    public RuntimeCharacterNode rtCharNode;
    [SerializeField] 
    private VisualTreeAsset positionerWindowXML;

    [MenuItem("VNFramework/Test Window")]
    private static void ShowWindow()
    {
        var wnd = GetWindow<CharacterPositionerWindow>();
        wnd.titleContent = new GUIContent("Tester Window");
    }

    private void LoadCharacterNode()
    {
        CharacterDisplayer cd = new CharacterDisplayer();
        cd.DisplayOutfit(rtCharNode.outfit);

        templateContainer.Q<VisualElement>("sceneContainer").Add(cd);
        cd.SendToBack();
        cd.RegisterCallback<GeometryChangedEvent>(Reposition);
    }

    private CharacterDisplayer currentlyManipulatedCharacterDisplayer;
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

        currentlyManipulatedCharacterDisplayer = cd;
    }

    private VisualElement templateContainer;
    private void OnInitializationGeo(GeometryChangedEvent geoChange)
    {
        // Import UXML
        rootVisualElement.Add(positionerWindowXML.Instantiate());

        templateContainer = rootVisualElement.Children().FirstOrDefault();
        Debug.Assert(templateContainer != null, nameof(templateContainer) + " != null");
        templateContainer.style.flexGrow = 1;
        templateContainer.style.flexShrink = 1;
        templateContainer.style.flexBasis = new StyleLength(100f);

        var btn = rootVisualElement.Q<ToolbarButton>("dbgBtn");
        btn.clicked += SavePosition;

        LoadCharacterNode();

        rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnInitializationGeo);
    }

    private void SavePosition()
    {
        if (currentlyManipulatedCharacterDisplayer == null)
        {
            Debug.Log("No character displayer set. How did this happen!? Plz report bug.");
            return;
        }

        var sceneCont = templateContainer.Q<VisualElement>("sceneContainer");
        var width = sceneCont.layout.width;
        var height = sceneCont.layout.height;
        var pos = currentlyManipulatedCharacterDisplayer.transform.position;
        
        SerializedObject so = new SerializedObject(rtCharNode);
        so.FindProperty("spawnPosition").vector2Value = new Vector2(pos.x / width, pos.y / height);
        so.ApplyModifiedProperties();
    }

    public void OnEnable()
    {
        rootVisualElement.StretchToParentSize();
        rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnInitializationGeo);
    }
}