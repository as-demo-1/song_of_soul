namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Storage;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// A field for the Inventory System Database within the Main Editor Manager.
    /// </summary>
    public class InventorySystemDatabaseMainField : VisualElement
    {
        private ObjectField m_DatabaseField;
        protected InventoryMainWindow m_InventoryMainWindow;
        
        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="inventoryMainWindow">The Inventory Main Window.</param>
        public InventorySystemDatabaseMainField(InventoryMainWindow inventoryMainWindow)
        {
            m_InventoryMainWindow = inventoryMainWindow;
            
            style.flexDirection = FlexDirection.Row;
            style.minHeight = 22;

            var moreOptions = new IconOptionButton(IconOption.Cog);
            m_DatabaseField = new ObjectField("Database");
            m_DatabaseField.tooltip =
                "The inventory database is used to store all the inventory objects (item category, definition, recipes, ect).";
            m_DatabaseField.style.flexGrow = 1;
            m_DatabaseField.objectType = typeof(InventorySystemDatabase);
            m_DatabaseField.RegisterValueChangedCallback(evt =>
            {
                m_InventoryMainWindow.Database = evt.newValue as InventorySystemDatabase;
                moreOptions.SetEnabled(m_InventoryMainWindow.Database != null);
            });
            m_DatabaseField.value = m_InventoryMainWindow.Database;
            Add(m_DatabaseField);

            moreOptions.clicked += () => { ShowDatabaseMoreOptionsMenu(); };
            moreOptions.style.marginTop = 2;
            moreOptions.SetEnabled(m_InventoryMainWindow.Database != null);
            Add(moreOptions);

            var createDatabaseButton = new Button();
            createDatabaseButton.tooltip = "Creates a new inventory database.";
            createDatabaseButton.text = "New";
            createDatabaseButton.clickable.clicked += CreateDatabase;
            Add(createDatabaseButton);
        }

        /// <summary>
        /// Refresh the database.
        /// </summary>
        public void Refresh()
        {
            m_DatabaseField.value = m_InventoryMainWindow.Database;
        }
        
        /// <summary>
        /// Creates a new database.
        /// </summary>
        private void CreateDatabase()
        {
            var path = EditorUtility.SaveFilePanel("Create Database", "Assets", "InventoryDatabase", "asset");
            if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                var database = ScriptableObject.CreateInstance<InventorySystemDatabase>();

                // Save the database.
                path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                path = AssetDatabaseUtility.GetPathForNewDatabase(path, out var newFolderPath);

                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(database, path);
                AssetDatabase.ImportAsset(path);

                m_DatabaseField.value = database;

                m_InventoryMainWindow.Database = database;

                //Add an "All" ItemCategory
                var allCategory = ItemCategoryEditorUtility.AddItemCategory("All", m_InventoryMainWindow.Database, m_InventoryMainWindow.GetDatabaseDirectory());
                var descriptionAttribute = ItemCategoryEditorUtility.AddAttribute(allCategory, "Description", 1);
                var newDescriptionAttribute = descriptionAttribute.ChangeType(typeof(string));
                (newDescriptionAttribute as Attribute<string>).SetOverrideValue("This is the item description. Set the 'Description' Attribute Override value to change it.");

                var iconAttribute = ItemCategoryEditorUtility.AddAttribute(allCategory, "Icon", 1);
                iconAttribute.ChangeType(typeof(Sprite));

                ItemCategoryEditorUtility.SetItemCategoryDirty(allCategory, true);
                Shared.Editor.Utility.EditorUtility.SetDirty(database);

                var categoryManager = m_InventoryMainWindow.GetManager<ItemCategoryManager>();

                categoryManager.Refresh();
                categoryManager.Select(allCategory);
            }
        }

        /// <summary>
        /// Duplicates the selected database.
        /// </summary>
        private void DuplicateDatabase()
        {
            if (m_DatabaseField.value == null) {
                Debug.LogWarning("You must select a database to duplicate in the object field.");
                return;
            }

            var path = EditorUtility.SaveFilePanel("Duplicate Database", "Assets", "InventoryDatabase", "asset");
            if (path.Length != 0 && Application.dataPath.Length < path.Length) {

                path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));

                var database = DatabaseDuplicator.DuplicateDatabase(m_DatabaseField.value as InventorySystemDatabase, path);

                m_DatabaseField.value = database;
            }
        }
        
        /// <summary>
        /// Populates and shows the More Options Generic Menu.
        /// </summary>
        private void ShowDatabaseMoreOptionsMenu()
        {
            var moreOptions = new GenericMenu();
            moreOptions.AddItem(new GUIContent("Duplicate"), false, () => { DuplicateDatabase(); });
            moreOptions.AddItem(new GUIContent("Generate C# script"), false, () =>
            {
                var databasePath = m_InventoryMainWindow.GetDatabaseDirectory();
                var path = EditorUtility.SaveFolderPanel("Generate Database strings", databasePath, "");
                if (path.Length != 0 && Application.dataPath.Length < path.Length) {

                    path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                    DatabaseNamesScriptGenerator.GenerateDatabaseNamesScript(path, m_InventoryMainWindow.Database);
                }
            });

            moreOptions.ShowAsContext();
        }
    }
}