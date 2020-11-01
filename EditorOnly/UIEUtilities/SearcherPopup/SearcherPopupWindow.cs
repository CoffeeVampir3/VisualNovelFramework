using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VisualNovelFramework.Elements.Utils
{
    /// <summary>
    ///     This whole thing is a giant hack. It's a transparent window listening for picker events.
    ///     When it receives the right ones, it closes and triggers it's return function callback.
    /// </summary>
    public class SearcherPopupWindow : EditorWindow
    {
        private static readonly int currentPicker = 0;
        private static System.Action<Object> returnFunction;
        private static Object currentPickerObject;
        private static MethodInfo genericPickerCaller;
        private static Type pickerType = null;

        private static bool hasStarted = false;

        public static void SetPickerCallback(Action<Object> callback)
        {
            returnFunction = callback;
        }

        public static void SetObjectPickerType<T>() where T : Object
        {
            pickerType = typeof(T);
        }

        public void GenericPickerCall()
        {
            var m = typeof(EditorGUIUtility).GetMethod(nameof(EditorGUIUtility.ShowObjectPicker),
                BindingFlags.Public | BindingFlags.Static);
            genericPickerCaller = m.MakeGenericMethod(pickerType);
            genericPickerCaller.Invoke(null, new object[] {null, false, "", currentPicker});
        }

        public void Reset()
        {
            currentPickerObject = null;
            hasStarted = false;
        }

        private void OnGUI()
        {
            if (!hasStarted)
            {
                GenericPickerCall();
                hasStarted = true;
            }

            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                currentPickerObject = EditorGUIUtility.GetObjectPickerObject();
            }
            else if (Event.current.commandName == "ObjectSelectorClosed")
            {
                if (currentPickerObject != null)
                    returnFunction.Invoke(currentPickerObject);
                Close();
            }
        }
    }
}