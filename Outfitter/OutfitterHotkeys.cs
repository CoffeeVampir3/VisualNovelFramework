using UnityEngine;

namespace VisualNovelFramework.Outfitter
{
    public partial class Outfitter
    {
        private void OnGUI()
        {
            Event current = Event.current;
            if (current.type != EventType.KeyDown)
                return;

            switch (current.keyCode)
            {
                case KeyCode.S:
                    CheckForSaveInputs(current);
                    return;
                case KeyCode.N:
                    if (current.control)
                    {
                        //Ctrl+N
                        NewOutfitMenu(null);
                        current.Use();
                    }
                    return;
                case KeyCode.R:
                    if (current.control)
                    {
                        //Ctrl+R
                        RenameOutfitMenu(null);
                        current.Use();
                    }
                    return;
                case KeyCode.L:
                    if (current.control)
                    {
                        //Ctrl+L
                        LoadCharacterMenu(null);
                        current.Use();
                    }
                    return;
            }
        }

        private void CheckForSaveInputs(Event current)
        {
            if (current.control)
            {
                if (current.shift)
                {
                    //Save as cntrl + shift + s
                    SaveOutfitAsMenu(null);
                    current.Use();
                    return;
                }

                //Save cntrl + s
                SaveOutfitMenu(null);
                current.Use();
            }
        }
    }
}