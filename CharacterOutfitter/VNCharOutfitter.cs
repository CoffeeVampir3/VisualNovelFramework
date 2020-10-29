using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using VisualNovelFramework.Elements;
using VisualNovelFramework.Outfitting;
using Object = UnityEngine.Object;

namespace VisualNovelFramework.Outfitter
{
    public partial class VNCharOutfitter : EditorWindow
    {
        //[MenuItem("VNFramework/Outfitter")]
        public static void ShowOutfitter()
        {
            VNCharOutfitter wnd = GetWindow<VNCharOutfitter>();
            wnd.titleContent = new GUIContent("Outfitter");
        }

        private VisualElement characterDisplayerRoot;
        private OutfitPreviewer outfitPreviewer;
        private CharacterCompositor currentCompositor = null;
        private CharacterLayer currentSelectedLayer = null;
        private readonly List<Texture2D> images = new List<Texture2D>();
        private CharacterOutfit workingOutfit = null;
        private ListView layerImageLister;

        private void LoadDefaultToWorkingOutfit(CharacterCompositor compositor)
        {
            if (workingOutfit != null)
            {
                //TODO:: Warn user about saving first!!!
                workingOutfit = null;
            }

            workingOutfit = CreateInstance<CharacterOutfit>();

            foreach (var cl in compositor.layers)
            {
                if (cl.isMultilayer)
                    continue;

                //workingOutfit.SetLayerDefault(cl);
            }
        }

        private void LoadLayerImages(CharacterCompositor compositor)
        {
            currentCompositor = compositor;

            if (workingOutfit == null)
            {
                Debug.LogError("Working outfit uninitialized?");
            }

            outfitPreviewer.DisplayOutfit(workingOutfit);
        }

        private void LoadLayers(CharacterCompositor compositor)
        {
            //layerSelector
            Foldout selectorFoldout = rootVisualElement.Q<Foldout>("layerSelector");
            selectorFoldout.Clear();
            for (int i = 0; i < compositor.layers.Count; i++)
            {
                SerializedObject so = new SerializedObject(compositor.layers[i]);
                ObjectField of = new ObjectField();
                of.Bind(so);
                of.objectType = typeof(CharacterLayer);
                of.SetValueWithoutNotify(compositor.layers[i]);

                if (compositor.layers[i].isMultilayer)
                {
                    of.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
                }
                of.RegisterCallback<ClickEvent>(LoadLayerList);
                selectorFoldout.Add(of);
            }
        }

        private void SelectActiveLayerItem(CharacterLayer cl)
        {
            if (!workingOutfit.outfitDictionary.TryGetValue(cl, out var items)) 
                return;
            
            if (items.Count <= 0)
                return;
            
            layerImageLister.ScrollToItem(0);
            //layerImageLister.SetSelectionWithoutNotify();
        }
        
        private void LoadLayerList(ClickEvent evt)
        {
            if (!(evt.target is ObjectField obField)) 
                return;
            
            if (obField.value == null || !(obField.value is CharacterLayer cl)) 
                return;

            if (workingOutfit == null)
                workingOutfit = CreateInstance<CharacterOutfit>();

            images.Clear();
            currentSelectedLayer = cl;
            layerImageLister.itemHeight = (int)(currentCompositor.layerAspectRatio * 200f);
            for (int i = 0; i < cl.textures.Count; i++)
            {
                images.Add(cl.GetTextureAt(i));
            }

            layerImageLister.Refresh();
            layerImageLister.ClearSelection();
            SelectActiveLayerItem(currentSelectedLayer);
        }

        private void LoadCompositor(ChangeEvent<Object> e)
        {
            CharacterCompositor compositor = e.newValue as CharacterCompositor;
            if (compositor != null)
            {
                var layerPanel = rootVisualElement.Q<VisualElement>("LayerPanel");
                float newWidth = Mathf.Clamp(200f, 200f, 800f);
                layerPanel.style.maxWidth = newWidth;
                layerPanel.style.minWidth = newWidth;
                layerPanel.visible = true;
                
                workingOutfit = CreateInstance<CharacterOutfit>();
                characterDisplayerRoot.visible = true;
                
                //LoadDefaultToWorkingOutfit(compositor);
                //LoadLayerImages(compositor);
                LoadLayers(compositor);
            }
            else
            {
                var layerPanel = rootVisualElement.Q<VisualElement>("LayerPanel");
                layerPanel.visible = false;
                characterDisplayerRoot.visible = false;
                currentCompositor = null;
                workingOutfit = null;
            }
        }

        private void OnLayerItemClicked(ClickEvent e)
        {
            if (currentSelectedLayer == null)
                return;

            if (!(e.currentTarget is Image img)) 
                return;

            if (!imageToIndex.TryGetValue(img, out var index)) 
                return;
            
            workingOutfit.AddOrRemoveExistingItem(currentSelectedLayer, index);
            outfitPreviewer.DisplayOutfit(workingOutfit);
        }
        
    }
}