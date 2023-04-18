

namespace Opsive.UltimateInventorySystem.Editor.Utility
{
    using Opsive.Shared.Editor.Inspectors.Input;
    using UnityEditor;
    
    /// <summary>
    /// Updates the Unity Input Builder to create the correct button bindings.
    /// </summary>
    public class InventoryInputBuilder
    {
        /// <summary>
        /// Update the Input Manager to add all of the correct controls.
        /// </summary>
        public static void UpdateInputManager()
        {
            var serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            var axesProperty = serializedObject.FindProperty("m_Axes");

            // Unity defined axis:
            UnityInputBuilder.AddInputAxis(axesProperty, "Horizontal", "left", "right", "a", "d", 1000, 0.001f, 3, true, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Vertical", "down", "up", "s", "w", 1000, 0.001f, 3, true, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Fire1", "", "left ctrl", "", "mouse 0", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Fire2", "", "", "", "mouse 1", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Fire3", "", "left shift", "", "mouse 2", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Jump", "", "space", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Mouse X", "", "", "", "", 0, 0, 0.1f, false, false, UnityInputBuilder.AxisType.Mouse, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Mouse Y", "", "", "", "", 0, 0, 0.1f, false, false, UnityInputBuilder.AxisType.Mouse, UnityInputBuilder.AxisNumber.Y);
            UnityInputBuilder.AddInputAxis(axesProperty, "Mouse ScrollWheel", "", "", "", "", 0, 0, 0.1f, false, false, UnityInputBuilder.AxisType.Mouse, UnityInputBuilder.AxisNumber.Three);
            UnityInputBuilder.AddInputAxis(axesProperty, "Horizontal", "", "", "", "", 1000, 0.19f, 1, false, false, UnityInputBuilder.AxisType.Joystick, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Vertical", "", "", "", "", 1000, 0.19f, 1, false, true, UnityInputBuilder.AxisType.Joystick, UnityInputBuilder.AxisNumber.Y);
            UnityInputBuilder.AddInputAxis(axesProperty, "Fire1", "", "", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.Joystick, UnityInputBuilder.AxisNumber.Ten);
            UnityInputBuilder.AddInputAxis(axesProperty, "Fire2", "", "", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.Joystick, UnityInputBuilder.AxisNumber.Nine);
            UnityInputBuilder.AddInputAxis(axesProperty, "Fire3", "", "joystick button 2", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Jump", "", "joystick button 0", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Mouse X", "", "", "", "", 0, 0.19f, 1, false, false, UnityInputBuilder.AxisType.Joystick, UnityInputBuilder.AxisNumber.Four);
            UnityInputBuilder.AddInputAxis(axesProperty, "Mouse Y", "", "", "", "", 0, 0.19f, 1, false, true, UnityInputBuilder.AxisType.Joystick, UnityInputBuilder.AxisNumber.Five);

            // New axis:
            UnityInputBuilder.AddInputAxis(axesProperty, "Open Panel", "", "i", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Open Panel", "", "joystick button 9", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Close Panel", "", "escape", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Close Panel", "", "joystick button 3", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);

            UnityInputBuilder.AddInputAxis(axesProperty, "Next", "", "t", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Next", "", "joystick button 5", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Previous", "", "r", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Previous", "", "joystick button 4", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);

            UnityInputBuilder.AddInputAxis(axesProperty, "Equip First Item", "", "1", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Equip Second Item", "", "2", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Equip Third Item", "", "3", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Equip Fourth Item", "", "4", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Equip Fifth Item", "", "5", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Equip Sixth Item", "", "6", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Equip Seventh Item", "", "7", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Equip Eighth Item", "", "8", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Equip Ninth Item", "", "9", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Equip Tenth Item", "", "0", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            
            UnityInputBuilder.AddInputAxis(axesProperty, "Action", "", "f", "", "e", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axesProperty, "Action", "", "joystick button 1", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
