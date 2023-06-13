// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    [Serializable]
    public class LocalizationToolsWindowPrefs
    {
        public string mainLocalizationLanguage = DefaultMainLocalizationLanguage;

        public List<string> databaseGuids = new List<string>();

        public List<DialogueDatabase> databases = new List<DialogueDatabase>();

        private const string LocToolsKey = "DialogueSystem.LocalizationToolsPrefs";
        private const string DefaultMainLocalizationLanguage = "en";

        public static LocalizationToolsWindowPrefs Load()
        {
            var prefs = EditorPrefs.HasKey(LocToolsKey) ? JsonUtility.FromJson<LocalizationToolsWindowPrefs>(EditorPrefs.GetString(LocToolsKey)) : null;
            if (prefs != null)
            {
                prefs.databases.Clear();
                foreach (string guid in prefs.databaseGuids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(assetPath)) continue;
                    var database = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(assetPath);
                    if (database == null) continue;
                    prefs.databases.Add(database);
                }
                return prefs;
            }
            else
            {
                return new LocalizationToolsWindowPrefs();
            }
        }

        public void Save()
        {
            databaseGuids.Clear();
            foreach (DialogueDatabase database in databases)
            {
                if (database == null) continue;
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(database));
                databaseGuids.Add(guid);
            }
            EditorPrefs.SetString(LocToolsKey, JsonUtility.ToJson(this));
        }

        public void Clear()
        {
            mainLocalizationLanguage = DefaultMainLocalizationLanguage;
            databaseGuids.Clear();
            databases.Clear();
        }
    }
}
