/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The MainManagerWindow is an editor window which contains all of the sub managers. This window draws the high level menu options and draws
    /// the selected sub manager.
    /// </summary>
    [InitializeOnLoad]
    public class InventoryMainWindow : MainManagerWindow
    {

        private static InventoryMainWindow s_Instance;
        public static InventoryMainWindow Instance => s_Instance;

        protected override string AssetName => AssetInfo.Name;
        protected override string AssetVersion => AssetInfo.Version;
        protected override string UpdateCheckURL => string.Format("https://opsive.com/asset/UpdateCheck.php?asset=UltimateInventorySystem&version={0}&unityversion={1}&devplatform={2}&targetplatform={3}",
                                            AssetInfo.Version, Application.unityVersion, Application.platform, EditorUserBuildSettings.activeBuildTarget);
        protected override string LatestVersionKey => "Opsive.UltimateInventorySystem.Editor.LatestVersion";
        protected override string LastUpdateCheckKey => "Opsive.UltimateInventorySystem.Editor.LastUpdateCheck";
        protected override string ManagerNamespace => "Opsive.UltimateInventorySystem.Editor";

        public static InventorySystemDatabase InventorySystemDatabase {
            get {
                if (s_Instance != null) { return s_Instance.Database; }

                if (!string.IsNullOrEmpty(DatabaseGUID)) {
                    return Shared.Editor.Utility.EditorUtility.LoadAsset<InventorySystemDatabase>(DatabaseGUID);
                }

                return null;
            }
        }

        private static string DatabaseGUIDKey => "Opsive.UltimateInventorySystem.DatabaseGUID." + Application.productName;

        // The database path is based on the project.
        private static string DatabaseGUID { get => EditorPrefs.GetString(DatabaseGUIDKey, string.Empty); }

        private List<int> m_RequiredDatabaseManagers = new List<int>();

        private InventorySystemDatabase m_Database;
        public InventorySystemDatabase Database {
            get => m_Database;
            set { UpdateDatabase(value); }
        }

        public event Action OnFocusEvent;
        public static event Action OnLostFocusEvent;

        /// <summary>
        /// Perform editor checks as soon as the scripts are done compiling.
        /// </summary>
        static InventoryMainWindow()
        {
            EditorApplication.update += EditorStartup;
        }
        
        /// <summary>
        /// Initializes the Main Manager.
        /// </summary> 
        [MenuItem("Tools/Opsive/Ultimate Inventory System/Main Manager", false, 0)]
        public static InventoryMainWindow ShowWindow()
        {
            var window = EditorWindow.GetWindow<InventoryMainWindow>(false, "Inventory Manager");
            window.minSize = new Vector2(800, 550);
            return window;
        }

        /// <summary>
        /// Initializes the Main Manager and shows the Item Categories Manager.
        /// </summary> 
        [MenuItem("Tools/Opsive/Ultimate Inventory System/Item Categories Manager", false, 11)]
        public static void ShowItemCategoriesManagerWindow()
        {
            var window = ShowWindow();
            window.Open(typeof(ItemCategoryManager));
        }

        /// <summary>
        /// Initializes the Main Manager and shows the Item Definitions Manager.
        /// </summary> 
        [MenuItem("Tools/Opsive/Ultimate Inventory System/Item Definitions Manager", false, 12)]
        public static void ShowItemDefinitionsManagerWindow()
        {
            var window = ShowWindow();
            window.Open(typeof(ItemDefinitionManager));
        }

        /// <summary>
        /// Initializes the Main Manager and shows the Crafting Categories Manager.
        /// </summary> 
        [MenuItem("Tools/Opsive/Ultimate Inventory System/Crafting Categories Manager", false, 13)]
        public static void ShowCraftingCategoriesManagerWindow()
        {
            var window = ShowWindow();
            window.Open(typeof(CraftingCategoryManager));
        }

        /// <summary>
        /// Initializes the Main Manager and shows the Crafting Recipes Manager.
        /// </summary> 
        [MenuItem("Tools/Opsive/Ultimate Inventory System/Crafting Recipes Manager", false, 14)]
        public static void ShowCraftingRecipliesManagerWindow()
        {
            var window = ShowWindow();
            window.Open(typeof(CraftingRecipeManager));
        }

        /// <summary>
        /// Initializes the Main Manager and shows the Currencies Manager.
        /// </summary> 
        [MenuItem("Tools/Opsive/Ultimate Inventory System/Currencies Manager", false, 15)]
        public static void ShowCurrenciesManagerWindow()
        {
            var window = ShowWindow();
            window.Open(typeof(CurrencyManager));
        }

        /// <summary>
        /// Initializes the Main Manager and shows the Currencies Manager.
        /// </summary> 
        [MenuItem("Tools/Opsive/Ultimate Inventory System/UI Designer", false, 16)]
        public static void ShowUIDesignerWindow()
        {
            var window = ShowWindow();
            window.Open(typeof(UIDesignerManager));
        }

        /// <summary>
        /// Initializes the Main Manager and shows the Integrations Manager.
        /// </summary> 
        [MenuItem("Tools/Opsive/Ultimate Inventory System/Integrations Manager", false, 26)]
        public static void ShowIntegrationsManagerWindow()
        {
            var window = ShowWindow();
            window.Open(typeof(IntegrationsManager));
        }
        
        /// <summary>
        /// Show the editor window if it hasn't been shown before and also setup.
        /// </summary>
        private static void EditorStartup()
        {
            if (EditorApplication.isCompiling) {
                return;
            }
            
            var dataPathSplit = Application.dataPath.Split('/');
            var projectName = dataPathSplit[dataPathSplit.Length - 2];

            if (!EditorPrefs.GetBool(projectName+"Opsive.UltimateInventorySystem.Editor.MainManagerShown", false)) {
                EditorPrefs.SetBool(projectName+"Opsive.UltimateInventorySystem.Editor.MainManagerShown", true);
                ShowWindow();
            }

            if (!EditorPrefs.HasKey(projectName+"Opsive.UltimateInventorySystem.Editor.UpdateProject") 
                || EditorPrefs.GetBool(projectName+"Opsive.UltimateInventorySystem.Editor.UpdateProject", true)) {
                EditorUtility.DisplayDialog("Project Settings Setup", "Thank you for purchasing the " + AssetInfo.Name +".\n\n" +
                                                                      "This wizard will ask questions related to updating your project.", "OK");
                UpdateProjectSettings();
            }
            EditorPrefs.SetBool(projectName+"Opsive.UltimateInventorySystem.Editor.UpdateProject", false);

            EditorApplication.update -= EditorStartup;
        }
        
        /// <summary>
        /// Show the project settings dialogues.
        /// </summary>
        private static void UpdateProjectSettings()
        {
            if (EditorUtility.DisplayDialog("Update Input Manager?", "Do you want to update the Input Manager?\n\n" +
                                                                     "If you have already updated the Input Manager or are using custom inputs you can select No.", "Yes", "No")) {
                InventoryInputBuilder.UpdateInputManager();
            }
        }

        /// <summary>
        /// The window has been enabled.
        /// </summary>
        protected override void OnEnable()
        {
            s_Instance = this;
            rootVisualElement.styleSheets.Add(CommonStyles.StyleSheet);
            rootVisualElement.styleSheets.Add(ControlTypeStyles.StyleSheet);

            // Initialize the database.
            if (Application.isPlaying) {
                m_Database = FindObjectOfType<InventorySystemManager>().Database;
            } else  if (!string.IsNullOrEmpty(DatabaseGUID)) {
                m_Database = Shared.Editor.Utility.EditorUtility.LoadAsset<InventorySystemDatabase>(DatabaseGUID);
            }

            // Ensure the database is valid.
            ValidateDatabase();
            
            //Build the Managers after validating the database.
            base.OnEnable();

            for (int i = 0; i < m_Managers.Length; ++i) {
                // If there isn't a database then the button should start disabled.
                if (m_Database == null && m_RequiredDatabaseManagers.Contains(i)) {
                    m_MenuButtons[i].SetEnabled(false);
                }
            }

            if (m_MenuSelection < 0 || m_MenuSelection >= m_Managers.Length) {
                return;
            }
            m_Managers[m_MenuSelection].Refresh();
        }

        /// <summary>
        /// Builds the array which contains all of the IManager objects.
        /// </summary>
        /// <returns>True if new managers were added.</returns>
        protected override bool BuildManagers()
        {
            if (!base.BuildManagers()) {
                return false;
            }

            for (int i = 0; i < m_Managers.Length; ++i) {
                if (m_Managers[i].GetType().GetCustomAttributes(typeof(RequireDatabase), true).Length > 0) {
                    m_RequiredDatabaseManagers.Add(i);
                }
            }
            
            return true;
        }

        /// <summary>
        /// Updates the data base to the specified value.
        /// </summary>
        /// <param name="database">The new database value.</param>
        private void UpdateDatabase(InventorySystemDatabase database)
        {
            if (Application.isPlaying) {
                var loadedDatabase = FindObjectOfType<InventorySystemManager>().Database;

                if (loadedDatabase != database) {
                    Debug.LogWarning("The database in the main manager must matched the database loaded in the scene.");
                    return;
                }
            }

            if (m_Database == database) { return; }

            m_Database = database;
            if (m_Database != null) {
                EditorPrefs.SetString(DatabaseGUIDKey, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_Database)));
            }

            ValidateDatabase();

            // The menus should reflect the database change.
            for (int i = 0; i < m_RequiredDatabaseManagers.Count; ++i) {
                m_MenuButtons[m_RequiredDatabaseManagers[i]].SetEnabled(m_Database != null);
                m_Managers[m_RequiredDatabaseManagers[i]].BuildVisualElements();
            }

            // The selected manager should update to reflect the change.
            m_Managers[m_MenuSelection].Refresh();
        }

        /// <summary>
        /// Returns the location of the database directory.
        /// </summary>
        /// <returns>The location of the database directory.</returns>
        public string GetDatabaseDirectory()
        {
            return DatabaseValidator.GetDatabaseDirectory(m_Database);
        }

        /// <summary>
        /// Select the menu at the index provided.
        /// </summary>
        /// <param name="index">The menu index.</param>
        public override void SelectMenu(int index)
        {
            // The menu can't be selected if it's not enabled.
            if (index >=0 && index < m_MenuButtons.Length && !m_MenuButtons[index].enabledSelf) {
                index = 0;
            }

            base.SelectMenu(index);
        }

        /// <summary>
        /// Validate the database.
        /// </summary>
        private void ValidateDatabase()
        {
            if (m_Database == null) { return; }

            var valid = DatabaseValidator.CheckIfValid(m_Database, false);
            if (valid == false) {
                Debug.LogWarning("The database is not valid. An autofix process will now run.");
                valid = DatabaseValidator.CheckIfValid(m_Database, true);
                if (valid == false) {
                    Debug.LogError("The database has errors and some could not be automatically fixed.");
                } else {
                    Debug.Log("The database errors have been fixed automatically.");
                }
            }
        }

        /// <summary>
        /// Navigates to the specified object.
        /// </summary>
        /// <param name="obj">The object that should be selected.</param>
        /// <param name="showWindow">Should the window be shown if it is not already?</param>
        public static void NavigateTo(object obj, bool showWindow = false)
        {
            if (showWindow) {
                ShowWindow();
            }

            if (s_Instance == null) {
                return;
            }
            s_Instance.NavigateToInternal(obj);
        }

        /// <summary>
        /// Navigates to the specified object.
        /// </summary>
        /// <param name="obj">The object that should be selected.</param>
        private void NavigateToInternal(object obj)
        {
            if (obj is Core.ItemCategory itemCategory) {
                var index = Open(typeof(ItemCategoryManager));
                (m_Managers[index] as ItemCategoryManager).Select(itemCategory);
            }
            if (obj is Core.ItemDefinition itemDefinition) {
                var index = Open(typeof(ItemDefinitionManager));
                (m_Managers[index] as ItemDefinitionManager).Select(itemDefinition);
            }
            if (obj is Crafting.CraftingCategory craftingCategory) {
                var index = Open(typeof(CraftingCategoryManager));
                (m_Managers[index] as CraftingCategoryManager).Select(craftingCategory);
            }
            if (obj is Crafting.CraftingRecipe craftingRecipe) {
                var index = Open(typeof(CraftingRecipeManager));
                (m_Managers[index] as CraftingRecipeManager).Select(craftingRecipe);
            }
            if (obj is Exchange.Currency currency) {
                var index = Open(typeof(CurrencyManager));
                (m_Managers[index] as CurrencyManager).Select(currency);
            }
        }

        /// <summary>
        /// An undo or redo has been performed. Refresh the display.
        /// </summary>
        protected override void OnUndoRedo()
        {
            if (Database == null) { return; }

            // After an undo objects need to be forced to deserialize.
            Database.Initialize(true);

            base.OnUndoRedo();
        }

        /// <summary>
        /// Reload database on focus.
        /// </summary>
        protected override void OnFocus()
        {
            if (m_Database == null) { return; }

            OnFocusEvent?.Invoke();

            if (m_Database == Database) { return; }

            UpdateDatabase(m_Database);

            base.OnFocus();
        }

        /// <summary>
        /// Send event when lost focus.
        /// </summary>
        void OnLostFocus()
        {
            OnLostFocusEvent?.Invoke();
        }

        /// <summary>
        /// MainManager destructor.
        /// </summary>
        ~InventoryMainWindow()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.ClearAll();
        }
    }

    /// <summary>
    /// Attribute which specifies that the manager requires a database in order to activate.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RequireDatabase : Attribute
    {
        public RequireDatabase() { }
    }
}