namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using System;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.Shared.Editor.Utility;
    using Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.Utility.InventoryDatabaseImportExport;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Storage;
    using Opsive.UltimateInventorySystem.Utility;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    using EditorUtility = UnityEditor.EditorUtility;

    /// <summary>
    /// The Import & Export menu is to import and export Inventory database data.
    /// </summary>
    [OrderedEditorItem("Import/Export", 70)]
    [RequireDatabase]
    public class ImportExportManager : InventoryManager
    {
        protected VisualElement m_Container;

        private InventorySystemDatabaseMainField m_InventorySystemDatabaseMainField;

        protected VisualElement m_ImportExportContainer;
        protected ObjectFieldWithNestedInspector<ImportExportModule,SimpleObjectInspector> m_ImportExportModuleField;
        protected Button m_ImportButton;
        protected Button m_ExportButton;

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            // Database Creation.
            m_InventorySystemDatabaseMainField = new InventorySystemDatabaseMainField(m_InventoryMainWindow);

            m_ManagerContentContainer.Add(m_InventorySystemDatabaseMainField);

            m_Container = new ScrollView(ScrollViewMode.Vertical);

            m_ManagerContentContainer.Add(m_Container);
            
            var descriptionBox = CreateDescriptionBox(
                "The Import/Export system allows you to import and export parts of your inventory system database.\n" +
                "This process does not create databases, importing modifies an existing database.\n" +
                "\n" +
                "This feature allows you to quickly add groups of items or keep a readable data table of your items for use outside of Unity.\n" +
                "Note that the capabilities of the importer and exporter are limited. Not all data can be exported in a readable format.\n" +
                "Unity Objects, Custom Recipes, and most Attributes are not supported for import and export" +
                "\n" +
                "This feature should NOT be used to make backups of your Inventory database, for that use version control and/or use the unity package exporter to export your Database Folder.\n" +
                "\n" +
                "This Manager allows you to create your own Importer/Exporters by inheriting the 'Import Export Module' class. Learn more about it in the documentation.");
            m_Container.Add(descriptionBox);

            m_ImportExportContainer = new VisualElement();
            m_ImportExportContainer.AddToClassList(InventoryManagerStyles.SubMenu);
            m_Container.Add(m_ImportExportContainer);

            m_ImportExportModuleField = new ObjectFieldWithNestedInspector<ImportExportModule, SimpleObjectInspector>(
                "Import Export Module",
                Shared.Editor.Utility.EditorUtility.LoadAsset<ImportExportModule>("6e299b23779b4a44db7e114267053e20"),
                "The Import Export Module is a scriptable object which allows you to choose how to export your database",
                null, false);
            
            m_ImportExportContainer.Add(m_ImportExportModuleField);

            var buttons = new VisualElement();
            buttons.AddToClassList(CommonStyles.s_HorizontalAlignCenter);
            
            m_ImportButton = new Button();
            m_ImportButton.AddToClassList(InventoryManagerStyles.SubMenuButton);
            m_ImportButton.text = "Import";
            m_ImportButton.clicked += HandleImportClick;
            buttons.Add(m_ImportButton);
            
            m_ExportButton = new Button();
            m_ExportButton.AddToClassList(InventoryManagerStyles.SubMenuButton);
            m_ExportButton.text = "Export";
            m_ExportButton.clicked += HandleExportClick;
            buttons.Add(m_ExportButton);
            
            m_ImportExportContainer.Add(buttons);
        }

        private void HandleExportClick()
        {
            if (m_InventoryMainWindow.Database == null) {
                EditorUtility.DisplayDialog("Database is null",
                    $"You are trying to export your database. But your database is null.\n" +
                    $"Please create a new database before importing data.",
                    "OK");
                
                return;
            }
            
            if (m_ImportExportModuleField.value == null) {
                EditorUtility.DisplayDialog("Import Export Module is null",
                    $"You are trying to export your database. But your import export module is null.\n" +
                    $"Please assign an import export module.",
                    "OK");
                
                return;
            }
            
            m_ImportExportModuleField.value.Export(m_InventoryMainWindow.Database);
        }

        private void HandleImportClick()
        {
            if (m_InventoryMainWindow.Database == null) {
                EditorUtility.DisplayDialog("Database is null",
                    $"You are trying to import data into your database. But your database is null.\n" +
                    $"Please create a new database before importing data.",
                    "OK");
                
                return;
            }
            
            if (m_ImportExportModuleField.value == null) {
                EditorUtility.DisplayDialog("Import Export Module is null",
                    $"You are trying to import your database. But your import export module is null.\n" +
                    $"Please assign an import export module.",
                    "OK");
                
                return;
            }
            
            var doImport = EditorUtility.DisplayDialog("Importing Database info?",
                $"You are trying to import data into your database. This action will modify the currently selected database.\n" +
                $"Changes made during the import process cannot be undone and some values may be lost. Always make a backup, using version control, before importing data to your database\n" +
                $"Are you sure you would like to import data anyways?",
                "Yes",
                "No");

            if (!doImport) { return; }
            
            m_ImportExportModuleField.value.Import(m_InventoryMainWindow.Database);
        }

        private VisualElement CreateDescriptionBox(string description)
        {
            var descriptionBox = new VisualElement();
            descriptionBox.AddToClassList(InventoryManagerStyles.SubMenu);
            descriptionBox.style.marginTop = 20f;
            var descriptionLabel = new Label();
            descriptionLabel.AddToClassList(InventoryManagerStyles.SubMenuIconDescription);
            descriptionLabel.text = description;
            descriptionBox.Add(descriptionLabel);

            return descriptionBox;
        }
    }
}