using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.Elements;
using VisualNovelFramework.Elements.Utils;
using VisualNovelFramework.Outfitting;

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
        
        private void SetupCharacterSelector()
        {
            charSelector = rootVisualElement.Q<ObjectField>("characterSelector");
            charSelector.objectType = typeof(Character);

            charSelector.RegisterValueChangedCallback(LoadCharacter);
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

                img.RegisterCallback<ClickEvent>(OnTextureItemClicked);
            };
        }
        
        private void SetupFileMenu()
        {
            var menu = rootVisualElement.Q<ToolbarMenu>("fileMenu");
            
            menu.menu.AppendAction("Load Character", LoadCharacterMenu);
            menu.menu.AppendAction("New Outfit", NewOutfitMenu);
            menu.menu.AppendSeparator();
            menu.menu.AppendAction("Rename Outfit", RenameOutfitMenu);
            menu.menu.AppendAction("Save Outfit to Character", SaveOutfitMenu);
            menu.menu.AppendSeparator();
            menu.menu.AppendAction("Delete Outfit From Character", DeleteOutfitMenu);
        }

        private OutfitDropdownWindow outfitDropdown = null;
        private void SetupOutfitDropdown()
        {
            var menu = rootVisualElement.Q<ToolbarMenu>("outfitDropdown");

            menu.RegisterCallback<ClickEvent>((e) =>
            {
                if (currentCharacter == null)
                    return;
                
                Rect newPos = new Rect(this.position.x + menu.worldBound.x,
                    this.position.y - 360, 200, 400);
                outfitDropdown = CreateInstance<OutfitDropdownWindow>();
                outfitDropdown.ShowAsDropDown(newPos, new Vector2(200, 400));
                
                outfitDropdown.browser.BindToList(currentCharacter.outfits, OnOutfitClicked);
            });
        }

        private void OnOutfitClicked(UnityEngine.Object target)
        {
            if (target == null || !(target is CharacterOutfit co)) 
                return;
            
            LoadOutfit(co);
            if(outfitDropdown != null)
                outfitDropdown.Close();
        }
        
        private void SetupOutfitLabel()
        {
            var label = rootVisualElement.Q<VisualElement>("outfitLabel");
            outfitLabel = label.Q<Label>("itemLabel");
            outfitLabel.SetEnabled(false);
            outfitLabel.text = "No Outfit Selected.";
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
            
            layerList = rootVisualElement.Q<ModularList>("layerList");
            poseList = rootVisualElement.Q<ModularList>("poseList");

            SetupOutfitLabel();
            SetupFileMenu();
            SetupCharacterSelector();
            SetupLayerListView();
            SetupOutfitDropdown();
            SetupPreviewer();
        }
    }
}