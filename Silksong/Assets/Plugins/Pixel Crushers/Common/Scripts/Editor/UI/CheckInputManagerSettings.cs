// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers
{

    /// <summary>
    /// This script runs when Unity starts or reloads assemblies after compilation.
    /// If it hasn't yet asked, it asks to set the InputDeviceManager standard inputs.
    /// </summary>
    [InitializeOnLoad]
    public static class CheckInputManagerSettings
    {

        private const string CheckedInputManagerSettingsEditorPrefsKey = "PixelCrushers.CheckedInputManagerSettings";

        static CheckInputManagerSettings()
        {
#if !USE_NEW_INPUT
            var alreadyAsked = EditorPrefs.GetBool(CheckedInputManagerSettingsEditorPrefsKey, false);
            EditorPrefs.SetBool(CheckedInputManagerSettingsEditorPrefsKey, true);
            if (InputDeviceManagerEditor.HasStandardInputDefinitions() || alreadyAsked) return;
            if (EditorUtility.DisplayDialog("Add Input Manager Settings?", 
                "Do you want to add standard input definitions for joystick axes so the Input Device Manager can detect when the player is using a joystick?", "Yes", "No"))
            {
                InputDeviceManagerEditor.AddStandardInputDefinitions();
                Debug.Log("Added standard input definitions.");
            }
#endif
        }

    }
}
