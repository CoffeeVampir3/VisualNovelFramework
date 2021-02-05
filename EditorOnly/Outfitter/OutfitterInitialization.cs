using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.Editor.Elements;
using VisualNovelFramework.VNCharacter;

namespace VisualNovelFramework.Editor.Outfitter
{
    public partial class Outfitter
    {
        public const string OutfitterXMLPath =
            "Assets/VisualNovelFramework/EditorOnly/Outfitter/Outfitter2.0.uxml";

        private OutfitDropdownWindow outfitDropdown = null;

        private OutfitPreviewer outfitPreviewer;

        [MenuItem("VNFramework/Outfitter")]
        public static void ShowOutfitter()
        {
            var wnd = GetWindow<Outfitter>();
            wnd.titleContent = new GUIContent("Outfitter");
        }
        
        public void LoadFromExternal(Character character)
        {
            if (character == null)
                return;
            
            titleContent = new GUIContent("Outfitter");
            charSelector = rootVisualElement.Q<ObjectField>("characterSelector");
            charSelector.SetValueWithoutNotify(character);
            LoadCharacter(character);
        }

        private void SetupCharacterSelector()
        {
            charSelector = rootVisualElement.Q<ObjectField>("characterSelector");
            charSelector.objectType = typeof(Character);

            charSelector.RegisterValueChangedCallback(LoadCharacter);
        }

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
                var img = e as Image;
                img.image = images[i];

                imageToIndex.Add(img, i);
                img.AddToClassList("charImgStyle");

                var aspectRatio = (float) images[i].height / images[i].width;
                img.style.width = layerImageLister.layout.width;
                img.style.height = layerImageLister.layout.width * aspectRatio;

                layerImageLister.itemHeight = (int) (layerImageLister.layout.width * aspectRatio);

                img.RegisterCallback<ClickEvent>(OnTextureItemClicked);
            };
        }

        private void SetupFileMenu()
        {
            var menu = rootVisualElement.Q<ToolbarMenu>("fileMenu");

            menu.menu.AppendAction("Load Character (Ctrl+L)", LoadCharacterMenu);
            menu.menu.AppendAction("New Outfit (Ctrl+N)", NewOutfitMenu);
            menu.menu.AppendSeparator();
            menu.menu.AppendAction("Rename Outfit (Ctrl+R)", RenameOutfitMenu);
            menu.menu.AppendAction("Delete Outfit", DeleteOutfitMenu);
            menu.menu.AppendSeparator();
            menu.menu.AppendAction("Save Outfit (Ctrl+S)", SaveOutfitMenu);
            menu.menu.AppendAction("Save Outfit As (Ctrl+Shift+S)", SaveOutfitAsMenu);
        }

        private void SetupOutfitDropdown()
        {
            var menu = rootVisualElement.Q<ToolbarMenu>("outfitDropdown");

            menu.RegisterCallback<ClickEvent>(e =>
            {
                if (currentCharacter == null)
                    return;

                var newPos = new Rect(position.x + menu.worldBound.x,
                    position.y - 360, 200, 400);
                outfitDropdown = CreateInstance<OutfitDropdownWindow>();
                outfitDropdown.ShowAsDropDown(newPos, new Vector2(200, 400));

                outfitDropdown.browser.BindToList(currentCharacter.outfits, OnOutfitClicked);
            });
        }

        private void OnOutfitClicked(Object target)
        {
            if (target == null || !(target is CharacterOutfit co))
                return;

            LoadOutfit(co);
            if (outfitDropdown != null)
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
            var root = rootVisualElement;

            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(OutfitterXMLPath);
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