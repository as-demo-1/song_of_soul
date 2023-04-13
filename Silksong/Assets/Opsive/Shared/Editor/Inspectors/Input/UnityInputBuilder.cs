/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.Inspectors.Input
{
    using System.Collections.Generic;
    using UnityEditor;

    /// <summary>
    /// Updates the Unity input manager with the correct button bindings.
    /// </summary>
    public class UnityInputBuilder
    {
        /// <summary>
        /// The elements axis type within the InputManager.
        /// </summary>
        public enum AxisType
        {
            KeyMouseButton, Mouse, Joystick
        }
        /// <summary>
        /// The element's axis number within the InputManager.
        /// </summary>
        public enum AxisNumber
        {
            X, Y, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Eleven, Twelve, Thirteen, Fourteen, Fifteen, Sixteen, Seventeen, Eighteen, Nineteen, Twenty
        }

        private static Dictionary<string, int> s_FoundAxes;
        public static Dictionary<string, int> FoundAxes { get { return s_FoundAxes; } set { s_FoundAxes = value; } }

        /// <summary>
        /// Adds a new axis to the InputManager.
        /// </summary>
        /// <param name="axesProperty">The array of all of the axes.</param>
        /// <param name="name">The name of the new axis.</param>
        /// <param name="negativeButton">The name of the negative button of the new axis.</param>
        /// <param name="positiveButton">The name of the positive button of the new axis.</param>
        /// <param name="altNegativeButton">The name of the alternative negative button of the new axis.</param>
        /// <param name="altPositiveButton">The name of the alternative positive button of the new axis.</param>
        /// <param name="sensitivity">The sensitivity of the new axis.</param>
        /// <param name="gravity">The gravity of the new axis.</param>
        /// <param name="dead">The dead value of the new axis.</param>
        /// <param name="snap">Does the new axis snap?</param>
        /// <param name="invert">Is the axis inverted?</param>
        /// <param name="axisType">The type of axis to add.</param>
        /// <param name="axisNumber">The index of the axis.</param>
        public static void AddInputAxis(SerializedProperty axesProperty, string name, string negativeButton, string positiveButton,
                                string altNegativeButton, string altPositiveButton, float gravity, float dead, float sensitivity, bool snap, bool invert, AxisType axisType, AxisNumber axisNumber)
        {
            var property = FindAxisProperty(axesProperty, name);
            property.FindPropertyRelative("m_Name").stringValue = name;
            property.FindPropertyRelative("negativeButton").stringValue = negativeButton;
            property.FindPropertyRelative("positiveButton").stringValue = positiveButton;
            property.FindPropertyRelative("altNegativeButton").stringValue = altNegativeButton;
            property.FindPropertyRelative("altPositiveButton").stringValue = altPositiveButton;
            property.FindPropertyRelative("gravity").floatValue = gravity;
            property.FindPropertyRelative("dead").floatValue = dead;
            property.FindPropertyRelative("sensitivity").floatValue = sensitivity;
            property.FindPropertyRelative("snap").boolValue = snap;
            property.FindPropertyRelative("invert").boolValue = invert;
            property.FindPropertyRelative("type").intValue = (int)axisType;
            property.FindPropertyRelative("axis").intValue = (int)axisNumber;
        }

        /// <summary>
        /// Searches for a property with the given name and axis type within the axes property array. If no property is found then a new one will be created.
        /// </summary>
        /// <param name="axisProperty">The array to search through.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="autoCreate">Should a property be automatically created if it does not exist?</param>
        /// <returns>The found axis property.</returns>
        public static SerializedProperty FindAxisProperty(SerializedProperty axisProperty, string name, bool autoCreate = true)
        {
            SerializedProperty foundProperty = null;
            // As new axes are being added make sure a previous axis is not overwritten because the name matches.
            if (s_FoundAxes == null) {
                s_FoundAxes = new Dictionary<string, int>();
            }
            s_FoundAxes.TryGetValue(name, out var existingCount);
            var localCount = 0;
            for (int i = 0; i < axisProperty.arraySize; ++i) {
                var property = axisProperty.GetArrayElementAtIndex(i);
                if (property.FindPropertyRelative("m_Name").stringValue.Equals(name)) {
                    if (localCount == existingCount) {
                        foundProperty = property;
                        break;
                    }
                    localCount++;
                }
            }
            if (existingCount == 0) {
                s_FoundAxes.Add(name, 1);
            } else {
                s_FoundAxes[name] = existingCount + 1;
            }

            // If no property was found then create a new one.
            if (foundProperty == null) {
                axisProperty.InsertArrayElementAtIndex(axisProperty.arraySize);
                foundProperty = axisProperty.GetArrayElementAtIndex(axisProperty.arraySize - 1);
            }

            return foundProperty;
        }
    }
}