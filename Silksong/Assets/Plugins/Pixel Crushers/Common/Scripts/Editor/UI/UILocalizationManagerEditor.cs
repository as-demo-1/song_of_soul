// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers
{

    [CustomEditor(typeof(UILocalizationManager), true)]
    public class UILocalizationManagerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button(new GUIContent("Reset Language PlayerPrefs", "Delete the language selection saved in PlayerPrefs.")))
            {
                PlayerPrefs.DeleteKey((target as UILocalizationManager).currentLanguagePlayerPrefsKey);
            }
        }

   }
}