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
using Debug = UnityEngine.Debug;

namespace VisualNovelFramework.EditorOnly.CharacterPositioner
{
    public class CharacterPositioner : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset windowXML;
        [SerializeField] 
        private StyleSheet charStyleXML;
        [SerializeField]
        public Character targetCharacter;
        [SerializeField] 
        public CharacterOutfit targetOutfit;
        [SerializeField] 
        public SceneAction targetAction;
        [SerializeField] 
        private VNGlobalSettings settings;
        
        [MenuItem("VNFramework/CharacterPositioner")]
        private static void ShowWindow()
        {
            var wnd = GetWindow<CharacterPositioner>();
            wnd.titleContent = new GUIContent("CharacterPositioner");
        }

        private OutfitPreviewer outfitPreviewer;
        private void InitializeSetup()
        {
            outfitPreviewer = new OutfitPreviewer();
            outfitPreviewer.styleSheets.Add(charStyleXML);
            
            outfitPreviewer.DisplayOutfit(targetOutfit);
            outfitPreviewer.AddManipulator(new MouseWheelResizer(outfitPreviewer));
            outfitPreviewer.AddManipulator(new ContentDragger());

            var cl = rootVisualElement.Q<VisualElement>("characterLayer");
            cl.Add(outfitPreviewer);

            outfitPreviewer.RegisterCallback<GeometryChangedEvent>(OnPreviewerGeo);
        }

        private void SetupDebugSaveButton()
        {
            var m = rootVisualElement.Q<Button>("testButton");

            m.clicked += OnClick;
        }

        private void OnClick()
        {
            curWindowWidth = rootVisualElement.parent.layout.width;
            curWindowHeight = rootVisualElement.parent.layout.height;

            var currentPos = outfitPreviewer.transform.position;

            float posPointX = (currentPos.x) / curWindowWidth;
            float posPointY = (currentPos.y) / curWindowHeight;

            targetAction.transform.position = new Vector2(posPointX, posPointY);
            targetAction.transform.scale = targetAction.transform.ScaleFromWindow(
                outfitPreviewer.transform.scale, 
                curWindowWidth, curWindowHeight,
                settings.targetResolution.x, 
                settings.targetResolution.y);
        }

        private float curWindowWidth;
        private float curWindowHeight;
        /// <summary>
        /// Rescales the outfit previewers
        /// </summary>
        private void OnWindowResize(GeometryChangedEvent geo)
        {
            var oldPos = outfitPreviewer.transform.position;
            float posPointX = (oldPos.x) / curWindowWidth;
            float posPointY = (oldPos.y) / curWindowHeight;
            
            curWindowWidth = rootVisualElement.parent.layout.width;
            curWindowHeight = rootVisualElement.parent.layout.height;
            
            Vector3 scale = targetAction.transform.ScaleToWindow(
                curWindowWidth, curWindowHeight, 
                settings.targetResolution.x, 
                settings.targetResolution.y);

            outfitPreviewer.transform.scale = scale;
            
            var newPos = new Vector3(posPointX * curWindowWidth, posPointY * curWindowHeight);
            outfitPreviewer.transform.position = newPos;
        }

        private void OnPreviewerGeo(GeometryChangedEvent geoChange)
        {
            curWindowWidth = rootVisualElement.parent.layout.width;
            curWindowHeight = rootVisualElement.parent.layout.height;
            
            Vector3 scale = targetAction.transform.ScaleToWindow(
                curWindowWidth, curWindowHeight, 
                settings.targetResolution.x, 
                settings.targetResolution.y);

            outfitPreviewer.transform.position = targetAction.transform.
                GetScreenPositionUIE(curWindowWidth, curWindowHeight);
            outfitPreviewer.transform.scale = scale;
            
            outfitPreviewer.UnregisterCallback<GeometryChangedEvent>(OnPreviewerGeo);
        }

        private void OnInitializationGeo(GeometryChangedEvent geoChange)
        {
            windowXML.Instantiate();
            rootVisualElement.Add(windowXML.Instantiate());
            
            var templateContainer = rootVisualElement.Children().FirstOrDefault();
            Debug.Assert(templateContainer != null, nameof(templateContainer) + " != null");
            templateContainer.style.flexGrow = 1;
            templateContainer.style.flexShrink = 1;
            templateContainer.style.flexBasis = new StyleLength(100f);
        
            SetupDebugSaveButton();
            InitializeSetup();
            
            rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnInitializationGeo);
            
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnWindowResize);
        }

        private void OnEnable()
        {
            rootVisualElement.StretchToParentSize();
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnInitializationGeo);
        }
        
    }
}