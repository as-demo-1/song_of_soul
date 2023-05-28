// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace PixelCrushers
{

    /// <summary>
    /// Utility menu items for the Save System.
    /// </summary>
    public static class SaveSystemEditorUtility
    {

        private static HashSet<string> s_keys = new HashSet<string>();

        [MenuItem("Tools/Pixel Crushers/Common/Save System/Assign Unique Keys...", false, 0)]
        public static void AssignUniqueKeysDialog()
        {
            if (EditorUtility.DisplayDialog("Assign Unique Saver Keys", "Assign unique keys to all Saver components in the current scene whose Key fields are currently blank?", "OK", "Cancel"))
            {
                AssignUniqueKeysInScene();
            }
        }

        public static void AssignUniqueKeysInScene()
        {
            s_keys.Clear();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var s = EditorSceneManager.GetSceneAt(i);
                if (s.isLoaded)
                {
                    var allGameObjects = s.GetRootGameObjects();
                    for (int j = 0; j < allGameObjects.Length; j++)
                    {
                        AssignUniqueKeysInTransformHierarchy(allGameObjects[j].transform);
                    }
                }
            }
            s_keys.Clear();
        }

        private static void AssignUniqueKeysInTransformHierarchy(Transform t)
        {
            if (t == null) return;
            var savers = t.GetComponents<Saver>();
            for (int i = 0; i < savers.Length; i++)
            {
                var saver = savers[i];
                if (string.IsNullOrEmpty(saver._internalKeyValue))
                {
                    // Key is not assigned yet. Assign one:
                    AssignNewKey(saver, " [new]");
                }
                else if (s_keys.Contains(saver._internalKeyValue))
                {
                    // Key is already used. Assign a new one:
                    AssignNewKey(saver, " [key '" + saver._internalKeyValue + "' already exists; make unique]");
                }
                s_keys.Add(saver._internalKeyValue);
            }
            foreach (Transform child in t)
            {
                AssignUniqueKeysInTransformHierarchy(child);
            }
        }

        private static void AssignNewKey(Saver saver, string reason)
        {
            if (saver == null) return;
            var key = CleanName(saver.name) + "_" + Mathf.Abs(saver.GetInstanceID());
            Debug.Log(saver.name + "." + saver.GetType().Name + ".Key = " + key + reason, saver);
            Undo.RecordObject(saver, "Key");
            saver._internalKeyValue = key;
            saver.appendSaverTypeToKey = false;
        }

        private static string CleanName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return System.Text.RegularExpressions.Regex.Replace(name, @"[\(\s\)\.\,\'""]", string.Empty);
        }
    }
}
