using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.Elements;
using VisualNovelFramework.Elements.Utils;

namespace VisualNovelFramework.Outfitter
{
    public partial class Outfitter
    {
        [MenuItem("VNFramework/Outfitter")]
        public static void ShowOutfitter()
        {
            Outfitter wnd = GetWindow<Outfitter>();
            wnd.titleContent = new GUIContent("Outfitter");
        }
        
        private ModularList poseList = null;
        private ModularList layerList = null;
        
        private void SetupCompositorFrame()
        {
            layerList = rootVisualElement.Q<ModularList>("layerList");
            poseList = rootVisualElement.Q<ModularList>("poseList");
        }
        
        private void SetupSaveButton()
        {
            var saveBtn = rootVisualElement.Q<Button>("SaveCharBtn");

            saveBtn.clicked += OnSaveButtonClicked;
        }
        
        private ObjectField charSelector = null;
        private void SetupCharacterSelector()
        {
            charSelector = rootVisualElement.Q<ObjectField>("characterSelector");
            charSelector.objectType = typeof(Character);

            charSelector.RegisterValueChangedCallback(LoadCharacter);
            //charSelector.RegisterCallback<ClickEvent>(OnCharacterFieldClicked);
        }

        private OutfitPreviewer outfitPreviewer;
        private void SetupPreviewer()
        {
            var prvwPane = rootVisualElement.Q<VisualElement>("previewerPane");
            
            outfitPreviewer = new OutfitPreviewer();
            prvwPane.style.alignSelf = new StyleEnum<Align>(Align.Center);
            prvwPane.Add(outfitPreviewer);
            prvwPane.AddManipulator(new ZoomManipulator());
            prvwPane.AddManipulator(new ContentDragger());

            outfitPreviewer.visible = true;
        }

        private void SetupLayerListView()
        {
            layerImageLister = rootVisualElement.Q<ListView>("layerImageList");
            layerImageLister.itemsSource = images;
            layerImageLister.reorderable = false;
            layerImageLister.style.alignContent = new StyleEnum<Align>(Align.Center);
            layerImageLister.makeItem = () => new Image();
            layerImageLister.style.flexGrow = 1.0f;

            layerImageLister.bindItem = (e, i) =>
            {
                Image img = (e as Image);
                img.image = images[i];

                imageToIndex.Add(img, i);
                img.AddToClassList("charImgStyle");
                img.style.width = 200f;
                img.style.height = currentCompositor.layerAspectRatio * 200f;

                img.RegisterCallback<ClickEvent>(OnLayerItemClicked);
            };
        }
        
        public void OnEnable()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/VisualNovelFramework/Outfitter/Outfitter2.0.uxml");
            VisualElement tree = visualTree.Instantiate();
            root.Add(tree);

            var templateContainer = root.Children().FirstOrDefault();
            Debug.Assert(templateContainer != null, nameof(templateContainer) + " != null");
            templateContainer.style.flexGrow = 1;
            templateContainer.style.flexShrink = 1;
            templateContainer.style.flexBasis = new StyleLength(100f);
            
            SetupCompositorFrame();
            SetupCharacterSelector();
            SetupSaveButton();
            SetupLayerListView();
            SetupPreviewer();
        }
    }
}