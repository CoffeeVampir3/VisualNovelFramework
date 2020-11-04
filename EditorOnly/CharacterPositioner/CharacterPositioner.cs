using System.Linq;
using UnityEditor;
using UnityEditor.ShaderGraph.Drawing;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.Editor.Elements;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.GraphFramework.GraphRuntime;
using VisualNovelFramework.VNCharacter;
using ContentDragger = VisualNovelFramework.Editor.Outfitter.ContentDragger;

namespace VisualNovelFramework.EditorOnly.CharacterPositioner
{
    public class CharacterPositioner : EditorWindow
    {
        private const string windowXMLPath =
            "Assets/VisualNovelFramework/EditorOnly/CharacterPositioner/CharacterPositioner.uxml";

        private const string charStyleXML = "Assets/VisualNovelFramework/EditorOnly/Outfitter/CharacterImageStyle.uss";
        
        [MenuItem("VNFramework/CharacterPositioner")]
        private static void ShowWindow()
        {
            var wnd = GetWindow<CharacterPositioner>();
            wnd.titleContent = new GUIContent("CharacterPositioner");
        }

        private OutfitPreviewer outfitPreviewer;
        private void TestSetup()
        {
            var m = CoffeeAssetDatabase.FindAssetsOfType<Character>();
            if (m.Count == 0)
                return;

            var testChar = m[0];
            var testOutfit = testChar.outfits[0];
            
            outfitPreviewer = new OutfitPreviewer();

            outfitPreviewer.style.minWidth = rootVisualElement.layout.width;
            outfitPreviewer.style.minHeight = rootVisualElement.layout.height;

            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(charStyleXML);
            outfitPreviewer.styleSheets.Add(style);
            
            outfitPreviewer.DisplayOutfit(testOutfit);
            outfitPreviewer.AddManipulator(new MouseWheelResizer(outfitPreviewer));
            outfitPreviewer.AddManipulator(new ContentDragger());

            rootVisualElement.Add(outfitPreviewer);
            
            outfitPreviewer.RegisterCallback<GeometryChangedEvent>(OnPreviewerGeo);
        }

        private void SetupDebugSaveButton()
        {
            var m = rootVisualElement.Q<Button>("testButton");

            m.clicked += OnClick;
        }

        private void OnClick()
        {
            var m = CoffeeAssetDatabase.FindAssetsOfType<SceneAction>();
            if (m.Count == 0)
                return;

            float anchorX = rootVisualElement.parent.layout.width;
            float anchorY = rootVisualElement.parent.layout.height;
            float posPointX = outfitPreviewer.transform.position.x / anchorX;
            float posPointY = outfitPreviewer.transform.position.y / anchorY;
            
            m[0].transform.anchorPosition = new Vector2(posPointX, posPointY);
            m[0].transform.scale = outfitPreviewer.transform.scale;
        }

        private void OnPreviewerGeo(GeometryChangedEvent geoChange)
        {
            var m = CoffeeAssetDatabase.FindAssetsOfType<SceneAction>();
            if (m.Count == 0)
                return;
            
            float anchorX = rootVisualElement.parent.layout.width;
            float anchorY = rootVisualElement.parent.layout.height;
            outfitPreviewer.transform.position = m[0].transform.
                GetScreenPositionUIE(anchorX, anchorY);
            outfitPreviewer.transform.scale = m[0].transform.scale;
            
            outfitPreviewer.UnregisterCallback<GeometryChangedEvent>(OnPreviewerGeo);
        }

        private void OnInitializationGeo(GeometryChangedEvent geoChange)
        {
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(windowXMLPath);
            VisualElement tree = visualTree.Instantiate();
            rootVisualElement.Add(tree);
            
            var templateContainer = rootVisualElement.Children().FirstOrDefault();
            Debug.Assert(templateContainer != null, nameof(templateContainer) + " != null");
            templateContainer.style.flexGrow = 1;
            templateContainer.style.flexShrink = 1;
            templateContainer.style.flexBasis = new StyleLength(100f);
        
            SetupDebugSaveButton();
            TestSetup();
            
            rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnInitializationGeo);
        }

        private void OnEnable()
        {
            rootVisualElement.StretchToParentSize();
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnInitializationGeo);
        }
        
    }
}