// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.IO;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// Undo functionality for Dialogue Editor window.
    /// </summary>
    public partial class DialogueEditorWindow : EditorWindow
    {

        private void HandleUndo()
        {
            switch (toolbar.current)
            {
                case Toolbar.Tab.Actors:
                    break;
                case Toolbar.Tab.Items:
                    break;
                case Toolbar.Tab.Locations:
                    break;
                case Toolbar.Tab.Variables:
                    break;
                case Toolbar.Tab.Conversations:
                    // If undid creating a conversation, reset conversation section:
                    if (currentConversation != null)
                    {
                        if (database.GetConversation(currentConversation.id) == null)
                        {
                            if (debug) Debug.Log("Dialogue Editor: Undo create conversation");
                            ResetConversationSection();
                            Repaint();
                        }
                        else if (currentEntry != null && !currentConversation.dialogueEntries.Contains(currentEntry))
                        {
                            if (debug) Debug.Log("Dialogue Editor: Undo create dialogue entry");
                            RefreshConversation();
                            Repaint();
                        }
                    }
                    break;
            }
        }

        private void RecordUndo()
        {
            if (database == null) return;
            var eventType = Event.current.type;
            if (eventType == EventType.MouseUp || eventType == EventType.MouseDrag ||
                eventType == EventType.KeyUp || eventType == EventType.ContextClick)
            {
                if (registerCompleteObjectUndo)
                {
                    Undo.RegisterCompleteObjectUndo(database, "Dialogue Database");
                }
                else
                {
                    Undo.RecordObject(database, "Dialogue Database");
                }
            }
        }

        public void SetDatabaseDirty(string reason)
        {
            if (debug)
            {
                if (database == null)
                {
                    if (string.IsNullOrEmpty(reason))
                    {
                        Debug.LogWarning("Dialogue Editor wants to mark database to be saved, but it doesn't have a reference to a database!");
                    }
                    else
                    {
                        Debug.LogWarning("Dialogue Editor wants to mark database to be saved after change '" + reason + "', but it doesn't have a reference to a database!");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(reason))
                    {
                        Debug.Log("Dialogue Editor marking database '" + database.name + "' to be saved.", database);
                    }
                    else
                    {
                        Debug.Log("Dialogue Editor marking database '" + database.name + "' to be saved after change '" + reason + "'.", database);
                    }
                }
            }
            if (database != null) EditorUtility.SetDirty(database);
        }

        private void MakeAutoBackup()
        {
            if (database == null) return;
            try
            {
                var path = AssetDatabase.GetAssetPath(database);
                if (path.EndsWith("(Auto-Backup).asset"))
                {
                    Debug.Log("Dialogue Editor: Not creating an auto-backup. You're already editing the auto-backup file.");
                    return;
                }
                var backupPath = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + " (Auto-Backup).asset";
                if (!string.IsNullOrEmpty(autoBackupFolder))
                {
                    backupPath = autoBackupFolder + "/" + Path.GetFileNameWithoutExtension(path) + " (Auto-Backup).asset";
                }
                if (debug) Debug.Log("Dialogue Editor: Making auto-backup " + backupPath);
                EditorUtility.DisplayProgressBar("Dialogue Editor Auto-Backup", "Creating auto-backup " + Path.GetFileNameWithoutExtension(backupPath), 0);
                AssetDatabase.DeleteAsset(backupPath);
                AssetDatabase.CopyAsset(path, backupPath);
                AssetDatabase.Refresh();
#if !(UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9)
                if (!string.IsNullOrEmpty(AssetImporter.GetAtPath(backupPath).assetBundleName))
                {
                    AssetImporter.GetAtPath(backupPath).assetBundleVariant = string.Empty;
                    AssetImporter.GetAtPath(backupPath).assetBundleName = string.Empty;
                }
#endif
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

    }

}