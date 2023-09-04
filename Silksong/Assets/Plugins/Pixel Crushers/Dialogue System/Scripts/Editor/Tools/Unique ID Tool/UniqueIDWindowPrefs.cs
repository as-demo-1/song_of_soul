// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This class manages the Unique ID Window prefs. It allows the window to save
    /// prefs to EditorPrefs between sessions.
    /// </summary>
    [Serializable]
    public class UniqueIDWindowPrefs : ISerializationCallbackReceiver
    {

        private const string UniqueIDWindowPrefsKey = "PixelCrushers.DialogueSystem.UniqueIDTool";

        public List<DialogueDatabase> databases = new List<DialogueDatabase>();

        public List<string> guids = new List<string>();

        public UniqueIDWindowPrefs() { }

        /// <summary>
        /// Clears the prefs.
        /// </summary>
        public void Clear()
        {
            databases.Clear();
        }

        /// <summary>
        /// Deletes the prefs from EditorPrefs.
        /// </summary>
        public static void DeleteEditorPrefs()
        {
            EditorPrefs.DeleteKey(UniqueIDWindowPrefsKey);
        }

        /// <summary>
        /// Load the prefs from EditorPrefs.
        /// </summary>
        public static UniqueIDWindowPrefs Load()
        {
            var prefs = EditorPrefs.HasKey(UniqueIDWindowPrefsKey) ? JsonUtility.FromJson<UniqueIDWindowPrefs>(EditorPrefs.GetString(UniqueIDWindowPrefsKey))
                : new UniqueIDWindowPrefs();
            if (prefs == null) prefs = new UniqueIDWindowPrefs();
            return prefs;
        }

        /// <summary>
        /// Save the prefs to EditorPrefs.
        /// </summary>
        public void Save()
        {
            EditorPrefs.SetString(UniqueIDWindowPrefsKey, JsonUtility.ToJson(this));
        }

        public void OnBeforeSerialize()
        {
            guids.Clear();
            foreach (var database in databases)
            {
                if (database == null) continue;
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(database));
                guids.Add(guid);
            }
        }

        public void OnAfterDeserialize() { }

        public void RelinkDatabases()
        {
            databases.Clear();
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(assetPath)) continue;
                var database = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(assetPath);
                if (database == null) continue;
                databases.Add(database);
            }
        }
    }
}
