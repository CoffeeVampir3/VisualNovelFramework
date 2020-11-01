using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.Elements.Utils;
using VisualNovelFramework.Outfitting;

namespace VisualNovelFramework.CharacterBuilder
{
    /// <summary>
    ///     Window initialization for the character builder.
    /// </summary>
    public partial class CharacterBuilder
    {
        public const string CharacterBuilderPath =
            "Assets/VisualNovelFramework/EditorOnly/CharacterBuilder/CharacterBuilder.uxml";

        [MenuItem("VNFramework/Character Builder")]
        public static void ShowExample()
        {
            var wnd = GetWindow<CharacterBuilder>();
            wnd.titleContent = new GUIContent("CharacterBuilder");
        }

        private void SetupCompositorFrame()
        {
            layerList = rootVisualElement.Q<ModularList>("layerList");
            poseList = rootVisualElement.Q<ModularList>("poseList");

            var layerListOnAddClicked =
                new CreateNamedItemButton(layerList,
                    CreateInstance<CharacterLayer>, OnLayerItemSelected, "+");

            var poseListOnAddClicked =
                new CreateNamedItemButton(poseList,
                    CreateInstance<CharacterPose>, OnPoseItemSelected, "+");

            layerList.AddFoldoutDynamicButton(layerListOnAddClicked);
            poseList.AddFoldoutDynamicButton(poseListOnAddClicked);

            var layerRemoveBtn = new RemoveElementButton(layerList, OnLayerItemDelete, "-");
            var poseRemoveBtn = new RemoveElementButton(poseList, OnPoseItemDelete, "-");

            layerList.AddListItemDynamicButton(layerRemoveBtn);
            poseList.AddListItemDynamicButton(poseRemoveBtn);
        }

        private void SetupLayerSelector()
        {
            textureList = rootVisualElement.Q<ModularList>("layerTextures");
            multilayerToggle = rootVisualElement.Q<Toggle>("isLayerMultilayer");

            var searchAction = new SearchForTypeButton<Texture2D>(AddPickedItemToLayerList, "+");
            var removeButton = new RemoveElementButton(textureList, null, "-");

            textureList.AddFoldoutDynamicButton(searchAction);
            textureList.AddListItemDynamicButton(removeButton);

            multilayerToggle.RegisterValueChangedCallback(e =>
            {
                if (currentWorkingLayer != null) currentWorkingLayer.isMultilayer = e.newValue;
            });
        }

        private void SetupCharacterSelector()
        {
            charSelector = rootVisualElement.Q<ObjectField>("characterSelector");
            charSelector.objectType = typeof(Character);

            charSelector.RegisterValueChangedCallback(LoadCharacter);
            charSelector.RegisterCallback<ClickEvent>(OnCharacterFieldClicked);
        }

        private void SetupFileMenu()
        {
            var menu = rootVisualElement.Q<ToolbarMenu>("fileMenu");

            menu.menu.AppendAction("New (Ctrl+N)", CreateNewCharacterMenu);
            menu.menu.AppendAction("Load (Ctrl+L)", LoadCharacterMenu);
            menu.menu.AppendSeparator();
            menu.menu.AppendAction("Rename (Ctrl+R)", RenameCharacterMenu);
            menu.menu.AppendSeparator();
            menu.menu.AppendAction("Save (Ctrl+S)", SaveCharacterMenu);
            menu.menu.AppendAction("Save As (Ctrl+Shift+S)", SaveCharacterAsMenu);
        }

        public void OnEnable()
        {
            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CharacterBuilderPath);
            VisualElement labelFromUXML = visualTree.Instantiate();
            root.Add(labelFromUXML);

            var templateContainer = root.Children().FirstOrDefault();
            Debug.Assert(templateContainer != null, nameof(templateContainer) + " != null");
            templateContainer.style.flexGrow = 1;
            templateContainer.style.flexShrink = 1;

            previewer = root.Q<VisualElement>("previewImageContainer");

            aspectRatioField = root.Q<FloatField>("aspectRatio");
            aspectRatioField.RegisterCallback<ChangeEvent<float>>(e =>
            {
                if (currentCompositor != null) currentCompositor.layerAspectRatio = e.newValue;
            });

            //Setup order does not matter.
            SetupFileMenu();
            SetupLayerSelector();
            SetupCompositorFrame();
            SetupCharacterSelector();

            HideCompositorFrame();
        }
    }
}