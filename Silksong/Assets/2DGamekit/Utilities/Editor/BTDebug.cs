using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BTAI;

namespace Gamekit2D
{
    public class BTDebug : EditorWindow
    {
        protected BTAI.Root _currentRoot = null;


        [MenuItem("Kit Tools/Behaviour Tree Debug")]
        static void OpenWindow()
        {
            BTDebug btdebug = GetWindow<BTDebug>();
            btdebug.Show();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Only work during play mode.", MessageType.Info);
            }
            else
            {
                if (_currentRoot == null)
                    FindRoot();
                else
                {
                    RecursiveTreeParsing(_currentRoot, 0, true);
                }
            }
        }

        void Update()
        {
            Repaint();
        }

        void RecursiveTreeParsing(Branch branch, int indent, bool parentIsActive)
        {
            List<BTNode> nodes = branch.Children();

            for(int i = 0; i < nodes.Count; ++i)
            {
                EditorGUI.indentLevel = indent;

                bool isActiveChild = branch.ActiveChild() == i;
                GUI.color = (isActiveChild && parentIsActive) ? Color.green : Color.white;
                EditorGUILayout.LabelField(nodes[i].ToString());

                if (nodes[i] is Branch)
                    RecursiveTreeParsing(nodes[i] as Branch, indent + 1, isActiveChild);
            }
        }

        void FindRoot()
        {
            if (Selection.activeGameObject == null)
            {
                _currentRoot = null;
                return;
            }

            IBTDebugable debugable = Selection.activeGameObject.GetComponentInChildren<IBTDebugable>();

            if(debugable != null)
            {
                _currentRoot = debugable.GetAIRoot();
            }
        }

    }
}