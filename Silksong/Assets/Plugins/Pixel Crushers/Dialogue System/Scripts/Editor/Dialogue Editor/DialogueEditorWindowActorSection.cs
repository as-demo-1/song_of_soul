// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the Actors tab.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        [SerializeField]
        private AssetFoldouts actorFoldouts = new AssetFoldouts();

        [SerializeField]
        private string actorFilter = string.Empty;

        [SerializeField]
        private bool hideFilteredOutActors = false;

        private ReorderableList actorReorderableList = null;

        [SerializeField]
        private int actorListSelectedIndex = -1;

        [SerializeField]
        private bool actorTexturesFoldout = false;

        [SerializeField]
        private bool actorSpritesFoldout = false;

        private HashSet<int> syncedActorIDs = null;

        private List<Actor> filteredActors;

        private void ResetActorSection()
        {
            actorFoldouts = new AssetFoldouts();
            actorAssetList = null;
            actorReorderableList = null;
            actorListSelectedIndex = -1;
            syncedActorIDs = null;
        }

        private void DrawActorSection()
        {
            showStateFieldAsQuest = false;
            if (actorReorderableList == null) InitializeActorReorderableList();
            var filterChanged = DrawFilterMenuBar("Actor", DrawActorMenu, ref actorFilter, ref hideFilteredOutActors);
            if (filterChanged) InitializeActorReorderableList();
            if (database.syncInfo.syncActors)
            {
                DrawActorSyncDatabase();
                if (syncedActorIDs == null) RecordSyncedActorIDs();
            }
            actorReorderableList.DoLayoutList();
        }

        private bool HideFilteredOutActors()
        {
            return hideFilteredOutActors && !string.IsNullOrEmpty(actorFilter);
        }

        private void InitializeActorReorderableList()
        {
            if (HideFilteredOutActors())
            {
                filteredActors = database.actors.FindAll(actor => EditorTools.IsAssetInFilter(actor, actorFilter));
                actorReorderableList = new ReorderableList(filteredActors, typeof(Actor), true, true, true, true);
            }
            else
            {
                filteredActors = database.actors;
                actorReorderableList = new ReorderableList(database.actors, typeof(Actor), true, true, true, true);
            }
            actorReorderableList.drawHeaderCallback = DrawActorListHeader;
            actorReorderableList.drawElementCallback = DrawActorListElement;
            actorReorderableList.drawElementBackgroundCallback = DrawActorListElementBackground;
            actorReorderableList.onAddCallback = OnActorListAdd;
            actorReorderableList.onRemoveCallback = OnActorListRemove;
            actorReorderableList.onSelectCallback = OnActorListSelect;
            actorReorderableList.onReorderCallback = OnActorListReorder;
        }

        private void DrawActorListHeader(Rect rect)
        {
            var fieldWidth = (rect.width - 14) / 4;
            EditorGUI.LabelField(new Rect(rect.x + 14, rect.y, fieldWidth, rect.height), "Name");
            EditorGUI.LabelField(new Rect(rect.x + 14 + fieldWidth + 2, rect.y, 3 * fieldWidth - 2, rect.height), "Description");
        }

        private void DrawActorListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < filteredActors.Count)) return;
            var nameControl = "ActorName" + index;
            var descriptionControl = "ActorDescription" + index;
            var actor = filteredActors[index];
            var fieldWidth = rect.width / 4;
            EditorGUI.BeginDisabledGroup(!EditorTools.IsAssetInFilter(actor, actorFilter) || IsActorSyncedFromOtherDB(actor));
            var actorName = actor.Name;
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName(nameControl);
            actorName = EditorGUI.TextField(new Rect(rect.x, rect.y + 2, fieldWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, actorName);
            if (EditorGUI.EndChangeCheck())
            {
                actor.Name = actorName;
                SetDatabaseDirty("Actor Name");
            }
            var description = actor.Description;
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName(descriptionControl);
            description = EditorGUI.TextField(new Rect(rect.x + fieldWidth + 2, rect.y + 2, 3 * fieldWidth - 2, EditorGUIUtility.singleLineHeight), GUIContent.none, description);
            if (EditorGUI.EndChangeCheck())
            {
                actor.Description = description;
                SetDatabaseDirty("Actor Description");
            }
            EditorGUI.EndDisabledGroup();
            var focusedControl = GUI.GetNameOfFocusedControl();
            if (string.Equals(nameControl, focusedControl) || string.Equals(descriptionControl, focusedControl))
            {
                inspectorSelection = actor;
            }
        }

        private void DrawActorListElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < filteredActors.Count)) return;
            var actor = filteredActors[index];
            if (EditorTools.IsAssetInFilter(actor, actorFilter))
            {
                ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, isActive, isFocused, true);
            }
            else
            {
                EditorGUI.DrawRect(rect, new Color(0.225f, 0.225f, 0.225f, 1));
            }
        }

        private void OnActorListAdd(ReorderableList list)
        {
            AddNewActor();
        }

        private void OnActorListRemove(ReorderableList list)
        {
            if (!(0 <= list.index && list.index < filteredActors.Count)) return;
            var actor = filteredActors[list.index];
            if (actor == null) return;
            if (IsActorSyncedFromOtherDB(actor)) return;
            var deletedLastOne = list.count == 1;
            if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", EditorTools.GetAssetName(actor)), "Are you sure you want to delete this actor?", "Delete", "Cancel"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                if (HideFilteredOutActors())
                {
                    database.actors.Remove(actor);
                }
                if (deletedLastOne) inspectorSelection = null;
                else inspectorSelection = (list.index < list.count) ? filteredActors[list.index] : (list.count > 0) ? filteredActors[list.count - 1] : null;
                SetDatabaseDirty("Remove Actor");
            }
        }

        private void OnActorListReorder(ReorderableList list)
        {
            SetDatabaseDirty("Reorder Actors");
        }

        private void OnActorListSelect(ReorderableList list)
        {
            if (!(0 <= list.index && list.index < filteredActors.Count)) return;
            inspectorSelection = filteredActors[list.index];
            actorListSelectedIndex = list.index;
        }

        public void DrawSelectedActorSecondPart()
        {
            var actor = inspectorSelection as Actor;
            if (actor == null) return;
            DrawFieldsFoldout<Actor>(actor, actorListSelectedIndex, actorFoldouts);
            DrawAssetSpecificPropertiesSecondPart(actor, actorListSelectedIndex, actorFoldouts);
        }

        private void DrawActorMenu()
        {
            if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("New Actor"), false, AddNewActor);
                menu.AddItem(new GUIContent("Sort/By Name"), false, SortActorsByName);
                menu.AddItem(new GUIContent("Sort/By ID"), false, SortActorsByID);
                menu.AddItem(new GUIContent("Sync From DB"), database.syncInfo.syncActors, ToggleSyncActorsFromDB);
                menu.ShowAsContext();
            }
        }

        private void AddNewActor()
        {
            AddNewAsset<Actor>(database.actors);
        }

        private void SortActorsByName()
        {
            database.actors.Sort((x, y) => x.Name.CompareTo(y.Name));
            InitializeActorReorderableList();
            SetDatabaseDirty("Sort Actors by Name");
        }

        private void SortActorsByID()
        {
            database.actors.Sort((x, y) => x.id.CompareTo(y.id));
            InitializeActorReorderableList();
            SetDatabaseDirty("Sort Actors by ID");
        }

        private void ToggleSyncActorsFromDB()
        {
            database.syncInfo.syncActors = !database.syncInfo.syncActors;
            InitializeActorReorderableList();
            SetDatabaseDirty("Toggle Sync Actors");
        }

        private void DrawActorSyncDatabase()
        {
            EditorGUILayout.BeginHorizontal();
            DialogueDatabase newDatabase = EditorGUILayout.ObjectField(new GUIContent("Sync From", "Database to sync actors from."),
                                                                       database.syncInfo.syncActorsDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;
            if (newDatabase != database.syncInfo.syncActorsDatabase)
            {
                database.syncInfo.syncActorsDatabase = newDatabase;
                database.SyncActors();
                InitializeActorReorderableList();
                syncedActorIDs = null;
                SetDatabaseDirty("Change Actor Sync Database");
            }
            if (GUILayout.Button(new GUIContent("Sync Now", "Syncs from the database."), EditorStyles.miniButton, GUILayout.Width(72)))
            {
                database.SyncActors();
                InitializeActorReorderableList();
                syncedActorIDs = null;
                SetDatabaseDirty("Sync Actors");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void RecordSyncedActorIDs()
        {
            syncedActorIDs = new HashSet<int>();
            if (database.syncInfo.syncActors && database.syncInfo.syncActorsDatabase != null)
            {
                database.syncInfo.syncActorsDatabase.actors.ForEach(x => syncedActorIDs.Add(x.id));
            }
        }

        public bool IsActorSyncedFromOtherDB(Actor actor)
        {
            return actor != null && syncedActorIDs != null && syncedActorIDs.Contains(actor.id);
        }

        private void DrawActorPortrait(Actor actor)
        {
            if (actor == null) return;

            // Display Name:
            var displayNameField = Field.Lookup(actor.fields, "Display Name");
            var hasDisplayNameField = (displayNameField != null);
            var useDisplayNameField = EditorGUILayout.Toggle(new GUIContent("Use Display Name", "Tick to use a Display Name in UIs that's different from the Name."), hasDisplayNameField);
            if (hasDisplayNameField && !useDisplayNameField)
            {
                actor.fields.Remove(displayNameField);
                SetDatabaseDirty("Don't Use Display Name");
            }
            else if (useDisplayNameField)
            {
                if (!hasDisplayNameField && string.IsNullOrEmpty(actor.LookupValue("Display Name")))
                {
                    Field.SetValue(actor.fields, "Display Name", actor.Name);
                }
                DrawRevisableTextField(displayNameLabel, actor, null, actor.fields, "Display Name");
                DrawLocalizedVersions(actor, actor.fields, "Display Name {0}", false, FieldType.Text);
            }

            // AI - Voice
            DrawAIVoiceSelection(actor);

            // Portrait Textures:
            actorTexturesFoldout = EditorGUILayout.Foldout(actorTexturesFoldout, new GUIContent("Portrait Textures", "Portrait images using texture assets."));
            if (actorTexturesFoldout)
            {

                try
                {
                    var newPortrait = EditorGUILayout.ObjectField(new GUIContent("Portraits", "This actor's portrait. Only necessary if your UI uses portraits."),
                                                             actor.portrait, typeof(Texture2D), false, GUILayout.Height(64)) as Texture2D;
                    if (newPortrait != actor.portrait)
                    {
                        actor.portrait = newPortrait;
                        ClearActorInfoCaches();
                        SetDatabaseDirty("Actor Portrait");
                    }
                }
                catch (NullReferenceException)
                {
                }
                int indexToDelete = -1;
                if (actor.alternatePortraits == null) actor.alternatePortraits = new List<Texture2D>();
                for (int i = 0; i < actor.alternatePortraits.Count; i++)
                {
                    try
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        try
                        {
                            EditorGUILayout.BeginVertical(GUILayout.Width(27));
                            EditorGUILayout.LabelField(string.Empty, GUILayout.Width(5), GUILayout.Height(16));
                            EditorGUILayout.LabelField(string.Format("[{0}]", i + 2), CenteredLabelStyle, GUILayout.Width(27));
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(string.Empty, GUILayout.Width(5));
                            if (GUILayout.Button(new GUIContent(" ", "Delete this portrait."), "OL Minus", GUILayout.Width(16), GUILayout.Height(16)))
                            {
                                indexToDelete = i;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        finally
                        {
                            EditorGUILayout.EndVertical();
                        }

                        try
                        {
                            EditorGUI.BeginChangeCheck();
                            actor.alternatePortraits[i] = EditorGUILayout.ObjectField(actor.alternatePortraits[i], typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64)) as Texture2D;
                            if (EditorGUI.EndChangeCheck()) SetDatabaseDirty("Actor Portrait");
                        }
                        catch (NullReferenceException)
                        {
                        }
                    }
                    finally
                    {
                        EditorGUILayout.EndHorizontal();
                    }
                }
                if (indexToDelete > -1)
                {
                    actor.alternatePortraits.RemoveAt(indexToDelete);
                    SetDatabaseDirty("Delete Portrait");
                }

                EditorGUILayout.LabelField(string.Empty, GUILayout.Height(4));

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent(" ", "Add new alternate portrait image."), "OL Plus", GUILayout.Height(16)))
                {
                    actor.alternatePortraits.Add(null);
                    SetDatabaseDirty("Add Portrait");
                }
                EditorGUILayout.LabelField(string.Empty, GUILayout.Width(12));
                EditorGUILayout.EndHorizontal();
            }

            // Portrait Sprites:
            EditorGUILayout.BeginHorizontal();
            actorSpritesFoldout = EditorGUILayout.Foldout(actorSpritesFoldout, new GUIContent("Portrait Sprites", "Portrait images using sprite assets."));
            GUILayout.FlexibleSpace();
            DrawAIPortraitSprites(actor);
            EditorGUILayout.EndHorizontal();
            if (actorSpritesFoldout)
            {
                try
                {
                    var newPortrait = EditorGUILayout.ObjectField(new GUIContent("Portraits", "This actor's portrait. Only necessary if your UI uses portraits."),
                                                             actor.spritePortrait, typeof(Sprite), false, GUILayout.Height(64)) as Sprite;
                    if (newPortrait != actor.spritePortrait)
                    {
                        actor.spritePortrait = newPortrait;
                        ClearActorInfoCaches();
                        SetDatabaseDirty("Actor Portrait");
                    }
                }
                catch (NullReferenceException)
                {
                }
                int indexToDelete = -1;
                if (actor.spritePortraits == null) actor.spritePortraits = new List<Sprite>();
                for (int i = 0; i < actor.spritePortraits.Count; i++)
                {
                    try
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        try
                        {
                            EditorGUILayout.BeginVertical(GUILayout.Width(27));
                            EditorGUILayout.LabelField(string.Empty, GUILayout.Width(5), GUILayout.Height(16));
                            EditorGUILayout.LabelField(string.Format("[{0}]", i + 2), CenteredLabelStyle, GUILayout.Width(27));
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(string.Empty, GUILayout.Width(5));
                            if (GUILayout.Button(new GUIContent(" ", "Delete this portrait."), "OL Minus", GUILayout.Width(16), GUILayout.Height(16)))
                            {
                                indexToDelete = i;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        finally
                        {
                            EditorGUILayout.EndVertical();
                        }

                        try
                        {
                            EditorGUI.BeginChangeCheck();
                            actor.spritePortraits[i] = EditorGUILayout.ObjectField(actor.spritePortraits[i], typeof(Sprite), false, GUILayout.Width(64), GUILayout.Height(64)) as Sprite;
                            if (EditorGUI.EndChangeCheck()) SetDatabaseDirty("Actor Portrait");
                        }
                        catch (NullReferenceException)
                        {
                        }
                    }
                    finally
                    {
                        EditorGUILayout.EndHorizontal();
                    }
                }
                if (indexToDelete > -1)
                {
                    actor.spritePortraits.RemoveAt(indexToDelete);
                    SetDatabaseDirty("Delete Portrait");
                }

                EditorGUILayout.LabelField(string.Empty, GUILayout.Height(4));

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent(" ", "Add new alternate portrait image."), "OL Plus", GUILayout.Height(16)))
                {
                    actor.spritePortraits.Add(null);
                    SetDatabaseDirty("Add Portrait");
                }
                EditorGUILayout.LabelField(string.Empty, GUILayout.Width(12));
                EditorGUILayout.EndHorizontal();
            }

            // The rest: node color, Is Player, etc.
            DrawActorNodeColor(actor);

            EditorGUI.BeginChangeCheck();
            actor.IsPlayer = EditorGUILayout.Toggle(new GUIContent("Is Player", ""), actor.IsPlayer);
            if (EditorGUI.EndChangeCheck()) SetDatabaseDirty("IsPlayer");

            DrawActorPrimaryFields(actor);
        }

        private void DrawActorPrimaryFields(Actor actor)
        {
            if (actor == null || actor.fields == null || template.actorPrimaryFieldTitles == null) return;

            var descriptionField = Field.Lookup(actor.fields, DialogueSystemFields.Description);
            if (descriptionField != null)
            {
                DrawMainSectionField(descriptionField);
            }

            foreach (var field in actor.fields)
            {
                var fieldTitle = field.title;
                if (string.IsNullOrEmpty(fieldTitle)) continue;
                if (!template.actorPrimaryFieldTitles.Contains(field.title)) continue;
                DrawMainSectionField(field);
            }
        }

        private const string NodeColorFieldTitle = "NodeColor";

        private void DrawActorNodeColor(Actor actor)
        {
            if (actor == null) return;
            EditorGUILayout.BeginHorizontal();
            var useNodeColor = actor.FieldExists(NodeColorFieldTitle);
            var toggleValue = EditorGUILayout.Toggle(new GUIContent("Custom Node Color", "Specify a custom color for this actor's conversation nodes in the editor."), useNodeColor);
            if (toggleValue != useNodeColor)
            {
                ClearActorInfoCaches();
                switch (toggleValue)
                {
                    case true:
                        actor.fields.Add(new Field(NodeColorFieldTitle, (actor.LookupBool("IsPlayer") ? "Blue" : "Gray"), FieldType.Text));
                        break;
                    case false:
                        actor.fields.Remove(actor.fields.Find(x => string.Equals(x.title, NodeColorFieldTitle)));
                        break;
                }
                SetDatabaseDirty("Actor Node Color");
            }
            if (toggleValue == true)
            {
                var nodeColorField = actor.fields.Find(x => string.Equals(x.title, NodeColorFieldTitle));
                if (nodeColorField != null)
                {
                    if (string.IsNullOrEmpty(nodeColorField.value)) // Make sure we have a valid color.
                    {
                        nodeColorField.value = actor.IsPlayer ? "Blue" : "Gray";
                    }
                    EditorGUI.BeginChangeCheck();
#if UNITY_5 || UNITY_2017
                    var nodeColor = EditorGUILayout.ColorField(GUIContent.none, EditorTools.NodeColorStringToColor(nodeColorField.value), true, true, false, null);
#else
                    var nodeColor = EditorGUILayout.ColorField(GUIContent.none, EditorTools.NodeColorStringToColor(nodeColorField.value), true, true, false);
#endif
                    nodeColor.a = 1; // Force solid alpha.
                    if (EditorGUI.EndChangeCheck())
                    {
                        nodeColorField.value = Tools.ToWebColor(nodeColor);
                        SetDatabaseDirty("Actor Node Color");
                    }

                    //---Was: (Used to use Unity's built-in node style, but it had limited color options.)
                    //int index = 0;
                    //for (int i = 0; i < EditorTools.StylesColorStrings.Length; i++)
                    //{
                    //    if (string.Equals(nodeColorField.value, EditorTools.StylesColorStrings[i]))
                    //    {
                    //        index = i;
                    //    }
                    //}
                    //var newIndex = EditorGUILayout.Popup(index, EditorTools.StylesColorStrings);
                    //if (newIndex != index)
                    //{
                    //    ClearActorInfoCaches();
                    //    nodeColorField.value = (0 <= newIndex && newIndex < EditorTools.StylesColorStrings.Length)
                    //        ? EditorTools.StylesColorStrings[newIndex]
                    //        : (actor.LookupBool("IsPlayer") ? "Blue" : "Gray");
                    //}
                }
            }
            EditorGUILayout.EndHorizontal();
        }

    }

}