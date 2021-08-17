using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Gamekit2D
{
    [CustomEditor(typeof(Damager))]
    public class DamagerEditor : Editor
    {
        static BoxBoundsHandle s_BoxBoundsHandle = new BoxBoundsHandle();
        static Color s_EnabledColor = Color.green + Color.grey;

        SerializedProperty m_DamageProp;
        SerializedProperty m_OffsetProp;
        SerializedProperty m_SizeProp;
        SerializedProperty m_OffsetBasedOnSpriteFacingProp;
        SerializedProperty m_SpriteRendererProp;
        SerializedProperty m_CanHitTriggersProp;
        SerializedProperty m_ForceRespawnProp;
        SerializedProperty m_IgnoreInvincibilityProp;
        SerializedProperty m_HittableLayersProp;
        SerializedProperty m_OnDamageableHitProp;
        SerializedProperty m_OnNonDamageableHitProp;

        void OnEnable ()
        {
            m_DamageProp = serializedObject.FindProperty ("damage");
            m_OffsetProp = serializedObject.FindProperty("offset");
            m_SizeProp = serializedObject.FindProperty("size");
            m_OffsetBasedOnSpriteFacingProp = serializedObject.FindProperty("offsetBasedOnSpriteFacing");
            m_SpriteRendererProp = serializedObject.FindProperty("spriteRenderer");
            m_CanHitTriggersProp = serializedObject.FindProperty("canHitTriggers");
            m_ForceRespawnProp = serializedObject.FindProperty("forceRespawn");
            m_IgnoreInvincibilityProp = serializedObject.FindProperty("ignoreInvincibility");
            m_HittableLayersProp = serializedObject.FindProperty("hittableLayers");
            m_OnDamageableHitProp = serializedObject.FindProperty("OnDamageableHit");
            m_OnNonDamageableHitProp = serializedObject.FindProperty("OnNonDamageableHit");
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();

            EditorGUILayout.PropertyField(m_DamageProp);
            EditorGUILayout.PropertyField(m_OffsetProp);
            EditorGUILayout.PropertyField(m_SizeProp);
            EditorGUILayout.PropertyField(m_OffsetBasedOnSpriteFacingProp);
            if(m_OffsetBasedOnSpriteFacingProp.boolValue)
                EditorGUILayout.PropertyField(m_SpriteRendererProp);
            EditorGUILayout.PropertyField(m_CanHitTriggersProp);
            EditorGUILayout.PropertyField(m_ForceRespawnProp);
            EditorGUILayout.PropertyField(m_IgnoreInvincibilityProp);
            EditorGUILayout.PropertyField(m_HittableLayersProp);
            EditorGUILayout.PropertyField(m_OnDamageableHitProp);
            EditorGUILayout.PropertyField(m_OnNonDamageableHitProp);

            serializedObject.ApplyModifiedProperties ();
        }

        void OnSceneGUI ()
        {
            Damager damager = (Damager)target;

            if (!damager.enabled)
                return;

            Matrix4x4 handleMatrix = damager.transform.localToWorldMatrix;
            handleMatrix.SetRow(0, Vector4.Scale(handleMatrix.GetRow(0), new Vector4(1f, 1f, 0f, 1f)));
            handleMatrix.SetRow(1, Vector4.Scale(handleMatrix.GetRow(1), new Vector4(1f, 1f, 0f, 1f)));
            handleMatrix.SetRow(2, new Vector4(0f, 0f, 1f, damager.transform.position.z));
            using (new Handles.DrawingScope(handleMatrix))
            {
                s_BoxBoundsHandle.center = damager.offset;
                s_BoxBoundsHandle.size = damager.size;

                s_BoxBoundsHandle.SetColor(s_EnabledColor);
                EditorGUI.BeginChangeCheck();
                s_BoxBoundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(damager, "Modify Damager");

                    damager.size = s_BoxBoundsHandle.size;
                    damager.offset = s_BoxBoundsHandle.center;
                }
            }
        }
    }
}