// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the Items tab. Since the quest system
    /// also uses the Item table, this part handles quests as well as standard items.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        [SerializeField]
        private AssetFoldouts itemFoldouts = new AssetFoldouts();

        [SerializeField]
        private string itemFilter = string.Empty;

        private bool needToBuildLanguageListFromItems = true;

        private ReorderableList itemReorderableList = null;

        [SerializeField]
        private int itemListSelectedIndex = -1;

        [SerializeField]
        private int questEntrySelectedIdx = -1;

        [SerializeField]
        private bool showCompactQuestEntryList = true; // Thanks to Tasta for compact view idea.

        private HashSet<int> syncedItemIDs = null;

        private int isAddingNewFieldToEntryNumber = -1;
        private Field newEntryField;
        
        private void ResetItemSection()
        {
            itemFoldouts = new AssetFoldouts();
            itemAssetList = null;
            UpdateTreatItemsAsQuests(template.treatItemsAsQuests);
            needToBuildLanguageListFromItems = true;
            itemReorderableList = null;
            itemListSelectedIndex = -1;
            syncedItemIDs = null;
        }

        private void UpdateTreatItemsAsQuests(bool newValue)
        {
            if (newValue != template.treatItemsAsQuests)
            {
                template.treatItemsAsQuests = newValue;
                toolbar.UpdateTabNames(newValue);
            }
        }

        private void BuildLanguageListFromItems()
        {
            if (database == null || database.items == null) return;
            database.items.ForEach(item => { if (item.fields != null) BuildLanguageListFromFields(item.fields); });
            needToBuildLanguageListFromItems = false;
        }

        private void DrawItemSection()
        {
            if (template.treatItemsAsQuests)
            {
                if (needToBuildLanguageListFromItems) BuildLanguageListFromItems();
                if (itemReorderableList == null) InitializeItemReorderableList();
                DrawFilterMenuBar("Quests/Item", DrawItemMenu, ref itemFilter);
                if (database.syncInfo.syncItems)
                {
                    DrawItemSyncDatabase();
                    if (syncedItemIDs == null) RecordSyncedItemIDs();
                }
                itemReorderableList.DoLayoutList();
            }
            else
            {
                if (itemReorderableList == null) InitializeItemReorderableList();
                DrawFilterMenuBar("Item", DrawItemMenu, ref itemFilter);
                if (database.syncInfo.syncItems) DrawItemSyncDatabase();
                itemReorderableList.DoLayoutList();
            }
        }

        private void InitializeItemReorderableList()
        {
            itemReorderableList = new ReorderableList(database.items, typeof(Item), true, true, true, true);
            itemReorderableList.drawHeaderCallback = DrawItemListHeader;
            itemReorderableList.drawElementCallback = DrawItemListElement;
            itemReorderableList.drawElementBackgroundCallback = DrawItemListElementBackground;
            if (template.treatItemsAsQuests)
            {
                itemReorderableList.onAddDropdownCallback = OnAddItemOrQuestDropdown;
            }
            else
            {
                itemReorderableList.onAddCallback = OnItemListAdd;
            }            
            itemReorderableList.onRemoveCallback = OnItemListRemove;
            itemReorderableList.onSelectCallback = OnItemListSelect;
            itemReorderableList.onReorderCallback = OnItemListReorder;
        }

        private const float ItemReorderableListTypeWidth = 40f;

        private void DrawItemListHeader(Rect rect)
        {
            if (template.treatItemsAsQuests)
            {
                var fieldWidth = (rect.width - 14 - ItemReorderableListTypeWidth) / 4;
                EditorGUI.LabelField(new Rect(rect.x + 14, rect.y, ItemReorderableListTypeWidth, rect.height), "Type");
                EditorGUI.LabelField(new Rect(rect.x + 14 + ItemReorderableListTypeWidth, rect.y, fieldWidth, rect.height), "Name");
                EditorGUI.LabelField(new Rect(rect.x + 14 + ItemReorderableListTypeWidth + fieldWidth + 2, rect.y, 3 * fieldWidth - 2, rect.height), "Description");
            }
            else
            {
                var fieldWidth = (rect.width - 14) / 4;
                EditorGUI.LabelField(new Rect(rect.x + 14, rect.y, fieldWidth, rect.height), "Name");
                EditorGUI.LabelField(new Rect(rect.x + 14 + fieldWidth + 2, rect.y, 3 * fieldWidth - 2, rect.height), "Description");
            }
        }

        private void DrawItemListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < database.items.Count)) return;
            var nameControl = "ItemName" + index;
            var descriptionControl = "ItemDescription" + index;
            var item = database.items[index];
            var itemName = item.Name;
            var description = item.Description;
            EditorGUI.BeginDisabledGroup(!EditorTools.IsAssetInFilter(item, itemFilter) || IsItemSyncedFromOtherDB(item));
            if (template.treatItemsAsQuests)
            {
                var fieldWidth = (rect.width - ItemReorderableListTypeWidth) / 4;
                EditorGUI.LabelField(new Rect(rect.x, rect.y + 2, ItemReorderableListTypeWidth, EditorGUIUtility.singleLineHeight), item.IsItem ? "Item" : "Quest");
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(nameControl);
                itemName = EditorGUI.TextField(new Rect(rect.x + ItemReorderableListTypeWidth, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, item.Name);
                if (EditorGUI.EndChangeCheck()) item.Name = itemName;
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(descriptionControl);
                description = EditorGUI.TextField(new Rect(rect.x + ItemReorderableListTypeWidth + fieldWidth + 2, rect.y + 2, 3 * fieldWidth - 2, EditorGUIUtility.singleLineHeight), GUIContent.none, description);
                if (EditorGUI.EndChangeCheck()) item.Description = description;
            }
            else
            {
                var fieldWidth = rect.width / 4;
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(nameControl);
                itemName = EditorGUI.TextField(new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, item.Name);
                if (EditorGUI.EndChangeCheck()) item.Name = itemName;
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(descriptionControl);
                description = EditorGUI.TextField(new Rect(rect.x + fieldWidth + 2, rect.y, 3 * fieldWidth - 2, EditorGUIUtility.singleLineHeight), GUIContent.none, description);
                if (EditorGUI.EndChangeCheck()) item.Description = description;
            }
            EditorGUI.EndDisabledGroup();
            var focusedControl = GUI.GetNameOfFocusedControl();
            if (string.Equals(nameControl, focusedControl) || string.Equals(descriptionControl, focusedControl))
            {
                inspectorSelection = item;
            }
        }

        private void DrawItemListElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < database.items.Count)) return;
            var item = database.items[index];
            if (EditorTools.IsAssetInFilter(item, itemFilter))
            {
                ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, isActive, isFocused, true);
            }
            else
            {
                EditorGUI.DrawRect(rect, new Color(0.225f, 0.225f, 0.225f, 1));
            }
        }

        private void OnAddItemOrQuestDropdown(Rect buttonRect, ReorderableList list)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Quest"), false, OnAddNewQuest, null);
            menu.AddItem(new GUIContent("Item"), false, OnAddNewItem, null);
            menu.ShowAsContext();
        }

        private void OnItemListAdd(ReorderableList list)
        {
            AddNewItem();
        }

        private void OnItemListRemove(ReorderableList list)
        {
            if (!(0 <= list.index && list.index < database.items.Count)) return;
            var item = database.items[list.index];
            if (item == null) return;
            if (IsItemSyncedFromOtherDB(item)) return;
            var deletedLastOne = list.count == 1;
            if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", EditorTools.GetAssetName(item)), "Are you sure you want to delete this?", "Delete", "Cancel"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                if (deletedLastOne) inspectorSelection = null;
                else inspectorSelection = (list.index < list.count) ? database.items[list.index] : (list.count > 0) ? database.items[list.count - 1] : null;
                SetDatabaseDirty("Remove Item");
            }
        }

        private void OnItemListReorder(ReorderableList list)
        {
            SetDatabaseDirty("Reorder Items");
        }

        private void OnItemListSelect(ReorderableList list)
        {
            if (!(0 <= list.index && list.index < database.items.Count)) return;
            inspectorSelection = database.items[list.index];
            itemListSelectedIndex = list.index;
        }

        public void DrawSelectedItemSecondPart()
        {
            var item = inspectorSelection as Item;
            if (item == null) return;
            DrawFieldsFoldout<Item>(item, itemListSelectedIndex, itemFoldouts);
            DrawAssetSpecificPropertiesSecondPart(item, itemListSelectedIndex, itemFoldouts);
        }

        private void DrawItemMenu()
        {
            if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("New Item"), false, AddNewItem);
                if (template.treatItemsAsQuests)
                {
                    menu.AddItem(new GUIContent("New Quest"), false, AddNewQuest);
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("New Quest"));
                }
                menu.AddItem(new GUIContent("Use Quest System"), template.treatItemsAsQuests, ToggleUseQuestSystem);
                menu.AddItem(new GUIContent("Sort/By Name"), false, SortItemsByName);
                menu.AddItem(new GUIContent("Sort/By Group"), false, SortItemsByGroup);
                menu.AddItem(new GUIContent("Sort/By ID"), false, SortItemsByID);
                menu.AddItem(new GUIContent("Sync From DB"), database.syncInfo.syncItems, ToggleSyncItemsFromDB);
                menu.ShowAsContext();
            }
        }

        private void OnAddNewItem(object data)
        {
            AddNewItem();
        }

        private void OnAddNewQuest(object data)
        {
            AddNewQuest();
        }

        private void AddNewItem()
        {
            AddNewAssetFromTemplate<Item>(database.items, (template != null) ? template.itemFields : null, "Item");
            SetDatabaseDirty("Add New Item");
        }

        private void AddNewQuest()
        {
            AddNewAssetFromTemplate<Item>(database.items, (template != null) ? template.questFields : null, "Quest");
            BuildLanguageListFromItems();
            SetDatabaseDirty("Add New Quest");
        }

        private void SortItemsByName()
        {
            database.items.Sort((x, y) => x.Name.CompareTo(y.Name));
            SetDatabaseDirty("Sort by Name");
        }

        private void SortItemsByGroup()
        {
            database.items.Sort((x, y) => (x.Group == null) ? -1 : x.Group.CompareTo(y.Group));
            SetDatabaseDirty("Sort by Group");
        }

        private void SortItemsByID()
        {
            database.items.Sort((x, y) => x.id.CompareTo(y.id));
            SetDatabaseDirty("Sort by ID");
        }

        private void ToggleUseQuestSystem()
        {
            UpdateTreatItemsAsQuests(!template.treatItemsAsQuests);
            showStateFieldAsQuest = template.treatItemsAsQuests;
        }

        private void ToggleSyncItemsFromDB()
        {
            database.syncInfo.syncItems = !database.syncInfo.syncItems;
            SetDatabaseDirty("Toggle Sync Items");
        }

        private void DrawItemSyncDatabase()
        {
            EditorGUILayout.BeginHorizontal();
            DialogueDatabase newDatabase = EditorGUILayout.ObjectField(new GUIContent("Sync From", "Database to sync items/quests from."),
                                                                       database.syncInfo.syncItemsDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;
            if (newDatabase != database.syncInfo.syncItemsDatabase)
            {
                database.syncInfo.syncItemsDatabase = newDatabase;
                database.SyncItems();
                InitializeItemReorderableList();
                syncedItemIDs = null;
                SetDatabaseDirty("Change Sync Items Database");
            }
            if (GUILayout.Button(new GUIContent("Sync Now", "Syncs from the database."), EditorStyles.miniButton, GUILayout.Width(72)))
            {
                database.SyncItems();
                InitializeItemReorderableList();
                syncedItemIDs = null;
                SetDatabaseDirty("Manual Sync Items");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void RecordSyncedItemIDs()
        {
            syncedItemIDs = new HashSet<int>();
            if (database.syncInfo.syncItems && database.syncInfo.syncItemsDatabase != null)
            {
                database.syncInfo.syncItemsDatabase.items.ForEach(x => syncedItemIDs.Add(x.id));
            }
        }

        public bool IsItemSyncedFromOtherDB(Item item)
        {
            return item != null && syncedItemIDs != null && syncedItemIDs.Contains(item.id);
        }

        private void DrawItemPropertiesFirstPart(Item item)
        {
            if (!item.IsItem) DrawQuestProperties(item);
        }

        private void DrawQuestProperties(Item item)
        {
            if (item == null || item.fields == null) return;

            // Display Name:
            var displayNameField = Field.Lookup(item.fields, "Display Name");
            var hasDisplayNameField = (displayNameField != null);
            var useDisplayNameField = EditorGUILayout.Toggle(new GUIContent("Use Display Name", "Tick to use a Display Name in UIs that's different from the Name."), hasDisplayNameField);
            if (hasDisplayNameField && !useDisplayNameField)
            {
                item.fields.Remove(displayNameField);
                SetDatabaseDirty("Don't Use Display Name");
            }
            else if (useDisplayNameField)
            {
                EditTextField(item.fields, "Display Name", "The name to show in UIs.", false);
                DrawLocalizedVersions(item.fields, "Display Name {0}", false, FieldType.Text);
            }

            // Group:
            var groupField = Field.Lookup(item.fields, "Group");
            var hasGroupField = (groupField != null);
            var useGroupField = EditorGUILayout.Toggle(new GUIContent("Use Groups", "Tick to organize this quest under a quest group."), hasGroupField);
            if (hasGroupField && !useGroupField)
            {
                item.fields.Remove(groupField);
                SetDatabaseDirty("Don't Use Groups");
            }
            else if (useGroupField)
            {
                if (groupField == null)
                {
                    groupField = new Field("Group", string.Empty, FieldType.Text);
                    item.fields.Add(groupField);
                    SetDatabaseDirty("Create Group Field");
                }
                if (groupField.typeString == "CustomFieldType_Text")
                {
                    EditTextField(item.fields, "Group", "The group this quest belongs to.", false);
                    DrawLocalizedVersions(item.fields, "Group {0}", false, FieldType.Text);
                }
                else
                {
                    DrawField(new GUIContent("Group", "The group this quest belongs to."), groupField, false);
                }
            }

            // State:
            //EditorGUILayout.BeginHorizontal();
            Field stateField = Field.Lookup(item.fields, "State");
            if (stateField == null)
            {
                stateField = new Field("State", "unassigned", FieldType.Text);
                item.fields.Add(stateField);
                SetDatabaseDirty("Create State Field");
            }
            //EditorGUILayout.LabelField(new GUIContent("State", "The starting state of the quest."), GUILayout.Width(140));
            stateField.value = DrawQuestStateField(new GUIContent("State", "The starting state of the quest."), stateField.value);
            //EditorGUILayout.EndHorizontal();

            // Trackable:
            bool trackable = item.LookupBool("Trackable");
            bool newTrackable = EditorGUILayout.Toggle(new GUIContent("Trackable", "Tick to mark this quest trackable in a gameplay HUD."), trackable);
            if (newTrackable != trackable)
            {
                Field.SetValue(item.fields, "Trackable", newTrackable);
                SetDatabaseDirty("Create Trackable Field");
            }

            // Track on Start (only if Trackable):
            if (trackable)
            {
                bool track = item.LookupBool("Track");
                bool newTrack = EditorGUILayout.Toggle(new GUIContent("Track on Start", "Tick to show in HUD when the quest becomes active without the player having to toggle tracking on in the quest log window first."), track);
                if (newTrack != track)
                {
                    Field.SetValue(item.fields, "Track", newTrack);
                    SetDatabaseDirty("Create Track Field");
                }
            }

            // Abandonable:
            bool abandonable = item.LookupBool("Abandonable");
            bool newAbandonable = EditorGUILayout.Toggle(new GUIContent("Abandonable", "Tick to mark this quest abandonable in the quest window."), abandonable);
            if (newAbandonable != abandonable)
            {
                Field.SetValue(item.fields, "Abandonable", newAbandonable);
                SetDatabaseDirty("Create Abandonable Field");
            }

            // Has Entries:
            bool hasQuestEntries = item.FieldExists("Entry Count");
            bool newHasQuestEntries = EditorGUILayout.Toggle(new GUIContent("Has Entries (Subtasks)", "Tick to add quest entries to this quest."), hasQuestEntries);
            if (newHasQuestEntries != hasQuestEntries) ToggleHasQuestEntries(item, newHasQuestEntries);

            // Other main fields specified in template:
            DrawOtherQuestPrimaryFields(item);

            // Descriptions:
            EditTextField(item.fields, "Description", "The description when the quest is active.", true);
            DrawLocalizedVersions(item.fields, "Description {0}", false, FieldType.Text);
            EditTextField(item.fields, "Success Description", "The description when the quest has been completed successfully. If blank, the Description field is used.", true);
            DrawLocalizedVersions(item.fields, "Success Description {0}", false, FieldType.Text);
            EditTextField(item.fields, "Failure Description", "The description when the quest has failed. If blank, the Description field is used.", true);
            DrawLocalizedVersions(item.fields, "Failure Description {0}", false, FieldType.Text);

            // Entries:
            if (newHasQuestEntries) DrawQuestEntries(item);
        }

        private static List<string> questBuiltInFieldTitles = new List<string>(new string[] { "Display Name", "Group", "State", "Trackable", "Track", "Abandonable" });

        private void DrawOtherQuestPrimaryFields(Item item)
        {
            if (item == null || item.fields == null || template.questPrimaryFieldTitles == null) return;
            foreach (var field in item.fields)
            {
                var fieldTitle = field.title;
                if (string.IsNullOrEmpty(fieldTitle)) continue;
                if (!template.questPrimaryFieldTitles.Contains(field.title)) continue;
                if (questBuiltInFieldTitles.Contains(fieldTitle)) continue;
                if (fieldTitle.StartsWith("Description") || fieldTitle.StartsWith("Success Description") || fieldTitle.StartsWith("Failure Description")) continue;
                DrawMainSectionField(field);
            }
        }

        private void ToggleHasQuestEntries(Item item, bool hasEntries)
        {
            SetDatabaseDirty("Toggle Has Quest Entries");
            if (hasEntries)
            {
                if (!item.FieldExists("Entry Count")) Field.SetValue(item.fields, "Entry Count", (int)0);
            }
            else
            {
                int entryCount = Field.LookupInt(item.fields, "Entry Count");
                if (entryCount > 0)
                {
                    if (!EditorUtility.DisplayDialog("Delete all entries?", "You cannot undo this action.", "Delete", "Cancel"))
                    {
                        return;
                    }
                }
                item.fields.RemoveAll(field => field.title.StartsWith("Entry "));
            }
        }

        private void DrawQuestEntries(Item item)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Quest Entries", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Compact View", GUILayout.Width(90));
            showCompactQuestEntryList = EditorGUILayout.Toggle(GUIContent.none, showCompactQuestEntryList, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();

            int entryCount = Field.LookupInt(item.fields, "Entry Count");

            string[] entryTabs = null;
            if (showCompactQuestEntryList)
            {
                entryTabs = new string[entryCount];
                for (int i = 1; i <= entryCount; i++)
                {
                    entryTabs[i - 1] = "Entry " + i;
                }
                questEntrySelectedIdx = GUILayout.Toolbar(questEntrySelectedIdx, entryTabs);
            }

            EditorWindowTools.StartIndentedSection();
            //int entryCount = Field.LookupInt(item.fields, "Entry Count");
            int entryToDelete = -1;
            int entryToMoveUp = -1;
            int entryToMoveDown = -1;
            for (int i = 1; i <= entryCount; i++)
            {
                if (showCompactQuestEntryList && (i != questEntrySelectedIdx + 1)) continue;
                DrawQuestEntry(item, i, entryCount, ref entryToDelete, ref entryToMoveUp, ref entryToMoveDown);
            }
            if (entryToDelete != -1)
            {
                DeleteQuestEntry(item, entryToDelete, entryCount);
                SetDatabaseDirty("Delete Quest Entry");
            }
            if (entryToMoveUp != -1)
            {
                MoveQuestEntryUp(item, entryToMoveUp, entryCount);
            }
            if (entryToMoveDown != -1)
            {
                MoveQuestEntryDown(item, entryToMoveDown, entryCount);
            }
            if (GUILayout.Button(new GUIContent("Add New Quest Entry", "Adds a new quest entry to this quest.")))
            {
                entryCount++;
                questEntrySelectedIdx = entryCount - 1;
                Field.SetValue(item.fields, "Entry Count", entryCount);
                Field.SetValue(item.fields, string.Format("Entry {0} State", entryCount), "unassigned");
                Field.SetValue(item.fields, string.Format("Entry {0}", entryCount), string.Empty);
                List<string> questLanguages = new List<string>();
                item.fields.ForEach(field =>
                {
                    if (field.title.StartsWith("Description "))
                    {
                        string language = field.title.Substring("Description ".Length);
                        questLanguages.Add(language);
                        languages.Add(language);
                    }
                });
                questLanguages.ForEach(language => item.fields.Add(new Field(string.Format("Entry {0} {1}", entryCount, language), string.Empty, FieldType.Localization)));

                if (entryCount > 1)
                {
                    // Copy any custom "Entry 1 ..." fields to "Entry # ..." fields:
                    var fieldCount = item.fields.Count;
                    for (int i = 0; i < fieldCount; i++)
                    {
                        var field = item.fields[i];
                        if (field.title.StartsWith("Entry 1 "))
                        {
                            // Skip Entry 1, Entry 1 State, or Entry 1 Language:
                            if (string.Equals(field.title, "Entry 1") || string.Equals(field.title, "Entry 1 State")) continue;
                            var afterEntryNumber = field.title.Substring("Entry 1 ".Length);
                            if (questLanguages.Contains(afterEntryNumber)) continue;

                            // Otherwise add:
                            var newFieldTitle = "Entry " + entryCount + " " + afterEntryNumber;
                            if (item.FieldExists(newFieldTitle)) continue;
                            var copiedField = new Field(field);
                            copiedField.title = newFieldTitle;
                            if (copiedField.type == FieldType.Text) copiedField.value = string.Empty;
                            item.fields.Add(copiedField);
                        }
                    }
                }

                SetDatabaseDirty("Add New Quest Entry");
            }
            EditorWindowTools.EndIndentedSection();
        }

        private void DrawQuestEntry(Item item, int entryNumber, int entryCount, ref int entryToDelete, ref int entryToMoveUp, ref int entryToMoveDown)
        {
            EditorGUILayout.BeginVertical("button");

            // Keep track of which fields we've already drawn:
            List<Field> alreadyDrawn = new List<Field>();

            // Heading:
            EditorGUILayout.BeginHorizontal();
            string entryTitle = string.Format("Entry {0}", entryNumber);
            EditorGUILayout.LabelField(entryTitle);
            GUILayout.FlexibleSpace();

            //--- Framework for future move up/down buttons:
            EditorGUI.BeginDisabledGroup(entryNumber <= 1);
            if (GUILayout.Button(new GUIContent("↑", "Move up"), EditorStyles.miniButtonLeft, GUILayout.Width(22)))
            {
                entryToMoveUp = entryNumber;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(entryNumber >= entryCount);
            if (GUILayout.Button(new GUIContent("↓", "Move down"), EditorStyles.miniButtonMid, GUILayout.Width(22)))
            {
                entryToMoveDown = entryNumber;
            }
            EditorGUI.EndDisabledGroup();
            // Also change Delete button below to miniButtonRight

            if (GUILayout.Button(new GUIContent("-", "Delete"), EditorStyles.miniButtonRight, GUILayout.Width(22)))
            //if (GUILayout.Button(new GUIContent(" ", "Delete quest entry"), "OL Minus", GUILayout.Width(16)))
            {
                entryToDelete = entryNumber;
            }
            EditorGUILayout.EndHorizontal();

            // State:
            EditorGUILayout.BeginHorizontal();
            string stateTitle = entryTitle + " State";
            Field stateField = Field.Lookup(item.fields, stateTitle);
            if (stateField == null)
            {
                stateField = new Field(stateTitle, "unassigned", FieldType.Text);
                item.fields.Add(stateField);
                SetDatabaseDirty("Change Quest Entry State");
            }
            EditorGUILayout.LabelField(new GUIContent("State", "The starting state of this entry."), GUILayout.Width(140));
            stateField.value = DrawQuestStateField(stateField.value);
            EditorGUILayout.EndHorizontal();
            alreadyDrawn.Add(stateField);

            // Text:
            EditTextField(item.fields, entryTitle, "The text of this entry.", true, alreadyDrawn);
            DrawLocalizedVersions(item.fields, entryTitle + " {0}", false, FieldType.Text, alreadyDrawn);

            // Other "Entry # " fields:
            string entryTitleWithSpace = entryTitle + " ";
            string entryIDTitle = entryTitle + " ID";
            for (int i = 0; i < item.fields.Count; i++)
            {
                var field = item.fields[i];
                if (field.title == null) field.title = string.Empty;
                if (!alreadyDrawn.Contains(field) && field.title.StartsWith(entryTitleWithSpace) && !string.Equals(field.title, entryIDTitle))
                {
                    if (field.type == FieldType.Text && field.typeString == "CustomFieldType_Text")
                    {
                        EditTextField(item.fields, field.title, field.title, true, null);
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        DrawField(field);
                        EditorGUILayout.EndHorizontal();
                    }
                    alreadyDrawn.Add(field);
                }
            }

            // Add new entry field:
            if (isAddingNewFieldToEntryNumber == entryNumber)
            {
                EditorGUILayout.BeginHorizontal();
                if (newEntryField == null) newEntryField = new Field(string.Empty, string.Empty, FieldType.Text);
                newEntryField.title = EditorGUILayout.TextField(GUIContent.none, newEntryField.title);
                DrawFieldType(newEntryField);
                if (GUILayout.Button("Create", GUILayout.Width(80)))
                {
                    newEntryField.title = "Entry " + entryNumber + " " + newEntryField.title;
                    if (!item.FieldExists(newEntryField.title))
                    {
                        item.fields.Add(newEntryField);
                        isAddingNewFieldToEntryNumber = -1;
                    }
                }
                if (GUILayout.Button("Cancel", GUILayout.Width(80)))
                {
                    isAddingNewFieldToEntryNumber = -1;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("Add New Field To Entry"))
                {
                    isAddingNewFieldToEntryNumber = entryNumber;
                    newEntryField = null;
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DeleteQuestEntry(Item item, int entryNumber, int entryCount)
        {
            if (EditorUtility.DisplayDialog(string.Format("Delete entry {0}?", entryNumber), "You cannot undo this action.", "Delete", "Cancel"))
            {
                CutEntry(item, entryNumber, entryCount);
                SetDatabaseDirty("Delete Quest Entry");

                if (entryNumber == questEntrySelectedIdx + 1)
                {
                    questEntrySelectedIdx = Mathf.Max(questEntrySelectedIdx - 1, 0);
                }
            }
        }

        private void MoveQuestEntryUp(Item item, int entryNumber, int entryCount)
        {
            if (entryNumber <= 1) return;
            var clipboard = CutEntry(item, entryNumber, entryCount);
            entryCount--;
            PasteEntry(item, entryNumber - 1, entryCount, clipboard);
            questEntrySelectedIdx--;
        }

        private void MoveQuestEntryDown(Item item, int entryNumber, int entryCount)
        {
            if (entryNumber >= entryCount) return;
            var clipboard = CutEntry(item, entryNumber, entryCount);
            entryCount--;
            PasteEntry(item, entryNumber + 1, entryCount, clipboard);
            questEntrySelectedIdx++;
        }

        private List<Field> CutEntry(Item item, int entryNumber, int entryCount)
        {
            var clipboard = new List<Field>();

            // Remove the entry and put it on the clipboard:
            string entryFieldTitle = string.Format("Entry {0}", entryNumber);
            var extractedField = item.fields.Find(field => string.Equals(field.title, entryFieldTitle));
            clipboard.Add(extractedField);
            item.fields.Remove(extractedField);

            // Remove the other fields associated with the entry and put them on the clipboard:
            string entryPrefix = string.Format("Entry {0} ", entryNumber);
            var extractedFields = item.fields.FindAll(field => field.title.StartsWith(entryPrefix));
            clipboard.AddRange(extractedFields);
            item.fields.RemoveAll(field => field.title.StartsWith(entryPrefix));

            // Renumber any higher entries:
            for (int i = entryNumber + 1; i <= entryCount; i++)
            {
                Field entryField = Field.Lookup(item.fields, string.Format("Entry {0}", i));
                if (entryField != null) entryField.title = string.Format("Entry {0}", i - 1);
                string oldEntryPrefix = string.Format("Entry {0} ", i);
                string newEntryPrefix = string.Format("Entry {0} ", i - 1);
                for (int j = 0; j < item.fields.Count; j++)
                {
                    var field = item.fields[j];
                    if (field.title.StartsWith(oldEntryPrefix))
                    {
                        field.title = newEntryPrefix + field.title.Substring(oldEntryPrefix.Length);
                    }
                }
            }

            // Decrement the count:
            Field.SetValue(item.fields, "Entry Count", entryCount - 1);

            return clipboard;
        }

        private void PasteEntry(Item item, int entryNumber, int entryCount, List<Field> clipboard)
        {
            // Set the clipboard's new entry number:
            var stateField = clipboard.Find(field => field.title.EndsWith(" State"));
            if (stateField != null)
            {
                var oldEntryPrefix = stateField.title.Substring(0, stateField.title.Length - " State".Length);
                var newEntryPrefix = "Entry " + entryNumber;
                foreach (var field in clipboard)
                {
                    field.title = newEntryPrefix + field.title.Substring(oldEntryPrefix.Length);
                }
            }

            // Renumber any higher entries:
            for (int i = entryCount; i >= entryNumber; i--)
            {
                Field entryField = Field.Lookup(item.fields, string.Format("Entry {0}", i));
                if (entryField != null) entryField.title = string.Format("Entry {0}", i + 1);
                string oldEntryPrefix = string.Format("Entry {0} ", i);
                string newEntryPrefix = string.Format("Entry {0} ", i + 1);
                for (int j = 0; j < item.fields.Count; j++)
                {
                    var field = item.fields[j];
                    if (field.title.StartsWith(oldEntryPrefix))
                    {
                        field.title = newEntryPrefix + field.title.Substring(oldEntryPrefix.Length);
                    }
                }
            }

            // Add the clipboard entry:
            item.fields.AddRange(clipboard);

            // Increment the count:
            Field.SetValue(item.fields, "Entry Count", entryCount + 1);
        }

        private void DrawItemSpecificPropertiesSecondPart(Item item, int index, AssetFoldouts foldouts)
        {
            // Doesn't do anything right now.
        }

        private void SortItemFields(List<Field> fields)
        {
            List<Field> entryFields = fields.Where(field => field.title.StartsWith("Entry ")).OrderBy(field => field.title).ToList();
            fields.RemoveAll(field => entryFields.Contains(field));
            fields.AddRange(entryFields);
            SetDatabaseDirty("Sort Fields");
        }

    }

}