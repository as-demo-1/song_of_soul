// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers
{

    [CustomEditor(typeof(InputDeviceManager), true)]
    public class InputDeviceManagerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var inputDeviceManager = target as InputDeviceManager;
            if (GUILayout.Button(new GUIContent("Add Input Definitions", "If any of the buttons or axes listed above aren't in Unity's Input Manager, add them.")))
            {
                AddInputDefinitions(inputDeviceManager);
            }
            if (inputDeviceManager.joystickAxesToCheck == null || inputDeviceManager.joystickAxesToCheck.Length  == 0)
            {
                if (GUILayout.Button(new GUIContent("Check Default Joystick Axes", "Check joystick axis movement to detect switch to joystick mode.")))
                {
                    SetDefaultJoystickAxesToCheck(inputDeviceManager);
                }
            }
        }

        public static void SetDefaultJoystickAxesToCheck(InputDeviceManager inputDeviceManager)
        {
            if (inputDeviceManager == null) return;
            inputDeviceManager.joystickAxesToCheck = new string[] { "JoystickAxis1", "JoystickAxis2", "JoystickAxis3", "JoystickAxis4", "JoystickAxis6", "JoystickAxis7" };
            AddInputDefinitions(inputDeviceManager);
        }

        public static void AddInputDefinitions(InputDeviceManager inputDeviceManager)
        {
            if (inputDeviceManager == null) return;
            foreach (var button in inputDeviceManager.joystickButtonsToCheck)
            {
                AddInputDefinition(button);
            }
            foreach (var axis in inputDeviceManager.joystickAxesToCheck)
            {
                AddInputDefinition(axis);
            }
            foreach (var button in inputDeviceManager.keyButtonsToCheck)
            {
                AddInputDefinition(button);
            }
            foreach (var button in inputDeviceManager.backButtons)
            {
                AddInputDefinition(button);
            }
            Debug.Log("All input definitions are in Unity's Input Manager.");
        }

        public static bool HasStandardInputDefinitions()
        {
            return AxisDefined("JoystickAxis7");
        }

        public static void AddStandardInputDefinitions()
        {
            AddAxis(new InputAxis() { name = "JoystickAxis1", dead = 0.2f, sensitivity = 1f, type = AxisType.JoystickAxis, axis = 1, joyNum = 0, });
            AddAxis(new InputAxis() { name = "JoystickAxis2", dead = 0.2f, sensitivity = 1f, type = AxisType.JoystickAxis, axis = 2, joyNum = 0, });
            AddAxis(new InputAxis() { name = "JoystickAxis3", dead = 0.2f, sensitivity = 1f, type = AxisType.JoystickAxis, axis = 3, joyNum = 0, });
            AddAxis(new InputAxis() { name = "JoystickAxis4", dead = 0.2f, sensitivity = 1f, type = AxisType.JoystickAxis, axis = 4, joyNum = 0, });
            AddAxis(new InputAxis() { name = "JoystickAxis5", dead = 0.2f, sensitivity = 1f, type = AxisType.JoystickAxis, axis = 5, joyNum = 0, });
            AddAxis(new InputAxis() { name = "JoystickAxis6", dead = 0.2f, sensitivity = 1f, type = AxisType.JoystickAxis, axis = 6, joyNum = 0, });
            AddAxis(new InputAxis() { name = "JoystickAxis7", dead = 0.2f, sensitivity = 1f, type = AxisType.JoystickAxis, axis = 7, joyNum = 0, });
        }

        public static void AddInputDefinition(string inputName)
        {
            if (string.IsNullOrEmpty(inputName)) return;
            switch (inputName)
            {
                case "JoystickButton0":
                    AddAxis(new InputAxis() { name = inputName, positiveButton = "joystick button 0", gravity = 1000, dead = 0.0001f, sensitivity = 1000, type = AxisType.KeyOrMouseButton });
                    break;
                case "JoystickButton1":
                    AddAxis(new InputAxis() { name = inputName, positiveButton = "joystick button 1", gravity = 1000, dead = 0.0001f, sensitivity = 1000, type = AxisType.KeyOrMouseButton });
                    break;
                case "JoystickButton2":
                    AddAxis(new InputAxis() { name = inputName, positiveButton = "joystick button 2", gravity = 1000, dead = 0.0001f, sensitivity = 1000, type = AxisType.KeyOrMouseButton });
                    break;
                case "JoystickButton3":
                    AddAxis(new InputAxis() { name = inputName, positiveButton = "joystick button 3", gravity = 1000, dead = 0.0001f, sensitivity = 1000, type = AxisType.KeyOrMouseButton });
                    break;
                case "JoystickButton4":
                    AddAxis(new InputAxis() { name = inputName, positiveButton = "joystick button 4", gravity = 1000, dead = 0.0001f, sensitivity = 1000, type = AxisType.KeyOrMouseButton });
                    break;
                case "JoystickButton5":
                    AddAxis(new InputAxis() { name = inputName, positiveButton = "joystick button 5", gravity = 1000, dead = 0.0001f, sensitivity = 1000, type = AxisType.KeyOrMouseButton });
                    break;
                case "JoystickButton6":
                    AddAxis(new InputAxis() { name = inputName, positiveButton = "joystick button 6", gravity = 1000, dead = 0.0001f, sensitivity = 1000, type = AxisType.KeyOrMouseButton });
                    break;
                case "JoystickButton7":
                    AddAxis(new InputAxis() { name = inputName, positiveButton = "joystick button 7", gravity = 1000, dead = 0.0001f, sensitivity = 1000, type = AxisType.KeyOrMouseButton });
                    break;
                case "JoystickAxis1":
                    AddAxis(new InputAxis() { name = inputName, dead = 0.2f, sensitivity = 1f, type = AxisType.JoystickAxis, axis = 1, joyNum = 0, });
                    break;
                case "JoystickAxis2":
                    AddAxis(new InputAxis() { name = inputName, dead = 0.2f, sensitivity = 1f, type = AxisType.JoystickAxis, axis = 2, joyNum = 0, });
                    break;
                case "JoystickAxis3":
                    AddAxis(new InputAxis() { name = inputName, dead = 0.2f, sensitivity = 1f, type = AxisType.JoystickAxis, axis = 3, joyNum = 0, });
                    break;
                case "JoystickAxis4":
                    AddAxis(new InputAxis() { name = inputName, dead = 0.2f, sensitivity = 1f, type = AxisType.JoystickAxis, axis = 4, joyNum = 0, });
                    break;
                case "JoystickAxis5":
                    AddAxis(new InputAxis() { name = inputName, dead = 0.2f, sensitivity = 1f, type = AxisType.JoystickAxis, axis = 5, joyNum = 0, });
                    break;
                case "JoystickAxis6":
                    AddAxis(new InputAxis() { name = inputName, dead = 0.2f, sensitivity = 1f, type = AxisType.JoystickAxis, axis = 6, joyNum = 0, });
                    break;
                case "JoystickAxis7":
                    AddAxis(new InputAxis() { name = inputName, dead = 0.2f, sensitivity = 1f, type = AxisType.JoystickAxis, axis = 7, joyNum = 0, });
                    break;
                default:
                    AddUnrecognizedInputDefinition(inputName);
                    return;
            }
        }

        private static void AddUnrecognizedInputDefinition(string inputName)
        {
            if (string.IsNullOrEmpty(inputName)) return;
            if (inputName.ToLower().Contains("button"))
            {
                Debug.LogWarning("Will add button to Input Manager: " + inputName + " but you may need to check its values (Edit > Project Settings > Input).");
                var buttonName = ObjectNames.NicifyVariableName(inputName).ToLower();
                AddAxis(new InputAxis() { name = inputName, positiveButton = buttonName, gravity = 1000, dead = 0.0001f, sensitivity = 1000, type = AxisType.KeyOrMouseButton });
            }
            else
            {
                AddAxisUndefined(inputName);
            }
        }

        // From: https://plyoung.appspot.com/blog/manipulating-input-manager-in-script.html

        private static SerializedProperty GetChildProperty(SerializedProperty parent, string name)
        {
            SerializedProperty child = parent.Copy();
            child.Next(true);
            do
            {
                if (child.name == name) return child;
            }
            while (child.Next(false));
            return null;
        }

        public enum AxisType
        {
            KeyOrMouseButton = 0,
            MouseMovement = 1,
            JoystickAxis = 2
        };

        public class InputAxis
        {
            public string name;
            public string descriptiveName;
            public string descriptiveNegativeName;
            public string negativeButton;
            public string positiveButton;
            public string altNegativeButton;
            public string altPositiveButton;

            public float gravity;
            public float dead;
            public float sensitivity;

            public bool snap = false;
            public bool invert = false;

            public AxisType type;

            public int axis;
            public int joyNum;
        }

        private static bool AxisDefined(string axisName)
        {
#if USE_NEW_INPUT
            return true; // Assume InputActions will define axis.
#else
            try
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset");
                if (assets == null || assets.Length == 0) return true; // Gracefully skip if can't load InputManager.
                SerializedObject serializedObject = new SerializedObject(assets[0]);
                SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

                var valid = axesProperty.Next(true);
                valid = valid || axesProperty.Next(true);
                while (valid && axesProperty.Next(false))
                {
                    SerializedProperty axis = axesProperty.Copy();
                    if (axis.Next(true))
                    {
                        if (axis.stringValue == axisName) return true;
                    }
                }
                return false;
            }
            catch (System.InvalidOperationException)
            {
                return false;
            }
