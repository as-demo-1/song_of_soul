// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Draws the Dialogue Manager icon in the Hierarchy view.
    /// </summary>
    [InitializeOnLoad]
    public class DialogueSystemControllerHierarchyIcon
    {

        private const string IconFilename = "Assets/Gizmos/DialogueDatabase Icon.png";

        static Texture2D icon = null;

        static DialogueSystemControllerHierarchyIcon()
        {
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconFilename);
            if (icon == null) return;
            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
            EditorApplication.RepaintHierarchyWindow();
        }

        public static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            GameObject dialogueSystemControllerGameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            var dialogueSystemController = (dialogueSystemControllerGameObject != null) ? dialogueSystemControllerGameObject.GetComponent<DialogueSystemController>() : null;
            if ((icon != null) && (dialogueSystemController != null))
            {
                Rect rect = new Rect(selectionRect.width - 5f, selectionRect.y, selectionRect.width, selectionRect.height);
                GUI.Label(rect, icon);
            }
        }

    }

}
