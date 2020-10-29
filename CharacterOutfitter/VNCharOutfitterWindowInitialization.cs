using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VisualNovelFramework.Elements;
using VisualNovelFramework.Outfitting;
using Debug = UnityEngine.Debug;

namespace VisualNovelFramework.Outfitter
{
    /// <summary>
    /// These are parts of the initialization which are not coupled to the outfitting system.
    /// </summary>
    public partial class VNCharOutfitter
    {
        private readonly Dictionary<Image, int> imageToIndex = new Dictionary<Image, int>();

        private void SetupCompositorSelector()
        {
            ObjectField ccSelector = rootVisualElement.Q<ObjectField>("characterCompositorSelector");
            ccSelector.objectType = typeof(CharacterCompositor);

            ccSelector.RegisterValueChangedCallback(LoadCompositor);
        }
        
        private void SetupLayerListView()
        {
            layerImageLister = rootVisualElement.Q<ListView>("layerList");
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

        private void SaveOutfit()
        {
            if (workingOutfit == null)
            {
                Debug.LogError("Working outfit was null?");
                return;
            }
            
            Debug.Log("Saved.");
        }

        private void LoadOutfit()
        {
            Debug.LogError("Not Implemented");
        }
        
        public void OnEnable()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/VisualNovelFramework/CharacterOutfitter/VNCharOutfitter.uxml");
            VisualElement tree = visualTree.Instantiate();
            root.Add(tree);

            var templateContainer = root.Children().FirstOrDefault();
            Debug.Assert(templateContainer != null, nameof(templateContainer) + " != null");
            templateContainer.style.flexGrow = 1;
            templateContainer.style.flexShrink = 1;
            templateContainer.style.flexBasis = new StyleLength(100f);

            characterDisplayerRoot = tree.Q<VisualElement>("characterDisplayerRoot");

            var prvwPane = tree.Q<VisualElement>("previewerPane");
            
            outfitPreviewer = new OutfitPreviewer();
            prvwPane.style.alignSelf = new StyleEnum<Align>(Align.Center);
            prvwPane.AddManipulator(new ZoomManipulator());
            prvwPane.AddManipulator(new ContentDragger());
            prvwPane.Add(outfitPreviewer);

            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    "Assets/VisualNovelFramework/CharacterOutfitter/CharacterImageStyle.uss");
            characterDisplayerRoot.styleSheets.Add(styleSheet);
            
            Button saveButton = tree.Q<Button>("SaveOutfit");
            saveButton.clicked += SaveOutfit;
            
            Button loadButton = tree.Q<Button>("LoadOutfit");
            loadButton.clicked += LoadOutfit;

            SetupLayerListView();
            SetupCompositorSelector();
        }
    }
}