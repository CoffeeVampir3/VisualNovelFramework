using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.DialogueSystem.Nodes;
using VisualNovelFramework.DialogueSystem.VNScene;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.VNCharacter;

public class CharacterPositionerWindow : EditorWindow
{
    [SerializeField]
    private CharacterOutfit testOutfit;
    [SerializeField]
    private SerializedGraph testGraph;

    private RuntimeNode currentNode = null;
    
    [MenuItem("VNFramework/Test Window")]
    private static void ShowWindow()
    {
        var wnd = GetWindow<CharacterPositionerWindow>();
        wnd.titleContent = new GUIContent("Tester Window");
    }
    
    public RuntimeNode WalkGraphNextNode()
    {
        if (currentNode == null)
        {
            currentNode = testGraph.rootNode;
        }

        currentNode = currentNode.outputConnections.FirstOrDefault();
        return currentNode;
    }

    private List<CharacterDisplayer> displayers = new List<CharacterDisplayer>();
    public void ProcessNode(RuntimeNode node)
    {
        if (node is RuntimeCharacterNode charNode)
        {
            CharacterDisplayer cd = new CharacterDisplayer(); 
            Debug.Log(templateContainer.name);
            cd.DisplayOutfit(charNode.outfit);
            templateContainer.Q<VisualElement>("sceneContainer").Add(cd);
            cd.SendToBack();
            
            cd.AddManipulator(new CharacterDragManipulator());
            cd.AddManipulator(new CharacterWheelResizer(cd));

            displayers.Add(cd);
        }
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

        WalkGraphNextNode();
        ProcessNode(currentNode);
        WalkGraphNextNode();
        ProcessNode(currentNode);
        
        var btn = rootVisualElement.Q<ToolbarButton>("dbgBtn");

        btn.clicked += DebugObjectStates;

        rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnInitializationGeo);
    }

    public void DebugObjectStates()
    {
        foreach (var disp in displayers)
        {
            var sceneCont = templateContainer.Q<VisualElement>("sceneContainer");

            var width = sceneCont.layout.width;
            var height = sceneCont.layout.height;

            var posX = disp.transform.position.x / width;
            var posY = disp.transform.position.y / height;
            
            Debug.Log("Character:");
            Debug.Log(posX + " , " +  posY);
            Debug.Log(disp.transform.scale);

            disp.transform.position = new Vector2(width * .5f, height * .5f);
        }
    }

    public void OnEnable()
    {
        rootVisualElement.StretchToParentSize();
        rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnInitializationGeo);
    }
}