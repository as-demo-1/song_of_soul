// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the Locations tab. Locations are
    /// just treated as basic assets, so it uses the generic asset methods.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        [SerializeField]
        private AssetFoldouts locationFoldouts = new AssetFoldouts();

        [SerializeField]
        private string locationFilter = string.Empty;

        [SerializeField]
        private bool hideFilteredOutLocations = false;

        private ReorderableList locationReorderableList = null;

        [SerializeField]
        private int locationListSelectedIndex = -1;

        private HashSet<int> syncedLocationIDs = null;

        private void ResetLocationSection()
        {
            locationFoldouts = new AssetFoldouts();
            locationAssetList = null;
            locationReorderableList = null;
            locationListSelectedIndex = -1;
            syncedLocationIDs = null;
        }

        private void DrawLocationSection()
        {
            if (locationReorderableList == null) InitializeLocationReorderableList();
            DrawFilterMenuBar("Location", DrawLocationMenu, ref locationFilter, ref hideFilteredOutLocations);
            if (database.syncInfo.syncLocations)
            {
                DrawLocationSyncDatabase();
                if (syncedLocationIDs == null) RecordSyncedLocationIDs();
            }
            locationReorderableList.DoLayoutList();
        }

        private void InitializeLocationReorderableList()
        {
            locationReorderableList = new ReorderableList(database.locations, typeof(Location), true, true, true, true);
            locationReorderableList.drawHeaderCallback = DrawLocationListHeader;
            locationReorderableList.drawElementCallback = DrawLocationListElement;
            locationReorderableList.drawElementBackgroundCallback = DrawLocationListElementBackground;
            locationReorderableList.onAddCallback = OnLocationListAdd;
            locationReorderableList.onRemoveCallback = OnLocationListRemove;
            locationReorderableList.onSelectCallback = OnLocationListSelect;
            locationReorderableList.onReorderCallback = OnLocationListReorder;
        }

        private void DrawLocationListHeader(Rect rect)
        {
            var fieldWidth = (rect.width - 14) / 4;
            EditorGUI.LabelField(new Rect(rect.x + 14, rect.y, fieldWidth, rect.height), "Name");
            EditorGUI.LabelField(new Rect(rect.x + 14 + fieldWidth + 2, rect.y, 3 * fieldWidth - 2, rect.height), "Description");
        }

        private void DrawLocationListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < database.locations.Count)) return;
            var nameControl = "LocationName" + index;
            var descriptionControl = "LocationDescription" + index;
            var location = database.locations[index];
            EditorGUI.BeginDisabledGroup(!EditorTools.IsAssetInFilter(location, locationFilter) || IsLocationSyncedFromOtherDB(location));
            var fieldWidth = rect.width / 4;
            var locationName = location.Name;
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName(nameControl);
            locationName = EditorGUI.TextField(new Rect(rect.x, rect.y + 2, fieldWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, locationName);
            if (EditorGUI.EndChangeCheck()) location.Name = locationName;
            var description = location.Description;
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName(descriptionControl);
            description = EditorGUI.TextField(new Rect(rect.x + fieldWidth + 2, rect.y + 2, 3 * fieldWidth - 2, EditorGUIUtility.singleLineHeight), GUIContent.none, description);
            if (EditorGUI.EndChangeCheck()) location.Description = description;
            EditorGUI.EndDisabledGroup();
            var focusedControl = GUI.GetNameOfFocusedControl();
            if (string.Equals(nameControl, focusedControl) || string.Equals(descriptionControl, focusedControl))
            {
                inspectorSelection = location;
            }
        }

        private void DrawLocationListElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < database.locations.Count)) return;
            var location = database.locations[index];
            if (EditorTools.IsAssetInFilter(location, locationFilter))
            {
                ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, isActive, isFocused, true);
            }
            else
            {
                EditorGUI.DrawRect(rect, new Color(0.225f, 0.225f, 0.225f, 1));
            }
        }

        private void OnLocationListAdd(ReorderableList list)
        {
            AddNewLocation();
        }

        private void OnLocationListRemove(ReorderableList list)
        {
            if (!(0 <= list.index && list.index < database.locations.Count)) return;
            var location = database.locations[list.index];
            if (location == null) return;
            if (IsLocationSyncedFromOtherDB(location)) return;
            var deletedLastOne = list.count == 1;
            if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", EditorTools.GetAssetName(location)), "Are you sure you want to delete this location?", "Delete", "Cancel"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                if (deletedLastOne) inspectorSelection = null;
                else inspectorSelection = (list.index < list.count) ? database.locations[list.index] : (list.count > 0) ? database.locations[list.count - 1] : null;
                SetDatabaseDirty("Remove Location");
            }
        }

        private void OnLocationListReorder(ReorderableList list)
        {
            SetDatabaseDirty("Reorder Locations");
        }

        private void OnLocationListSelect(ReorderableList list)
        {
            if (!(0 <= list.index && list.index < database.locations.Count)) return;
            inspectorSelection = database.locations[list.index];
            locationListSelectedIndex = list.index;
        }

        public void DrawSelectedLocationSecondPart()
        {
            var location = inspectorSelection as Location;
            if (location == null) return;
            DrawFieldsFoldout<Location>(location, locationListSelectedIndex, locationFoldouts);
            DrawAssetSpecificPropertiesSecondPart(location, locationListSelectedIndex, locationFoldouts);
        }

        private void DrawLocationMenu()
        {
            if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("New Location"), false, AddNewLocation);
                menu.AddItem(new GUIContent("Sort/By Name"), false, SortLocationsByName);
                menu.AddItem(new GUIContent("Sort/By ID"), false, SortLocationsByID);
                menu.AddItem(new GUIContent("Sync From DB"), database.syncInfo.syncLocations, ToggleSyncLocationsFromDB);
                menu.ShowAsContext();
            }
        }

        private void AddNewLocation()
        {
            AddNewAsset<Location>(database.locations);
            SetDatabaseDirty("Add New Location");
        }

        private void SortLocationsByName()
        {
            database.locations.Sort((x, y) => x.Name.CompareTo(y.Name));
            SetDatabaseDirty("Sort Locations by Name");
        }

        private void SortLocationsByID()
        {
            database.locations.Sort((x, y) => x.id.CompareTo(y.id));
            SetDatabaseDirty("Sort Locations by ID");
        }

        private void ToggleSyncLocationsFromDB()
        {
            database.syncInfo.syncLocations = !database.syncInfo.syncLocations;
            SetDatabaseDirty("Toggle Sync Locations");
        }

        private void DrawLocationSyncDatabase()
        {
            EditorGUILayout.BeginHorizontal();
            DialogueDatabase newDatabase = EditorGUILayout.ObjectField(new GUIContent("Sync From", "Database to sync locations from."),
                                                                       database.syncInfo.syncLocationsDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;
            if (newDatabase != database.syncInfo.syncLocationsDatabase)
            {
                database.syncInfo.syncLocationsDatabase = newDatabase;
                database.SyncLocations();
                InitializeLocationReorderableList();
                syncedLocationIDs = null;
                SetDatabaseDirty("Change Location Sync Database");
            }
            if (GUILayout.Button(new GUIContent("Sync Now", "Syncs from the database."), EditorStyles.miniButton, GUILayout.Width(72)))
            {
                InitializeLocationReorderableList();
                syncedLocationIDs = null;
                database.SyncLocations();
                SetDatabaseDirty("Manual Sync Locations");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void RecordSyncedLocationIDs()
        {
            syncedLocationIDs = new HashSet<int>();
            if (database.syncInfo.syncLocations && database.syncInfo.syncLocationsDatabase != null)
            {
                database.syncInfo.syncLocationsDatabase.locations.ForEach(x => syncedLocationIDs.Add(x.id));
            }
        }

        public bool IsLocationSyncedFromOtherDB(Location location)
        {
            return location != null && syncedLocationIDs != null && syncedLocationIDs.Contains(location.id);
        }

    }

}