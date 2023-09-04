// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the GUI Styles used by the
    /// outline-style dialogue tree editor.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        private const string GroupBoxStyle = "button";

        private const int FoldoutIndentWidth = 16;

        private bool needToUpdateDialogueTreeGUIStyles = true;
        private GUIStyle npcLineGUIStyle = null;
        private GUIStyle pcLineGUIStyle = null;
        private GUIStyle grayGUIStyle = null;
        private GUIStyle npcLineLeafGUIStyle = null;
        private GUIStyle pcLineLeafGUIStyle = null;
        private GUIStyle pcLinkButtonGUIStyle = null;
        private GUIStyle npcLinkButtonGUIStyle = null;
        private GUIStyle entryGroupHeadingStyle = null;
        private GUIStyle entryGroupLabelStyle = null;
        private float entryGroupHeadingBaseFontSize;
        private float currentEntryGroupHeadingZoom;

        private void ResetDialogueTreeGUIStyles()
        {
            needToUpdateDialogueTreeGUIStyles = true;
        }

        private void CheckDialogueTreeGUIStyles()
        {
            if (needToUpdateDialogueTreeGUIStyles) UpdateDialogueTreeGUIStyles();
        }

        private void UpdateDialogueTreeGUIStyles()
        {
            needToUpdateDialogueTreeGUIStyles = false;
            pcLineGUIStyle = NewDialogueGUIStyle(template.pcLineColor, GUI.skin.label);
            npcLineGUIStyle = NewDialogueGUIStyle(template.npcLineColor, GUI.skin.label);
            grayGUIStyle = NewDialogueGUIStyle(template.repeatLineColor, GUI.skin.label);
            pcLineLeafGUIStyle = NewDialogueGUIStyle(template.pcLineColor, GUI.skin.label);
            npcLineLeafGUIStyle = NewDialogueGUIStyle(template.npcLineColor, GUI.skin.label);
            pcLineLeafGUIStyle.padding.left = FoldoutIndentWidth; // Indent to match foldout styles.
            npcLineLeafGUIStyle.padding.left = FoldoutIndentWidth;
            grayGUIStyle.padding.left = FoldoutIndentWidth;
            pcLinkButtonGUIStyle = NewDialogueGUIStyle(template.pcLineColor, EditorStyles.miniButton);
            npcLinkButtonGUIStyle = NewDialogueGUIStyle(template.npcLineColor, EditorStyles.miniButton);
            InitEntryGroupHeadingStyle();
            InitEntryGroupLabelStyle();
        }

        private void InitEntryGroupHeadingStyle()
        {
            entryGroupHeadingStyle = new GUIStyle(GUI.skin.button);
            entryGroupHeadingBaseFontSize = entryGroupHeadingStyle.fontSize;
            currentEntryGroupHeadingZoom = _zoom;
        }

        private void InitEntryGroupLabelStyle()
        {
            entryGroupLabelStyle = new GUIStyle(GUI.skin.label);
            entryGroupLabelStyle.alignment = TextAnchor.MiddleCenter;
        }

        private GUIStyle NewDialogueGUIStyle(Color color, GUIStyle baseStyle)
        {
            GUIStyle guiStyle = new GUIStyle(baseStyle);
            guiStyle.normal.textColor = color;
            guiStyle.hover.textColor = color;
            guiStyle.focused.textColor = color;
            guiStyle.active.textColor = color;
            guiStyle.onNormal.textColor = color;
            guiStyle.onHover.textColor = color;
            guiStyle.onFocused.textColor = color;
            guiStyle.onActive.textColor = color;
            guiStyle.wordWrap = true;
            return guiStyle;
        }

        private GUIStyle CenteredLabelStyle
        {
            get
            {
                GUIStyle centeredLabelStyle = new GUIStyle(EditorStyles.label);
                centeredLabelStyle.alignment = TextAnchor.MiddleCenter;
                return centeredLabelStyle;
            }
        }

    }

}