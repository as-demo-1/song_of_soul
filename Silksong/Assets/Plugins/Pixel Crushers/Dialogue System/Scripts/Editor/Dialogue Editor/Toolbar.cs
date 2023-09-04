// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the toolbar at the top of the window.
    /// </summary>
    [Serializable]
    public class Toolbar
    {

        public enum Tab { Database, Actors, Items, Locations, Variables, Conversations, Templates }

        public Tab current = Tab.Database;
        public Tab Current
        {
            get { return current; }
            set { current = value; }
        }

        private string[] ToolbarStrings = { "Database", "Actors", "Items", "Locations", "Variables", "Conversations", "Templates" };
        private const int ItemsToolbarIndex = 2;
        private const string ItemsToolbarString = "Items";
        private const string ItemsAsQuestsToolbarString = "Quests/Items";
        private const int TemplatesToolbarIndex = 6;
        private const string TemplatesToolbarString = "Templates";
        private const string WatchesToolbarString = "Watches";
        private const float ToolbarWidth = 700;

        public void UpdateTabNames(bool treatItemsAsQuests)
        {
            ToolbarStrings[ItemsToolbarIndex] = treatItemsAsQuests ? ItemsAsQuestsToolbarString : ItemsToolbarString;
            ToolbarStrings[TemplatesToolbarIndex] = Application.isPlaying ? WatchesToolbarString : TemplatesToolbarString;
        }

        public void Draw()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            current = (Tab)GUILayout.Toolbar((int)current, ToolbarStrings, GUILayout.Width(ToolbarWidth));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorWindowTools.DrawHorizontalLine();
        }

    }

}