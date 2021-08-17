using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Gamekit2D
{
    [CustomEditor(typeof(MovingPlatform))]
    public class MovingPlatformEditor : Editor
    {
        MovingPlatform m_MovingPlatform;

        private SerializedProperty m_PlatformCatcherProperty;
        private SerializedProperty m_IsMovingAtStartProperty;
        private SerializedProperty m_StartMovingOnlyWhenVisibleProperty;
        private SerializedProperty m_PlatformTypeProperty;
        private SerializedProperty m_PlatformSpeedProperty;
        private SerializedProperty m_PlatformNodesProperty;
        private SerializedProperty m_PlatformWaitTimeProperty;

        float m_PreviewPosition = 0;

        private void OnEnable()
        {
            m_PreviewPosition = 0;
            m_MovingPlatform = target as MovingPlatform;

            if(!EditorApplication.isPlayingOrWillChangePlaymode)
                MovingPlatformPreview.CreateNewPreview(m_MovingPlatform);

            m_PlatformCatcherProperty = serializedObject.FindProperty(nameof(m_MovingPlatform.platformCatcher));
            m_IsMovingAtStartProperty = serializedObject.FindProperty(nameof(m_MovingPlatform.isMovingAtStart));
            m_StartMovingOnlyWhenVisibleProperty = serializedObject.FindProperty(nameof(m_MovingPlatform.startMovingOnlyWhenVisible));
            m_PlatformTypeProperty = serializedObject.FindProperty(nameof(m_MovingPlatform.platformType));
            m_PlatformSpeedProperty = serializedObject.FindProperty(nameof(m_MovingPlatform.speed));
            m_PlatformNodesProperty = serializedObject.FindProperty(nameof(m_MovingPlatform.localNodes));
            m_PlatformWaitTimeProperty = serializedObject.FindProperty(nameof(m_MovingPlatform.waitTimes));
        }

        private void OnDisable()
        {
            MovingPlatformPreview.DestroyPreview();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_PlatformCatcherProperty);

            EditorGUI.BeginChangeCheck();
            m_PreviewPosition = EditorGUILayout.Slider("Preview position", m_PreviewPosition, 0.0f, 1.0f);
            if (EditorGUI.EndChangeCheck())
            {
                MovePreview();
            }

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(m_IsMovingAtStartProperty);

            if(m_IsMovingAtStartProperty.boolValue)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(m_StartMovingOnlyWhenVisibleProperty);
                EditorGUI.indentLevel -= 1;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(m_PlatformTypeProperty);
            EditorGUILayout.PropertyField(m_PlatformSpeedProperty);

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            if (GUILayout.Button("Add Node"))
            {
                Undo.RecordObject(target, "added node");

            
                Vector3 position = m_MovingPlatform.localNodes[m_MovingPlatform.localNodes.Length - 1] + Vector3.right;

                int index = m_PlatformNodesProperty.arraySize;
                m_PlatformNodesProperty.InsertArrayElementAtIndex(index);
                m_PlatformNodesProperty.GetArrayElementAtIndex(index).vector3Value = position;
                
                m_PlatformWaitTimeProperty.InsertArrayElementAtIndex(index);
                m_PlatformWaitTimeProperty.GetArrayElementAtIndex(index).floatValue = 0;
            }

            EditorGUIUtility.labelWidth = 64;
            int delete = -1;
            for (int i = 0; i < m_MovingPlatform.localNodes.Length; ++i)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();

                int size = 64;
                EditorGUILayout.BeginVertical(GUILayout.Width(size));
                EditorGUILayout.LabelField("Node " + i, GUILayout.Width(size));
                if (i != 0 && GUILayout.Button("Delete", GUILayout.Width(size)))
                {
                    delete = i;
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();

                if (i != 0)
                {
                    EditorGUILayout.PropertyField(m_PlatformNodesProperty.GetArrayElementAtIndex(i), new GUIContent("Pos"));
                    EditorGUILayout.PropertyField(m_PlatformWaitTimeProperty.GetArrayElementAtIndex(i), new GUIContent("Wait Time"));
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                
            }
            EditorGUIUtility.labelWidth = 0;

            if (delete != -1)
            {
                m_PlatformNodesProperty.DeleteArrayElementAtIndex(delete);
                m_PlatformWaitTimeProperty.DeleteArrayElementAtIndex(delete);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            MovePreview();

            for (int i = 0; i < m_MovingPlatform.localNodes.Length; ++i)
            {
                Vector3 worldPos;
                if (Application.isPlaying)
                {
                    worldPos = m_MovingPlatform.worldNode[i];
                }
                else
                {
                    worldPos = m_MovingPlatform.transform.TransformPoint(m_MovingPlatform.localNodes[i]);
                }


                Vector3 newWorld = worldPos; 
                if(i != 0)
                    newWorld = Handles.PositionHandle(worldPos, Quaternion.identity);

                Handles.color = Color.red;

                if (i == 0)
                {
                    if (m_MovingPlatform.platformType != MovingPlatform.MovingPlatformType.LOOP)
                        continue;

                    if (Application.isPlaying)
                    {
                        Handles.DrawDottedLine(worldPos, m_MovingPlatform.worldNode[m_MovingPlatform.worldNode.Length - 1], 10);
                    }
                    else
                    {
                        Handles.DrawDottedLine(worldPos, m_MovingPlatform.transform.TransformPoint(m_MovingPlatform.localNodes[m_MovingPlatform.localNodes.Length - 1]), 10);
                    }
                }
                else
                {
                    if (Application.isPlaying)
                    {
                        Handles.DrawDottedLine(worldPos, m_MovingPlatform.worldNode[i - 1], 10);
                    }
                    else
                    {
                        Handles.DrawDottedLine(worldPos, m_MovingPlatform.transform.TransformPoint(m_MovingPlatform.localNodes[i - 1]), 10);
                    }

                    if (worldPos != newWorld)
                    {
                        Undo.RecordObject(target, "moved point");
                        
                        m_PlatformNodesProperty.GetArrayElementAtIndex(i).vector3Value = m_MovingPlatform.transform.InverseTransformPoint(newWorld);
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }

        void MovePreview()
        {
            //compute pos from 0-1 preview pos

            if (Application.isPlaying)
                return;

            float step = 1.0f / (m_MovingPlatform.localNodes.Length - 1);

            int starting = Mathf.FloorToInt(m_PreviewPosition / step);

            if (starting > m_MovingPlatform.localNodes.Length-2)
                return;

            float localRatio = (m_PreviewPosition - (step * starting)) / step;

            Vector3 localPos = Vector3.Lerp(m_MovingPlatform.localNodes[starting], m_MovingPlatform.localNodes[starting + 1], localRatio);

            MovingPlatformPreview.preview.transform.position = m_MovingPlatform.transform.TransformPoint(localPos);

            SceneView.RepaintAll();
        }
    }
}