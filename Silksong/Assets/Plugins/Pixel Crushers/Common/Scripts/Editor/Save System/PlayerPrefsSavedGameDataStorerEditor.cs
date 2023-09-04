// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System;

namespace PixelCrushers
{

    [CustomEditor(typeof(PlayerPrefsSavedGameDataStorer), true)]
    public class PlayerPrefsSavedGameDataStorerEditor : Editor
    {

        private const int MaxSlots = 100;
        private string EmptySlotString = "-empty-";

        private List<string> m_keys;
        private ReorderableList m_list;

        protected virtual void OnEnable()
        {
            var storer = target as PlayerPrefsSavedGameDataStorer;

            // Get active keys:
            m_keys = new List<string>();
            int lastActiveSlot = -1;
            for (int i = 1; i < MaxSlots; i++)
            {
                if (storer.HasDataInSlot(i)) lastActiveSlot = i;
            }
            for (int i = 1; i <= lastActiveSlot; i++)
            {
                m_keys.Add(storer.HasDataInSlot(i) ? storer.GetPlayerPrefsKey(i) : EmptySlotString);
            }

            // Setup editor list:
            m_list = new ReorderableList(m_keys, typeof(string), false, true, false, true);
            m_list.drawHeaderCallback = OnDrawHeader;
            m_list.drawElementCallback = OnDrawElement;
            m_list.onRemoveCallback = OnRemoveElement;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawSavedGameList();
        }

        protected virtual void DrawSavedGameList()
        {
            if (m_list != null) m_list.DoLayoutList();
            if (GUILayout.Button(new GUIContent("Clear Saved Games", "Delete all PlayerPrefs keys associated with saved games.")))
            {
                if (EditorUtility.DisplayDialog("Clear Saved Games", "Delete all PlayerPrefs keys associated with saved games?", "OK", "Cancel"))
                {
                    ClearSavedGames();
                }
            }
        }

        private void OnDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Saved Games");
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < m_keys.Count)) return;
            var key = m_keys[index];
            int buttonWidth = 48;
            var keyRect = new Rect(rect.x, rect.y + 1, rect.width - buttonWidth, EditorGUIUtility.singleLineHeight);
            var showRect = new Rect(rect.x + rect.width - buttonWidth, rect.y + 1, buttonWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(keyRect, key);
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(string.Equals(key, EmptySlotString));
            if (GUI.Button(showRect, "Show"))
            {
                Debug.Log(key + ": " + PlayerPrefs.GetString(key));
            }
            EditorGUI.EndDisabledGroup();
        }

        private void OnRemoveElement(ReorderableList list)
        {
            if (!(0 <= list.index && list.index < m_keys.Count)) return;
            var key = m_keys[list.index];
            if (EditorUtility.DisplayDialog("Delete Saved Game", "Delete saved game " + key + "?", "OK", "Cancel"))
            {
                PlayerPrefs.DeleteKey(key);
                m_keys[list.index] = EmptySlotString;
            }
        }


        private void ClearSavedGames()
        {
            var baseKey = (target as PlayerPrefsSavedGameDataStorer).playerPrefsKeyBase;
            for (int i = 0; i < 100; i++)
            {
                var key = baseKey + i;
                if (PlayerPrefs.HasKey(key)) PlayerPrefs.DeleteKey(key);
            }
            m_keys.Clear();
            Repaint();
        }

    }

}