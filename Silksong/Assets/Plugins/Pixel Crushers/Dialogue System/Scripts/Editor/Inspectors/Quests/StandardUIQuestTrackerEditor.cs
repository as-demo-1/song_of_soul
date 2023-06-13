// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Custom editor for StandardUIQuestTracker that adds a button to 
    /// clear the visibility PlayerPrefs key.
    /// </summary>
    [CustomEditor(typeof(StandardUIQuestTracker), true)]
    public class StandardUIQuestTrackerEditor : Editor
    {
        private GUIContent buttonLabel;

        private void OnEnable()
        {
            buttonLabel = new GUIContent("Clear PlayerPrefs Key", "If you invoke the tracker's ShowTracker or HideTracker methods, it saves the current choice in a PlayerPrefs key. Click this to clear the key.");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var tracker = target as StandardUIQuestTracker;
            if (!string.IsNullOrEmpty(tracker.playerPrefsToggleKey))
            {
                if (GUILayout.Button(buttonLabel)) PlayerPrefs.DeleteKey(tracker.playerPrefsToggleKey);
            }
        }

    }

}
