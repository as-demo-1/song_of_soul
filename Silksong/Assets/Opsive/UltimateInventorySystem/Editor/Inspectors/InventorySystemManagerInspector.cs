/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Editor.Managers;
    using Opsive.UltimateInventorySystem.Storage;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Displays the inspector for the Inventory System Manager.
    /// </summary>
    [CustomEditor(typeof(InventorySystemManager), true)]
    public class InventorySystemManagerInspector : InspectorBase
    {
        protected override List<string> ExcludedFields => new List<string>() { "m_Database" };
        protected ObjectField m_DatabaseField;
        protected InventorySystemManager m_InventorySystemManager;

        /// <summary>
        /// Replace all the references from the previous database to the new one.
        /// </summary>
        [MenuItem("Assets/Ultimate Inventory System/Replace Database Objects")]
        private static void ReplaceObjectsBySelectedDatabase()
        {
            var database = InventoryMainWindow.InventorySystemDatabase;

            if (database == null) {
                Debug.LogWarning("Error: The database is null in the Ultimate Inventory System Manager. The database cannot be switched.");
                return;
            }

            database.Initialize(false);

            if (Selection.activeObject is IDatabaseSwitcher component) {
                FixComponent(component, database);
                Debug.Log("The database was successfully switched.");
                return;
            }

            if (Selection.activeObject is GameObject prefab) {
                FixPrefab(prefab, database);
                Debug.Log("The database was successfully switched.");
                return;
            }

            if (Selection.activeObject is SceneAsset sceneAsset) {
                var scenePath = AssetDatabase.GetAssetPath(sceneAsset);

                bool sceneChanged = false;
                var previousScenePath = EditorSceneManager.GetActiveScene().path;
                if (previousScenePath != scenePath) {
                    EditorSceneManager.OpenScene(scenePath);
                    sceneChanged = true;
                }

                ReplaceDatabaseInSelectedScene(database, 0, 1);
                EditorUtility.ClearProgressBar();

                if (sceneChanged) {
                    EditorSceneManager.OpenScene(previousScenePath);
                }

                Debug.Log("Database switch completed successfully.");
                return;
            }

            var folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (EditorUtility.DisplayDialog("Update Database Objects",
                $"This process will use the database within the Ultimate Inventory System Main Manager Editor Window '{database.name}' and cannot be reversed. " +
                $"Are you sure you want to update the objects to use the selected database? ",
                "Yes",
                "No")) {

                var scriptableObjectGUIDs = (AssetDatabase.FindAssets("t:ScriptableObject", new[] { folderPath }));
                var prefabGUIDs = (AssetDatabase.FindAssets("t:Prefab", new[] { folderPath }));
                var sceneGUIDs = (AssetDatabase.FindAssets("t:Scene", new[] { folderPath }));

                float scriptableProgressMax = scriptableObjectGUIDs.Length;
                float sceneProgressMax = sceneGUIDs.Length * 100f;
                float prefabProgressMax = prefabGUIDs.Length * 10f;
                float totalProgressMax = scriptableProgressMax + prefabProgressMax + sceneProgressMax;

                float progress = 0.01f;

                ShowProgressBar(progress);

                //Fix Scriptable Objects.

                for (var i = 0; i < scriptableObjectGUIDs.Length; i++) {
                    var guid = scriptableObjectGUIDs[i];
                    string myObjectPath = AssetDatabase.GUIDToAssetPath(guid);
                    var myObj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(myObjectPath);

                    if (!(myObj is IDatabaseSwitcher componentWithDatabase)) { continue; }

                    FixComponent(componentWithDatabase, database);

                    ShowProgressBar(i / totalProgressMax);
                }

                progress = scriptableProgressMax / totalProgressMax;
                ShowProgressBar(progress);

                //Fix Scenes

                bool sceneChanged = false;
                var startScenePath = EditorSceneManager.GetActiveScene().path;
                for (var i = 0; i < sceneGUIDs.Length; i++) {
                    var guid = sceneGUIDs[i];
                    var scenePath = AssetDatabase.GUIDToAssetPath(guid);
                    var previousScenePath = EditorSceneManager.GetActiveScene().path;

                    if (previousScenePath != scenePath) {
                        EditorSceneManager.OpenScene(scenePath);
                        sceneChanged = true;
                    }

                    ReplaceDatabaseInSelectedScene(database,
                        progress + ((i) / (float)sceneGUIDs.Length) * (1 - progress - 0.3f),
                        progress + ((i + 1) / (float)sceneGUIDs.Length) * (1 - progress - 0.3f));
                }

                if (sceneChanged) {
                    EditorSceneManager.OpenScene(startScenePath);
                }

                progress += sceneProgressMax / totalProgressMax;
                ShowProgressBar(progress);

                //Fix Prefabs.

                for (var i = 0; i < prefabGUIDs.Length; i++) {
                    var guid = prefabGUIDs[i];
                    string myObjectPath = AssetDatabase.GUIDToAssetPath(guid);
                    var myObj = AssetDatabase.LoadAssetAtPath<GameObject>(myObjectPath);

                    FixPrefab(myObj, database);

                    if (i % 50 == 0) {
                        ShowProgressBar(progress + i / totalProgressMax);
                    }
                }

                AssetDatabase.SaveAssets();
                EditorSceneManager.SaveOpenScenes();
                Debug.Log("The database was successfully switched.");
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// Replace any object that is not in the database by an equivalent object in the specified database.
        /// </summary>
        /// <param name="object">The object to modify.</param>
        /// <param name="database">The database.</param>
        public static void ReplaceInventoryObjectsBySelectedDatabaseEquivalents(IDatabaseSwitcher @object,
            InventorySystemDatabase database, bool dirtyMainObject)
        {
            var result = @object.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(database);

            if (result != null && result.m_ObjectsToDirty != null) {
                foreach (var obj in result.m_ObjectsToDirty) {
                    if (obj == null) { continue; }

                    EditorUtility.SetDirty(obj);
                    Shared.Editor.Utility.EditorUtility.SetDirty(obj);
                }
            }

            if (dirtyMainObject == false) { return; }

            var mainObj = @object as Object;

            if (mainObj == null) { return; }

            EditorUtility.SetDirty(mainObj);
            Shared.Editor.Utility.EditorUtility.SetDirty(mainObj);

            var component = mainObj as Component;

            if (component == null) { return; }

            EditorUtility.SetDirty(component.gameObject);
            Shared.Editor.Utility.EditorUtility.SetDirty(component.gameObject);

            if (!PrefabUtility.IsPartOfAnyPrefab(component)) { return; }

            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        }

        /// <summary>
        /// Fix references to component.
        /// </summary>
        /// <param name="object">The component to fix.</param>
        /// <param name="database">The database.</param>
        private static void FixComponent(IDatabaseSwitcher @object, InventorySystemDatabase database)
        {
            database.Initialize(false);
            if (@object.IsComponentValidForDatabase(database) == false) {
                ReplaceInventoryObjectsBySelectedDatabaseEquivalents(@object, database, true);
            }
        }

        /// <summary>
        /// Fix prefab references to objects in database.
        /// </summary>
        /// <param name="prefab">The prefab to fix.</param>
        /// <param name="database">The database.</param>
        private static void FixPrefab(GameObject prefab, InventorySystemDatabase database)
        {
            database.Initialize(false);

            var objs = prefab.GetComponentsInChildren<IDatabaseSwitcher>(true);

            if (objs.Length == 0) { return; }

            //TODO clean this up once Unity figures out how to correctly save a prefab without crashing or returning errors.
            /*var prefabPath = AssetDatabase.GetAssetPath(prefab);
            var nestedRootPrefab = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));

            if (nestedRootPrefab == null) {
                Debug.LogError($"The prefab for '{prefab.name}' could not be found");
                return;
            }
            
            objs = nestedRootPrefab.GetComponentsInChildren<IDatabaseSwitcher>(true);*/

            for (int i = 0; i < objs.Length; i++) {
                if (objs[i].IsComponentValidForDatabase(database)) { continue; }

                ReplaceInventoryObjectsBySelectedDatabaseEquivalents(objs[i], database, true);
            }

            //The correct way to save would be to use SaveAsPrefabAsset. But Unity has a bug that prevents that. It is fixed in 2020.2
            EditorUtility.SetDirty(prefab);
            Shared.Editor.Utility.EditorUtility.SetDirty(prefab);
            EditorSceneManager.MarkSceneDirty(prefab.scene);

            /*Debug.Log(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(nestedRootPrefab, prefabPath,out var success);

            if (success == false) {
                Debug.LogWarning($"The prefab '{nestedRootPrefab.name}' was not save successfully");
            }*/
        }

        /// <summary>
        /// Fix the references to the database objects for the scene.
        /// </summary>
        /// <param name="database">THe database.</param>
        private static void ReplaceDatabaseInSelectedScene(InventorySystemDatabase database, float progressMin, float progressMax)
        {
            database.Initialize(false);
            var scene = EditorSceneManager.GetActiveScene();
            Debug.Log("Modifying scene: " + scene.name);

            var inventorySystemManager = FindObjectOfType<InventorySystemManager>();
            if (inventorySystemManager != null) { inventorySystemManager.Database = database; }

            GameObject[] rootGameObjects = scene.GetRootGameObjects();

            for (var i = 0; i < rootGameObjects.Length; i++) {
                var rootGameObject = rootGameObjects[i];
                var childrenInterfaces = rootGameObject.GetComponentsInChildren<IDatabaseSwitcher>(true);
                foreach (var childInterface in childrenInterfaces) {
                    if (childInterface.IsComponentValidForDatabase(database)) { continue; }

                    ReplaceInventoryObjectsBySelectedDatabaseEquivalents(childInterface, database, true);
                }

                var progress = progressMin + (i / (float)rootGameObjects.Length) * (progressMax - progressMin);
                ShowProgressBar(progress);
            }

            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.SaveOpenScenes();
        }

        /// <summary>
        /// Validate the menu item action.
        /// </summary>
        /// <returns>True if valid.</returns>
        [MenuItem("Assets/Ultimate Inventory System/Replace Database Objects", true)]
        private static bool NewMenuOptionValidation()
        {
            if (Selection.activeObject is IDatabaseSwitcher) { return true; }

            if (Selection.activeObject is GameObject) { return true; }

            if (Selection.activeObject is SceneAsset) { return true; }

            string filePath = AssetDatabase.GetAssetPath(Selection.activeObject);
            FileAttributes attr = File.GetAttributes(filePath);
            return ((attr & FileAttributes.Directory) == FileAttributes.Directory);
        }

        /// <summary>
        /// Show the progress bar.
        /// </summary>
        /// <param name="percentage">The percentage between 0 and 1.</param>
        private static void ShowProgressBar(float percentage)
        {
            EditorUtility.DisplayProgressBar("Replacing Database References", "This process may take a while.", percentage);
        }

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_InventorySystemManager = target as InventorySystemManager;

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the Inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            var horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");

            m_DatabaseField = new ObjectField("Database");
            m_DatabaseField.objectType = typeof(InventorySystemDatabase);
            m_DatabaseField.style.flexGrow = 1;
            m_DatabaseField.style.flexShrink = 1;

            if (m_DatabaseField.value == null) {
                var manager = FindObjectOfType<InventorySystemManager>();
                m_DatabaseField.value = manager?.Database ?? InventoryMainWindow.InventorySystemDatabase;
            }

            m_DatabaseField.RegisterValueChangedCallback(evt =>
            {
                if (m_InventorySystemManager.Database == evt.newValue) { return; }
                DatabaseChanged();
            });
            horizontalLayout.Add(m_DatabaseField);

            var button = new Button();
            button.text = "Update Scene";
            button.tooltip = "Updates the scene to use the specified database.";
            button.clicked += DatabaseChanged;
            horizontalLayout.Add(button);
            container.Add(horizontalLayout);
        }

        /// <summary>
        /// Update the scene when a new database is set.
        /// </summary>
        private void DatabaseChanged()
        {
            m_InventorySystemManager.Database = m_DatabaseField.value as InventorySystemDatabase;

            if (m_InventorySystemManager.Database == null) {
                Debug.LogWarning("The database cannot be set to null.");
                return;
            }

            if (EditorUtility.DisplayDialog("Update Scene Database",
                $"This process will use the database within the Inventory System Manager '{m_InventorySystemManager.Database.name}' " +
                "from the scene component and cannot be reversed. " +
                "Are you sure you want to update the objects to use the selected database? ",
                "Yes",
                "No")) {

                ReplaceDatabaseInSelectedScene(m_InventorySystemManager.Database, 0, 1);
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Scene Database Updated",
                    "The scene database was updated. Check the console for any errors.\n\n" +
                    "Ensure you also update all of the other scenes and prefabs.",
                    "Ok");
            }
        }
    }
}