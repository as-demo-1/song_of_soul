using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gamekit2D
{
    [CustomEditor(typeof(PressurePad))]
    public class PressurePadEditor : Editor
    {
        SerializedProperty m_PlatformCatcherProp;
        SerializedProperty m_ActivationTypeProp;
        SerializedProperty m_RequiredCountProp;
        SerializedProperty m_RequiredMassProp;
        SerializedProperty m_DeactivatedBoxSpriteProp;
        SerializedProperty m_ActivatedBoxSpriteProp;
        SerializedProperty m_BoxesProp;
        SerializedProperty m_OnPressedProp;
        SerializedProperty m_OnReleaseProp;

        void OnEnable ()
        {
            m_PlatformCatcherProp = serializedObject.FindProperty ("platformCatcher");
            m_ActivationTypeProp = serializedObject.FindProperty("activationType");
            m_RequiredCountProp = serializedObject.FindProperty("requiredCount");
            m_RequiredMassProp = serializedObject.FindProperty("requiredMass");
            m_DeactivatedBoxSpriteProp = serializedObject.FindProperty("deactivatedBoxSprite");
            m_ActivatedBoxSpriteProp = serializedObject.FindProperty("activatedBoxSprite");
            m_BoxesProp = serializedObject.FindProperty("boxes");
            m_OnPressedProp = serializedObject.FindProperty("OnPressed");
            m_OnReleaseProp = serializedObject.FindProperty("OnRelease");
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();

            EditorGUILayout.PropertyField (m_PlatformCatcherProp);
            EditorGUILayout.PropertyField (m_ActivationTypeProp);
            if((PressurePad.ActivationType)m_ActivationTypeProp.enumValueIndex == PressurePad.ActivationType.ItemCount)
                EditorGUILayout.PropertyField (m_RequiredCountProp);
            else
                EditorGUILayout.PropertyField (m_RequiredMassProp);

            EditorGUILayout.PropertyField (m_DeactivatedBoxSpriteProp);
            EditorGUILayout.PropertyField (m_ActivatedBoxSpriteProp);
            EditorGUILayout.PropertyField (m_BoxesProp, true);
            
            
            EditorGUILayout.PropertyField (m_OnPressedProp);
            EditorGUILayout.PropertyField(m_OnReleaseProp);

            serializedObject.ApplyModifiedProperties ();
        }
    }
}