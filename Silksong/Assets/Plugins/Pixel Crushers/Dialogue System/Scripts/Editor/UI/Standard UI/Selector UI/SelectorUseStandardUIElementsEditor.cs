// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    [CustomEditor(typeof(SelectorUseStandardUIElements), true)]
    public class SelectorUseStandardUIElementsEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Add this component to a GameObject that has a Selector or Proximity Selector to tell it to use Standard UI elements instead of legacy Unity GUI. Your scene should have a Standard UI Selector Elements component somewhere.", MessageType.None);
            DrawDefaultInspector();
        }

    }

}
