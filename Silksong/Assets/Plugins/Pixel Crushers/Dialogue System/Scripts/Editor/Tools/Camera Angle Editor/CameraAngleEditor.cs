// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This Camera Angle Editor window helps you create camera angle prefabs that
    /// you can use with the Sequencer. To use the Camera Angle Editor:
    /// 
    /// -# Assign a subject (e.g., a character model in the scene) to the Subject field.
    /// -# Assign a game object to the Camera Angle Collection field. This object will be the 
    /// parent that contains the camera angles. The camera angles are child game objects that
    /// are offset from the parent's position.
    /// -# Select a child from the pop-up menu, or click the "+" button to add a new child.
    /// -# Edit the child's transform into the desired angle.
    /// -# When done, you can save it as a prefab or use it directly in the scene.
    /// </summary>
    public class CameraAngleEditor : EditorWindow
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Tools/Camera Angle Editor", false, 3)]
        public static void Init()
        {
            CameraAngleEditor window = EditorWindow.GetWindow(typeof(CameraAngleEditor), false, "Cam Angles") as CameraAngleEditor;
            window.Start();
        }

        private Transform subject = null;
        private Transform cameraAngleCollection = null;
        private Transform cameraAngle = null;
        private int cameraAngleIndex = 0;

        private List<Camera> disabledCameras = new List<Camera>();
        private Camera myCamera = null;

        public void Start()
        {
            DisableOtherCameras();
            AddMyCamera();
        }

        public void OnDestroy()
        {
            DestroyMyCamera();
            ReenableOtherCameras();
        }

        private void DisableOtherCameras()
        {
            Camera[] otherCameras = FindObjectsOfType(typeof(Camera)) as Camera[];
            disabledCameras.Clear();
            foreach (var camera in otherCameras)
            {
                if (camera.enabled)
                {
                    camera.enabled = false;
                    disabledCameras.Add(camera);
                }
            }
        }

        private void ReenableOtherCameras()
        {
            disabledCameras.ForEach(camera => camera.enabled = true);
            disabledCameras.Clear();
        }

        private void AddMyCamera()
        {
            GameObject go = new GameObject();
            go.name = "__Camera Angle Editor";
            myCamera = go.AddComponent<Camera>();
            myCamera.tag = "MainCamera";
        }

        private void DestroyMyCamera()
        {
            if (myCamera != null) DestroyImmediate(myCamera.gameObject);
            myCamera = null;
        }

        public void OnGUI()
        {
            //EditorGUIUtility.LookLikeControls();
            DrawSubjectField();
            DrawCameraAngleFields();
        }

        private void DrawSubjectField()
        {
            subject = EditorGUILayout.ObjectField("Subject", subject, typeof(Transform), true) as Transform;
            if (subject == null) EditorGUILayout.HelpBox("Assign a game object as the subject. The editor will position the camera relative to this subject.", MessageType.Info, true);
        }

        private void DrawCameraAngleFields()
        {
            cameraAngleCollection = EditorGUILayout.ObjectField("Camera Angle Collection", cameraAngleCollection, typeof(Transform), true) as Transform;
            if (cameraAngleCollection == null)
            {
                EditorGUILayout.HelpBox("Assign a game object as the camera angle collection. The camera angles will be defined by the relative positions of its children.", MessageType.Info, true);
                if (GUILayout.Button("Create New Collection")) CreateNewCameraAngleCollection();
            }
            else
            {
                DrawCameraAnglePopUp();
            }
        }

        private void DrawCameraAnglePopUp()
        {
            EditorGUILayout.BeginHorizontal();

            List<string> names = new List<string>();
            List<Transform> angles = new List<Transform>();
            foreach (Transform child in cameraAngleCollection.transform)
            {
                names.Add(child.name);
                angles.Add(child);
            }
            if (names.Count > 0)
            {
                int newCameraAngleIndex = EditorGUILayout.Popup("Camera Angle", Mathf.Clamp(cameraAngleIndex, 0, names.Count - 1), names.ToArray());
                if (newCameraAngleIndex != cameraAngleIndex)
                {
                    cameraAngleIndex = newCameraAngleIndex;
                    cameraAngle = angles[cameraAngleIndex];
                    Selection.activeGameObject = cameraAngle.gameObject;
                }
            }
            else
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField("No camera angles");
                GUI.enabled = true;
                cameraAngleIndex = 0;
                cameraAngle = null;
            }

#if UNITY_2018_3_OR_NEWER || UNITY_2019_1_OR_NEWER
            bool isPrefab = PrefabUtility.IsPartOfAnyPrefab(cameraAngleCollection);
#else
            bool isPrefab = (PrefabUtility.GetPrefabType(cameraAngleCollection) == PrefabType.Prefab);
#endif

            if (!isPrefab)
            {
                if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(22))) AddNewCameraAngle();
            }

            EditorGUILayout.EndHorizontal();

            if (isPrefab)
            {
                EditorGUILayout.HelpBox("This camera angle collection is a prefab. You can only add new angles to a scene object. To add an instance to the scene, click Instantiate below. When done, remember to apply changes back to the prefab.", MessageType.Warning, true);
                if (GUILayout.Button("Instantiate")) InstantiateCameraAngleCollectionPrefab();
            }
        }

        private void CreateNewCameraAngleCollection()
        {
            GameObject instance = new GameObject("New Camera Angle Collection");
            cameraAngleCollection = instance.transform;
        }

        private void InstantiateCameraAngleCollectionPrefab()
        {
            Transform instance = PrefabUtility.InstantiatePrefab(cameraAngleCollection) as Transform;
            if (instance != null) cameraAngleCollection = instance;
            cameraAngleIndex = -1;
        }

        private void AddNewCameraAngle()
        {
            GameObject child = new GameObject("New Camera Angle");
            child.transform.parent = cameraAngleCollection;
            child.transform.localPosition = Vector3.zero;
            Selection.activeGameObject = child;
            cameraAngle = child.transform;
            int index = 0;
            foreach (Transform aChild in cameraAngleCollection.transform)
            {
                if (aChild == child) cameraAngleIndex = index;
                index++;
            }
        }

        public void Update()
        {
            UpdateCamera();
        }

        private void UpdateCamera()
        {
            if (cameraAngle == null || subject == null) return;
            Camera.main.transform.position = subject.position + subject.rotation * cameraAngle.localPosition;
            Camera.main.transform.rotation = subject.rotation * cameraAngle.localRotation;
        }

    }

}
