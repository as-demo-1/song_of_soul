namespace Opsive.UltimateInventorySystem.Editor.Utility.InventoryDatabaseImportExport
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Text.RegularExpressions;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Editor.Managers;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.Storage;
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Export Module", menuName = "Ultimate Inventory System/Editor/Import Export/CSV Export Module")]
    public class CsvImportExportModule : ImportExportModule
    {
        [SerializeField] protected ItemCategoryImportExport m_ItemCategoryImportExport;
        [SerializeField] protected ItemDefinitionImportExport m_ItemDefinitionImportExport;
        [SerializeField] protected CraftingCategoryImportExport m_CraftingCategoryImportExport;
        [SerializeField] protected CraftingRecipeImportExport m_CraftingRecipeImportExport;
        [SerializeField] protected CurrencyImportExport m_CurrencyImportExport;

        public virtual List<DataTableImportExportBase> FetchDataTableImportExport()
        {
            var dataTableImportExports = new List<DataTableImportExportBase>();
            dataTableImportExports.Add(m_ItemCategoryImportExport);
            dataTableImportExports.Add(m_ItemDefinitionImportExport);
            dataTableImportExports.Add(m_CraftingCategoryImportExport);
            dataTableImportExports.Add(m_CraftingRecipeImportExport);
            dataTableImportExports.Add(m_CurrencyImportExport);

            return dataTableImportExports;
        }
        
        public override void Export(InventorySystemDatabase database)
        {
            var path = EditorUtility.SaveFilePanel("Export Database", "", "database", "csv");
            if (path.Length == 0) {
                Debug.Log("Export Database Aborted, invalid path.");
                return;
            }
            Debug.Log("Starting Export Database '"+database.name+"' using the CSV Import Export Module");

            ShowProgressBar(0.1f, false, "Starting Export");

            try {
                var dataTableImportExports = FetchDataTableImportExport();
                List<DataTable> tables = new List<DataTable>();
            
                string header = AssetInfo.Version;

                for (int i = 0; i < dataTableImportExports.Count; i++) {
                    if (m_ItemCategoryImportExport.Allow_Export == false) { continue; }

                    var headerKey = dataTableImportExports[i].ID_KEY;
                    header += " | " + headerKey;
                    ShowProgressBar(0.1f+0.7f*(i/(float)dataTableImportExports.Count), false, "Processing "+headerKey);
                    tables.Add(dataTableImportExports[i].ExportTable(database));
                }

                var headerTable = new DataTable();
                headerTable.Columns.Add(header);
                tables.Insert(0, headerTable);
            
                ShowProgressBar(0.8f, false, "Writing to file");

                ImportExportUtility.WriteTablesToCSV(tables, path);
            }
            catch (Exception e) {
                Debug.LogError("An Error occured during Export. "+e);
                EditorUtility.ClearProgressBar();
                throw;
            }
            
            
            
            EditorUtility.ClearProgressBar();
            Debug.Log("Export Complete. File saved in "+path);
        }

        public override void Import(InventorySystemDatabase database)
        {
            var path = EditorUtility.OpenFilePanel("Import Database", "", "csv");
            if (path.Length == 0) {
                Debug.Log("Import Database Aborted, invalid path.");
                return;
            }
            
            Debug.Log("Starting Import into Database '"+database.name+"' using the CSV Import Export Module");
            
            ShowProgressBar(0.1f, true, "Starting Import");

            try {
                var dataTableImportExports = FetchDataTableImportExport();

                var tableSplitters = new List<string>();
                for (int i = 0; i < dataTableImportExports.Count; i++) {
                    tableSplitters.Add(dataTableImportExports[i].ID_KEY);
                }
                
                ShowProgressBar(0.15f, true, "Reading CSV file");

                var dataTables = ImportExportUtility.ReadCsvToDataTables(tableSplitters, path);

                // First we import all the object without data because some object might reference each other and therefore they must all exist.
                for (int i = 0; i < dataTableImportExports.Count; i++) {
                    var importExport = dataTableImportExports[i];
                    if(importExport.Allow_Import == false){ continue;}
                    if(dataTables[i] == null){ continue; }

                    ShowProgressBar(0.15f+0.4f*(i/(float)dataTableImportExports.Count), true, "Creating "+importExport.ID_KEY);
                    
                    importExport.ImportTableObjects(database, dataTables[i]);
                }
                
                // Import the data to the existing objects.
                for (int i = 0; i < dataTableImportExports.Count; i++) {
                    var importExport = dataTableImportExports[i];
                    if(importExport.Allow_Import == false){ continue;}
                    if(dataTables[i] == null){ continue; }
                    
                    ShowProgressBar(0.55f+0.35f*(i/(float)dataTableImportExports.Count), true, "Importing Values for "+importExport.ID_KEY);

                    importExport.ImportTableObjectsData(database, dataTables[i]);
                }
                
                ShowProgressBar(0.9f, true, "Save Database");

                DirtyAndSaveEverything(database);

            }
            catch (Exception e) {
                Debug.LogError("An Error occured during Import: "+e);
                EditorUtility.ClearProgressBar();
                throw;
            }

            EditorUtility.ClearProgressBar();
            Debug.Log("Import Complete.");
        }
        
        /// <summary>
        /// Show the progress bar.
        /// </summary>
        /// <param name="percentage">The percentage between 0 and 1.</param>
        private static void ShowProgressBar(float percentage, bool importing, string description)
        {
            var title = importing ? "Importing database from CSV: " : "Exporting database to CSV: ";
            title += description;
            EditorUtility.DisplayProgressBar(title, "This process may take a while.", percentage);
        }
        
        /// <summary>
        /// Dirty all the new objects and save them.
        /// </summary>
        private void DirtyAndSaveEverything(InventorySystemDatabase database)
        {
            Shared.Editor.Utility.EditorUtility.SetDirty(database);

            for (int i = 0; i < database.ItemCategories.Length; i++) {
                var itemCategory = database.ItemCategories[i];
                itemCategory.Serialize();
                ItemCategoryEditorUtility.SetItemCategoryDirty(itemCategory, true);
            }
            for (int i = 0; i < database.ItemDefinitions.Length; i++) {
                var itemDefinition = database.ItemDefinitions[i];
                itemDefinition.Serialize();
                ItemDefinitionEditorUtility.SetItemDefinitionDirty(itemDefinition, true);
            }
            for (int i = 0; i < database.Currencies.Length; i++) {
                var currency = database.Currencies[i];
                currency.Serialize();
                CurrencyEditorUtility.SetCurrencyDirty(currency, true);
            }
            for (int i = 0; i < database.CraftingCategories.Length; i++) {
                var craftingCategory = database.CraftingCategories[i];
                craftingCategory.Serialize();
                CraftingCategoryEditorUtility.SetCraftingCategoryDirty(craftingCategory, true);
            }
            for (int i = 0; i < database.CraftingRecipes.Length; i++) {
                var recipe = database.CraftingRecipes[i];
                recipe.Serialize();
                CraftingRecipeEditorUtility.SetCraftingRecipeDirty(recipe, true);
            }

            AssetDatabase.SaveAssets();
        }
    }

    [Serializable]
    public class AttributeImportExport
    {
        public string ATTRIBUTES_KEY = "ATTRIBUTES";
        public string ATTRIBUTES_SHORT_KEY = "ATR";
        public string CATEGORY_ATTRIBUTE_PREFIX = "C";
        public string DEFINITION_ATTRIBUTE_PREFIX = "D";
        public string ITEM_ATTRIBUTE_PREFIX = "I";
        public string ATTRIBUTE_SEPERATOR = "\n";
        
        private string VARIANT_INHERIT_PREFIX = "I";
        private string VARIANT_OVERRIDE_PREFIX = "O";
        private string VARIANT_MODIFY_PREFIX = "M";
        
        private Dictionary<string, ImportAttributeInfo> m_ImportAttributeInfos;
        
        public virtual string GetAttributeKey(AttributeBase attribute)
        {
            var suffix = "";
            switch (attribute.AttributeCollectionIndex) {
                case 0 : suffix = CATEGORY_ATTRIBUTE_PREFIX; break;
                case 1 : suffix = DEFINITION_ATTRIBUTE_PREFIX; break;
                case 2 : suffix = ITEM_ATTRIBUTE_PREFIX; break;
                default: Debug.LogError("Attribute Collection Index out of range "+attribute.AttributeCollectionIndex); break;
            }
            
            return $"[{ATTRIBUTES_SHORT_KEY}:{suffix}]{attribute.Name}<{attribute.GetValueType()}>";
        }

        public virtual string GetAttributeForNameList(AttributeBase attribute, bool first)
        {
            var delimiter = first ? "" : ATTRIBUTE_SEPERATOR;
            return $"{delimiter}[{AttributeVariantPrefix(attribute)}]{attribute.Name}";
        }

        private string AttributeVariantPrefix(AttributeBase attribute)
        {
            switch (attribute.VariantType) {
                case VariantType.Inherit:
                    return VARIANT_INHERIT_PREFIX;
                case VariantType.Override:
                    return VARIANT_OVERRIDE_PREFIX;
                case VariantType.Modify:
                    return VARIANT_MODIFY_PREFIX;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void CreateAttributeInfosFromTable(DataTable table)
        {
            m_ImportAttributeInfos = new Dictionary<string, ImportAttributeInfo>();
            
            var columns = table.Columns;
            for (int i = 0; i < columns.Count; i++) {
                var columnName = columns[i].ColumnName;
                var result = Regex.Match(columnName, $@"\[{ATTRIBUTES_SHORT_KEY}:([{CATEGORY_ATTRIBUTE_PREFIX},{DEFINITION_ATTRIBUTE_PREFIX},{ITEM_ATTRIBUTE_PREFIX}])\](.*)<(.*)>");
                if (result.Success && result.Groups.Count == 4) {

                    var name = result.Groups[2].Value;
                    var type = TypeUtility.GetType(result.Groups[3].Value);
                    
                    var collectionPrefix = result.Groups[1].Value;
                    var collectionID = 0;
                    if (collectionPrefix == CATEGORY_ATTRIBUTE_PREFIX) {
                        collectionID = 0;
                    }else if (collectionPrefix == DEFINITION_ATTRIBUTE_PREFIX) {
                        collectionID = 1;
                    }else if (collectionPrefix == ITEM_ATTRIBUTE_PREFIX) {
                        collectionID = 2;
                    }
                    
                    m_ImportAttributeInfos.Add(
                        name,
                        new ImportAttributeInfo()
                        {
                            ColumnName = columnName,
                            Name =  name,
                            ValueType = type,
                            CollectionID = collectionID
                        });
                }
            }
        }

        public void IterateThroughAttributes(DataRow row, Action<ImportAttributeInfo> addAttribute)
        {
            var columns = row.Table.Columns;
            
            if (columns.Contains(ATTRIBUTES_KEY)) {
                var attributeNames = row[ATTRIBUTES_KEY] as string;
                var attributeNamesList = attributeNames.Split(new string[]{ATTRIBUTE_SEPERATOR}, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < attributeNamesList.Length; i++) {
                    var attributeNameAndVariant = attributeNamesList[i];
                    var result = Regex.Match(attributeNameAndVariant, $@"\[([{VARIANT_INHERIT_PREFIX},{VARIANT_OVERRIDE_PREFIX},{VARIANT_MODIFY_PREFIX}])\](.*)");
                    if (result.Success == false  || result.Groups.Count != 3) {
                        Debug.LogWarning("The Attribute name does not have the correct format it should be '\\[([I,O,M])\\](.*)' instead it was"+attributeNameAndVariant);
                        continue;
                    }

                    var variantPrefix = result.Groups[1].Value;
                    var attributeName = result.Groups[2].Value;
                    
                    var variantType = VariantType.Inherit;
                    if (variantPrefix == VARIANT_INHERIT_PREFIX) {
                        variantType = VariantType.Inherit;
                    }else if (variantPrefix == VARIANT_OVERRIDE_PREFIX) {
                        variantType = VariantType.Override;
                    }else if (variantPrefix == VARIANT_MODIFY_PREFIX) {
                        variantType = VariantType.Modify;
                    }

                    if (m_ImportAttributeInfos.TryGetValue(attributeName, out var attributeInfo) == false) {
                        Debug.LogWarning($"The Attribute with name '{attributeName}' could not be found in the columns and therefore it cannot be added.");
                        continue;
                    }

                    attributeInfo.VariantType = variantType;
                    attributeInfo.Value = row[attributeInfo.ColumnName];
                    
                    addAttribute?.Invoke(attributeInfo);
                }
            }
        }
        
        public virtual void SetAttributeValue(AttributeBase attribute, ImportAttributeInfo attributeInfo)
        {
            var newValue = attributeInfo.Value;
            var valueType = attributeInfo.ValueType;
            
            try
            {
                var result = TypeDescriptor.GetConverter(valueType).ConvertFromString(newValue as string);
                AttributeEditorUtility.SetOverrideValueAsObject(attribute, result);
            }
            catch
            {
                Debug.LogWarning($"The attribute '{attribute.Name}' cannot import the value '{newValue}' within type '<{valueType.FullName}>'");
            }

            // JSON doesn't work well within CSV because it has commas
            //EditorJsonUtility.FromJsonOverwrite(newValue as string, overrideValue);
        }

        public virtual object GetAttributeValue(AttributeBase attribute)
        {
            return attribute.GetValueAsObject();
            // JSON doesn't work well within CSV because it has commas
            //return EditorJsonUtility.ToJson(attribute.GetValueAsObject());
        }
    }

    public struct ImportAttributeInfo
    {
        public string ColumnName;
        public string Name;
        public Type ValueType;
        public int CollectionID;
        public VariantType VariantType;
        public object Value;
    }

    public abstract class DataTableImportExportBase
    {
        public bool Allow_Export = true;
        public bool Allow_Import = true;
        public abstract string ID_KEY { get; }
        public virtual string NAME_KEY => "NAME";

        public abstract DataTable ExportTable(InventorySystemDatabase database);

        public abstract void ImportTableObjects(InventorySystemDatabase database, DataTable table);

        public virtual void ImportTableObjectsData(InventorySystemDatabase database, DataTable table)
        {
            for (int i = 0; i < table.Rows.Count; i++) {
                var row = table.Rows[i];
                var objName = row[NAME_KEY] as string;

                ImportObjectData(objName, row, database);
            }
        }

        protected abstract void ImportObjectData(string objName, DataRow row, InventorySystemDatabase database);
    }
    
    public abstract class DataTableImportExport<T> : DataTableImportExportBase where T : class
    {
        public override void ImportTableObjects(InventorySystemDatabase database, DataTable table)
        {
            for (int i = 0; i < table.Rows.Count; i++) {
                var row = table.Rows[i];
                var objID = Convert.ToUInt32(row[ID_KEY]);
                var objName = row[NAME_KEY] as string;

                var existingObject = database.Get<T>(objName);
                if (existingObject != null) {
                    AssignValidID(existingObject, objID, database);
                    continue;
                }

                var newObj = AddObject(objName, row, database,  DatabaseValidator.GetDatabaseDirectory(database));

                AssignValidID(newObj, objID, database);
            }
            
        }

        private void AssignValidID(T obj, uint objID, InventorySystemDatabase database)
        {
            var objectWithID = (IObjectWithID) obj;
            if(objectWithID.ID == objID){ return; }
            
            var isIDValid = DatabaseValidator.IDIsAvailable(objID, GetObjectList(database));
            if (isIDValid) {
                objectWithID.ID = objID;
            } else {
                DatabaseValidator.AssignNewUniqueID((IObjectWithID) obj, GetObjectList(database));
            }
        }

        internal abstract IObjectWithID[] GetObjectList(InventorySystemDatabase database);

        public abstract T AddObject(string name, DataRow dataRow, InventorySystemDatabase database, string databasePath);
    }

    [Serializable]
    public class ItemCategoryImportExport : DataTableImportExport<ItemCategory>
    {
        public override string ID_KEY => "ITEM CATEGORY";

        public string IS_ABSTRACT_KEY = "IS ABSTRACT";
        public string IS_MUTABLE_KEY = "IS MUTABLE";
        public string IS_UNIQUE_KEY = "IS UNIQUE";
        public string PARENTS_KEY = "PARENTS";
        public string PARENT_LIST_SEPERATOR = "\n";
        public AttributeImportExport m_AttributeImportExport;

        public override DataTable ExportTable(InventorySystemDatabase database)
        {
            return CreateDataTable(database.ItemCategories);
        }

        public override void ImportTableObjectsData(InventorySystemDatabase database, DataTable table)
        {
            m_AttributeImportExport.CreateAttributeInfosFromTable(table);
            base.ImportTableObjectsData(database, table);
        }
        

        protected override void ImportObjectData(string objName, DataRow row, InventorySystemDatabase database)
        {
            var category = database.Get<ItemCategory>(objName);
            var columns = row.Table.Columns;
            
            if (columns.Contains(IS_ABSTRACT_KEY)) {
                category.IsAbstract = Convert.ToBoolean(row[IS_ABSTRACT_KEY]);
            }
            if (columns.Contains(IS_MUTABLE_KEY)) {
                category.IsMutable = Convert.ToBoolean(row[IS_MUTABLE_KEY]);
            }
            if (columns.Contains(IS_UNIQUE_KEY)) {
                category.IsUnique = Convert.ToBoolean(row[IS_UNIQUE_KEY]);
            }
            if (columns.Contains(PARENTS_KEY)) {
                var parentNames = row[PARENTS_KEY] as string;
                if (string.IsNullOrWhiteSpace(parentNames) == false) {
                    var parentNameList = ImportExportUtility.ObjectNameStringToList(parentNames, PARENT_LIST_SEPERATOR);
                    for (int i = 0; i < parentNameList.Length; i++) {
                        var parentName = parentNameList[i];
                        var parentCategory = database.Get<ItemCategory>(parentName);
                        if (parentCategory == null) {
                            Debug.LogWarning($"The Item Category '{objName}' has a parent '{parentName}' which does not exist in the database, it will be ignored.");
                            continue;
                        }

                        if (category.Parents.Contains(parentCategory) == false) {
                            category.AddParent(parentCategory);
                        }
                    }
                }
            }
            
            m_AttributeImportExport.IterateThroughAttributes(row, (attributeInfo) =>
            {
                var attribute = category.GetAttribute(attributeInfo.Name);
                if (attribute != null) {
                    if (attribute.AttributeCollectionIndex != attributeInfo.CollectionID) {
                        if (attributeInfo.CollectionID == 0) {
                            category.MoveAttributeToItemCategories(attribute);
                        }else if (attributeInfo.CollectionID == 1) {
                            category.MoveAttributeToItemDefinitions(attribute);
                        }else if (attributeInfo.CollectionID == 2) {
                            category.MoveAttributeToItemCategories(attribute);
                        }
                        
                        //Get the attribute again since it changed.
                        attribute = category.GetAttribute(attributeInfo.Name);
                    }

                    if (attribute.GetValueType() != attributeInfo.ValueType && attributeInfo.ValueType != null) {
                        AttributeEditorUtility.ChangeType(attribute, attributeInfo.ValueType);
                    }


                    if (attribute.VariantType != attributeInfo.VariantType) {
                        AttributeEditorUtility.SetVariantType(attribute, attributeInfo.VariantType);
                    }

                    if (attributeInfo.Value != null) {
                        m_AttributeImportExport.SetAttributeValue(attribute, attributeInfo);
                    }
                    

                } else {
                    var newAttribute = ItemCategoryEditorUtility.AddAttribute(category, attributeInfo.Name, attributeInfo.CollectionID);
                    if (attributeInfo.ValueType != null) {
                        newAttribute = AttributeEditorUtility.ChangeType(newAttribute, attributeInfo.ValueType);
                    }
                    
                    AttributeEditorUtility.SetVariantType(newAttribute, attributeInfo.VariantType);
                    m_AttributeImportExport.SetAttributeValue(newAttribute, attributeInfo);
                }
            });
            
            ItemCategoryEditorUtility.SetItemCategoryDirty(category, true);
        }

        public DataTable CreateDataTable(IReadOnlyList<ItemCategory> itemCategories)
        {
            var table = new DataTable();
            
            //Default columns, attribute columns are added dynamically.
            table.Columns.Add(ID_KEY, typeof(uint));     
            table.Columns.Add(NAME_KEY, typeof(string));     
            table.Columns.Add(IS_ABSTRACT_KEY, typeof(bool));
            table.Columns.Add(IS_MUTABLE_KEY, typeof(bool));
            table.Columns.Add(IS_UNIQUE_KEY, typeof(bool));
            table.Columns.Add(PARENTS_KEY, typeof(string));
            table.Columns.Add(m_AttributeImportExport.ATTRIBUTES_KEY, typeof(string));

            for (int i = 0; i < itemCategories.Count; i++) {
                AddItemCategoryToTable(itemCategories[i], table);
            }

            return table;
        }
        
        public void AddItemCategoryToTable(ItemCategory itemCategory, DataTable table)
        {
            var newRow = table.NewRow();
            
            newRow[ID_KEY] = itemCategory.ID;
            newRow[NAME_KEY] = itemCategory.name;
            newRow[IS_ABSTRACT_KEY] = itemCategory.IsAbstract;
            newRow[IS_MUTABLE_KEY] = itemCategory.IsMutable;
            newRow[IS_UNIQUE_KEY] = itemCategory.IsUnique;
            newRow[PARENTS_KEY] = ImportExportUtility.ObjectNameListToString(itemCategory.ParentsReadOnly, PARENT_LIST_SEPERATOR);

            var attributeNames = "";
            for (int i = 0; i < itemCategory.GetAttributesCount(); i++) {
                var attribute = itemCategory.GetAttributesAt(i);
                var attributeKey = m_AttributeImportExport.GetAttributeKey(attribute);
                
                //Add additional columns for each attribute.
                if (table.Columns.Contains(attributeKey) == false) {
                    table.Columns.Add(attributeKey, typeof(object));
                }

                newRow[attributeKey] = m_AttributeImportExport.GetAttributeValue(attribute);

                attributeNames += m_AttributeImportExport.GetAttributeForNameList(attribute, string.IsNullOrWhiteSpace(attributeNames));
            }
            
            newRow[m_AttributeImportExport.ATTRIBUTES_KEY] = attributeNames;
            
            table.Rows.Add(newRow);
        }

        internal override IObjectWithID[] GetObjectList(InventorySystemDatabase database)
        {
            return database.ItemCategories;
        }

        public override ItemCategory AddObject(string name, DataRow dataRow, InventorySystemDatabase database, string databasePath)
        {
            return ItemCategoryEditorUtility.AddItemCategory(name, database, databasePath);
        }
        
    }
    
    [Serializable]
    public class ItemDefinitionImportExport : DataTableImportExport<ItemDefinition>
    {
        public override string ID_KEY => "ITEM DEFINITION";
        public string CATEGORY_KEY = "CATEGORY";
        public string PARENT_KEY = "PARENT";
        public AttributeImportExport m_AttributeImportExport;

        public override DataTable ExportTable(InventorySystemDatabase database)
        {
            return CreateDataTable(database.ItemDefinitions);
        }

        public override void ImportTableObjectsData(InventorySystemDatabase database, DataTable table)
        {
            m_AttributeImportExport.CreateAttributeInfosFromTable(table);
            base.ImportTableObjectsData(database, table);
        }

        protected override void ImportObjectData(string objName, DataRow row, InventorySystemDatabase database)
        {
            var definition = database.Get<ItemDefinition>(objName);
            var columns = row.Table.Columns;
            
            if (columns.Contains(PARENT_KEY)) {
                var parentName = row[PARENT_KEY] as string;
                if (string.IsNullOrWhiteSpace(parentName) == false) {
                    var parentDefinition = database.Get<ItemDefinition>(parentName);
                    if (parentDefinition == null) {
                        Debug.LogWarning($"The Item Category '{objName}' has a parent '{parentName}' which does not exist in the database, it will be ignored.");
                    } else {
                        definition.SetParent(parentDefinition);
                    }
                }
            }
            
            m_AttributeImportExport.IterateThroughAttributes(row, (attributeInfo) =>
            {
                if (definition.TryGetAttribute(attributeInfo.Name, out var attribute)) {
                    
                    if (attribute.VariantType != attributeInfo.VariantType) {
                        AttributeEditorUtility.SetVariantType(attribute, attributeInfo.VariantType);
                    }

                    if (attributeInfo.Value != null) {
                        m_AttributeImportExport.SetAttributeValue(attribute, attributeInfo);
                    }
                    

                } else {
                    Debug.LogWarning($"Found an attribute '{attributeInfo.Name}' on Item Definition '{definition.name}' but it does not exist, so it won't be added.");
                }
            });
            
            ItemDefinitionEditorUtility.SetItemDefinitionDirty(definition, true);
        }

        internal override IObjectWithID[] GetObjectList(InventorySystemDatabase database)
        {
            return database.ItemDefinitions;
        }

        public override ItemDefinition AddObject(string name, DataRow dataRow, InventorySystemDatabase database, string databasePath)
        {
            var categoryName = dataRow[CATEGORY_KEY] as string;
            var category = database.Get<ItemCategory>(categoryName);
            return ItemDefinitionEditorUtility.AddItemDefinition(name, category, database, databasePath);
        }


        public DataTable CreateDataTable(IReadOnlyList<ItemDefinition> itemDefinitions)
        {
            var table = new DataTable();
            
            //Default columns, attribute columns are added dynamically.
            table.Columns.Add(ID_KEY, typeof(uint));     
            table.Columns.Add(NAME_KEY, typeof(string));     
            table.Columns.Add(CATEGORY_KEY, typeof(string));
            table.Columns.Add(PARENT_KEY, typeof(string));
            table.Columns.Add(m_AttributeImportExport.ATTRIBUTES_KEY, typeof(string));

            for (int i = 0; i < itemDefinitions.Count; i++) {
                AddObjectToTable(itemDefinitions[i], table);
            }

            return table;
        }
        
        public void AddObjectToTable(ItemDefinition itemDefinition, DataTable table)
        {
            var newRow = table.NewRow();
            
            newRow[ID_KEY] = itemDefinition.ID;
            newRow[NAME_KEY] = itemDefinition.name;
            newRow[CATEGORY_KEY] = itemDefinition.Category?.name;
            newRow[PARENT_KEY] = itemDefinition.Parent?.name;

            var attributeNames = "";
            for (int i = 0; i < itemDefinition.ItemDefinitionAttributeCollection.Count; i++) {
                var attribute = itemDefinition.ItemDefinitionAttributeCollection[i];
                AddAttributeToTable(table, newRow, attribute , ref attributeNames);
            }
            
            for (int i = 0; i < itemDefinition.DefaultItem.ItemAttributeCollection.Count; i++) {
                var attribute = itemDefinition.DefaultItem.ItemAttributeCollection[i];
                AddAttributeToTable(table, newRow, attribute , ref attributeNames);
            }
            
            newRow[m_AttributeImportExport.ATTRIBUTES_KEY] = attributeNames;
            
            table.Rows.Add(newRow);
        }

        private void AddAttributeToTable(DataTable table, DataRow newRow, AttributeBase attribute,  ref string attributeNames)
        {
            var attributeKey = m_AttributeImportExport.GetAttributeKey(attribute);

            //Add additional columns for each attribute.
            if (table.Columns.Contains(attributeKey) == false) {
                table.Columns.Add(attributeKey, typeof(object));
            }

            newRow[attributeKey] = m_AttributeImportExport.GetAttributeValue(attribute);

            attributeNames += m_AttributeImportExport.GetAttributeForNameList(attribute, string.IsNullOrWhiteSpace(attributeNames));
        }
    }

    [Serializable]
    public class CurrencyImportExport : DataTableImportExport<Currency>
    {
        public override string ID_KEY => "CURRENCY";
        public string PARENT_KEY = "PARENT";
        public string EXCHANGE_RATE_KEY = "EXCHANGE RATE";
        public string MAX_AMOUNT_KEY = "MAX AMOUNT";
        public string OVERFLOW_KEY = "OVERFLOW CURRENCY";
        public string FRACTION_KEY = "FRACTION CURRENCY";
        
        public override DataTable ExportTable(InventorySystemDatabase database)
        {
            return CreateDataTable(database.Currencies);
        }

        protected override void ImportObjectData(string objName, DataRow row, InventorySystemDatabase database)
        {
            var currency = database.Get<Currency>(objName);
            var columns = row.Table.Columns;

            if (columns.Contains(PARENT_KEY)) {
                var parentName = row[PARENT_KEY] as string;
                if (string.IsNullOrWhiteSpace(parentName) == false) {
                    var parentCurrency = database.Get<Currency>(parentName);
                    if (parentCurrency == null) {
                        Debug.LogWarning($"The Currency '{objName}' has a parent '{parentName}' which does not exist in the database, it will be ignored.");
                    } else {
                        currency.SetParent(parentCurrency);
                    }
                }
            }
            if (columns.Contains(EXCHANGE_RATE_KEY)) {
                currency.SetParentExchangeRate(Convert.ToDouble(row[EXCHANGE_RATE_KEY]));
            }
            if (columns.Contains(MAX_AMOUNT_KEY)) {
                currency.SetMaxAmount(Convert.ToInt32(row[MAX_AMOUNT_KEY]));
            }
            if (columns.Contains(OVERFLOW_KEY)) {
                var overflow = row[OVERFLOW_KEY] as string;
                if (string.IsNullOrWhiteSpace(overflow) == false) {
                    var overflowCurrency = database.Get<Currency>(overflow);
                    if (overflowCurrency == null) {
                        Debug.LogWarning($"The Currency '{objName}' has an overflow '{overflow}' which does not exist in the database, it will be ignored.");
                    } else {
                        currency.SetParent(overflowCurrency);
                    }
                }
            }
            if (columns.Contains(FRACTION_KEY)) {
                var fraction = row[FRACTION_KEY] as string;
                if (string.IsNullOrWhiteSpace(fraction) == false) {
                    var fractionCurrency = database.Get<Currency>(fraction);
                    if (fractionCurrency == null) {
                        Debug.LogWarning($"The Currency '{objName}' has a fraction '{fraction}' which does not exist in the database, it will be ignored.");
                    } else {
                        currency.SetParent(fractionCurrency);
                    }
                }
            }
            

            CurrencyEditorUtility.SetCurrencyDirty(currency,true);
        }


        public DataTable CreateDataTable(IReadOnlyList<Currency> currencies)
        {
            var table = new DataTable();
            
            //Default columns, attribute columns are added dynamically.
            table.Columns.Add(ID_KEY, typeof(uint));     
            table.Columns.Add(NAME_KEY, typeof(string));     
            table.Columns.Add(EXCHANGE_RATE_KEY, typeof(double));
            table.Columns.Add(PARENT_KEY, typeof(string));
            table.Columns.Add(MAX_AMOUNT_KEY, typeof(int));
            table.Columns.Add(OVERFLOW_KEY, typeof(string));
            table.Columns.Add(FRACTION_KEY, typeof(string));

            for (int i = 0; i < currencies.Count; i++) {
                AddObjectToTable(currencies[i], table);
            }

            return table;
        }
        
        public void AddObjectToTable(Currency currency, DataTable table)
        {
            var newRow = table.NewRow();
            
            newRow[ID_KEY] = currency.ID;
            newRow[NAME_KEY] = currency.name;
            newRow[EXCHANGE_RATE_KEY] = currency.ExchangeRateToParent;
            newRow[PARENT_KEY] = currency.Parent?.name;
            newRow[MAX_AMOUNT_KEY] = currency.MaxAmount;
            newRow[OVERFLOW_KEY] = currency.OverflowCurrency?.name;
            newRow[FRACTION_KEY] = currency.FractionCurrency?.name;

            table.Rows.Add(newRow);
        }

        internal override IObjectWithID[] GetObjectList(InventorySystemDatabase database)
        {
            return database.Currencies;
        }

        public override Currency AddObject(string name, DataRow dataRow, InventorySystemDatabase database, string databasePath)
        {
            return CurrencyEditorUtility.AddCurrency(name, database, databasePath);
        }
    }
    
    [Serializable]
    public class CraftingCategoryImportExport : DataTableImportExport<CraftingCategory>
    {
        public override string ID_KEY => "CRAFTING CATEGORY";
        public string IS_ABSTRACT_KEY = "IS ABSTRACT";
        public string PARENTS_KEY = "PARENTS";
        public string PARENT_LIST_SEPERATOR = "\n";
        public string RECIPE_TYPE_KEY = "RECIPE TYPE";
        
        public override DataTable ExportTable(InventorySystemDatabase database)
        {
            return CreateDataTable(database.CraftingCategories);
        }


        protected override void ImportObjectData(string objName, DataRow row, InventorySystemDatabase database)
        {
            var category = database.Get<CraftingCategory>(objName);
            var columns = row.Table.Columns;
            
            if (columns.Contains(IS_ABSTRACT_KEY)) {
                category.IsAbstract = Convert.ToBoolean(row[IS_ABSTRACT_KEY]);
            }
            if (columns.Contains(PARENTS_KEY)) {
                var parentNames = row[PARENTS_KEY] as string;
                if (string.IsNullOrWhiteSpace(parentNames) == false) {
                    var parentNameList = ImportExportUtility.ObjectNameStringToList(parentNames, PARENT_LIST_SEPERATOR);
                    for (int i = 0; i < parentNameList.Length; i++) {
                        var parentName = parentNameList[i];
                        var parentCategory = database.Get<CraftingCategory>(parentName);
                        if (parentCategory == null) {
                            Debug.LogWarning($"The Crafting Category '{objName}' has a parent '{parentName}' which does not exist in the database, it will be ignored.");
                            continue;
                        }

                        if (category.Parents.Contains(parentCategory) == false) {
                            category.AddParent(parentCategory);
                        }
                    }
                }
            }
            if (columns.Contains(RECIPE_TYPE_KEY)) {
                var recipeTypeName = row[RECIPE_TYPE_KEY] as string;
                var recipeType = TypeUtility.GetType(recipeTypeName);
                if (recipeType == null) {
                    Debug.LogWarning($"The Recipe Type '{recipeTypeName}' could not be found.");
                } else {
                    CraftingCategoryEditorUtility.SetRecipeType(category,TypeUtility.GetType(recipeTypeName));
                }
                
            }
            
            CraftingCategoryEditorUtility.SetCraftingCategoryDirty(category, true);
        }

        internal override IObjectWithID[] GetObjectList(InventorySystemDatabase database)
        {
            return database.CraftingCategories;
        }

        public override CraftingCategory AddObject(string name, DataRow dataRow, InventorySystemDatabase database, string databasePath)
        {
            return CraftingCategoryEditorUtility.AddCraftingCategory(name, database, databasePath);
        }

        public DataTable CreateDataTable(IReadOnlyList<CraftingCategory> craftingCategories)
        {
            var table = new DataTable();
            
            //Default columns, attribute columns are added dynamically.
            table.Columns.Add(ID_KEY, typeof(uint));     
            table.Columns.Add(NAME_KEY, typeof(string));     
            table.Columns.Add(IS_ABSTRACT_KEY, typeof(bool));
            table.Columns.Add(PARENTS_KEY, typeof(string));
            table.Columns.Add(RECIPE_TYPE_KEY, typeof(string));

            for (int i = 0; i < craftingCategories.Count; i++) {
                AddObjectToTable(craftingCategories[i], table);
            }

            return table;
        }
        
        public void AddObjectToTable(CraftingCategory craftingCategory, DataTable table)
        {
            var newRow = table.NewRow();
            
            newRow[ID_KEY] = craftingCategory.ID;
            newRow[NAME_KEY] = craftingCategory.name;
            newRow[IS_ABSTRACT_KEY] = craftingCategory.IsAbstract;
            newRow[PARENTS_KEY] = ImportExportUtility.ObjectNameListToString(craftingCategory.ParentsReadOnly, PARENT_LIST_SEPERATOR);
            newRow[RECIPE_TYPE_KEY] = craftingCategory.RecipeType.FullName;

            table.Rows.Add(newRow);
        }
    }
    
    [Serializable]
    public class CraftingRecipeImportExport : DataTableImportExport<CraftingRecipe>
    {
        public override string ID_KEY => "CRAFTING RECIPE";
        
        public string CATEGORY_KEY = "CATEGORY";
        public string ITEM_INGREDIENTS_KEY = "ITEM INGREDIENTS";
        public string DEFINITION_INGREDIENTS_KEY = "DEFINITION INGREDIENTS";
        public string CATEGORY_INGREDIENTS_KEY = "CATEGORY INGREDIENTS";
        public string OUTPUTS_KEY = "OUTPUTS";
        public string LIST_SEPERATOR = "\n"; 
        
        public override DataTable ExportTable(InventorySystemDatabase database)
        {
            return CreateDataTable(database.CraftingRecipes);
        }

        protected override void ImportObjectData(string objName, DataRow row, InventorySystemDatabase database)
        {
            var recipe = database.Get<CraftingRecipe>(objName);
            var columns = row.Table.Columns;
            
            
            if (columns.Contains(ITEM_INGREDIENTS_KEY)) {
                var itemAmountNames = GetAmountsFromString(row[ITEM_INGREDIENTS_KEY] as string);
                recipe.Ingredients.ItemAmounts.Clear();
                for (int i = 0; i < itemAmountNames.Count; i++) {
                    var amountName = itemAmountNames[i];
                    var itemDefinition = database.Get<ItemDefinition>(amountName.name);
                    if (itemDefinition != null) {
                        recipe.Ingredients.ItemAmounts.Add((amountName.amount, Item.Create(itemDefinition)));
                    }
                }
            }
            
            if (columns.Contains(DEFINITION_INGREDIENTS_KEY)) {
                var amountNames = GetAmountsFromString(row[DEFINITION_INGREDIENTS_KEY] as string);
                recipe.Ingredients.ItemDefinitionAmounts.Clear();
                for (int i = 0; i < amountNames.Count; i++) {
                    var amountName = amountNames[i];
                    var itemDefinition = database.Get<ItemDefinition>(amountName.name);
                    if (itemDefinition != null) {
                        recipe.Ingredients.ItemDefinitionAmounts.Add((amountName.amount, itemDefinition));
                    }
                }
            }
            
            if (columns.Contains(CATEGORY_INGREDIENTS_KEY)) {
                var amountNames = GetAmountsFromString(row[CATEGORY_INGREDIENTS_KEY] as string);
                recipe.Ingredients.ItemCategoryAmounts.Clear();
                for (int i = 0; i < amountNames.Count; i++) {
                    var amountName = amountNames[i];
                    var itemCategory = database.Get<ItemCategory>(amountName.name);
                    if (itemCategory != null) {
                        recipe.Ingredients.ItemCategoryAmounts.Add((amountName.amount, itemCategory));
                    }
                }
            }

            if (columns.Contains(OUTPUTS_KEY)) {
                var amountNames = GetAmountsFromString(row[OUTPUTS_KEY] as string);
                recipe.DefaultOutput.ItemAmounts.Clear();
                for (int i = 0; i < amountNames.Count; i++) {
                    var amountName = amountNames[i];
                    var itemDefinition = database.Get<ItemDefinition>(amountName.name);
                    if (itemDefinition != null) {
                        var newItem = Item.Create(itemDefinition);
                        recipe.DefaultOutput.ItemAmounts.Add((amountName.amount, newItem));
                    }
                }
            }

            CraftingRecipeEditorUtility.SetCraftingRecipeDirty(recipe, true);
        }

        private List<(int amount, string name)> GetAmountsFromString( string str)
        {
            var result = new List<(int amount, string name)>();
            var lines = str.Split(new []{LIST_SEPERATOR} , StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++) {
                var objectAmountStr = lines[i];
                var regexResult = Regex.Match(objectAmountStr, $@"(.*) x([0-9]*)");
                if (regexResult.Success && regexResult.Groups.Count == 3) {
                    var name = regexResult.Groups[1].Value;
                    var amount = Int32.Parse(regexResult.Groups[2].Value);
                    result.Add( (amount, name) );
                }
            }

            return result;
        }

        public DataTable CreateDataTable(IReadOnlyList<CraftingRecipe> recipes)
        {
            var table = new DataTable();
            
            //Default columns, attribute columns are added dynamically.
            table.Columns.Add(ID_KEY, typeof(uint));     
            table.Columns.Add(NAME_KEY, typeof(string));     
            table.Columns.Add(CATEGORY_KEY, typeof(string));
            table.Columns.Add(CATEGORY_INGREDIENTS_KEY, typeof(string));
            table.Columns.Add(DEFINITION_INGREDIENTS_KEY, typeof(string));
            table.Columns.Add(ITEM_INGREDIENTS_KEY, typeof(string));
            table.Columns.Add(OUTPUTS_KEY, typeof(string));

            for (int i = 0; i < recipes.Count; i++) {
                AddObjectToTable(recipes[i], table);
            }

            return table;
        }
        
        public void AddObjectToTable(CraftingRecipe recipe, DataTable table)
        {
            var newRow = table.NewRow();
            
            newRow[ID_KEY] = recipe.ID;
            newRow[NAME_KEY] = recipe.name;
            newRow[CATEGORY_KEY] = recipe.Category?.name;
            newRow[ITEM_INGREDIENTS_KEY] = ArrayObjectAmountToString(recipe.Ingredients.ItemAmounts);
            newRow[DEFINITION_INGREDIENTS_KEY] = ArrayObjectAmountToString(recipe.Ingredients.ItemDefinitionAmounts);
            newRow[CATEGORY_INGREDIENTS_KEY] = ArrayObjectAmountToString(recipe.Ingredients.ItemCategoryAmounts);
            newRow[OUTPUTS_KEY] = ArrayObjectAmountToString(recipe.DefaultOutput.ItemAmounts);

            table.Rows.Add(newRow);
        }

        private string ArrayObjectAmountToString<T, Ta>( ObjectAmounts<T, Ta> objectAmounts) where Ta : IObjectAmount<T> where T : class, IObjectWithIDReadOnly
        {
            if (objectAmounts == null) {
                return "";
            }

            var array = objectAmounts?.Array;
            if (array == null || array.Length == 0) {
                return "";
            }

            var s = string.Empty;
            for (int i = 0; i < array.Length; i++) {
                s += string.Format("{2}{0} x{1} ",
                    array[i].Object != null ? array[i].Object.name : "(none)",
                    array[i].Amount,
                    i != 0 ? LIST_SEPERATOR : "");
            }

            return s;
        }

        internal override IObjectWithID[] GetObjectList(InventorySystemDatabase database)
        {
            return database.CraftingRecipes;
        }

        public override CraftingRecipe AddObject(string name, DataRow dataRow, InventorySystemDatabase database, string databasePath)
        {var categoryName = dataRow[CATEGORY_KEY] as string;
            var category = database.Get<CraftingCategory>(categoryName);
            return CraftingRecipeEditorUtility.AddCraftingRecipe(name, category, database, databasePath);
        }
    }
}