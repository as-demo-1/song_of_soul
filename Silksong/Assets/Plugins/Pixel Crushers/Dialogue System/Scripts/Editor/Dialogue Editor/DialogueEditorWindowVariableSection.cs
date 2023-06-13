// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the Variables tab. Drawing is now
    /// handled by a DialogueEditorVariableView.
    /// </summary>
    public partial class DialogueEditorWindow
    {
        [SerializeField]
        private DialogueEditorVariableView variableView;

        private void ResetVariableSection()
        {
            variableView = new DialogueEditorVariableView();
            variableView.Initialize(database, template, true);
        }

        public void RefreshVariableView()
        {
            variableView.RefreshView();
            Repaint();
        }

        private void DrawVariableSection()
        {
            if (variableView == null || (variableView.database == null && database != null))
            {
                ResetVariableSection();
            }
            variableView.Draw();
            if (Application.isPlaying)
            {
                GUI.Label(new Rect(72, -4, 500, 30), "(Use Watches tab or Variable Viewer for runtime values.)");
            }
        }

    }

}