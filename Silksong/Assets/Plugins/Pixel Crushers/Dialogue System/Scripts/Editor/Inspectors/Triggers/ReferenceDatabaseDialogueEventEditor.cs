// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This is a base class for most of the "On dialogue event" editors.
    /// It simply adds a Reference Database field to the editor above
    /// the default inspector.
    /// </summary>
    public class ReferenceDatabaseDialogueEventEditor : Editor
    {

        protected virtual bool isDeprecated { get { return false; } }

        public void OnEnable()
        {
            EditorTools.SetInitialDatabaseIfNull();
        }

        public override void OnInspectorGUI()
        {
            EditorTools.DrawReferenceDatabase();
            if (isDeprecated) EditorWindowTools.DrawDeprecatedTriggerHelpBox();
            DrawDefaultInspector();
        }

    }

}
