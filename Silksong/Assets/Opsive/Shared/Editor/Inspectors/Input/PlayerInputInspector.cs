/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.Inspectors.Input
{
#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
    using Opsive.Shared.Editor.Inspectors.StateSystem;
#endif
    using Opsive.Shared.Input;
    using System;
    using UnityEditor;

    /// <summary>
    /// Shows a custom inspector for the PlayerInput.
    /// </summary>
    [CustomEditor(typeof(PlayerInput))]
    public class PlayerInputInspector :
#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
        StateBehaviorInspector
#else
        InspectorBase
#endif
    {
#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
        /// <summary>
        /// Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetDrawCallback()
        {
            var baseCallback = base.GetDrawCallback();

            baseCallback += () =>
            {
#else
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();
#endif

                EditorGUILayout.PropertyField(PropertyFromName("m_HorizontalLookInputName"));
                EditorGUILayout.PropertyField(PropertyFromName("m_VerticalLookInputName"));
                var lookVector = PropertyFromName("m_LookVectorMode");
                EditorGUILayout.PropertyField(lookVector);
                if (lookVector.enumValueIndex == (int)PlayerInput.LookVectorMode.Smoothed) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_LookSensitivity"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_LookSensitivityMultiplier"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_SmoothLookSteps"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_SmoothLookWeight"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_SmoothExponent"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_LookAccelerationThreshold"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(PropertyFromName("m_ControllerConnectedCheckRate"));
#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
                EditorGUILayout.PropertyField(PropertyFromName("m_ConnectedControllerState"));
#endif

                DrawInputFields();

                // Event fields should be last.
                if (Foldout("Events")) {
                    EditorGUI.indentLevel++;
                    Utility.InspectorUtility.UnityEventPropertyField(PropertyFromName("m_EnableGamplayInputEvent"));
                    EditorGUI.indentLevel--;
                }
#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
            };

            return baseCallback;
#else

            if (EditorGUI.EndChangeCheck()) {
                Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
#endif
        }

        /// <summary>
        /// Draws all of the fields related to input.
        /// </summary>
        protected virtual void DrawInputFields() { }
    }
}