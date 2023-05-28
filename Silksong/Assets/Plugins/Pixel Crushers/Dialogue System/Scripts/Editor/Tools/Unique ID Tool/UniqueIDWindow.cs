// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PixelCrushers.DialogueSystem
{

    public class UniqueIDWindow : EditorWindow
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Tools/Unique ID Tool", false, 3)]
        public static void OpenUniqueIDWindow()
        {
            instance = EditorWindow.GetWindow<UniqueIDWindow>(false, "Unique IDs");
        }

        public static UniqueIDWindow instance = null;

        // Private fields for the window:

        public UniqueIDWindowPrefs prefs = null;
        private Template template = null;

        private bool verbose = false;
        public string report;

        private Vector2 scrollPosition = Vector2.zero;

        void OnEnable()
        {
            minSize = new Vector2(340, 128);
            if (prefs == null)
            {
                prefs = UniqueIDWindowPrefs.Load();
                prefs.RelinkDatabases();
            }
            template = TemplateTools.LoadFromEditorPrefs();
        }

        void OnDisable()
        {
            if (prefs != null) prefs.Save();
        }

        void OnGUI()
        {
            // Validate prefs:
            if (prefs == null) prefs = UniqueIDWindowPrefs.Load();
            if (prefs.databases == null) prefs.databases = new List<DialogueDatabase>();

            // Draw window:
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            try
            {
                EditorGUILayout.HelpBox("This tool reassigns IDs so all Actors, Items, Locations, Conversations have unique IDs. " +
                                        "This allows your project to load multiple dialogue databases at runtime without conflicting IDs. " +
                                        "Actors, Items, and Locations with the same name will be assigned the same ID. " +
                                        "All conversations will have unique IDs, even if two conversations have the same title.",
                                        MessageType.None);
                DrawDatabaseSection();
                DrawButtonSection();
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// Draws the database list section.
        /// </summary>
        private void DrawDatabaseSection()
        {
            DrawDatabaseHeader();
            DrawDatabaseList();
        }

        private void DrawDatabaseHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dialogue Databases", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("All", EditorStyles.miniButtonLeft, GUILayout.Width(48)))
            {
                if (EditorUtility.DisplayDialog("Add All Databases in Project",
                                                string.Format("Do you want to find and add all dialogue databases in the entire project?", EditorWindowTools.GetCurrentDirectory()),
                                                "Ok", "Cancel"))
                {
                    AddAllDatabasesInProject();
                }
            }
            if (GUILayout.Button("Folder", EditorStyles.miniButtonMid, GUILayout.Width(48)))
            {
                if (EditorUtility.DisplayDialog("Add All Databases in Folder",
                                                string.Format("Do you want to find and add all dialogue databases in the folder {0}?", EditorWindowTools.GetCurrentDirectory()),
                                                "Ok", "Cancel"))
                {
                    AddAllDatabasesInFolder(EditorWindowTools.GetCurrentDirectory(), false);
                }
            }
            if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                prefs.databases.Add(null);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDatabaseList()
        {
            EditorGUI.BeginChangeCheck();
            EditorWindowTools.StartIndentedSection();
            DialogueDatabase databaseToDelete = null;
            for (int i = 0; i < prefs.databases.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                prefs.databases[i] = EditorGUILayout.ObjectField(prefs.databases[i], typeof(DialogueDatabase), false) as DialogueDatabase;
                if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(22))) databaseToDelete = prefs.databases[i];
                EditorGUILayout.EndHorizontal();

            }
            if (databaseToDelete != null) prefs.databases.Remove(databaseToDelete);
            EditorWindowTools.EndIndentedSection();
            if (EditorGUI.EndChangeCheck()) prefs.Save();
        }

        public void AddAllDatabasesInProject()
        {
            prefs.databases.Clear();
            AddAllDatabasesInFolder("Assets", true);
            prefs.Save();
        }

        public void AddAllDatabasesInFolder(string folderPath, bool recursive)
        {
            string[] filePaths = Directory.GetFiles(folderPath, "*.asset", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (string filePath in filePaths)
            {
                string assetPath = filePath.Replace("\\", "/");
                DialogueDatabase database = AssetDatabase.LoadAssetAtPath(assetPath, typeof(DialogueDatabase)) as DialogueDatabase;
                if ((database != null) && (!prefs.databases.Contains(database)))
                {
                    prefs.databases.Add(database);
                }
            }
            prefs.Save();
        }

        private void DrawButtonSection()
        {
            verbose = EditorGUILayout.Toggle("Verbose Logging", verbose);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawClearButton();
            DrawProcessButton();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the Clear button, and clears the prefs if clicked.
        /// </summary>
        private void DrawClearButton()
        {
            if (GUILayout.Button("Clear", GUILayout.Width(100)))
            {
                prefs.Clear();
                UniqueIDWindowPrefs.DeleteEditorPrefs();
            }
        }

        /// <summary>
        /// Draws the Process button, and processes the databases if clicked.
        /// </summary>
        private void DrawProcessButton()
        {
            if (GUILayout.Button("Process", GUILayout.Width(100))) ProcessDatabases();
        }

        private class IDConversion
        {
            public int oldID = 0;
            public int newID = 0;
            public IDConversion() { }
            public IDConversion(int oldID, int newID)
            {
                this.oldID = oldID;
                this.newID = newID;
            }
        }

        private class MasterIDs
        {
            public Dictionary<string, IDConversion> actors = new Dictionary<string, IDConversion>();
            public Dictionary<string, IDConversion> items = new Dictionary<string, IDConversion>();
            public Dictionary<string, IDConversion> locations = new Dictionary<string, IDConversion>();
            public Dictionary<string, IDConversion> variables = new Dictionary<string, IDConversion>();
            public HashSet<int> usedNewActorIDs = new HashSet<int>();
            public HashSet<int> usedNewItemIDs = new HashSet<int>();
            public HashSet<int> usedNewLocationIDs = new HashSet<int>();
            public HashSet<int> usedNewVariableIDs = new HashSet<int>();
            public HashSet<int> usedNewConversationIDs = new HashSet<int>();
            public int highestActorID = 0;
            public int highestItemID = 0;
            public int highestLocationID = 0;
            public int highestVariableID = 0;
            public int highestConversationID = 0;
        }

        public void ProcessDatabases()
        {
            try
            {
                report = "Unique ID Tool Report:";
                List<DialogueDatabase> distinct = prefs.databases.Distinct().ToList();
                if (distinct.Count == 0)
                {
                    report += " No databases to process.";
                    return;
                }
                MasterIDs masterIDs = new MasterIDs();
                for (int i = 0; i < distinct.Count; i++)
                {
                    var database = distinct[i];
                    if (database != null)
                    {
                        EditorUtility.DisplayProgressBar("Processing Databases (Phase 1/2)", database.name, i / prefs.databases.Count);
                        GetNewIDs(database, masterIDs);
                        if (!VerifyUniqueConversationIDs(database)) return;
                    }
                }
                for (int i = 0; i < distinct.Count; i++)
                {
                    var database = distinct[i];
                    if (database != null)
                    {
                        EditorUtility.DisplayProgressBar("Processing Databases (Phase 2/2)", database.name, i / prefs.databases.Count);
                        ProcessDatabase(database, masterIDs);
                        EditorUtility.SetDirty(database);
                    }
                }
                if (verbose) report += "\n";
                report += "Assigned unique IDs to " + prefs.databases.Count + " databases.";
                AssetDatabase.SaveAssets();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                Debug.Log(report);
            }
        }

        private bool VerifyUniqueConversationIDs(DialogueDatabase database)
        {
            var result = true;
            HashSet<int> conversationIDs = new HashSet<int>();
            for (int i = 0; i < database.conversations.Count; i++)
            {
                var conversation = database.conversations[i];
                if (!conversationIDs.Contains(conversation.id))
                {
                    conversationIDs.Add(conversation.id);
                }
                else
                {
                    var s = "Unable to process conversations. In database '" + database +
                        "' two or more conversations have the same conversation ID " + conversation.id + ":";
                    for (int j = 0; j < database.conversations.Count; j++)
                    {
                        s += "\n" + database.conversations[j].Title;
                    }
                    Debug.LogError(s, database);
                    report += "\n" + s;
                    result = false;
                }
            }
            return result;
        }

        private void GetNewIDs(DialogueDatabase database, MasterIDs masterIDs)
        {
            if (verbose) report += "\nDetermining new IDs for database " + database.name;
            GetNewActorIDs(database, masterIDs);
            GetNewItemIDs(database, masterIDs);
            GetNewLocationIDs(database, masterIDs);
            GetNewVariableIDs(database, masterIDs);
        }

        private void ProcessDatabase(DialogueDatabase database, MasterIDs masterIDs)
        {
            if (verbose) report += "\nConverting IDs in database " + database.name;
            ProcessConversations(database, masterIDs);
            ProcessActors(database, masterIDs);
            ProcessItems(database, masterIDs);
            ProcessLocations(database, masterIDs);
            ProcessVariables(database, masterIDs);
        }

        private void GetNewActorIDs(DialogueDatabase database, MasterIDs masterIDs)
        {
            foreach (var actor in database.actors)
            {
                if (masterIDs.actors.ContainsKey(actor.Name))
                {
                    masterIDs.actors[actor.Name].oldID = actor.id;
                }
                else
                {
                    int newID;
                    if (!masterIDs.usedNewActorIDs.Contains(actor.id))
                    {
                        // ID is unique so far, so no need to assign new ID.
                        newID = actor.id;
                    }
                    else
                    {
                        // Find a new ID:
                        newID = template.GetNextActorID(database); // Try new ID from database.baseID.
                        if (masterIDs.usedNewActorIDs.Contains(newID))
                        {
                            // If can't get a unique ID from database.baseID, use highest ID + 1.
                            if (actor.id <= masterIDs.highestActorID)
                            {
                                masterIDs.highestActorID++;
                                newID = masterIDs.highestActorID;
                            }
                            else
                            {
                                newID = actor.id;
                            }
                        }
                    }
                    masterIDs.highestActorID = Mathf.Max(masterIDs.highestActorID, newID);
                    masterIDs.usedNewActorIDs.Add(newID);
                    masterIDs.actors.Add(actor.Name, new IDConversion(actor.id, newID));
                }
            }
        }

        private void GetNewItemIDs(DialogueDatabase database, MasterIDs masterIDs)
        {
            foreach (var item in database.items)
            {
                if (masterIDs.items.ContainsKey(item.Name))
                {
                    masterIDs.items[item.Name].oldID = item.id;
                }
                else
                {
                    int newID;
                    if (!masterIDs.usedNewItemIDs.Contains(item.id))
                    {
                        // ID is unique so far, so no need to assign new ID.
                        newID = item.id;
                    }
                    else
                    {
                        // Find a new ID:
                        newID = template.GetNextItemID(database); // Try new ID from database.baseID.
                        if (masterIDs.usedNewItemIDs.Contains(newID))
                        {
                            // If can't get a unique ID from database.baseID, use highest ID + 1.
                            if (item.id <= masterIDs.highestItemID)
                            {
                                masterIDs.highestItemID++;
                                newID = masterIDs.highestItemID;
                            }
                            else
                            {
                                newID = item.id;
                            }
                        }
                    }
                    masterIDs.highestItemID = Mathf.Max(masterIDs.highestItemID, newID);
                    masterIDs.usedNewItemIDs.Add(newID);
                    masterIDs.items.Add(item.Name, new IDConversion(item.id, newID));
                }
            }
        }

        private void GetNewLocationIDs(DialogueDatabase database, MasterIDs masterIDs)
        {
            foreach (var location in database.locations)
            {
                if (masterIDs.locations.ContainsKey(location.Name))
                {
                    masterIDs.locations[location.Name].oldID = location.id;
                }
                else
                {
                    int newID;
                    if (!masterIDs.usedNewLocationIDs.Contains(location.id))
                    {
                        // ID is unique so far, so no need to assign new ID.
                        newID = location.id;
                    }
                    else
                    {
                        // Find a new ID:
                        newID = template.GetNextLocationID(database); // Try new ID from database.baseID.
                        if (masterIDs.usedNewLocationIDs.Contains(newID))
                        {
                            // If can't get a unique ID from database.baseID, use highest ID + 1.
                            if (location.id <= masterIDs.highestLocationID)
                            {
                                masterIDs.highestLocationID++;
                                newID = masterIDs.highestLocationID;
                            }
                            else
                            {
                                newID = location.id;
                            }
                        }
                    }
                    masterIDs.highestLocationID = Mathf.Max(masterIDs.highestLocationID, newID);
                    masterIDs.usedNewLocationIDs.Add(newID);
                    masterIDs.locations.Add(location.Name, new IDConversion(location.id, newID));
                }
            }
        }

        private void GetNewVariableIDs(DialogueDatabase database, MasterIDs masterIDs)
        {
            foreach (var variable in database.variables)
            {
                if (masterIDs.variables.ContainsKey(variable.Name))
                {
                    masterIDs.variables[variable.Name].oldID = variable.id;
                }
                else
                {
                    int newID;
                    if (!masterIDs.usedNewVariableIDs.Contains(variable.id))
                    {
                        // ID is unique so far, so no need to assign new ID.
                        newID = variable.id;
                    }
                    else
                    {
                        // Find a new ID:
                        newID = template.GetNextVariableID(database); // Try new ID from database.baseID.
                        if (masterIDs.usedNewVariableIDs.Contains(newID))
                        {
                            // If can't get a unique ID from database.baseID, use highest ID + 1.
                            if (variable.id <= masterIDs.highestVariableID)
                            {
                                masterIDs.highestVariableID++;
                                newID = masterIDs.highestVariableID;
                            }
                            else
                            {
                                newID = variable.id;
                            }
                        }
                    }
                    masterIDs.highestVariableID = Mathf.Max(masterIDs.highestVariableID, newID);
                    masterIDs.usedNewVariableIDs.Add(newID);
                    masterIDs.variables.Add(variable.Name, new IDConversion(variable.id, newID));
                }
            }
        }

        private void ProcessFieldIDs(DialogueDatabase database, List<Field> fields, MasterIDs masterIDs)
        {
            foreach (var field in fields)
            {
                int oldID = Tools.StringToInt(field.value);
                switch (field.type)
                {
                    case FieldType.Actor:
                        Actor actor = database.GetActor(oldID);
                        if (actor != null) field.value = FindIDConversion(actor.Name, masterIDs.actors, oldID).ToString();
                        break;
                    case FieldType.Item:
                        Item item = database.GetItem(oldID);
                        if (item != null) field.value = FindIDConversion(item.Name, masterIDs.items, oldID).ToString();
                        break;
                    case FieldType.Location:
                        Location location = database.GetLocation(oldID);
                        if (location != null) field.value = FindIDConversion(location.Name, masterIDs.locations, oldID).ToString();
                        break;
                }
            }
        }

        private int FindIDConversion(string name, Dictionary<string, IDConversion> dict, int oldID)
        {
            if (dict.ContainsKey(name))
            {
                return dict[name].newID;
            }
            else
            {
                var s = "Warning: No ID conversion entry found for " + name;
                Debug.LogWarning(s);
                report += "\n" + s;
                return oldID;
            }
        }

        private void ProcessActors(DialogueDatabase database, MasterIDs masterIDs)
        {
            foreach (var actor in database.actors)
            {
                int newID = FindIDConversion(actor.Name, masterIDs.actors, actor.id);
                if (newID != actor.id)
                {
                    if (verbose) report += string.Format("\nActor {0}: ID [{1}]-->[{2}]", actor.Name, actor.id, newID);
                    actor.id = newID;
                }
                ProcessFieldIDs(database, actor.fields, masterIDs);
            }
        }

        private void ProcessItems(DialogueDatabase database, MasterIDs masterIDs)
        {
            foreach (var item in database.items)
            {
                int newID = FindIDConversion(item.Name, masterIDs.items, item.id);
                if (newID != item.id)
                {
                    if (verbose) report += string.Format("\nItem {0}: ID [{1}]-->[{2}]", item.Name, item.id, newID);
                    item.id = newID;
                }
                ProcessFieldIDs(database, item.fields, masterIDs);
            }
        }

        private void ProcessLocations(DialogueDatabase database, MasterIDs masterIDs)
        {
            foreach (var location in database.locations)
            {
                int newID = FindIDConversion(location.Name, masterIDs.locations, location.id);
                if (newID != location.id)
                {
                    if (verbose) report += string.Format("\nLocation {0}: ID [{1}]-->[{2}]", location.Name, location.id, newID);
                    location.id = newID;
                }
                ProcessFieldIDs(database, location.fields, masterIDs);
            }
        }

        private void ProcessVariables(DialogueDatabase database, MasterIDs masterIDs)
        {
            foreach (var variable in database.variables)
            {
                int newID = FindIDConversion(variable.Name, masterIDs.variables, variable.id);
                if (newID != variable.id)
                {
                    if (verbose) report += string.Format("\nVariable {0}: ID [{1}]-->[{2}]", variable.Name, variable.id, newID);
                    variable.id = newID;
                }
                ProcessFieldIDs(database, variable.fields, masterIDs);
            }
        }

        private void ProcessConversations(DialogueDatabase database, MasterIDs masterIDs)
        {
            Dictionary<int, int> newIDs = GetNewConversationIDs(database, masterIDs);
            foreach (var conversation in database.conversations)
            {
                if (newIDs.ContainsKey(conversation.id))
                {
                    if (verbose) report += string.Format("\nConversation '{0}': ID [{1}]-->[{2}]", conversation.Title, conversation.id, newIDs[conversation.id]);
                    conversation.id = newIDs[conversation.id];
                    ProcessFieldIDs(database, conversation.fields, masterIDs);
                    foreach (DialogueEntry entry in conversation.dialogueEntries)
                    {
                        entry.conversationID = conversation.id;
                        ProcessFieldIDs(database, entry.fields, masterIDs);
                        foreach (var link in entry.outgoingLinks)
                        {
                            if (newIDs.ContainsKey(link.originConversationID)) link.originConversationID = newIDs[link.originConversationID];
                            if (newIDs.ContainsKey(link.destinationConversationID)) link.destinationConversationID = newIDs[link.destinationConversationID];
                        }
                    }
                }
            }
        }

        private Dictionary<int, int> GetNewConversationIDs(DialogueDatabase database, MasterIDs masterIDs)
        {
            Dictionary<int, int> newIDs = new Dictionary<int, int>();
            foreach (var conversation in database.conversations)
            {
                if (!masterIDs.usedNewConversationIDs.Contains(conversation.id))
                {
                    // ID is unique so far, so no need to assign new ID.
                    masterIDs.usedNewConversationIDs.Add(conversation.id);
                    masterIDs.highestConversationID = Mathf.Max(masterIDs.highestConversationID, conversation.id);
                }
                else
                {
                    // Find a new ID:
                    int newID;
                    newID = template.GetNextConversationID(database); // Try new ID from database.baseID.
                    if (masterIDs.usedNewConversationIDs.Contains(newID))
                    {
                        // If can't get a unique ID from database.baseID, use highest ID + 1.
                        masterIDs.highestConversationID++;
                        newID = masterIDs.highestConversationID;
                        if (newIDs.ContainsKey(conversation.id))
                        {
                            var s = "Unique ID Tool error: In '" + database + "' more than one conversation uses ID " + conversation.id + ".";
                            Debug.LogError(s, database);
                            report += "\n" + s;
                            newID = conversation.id;
                        }
                    }
                    masterIDs.highestConversationID = Mathf.Max(masterIDs.highestConversationID, newID);
                    masterIDs.usedNewConversationIDs.Add(newID);
                    newIDs.Add(conversation.id, newID);
                }
            }
            return newIDs;
        }

    }

}
