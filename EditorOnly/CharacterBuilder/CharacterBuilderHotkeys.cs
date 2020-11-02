using UnityEngine;

namespace VisualNovelFramework.Editor.CharacterBuilder
{
    public partial class CharacterBuilder
    {
        private void OnGUI()
        {
            var current = Event.current;
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
                        CreateNewCharacterMenu(null);
                        current.Use();
                    }

                    return;
                case KeyCode.R:
                    if (current.control)
                    {
                        //Ctrl+R
                        RenameCharacterMenu(null);
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
                    SaveCharacterAsMenu(null);
                    current.Use();
                    return;
                }

                //Save cntrl + s
                SaveCharacterMenu(null);
                current.Use();
            }
        }
    }
}