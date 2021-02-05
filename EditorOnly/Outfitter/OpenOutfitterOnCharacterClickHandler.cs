using System;
using UnityEditor.Callbacks;
using VisualNovelFramework.EditorExtensions;
using VisualNovelFramework.VNCharacter;

namespace VisualNovelFramework.Editor.Outfitter
{
    public class
        OpenOutfitterOnCharacterClickHandler : OpenWindowOnAssetClickedHandler<Character,
            Outfitter>
    {
        private static readonly Type[] dockNextTo = 
             {typeof(CharacterBuilder.CharacterBuilder)};
        
        [OnOpenAsset(2)]
        public static bool OnSerializedGraphOpened(int instanceID, int line)
        {
            var window = IsOpenedAssetTargetType(instanceID, 
                out var item,
                dockNextTo);
            if (window != null)
            {
                window.LoadFromExternal(item);
                return true;
            }

            return false;
        }
    }
}