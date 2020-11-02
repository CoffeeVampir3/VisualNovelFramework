using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelFramework.Editor.Elements
{
    public class NamerPopup : VisualElement
    {
        private readonly Action<string> namedCallback = null;
        private readonly TextField nameField = null;
        private EditorWindow window = null;

        public NamerPopup(Action<string> onNameCallback)
        {
            var popupTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/VisualNovelFramework/EditorOnly/UIEUtilities/NamerPopup/NamerPopup.uxml");

            var root = popupTree.Instantiate();

            var okayBtn = root.Q<Button>("okayButton");
            var cancelBtn = root.Q<Button>("cancelButton");
            nameField = root.Q<TextField>("nameField");

            root.RegisterCallback<KeyUpEvent>(ListenForKeys);
            okayBtn.clicked += OnOkay;
            cancelBtn.clicked += OnCancel;

            namedCallback = onNameCallback;
            Add(root);
        }

        private void ListenForKeys(KeyUpEvent evnt)
        {
            switch (evnt.keyCode)
            {
                case KeyCode.Return:
                    OnOkay();
                    return;
                case KeyCode.Escape:
                    OnCancel();
                    return;
            }
        }

        public void Popup()
        {
            window = EditorWindow.GetWindow<RenamerPopupWindow>();
            window.rootVisualElement.Clear();
            window.rootVisualElement.Add(this);
            window.rootVisualElement.StretchToParentSize();
            window.name = "Renamer";
            window.titleContent = new GUIContent("Naming Item:");

            var scrnCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            window.position = new Rect(scrnCenter.x, scrnCenter.y, 325f, 110f);
            window.maxSize = new Vector2(325f, 110f);
            window.minSize = window.maxSize;
            window.Repaint();
            window.Show();
            window.Focus();
            var l = nameField.Q<VisualElement>("unity-text-input");
            l.Focus();
            nameField.SelectAll();
        }

        private void OnOkay()
        {
            if (nameField.value.Length <= 0) return;

            namedCallback(nameField.value);
            window.Close();
        }

        private void OnCancel()
        {
            window.Close();
        }
    }
}