#endif
        }

        private static void AddAxis(InputAxis axis)
        {
            if (AxisDefined(axis.name)) return;

            Debug.Log("Added to Input Manager: " + axis.name);

            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

            axesProperty.arraySize++;
            serializedObject.ApplyModifiedProperties();

            SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);

            GetChildProperty(axisProperty, "m_Name").stringValue = axis.name;
            GetChildProperty(axisProperty, "descriptiveName").stringValue = axis.descriptiveName;
            GetChildProperty(axisProperty, "descriptiveNegativeName").stringValue = axis.descriptiveNegativeName;
            GetChildProperty(axisProperty, "negativeButton").stringValue = axis.negativeButton;
            GetChildProperty(axisProperty, "positiveButton").stringValue = axis.positiveButton;
            GetChildProperty(axisProperty, "altNegativeButton").stringValue = axis.altNegativeButton;
            GetChildProperty(axisProperty, "altPositiveButton").stringValue = axis.altPositiveButton;
            GetChildProperty(axisProperty, "gravity").floatValue = axis.gravity;
            GetChildProperty(axisProperty, "dead").floatValue = axis.dead;
            GetChildProperty(axisProperty, "sensitivity").floatValue = axis.sensitivity;
            GetChildProperty(axisProperty, "snap").boolValue = axis.snap;
            GetChildProperty(axisProperty, "invert").boolValue = axis.invert;
            GetChildProperty(axisProperty, "type").intValue = (int)axis.type;
            GetChildProperty(axisProperty, "axis").intValue = axis.axis - 1;
            GetChildProperty(axisProperty, "joyNum").intValue = axis.joyNum;

            serializedObject.ApplyModifiedProperties();
        }

        private static void AddAxisUndefined(string axisName)
        {
            if (AxisDefined(axisName)) return;
            Debug.LogWarning("Will add to Input Manager: " + axisName + " but you must set its values (Edit > Project Settings > Input).");
            AddAxis(new InputAxis() { name = axisName });
        }
    }
}