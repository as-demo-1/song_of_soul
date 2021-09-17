//using UnityEditor;
//using UnityEngine;

//namespace Gamekit2D
//{
//    [CustomEditor(typeof(PlayerInput))]
//    public class PlayerInputEditor : DataPersisterEditor
//    {
//        private bool m_IsPrefab = false;
//        private bool m_IsNotInstance = false;

//        protected override void OnEnable()
//        {
//            base.OnEnable();

//            m_IsPrefab = AssetDatabase.Contains(target);
//            m_IsNotInstance = PrefabUtility.GetCorrespondingObjectFromSource(target) == null;
//        }

//        public override void OnInspectorGUI()
//        {
//            if(m_IsPrefab || m_IsNotInstance)
//                base.OnInspectorGUI();
//            else
//            {
//                EditorGUILayout.HelpBox("Modify the prefab and not this instance", MessageType.Warning);
//                if (GUILayout.Button("Select Prefab"))
//                {
//                    Selection.activeObject = PrefabUtility.GetCorrespondingObjectFromSource(target);
//                }
//            }
//        }
//    }
//}