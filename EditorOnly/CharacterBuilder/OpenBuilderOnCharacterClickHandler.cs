using System;
using UnityEditor.Callbacks;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.VNCharacter;

namespace VisualNovelFramework.Editor.CharacterBuilder
{
    public class
        OpenBuilderOnCharacterClickHandler : OpenWindowOnAssetClickedHandler<Character,
            CharacterBuilder>
    {
        private static readonly Type[] dockNextTo = {typeof(Outfitter.Outfitter)};
        [OnOpenAsset(1)]
        public static bool OnSerializedGraphOpened(int instanceID, int line)
        {
            var window = IsOpenedAssetTargetType(instanceID, 
                out var item, 
                dockNextTo);
            if (window != null)
            {
                window.LoadFromExternal(item);
                return false;
            }

            return false;
        }
    }
}