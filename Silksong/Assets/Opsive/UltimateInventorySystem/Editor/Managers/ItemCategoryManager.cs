/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Editor.Utility.InventoryDatabaseImportExport;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The ItemCategoryManager shows the options related to the inventory categories.
    /// </summary>
    [OrderedEditorItem("Item Categories", 20)]
    [RequireDatabase]
    [System.Serializable]
    public class ItemCategoryManager : InventorySystemObjectBaseManager<ItemCategory>
    {
        private const string c_NewAttributeName = "Attribute";

        private Toggle m_IsAbstract;
        private VisualElement m_MutableAndUniqueContainer;
        private Toggle m_IsMutable;
        private Toggle m_IsUnique;
        private ColorField m_Color;
        private UnityObjectFieldWithPreview m_IconField;
        private ReorderableList m_ParentCategoriesReorderableList;
        private TabToolbar m_AttributeCollectionsTabToolbar;
        private ReorderableList m_AttributesReorderableList;
        private AttributeVisualElement m_AttributeVisualElement;
        private TabToolbar m_RelationshipsTabToolbar;
        private ReorderableList m_RelationshipsReorderableList;

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            base.BuildVisualElements();
            
            //Insert the more options just under the title, styling is used to show it next to it.
            /*var moreOptions = new IconOptionButton(IconOption.Cog);
            moreOptions.name = "ManagerMoreOptionButton";
            moreOptions.clicked += ShowItemCategoryMoreOptionsMenu;
            m_ManagerContentContainer.Insert(0, moreOptions);*/

            m_IsAbstract = new Toggle("Abstract");
            m_IsAbstract.tooltip =
                "An abstract category cannot have direct item definitions. And therefore will not be shown in the dropdown when selecting a category for an item definition.";
            m_IsAbstract.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                var setAbstract = (SelectedObject.IsAbstract || SelectedObject.Elements.Count == 0) ||
                 EditorUtility.DisplayDialog("Set the Item Category as Abstract?",
                     $"The Item Category '{SelectedObject}' you are trying to set abstract has direct Item Definitions." +
                     $" Setting it abstract will set all direct Item Definitions to a child or null category.\n" +
                     $"Are you sure you would like to set it to abstract?",
                     "Yes",
                     "No");

                if (setAbstract) {
                    ItemCategoryEditorUtility.SetIsAbstract(SelectedObject, evt.newValue);
                }

                m_MutableAndUniqueContainer.Clear();
                if (SelectedObject.IsAbstract == false) {
                    m_MutableAndUniqueContainer.Add(m_IsMutable);
                    m_MutableAndUniqueContainer.Add(m_IsUnique);
                }
                m_IsAbstract.SetValueWithoutNotify(SelectedObject.IsAbstract);
            });
            m_ContentPanel.Add(m_IsAbstract);

            m_MutableAndUniqueContainer = new VisualElement();
            m_ContentPanel.Add(m_MutableAndUniqueContainer);

            m_IsMutable = new Toggle("Mutable");
            m_IsMutable.tooltip =
                "The category mutability affects if its items can have their attribute modified at runtime";
            m_IsMutable.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                ItemCategoryEditorUtility.SetIsMutable(SelectedObject, evt.newValue);
            });
            m_MutableAndUniqueContainer.Add(m_IsMutable);


            m_IsUnique = new Toggle("Unique");
            m_IsUnique.tooltip =
                "If true the items in this category will not stack, Unique items can be duplicated and given a new ID when added to a collection to stay unique.";
            m_IsUnique.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                ItemCategoryEditorUtility.SetIsUnique(SelectedObject, evt.newValue);
            });
            m_MutableAndUniqueContainer.Add(m_IsUnique);

            m_Color = new ColorField("Editor Color");
            m_Color.tooltip = "The color of the box next to the name of the category.";
            m_Color.RegisterCallback<ChangeEvent<Color>>(evt =>
            {
                ItemCategoryEditorUtility.SetColor(SelectedObject, evt.newValue);
                Refresh();
            });
            m_ContentPanel.Add(m_Color);

            m_IconField = new UnityObjectFieldWithPreview();
            m_IconField.label = "Editor Icon";
            m_IconField.tooltip = "The icon used in the custom editors, displayed next to the object name.";
            m_IconField.objectType = typeof(Sprite);
            m_IconField.RegisterValueChangedCallback(evt =>
            {
                ItemCategoryEditorUtility.SetIcon(SelectedObject, evt.newValue as Sprite);
                Refresh();
            });
            m_ContentPanel.Add(m_IconField);

            var itemCategoryParentPicker = new ItemCategoryField("",
                m_InventoryMainWindow.Database,
                new (string, Action<ItemCategory>)[]
            {
                ("Add Parent", c =>
                {
                    ItemCategoryEditorUtility.AddItemCategoryParent(SelectedObject, c);
                    Refresh();
                })
            },
            (cat) => SelectedObject?.AddParentCondition(cat) ?? false);
            itemCategoryParentPicker.IncludeNullOption = false;

            m_ParentCategoriesReorderableList = new ReorderableList(null, (VisualElement parent, int index) =>
            {
                parent.Add(new ItemCategoryViewName());
            },
            (VisualElement parent, int index) =>
            {
                var itemCategoryViewName = parent.ElementAt(0) as ItemCategoryViewName;
                var category = m_ParentCategoriesReorderableList.ItemsSource[index] as ItemCategory;

                itemCategoryViewName.Refresh(category);
            },
            (VisualElement header) =>
            {
                header.Add(new Label("Parents"));
            }, null,
            () =>
            {
                itemCategoryParentPicker.SearchableListWindow.OpenPopUpWindow(
                    m_ParentCategoriesReorderableList.AddButton.worldBound.position,
                    m_ParentCategoriesReorderableList.AddButton.worldBound.size);
            },
            (int index) =>
            {
                ItemCategoryEditorUtility.RemoveItemCategoryParent(
                    SelectedObject,
                    m_ParentCategoriesReorderableList.ItemsSource[index] as ItemCategory);
                Refresh();
            }, (i1, i2) =>
            {
                ItemCategoryEditorUtility.SetItemCategoryDirty(SelectedObject, true);
                Refresh();
            });
            m_ParentCategoriesReorderableList.tooltip = "The parents of the item category. Defines which attributes are inherited.";
            m_ContentPanel.Add(m_ParentCategoriesReorderableList);

            var attributesBox = new VisualElement();
            attributesBox.name = "box";
            attributesBox.AddToClassList(ManagerStyles.BoxBackground);

            m_AttributeCollectionsTabToolbar = new TabToolbar(new string[]
            {
                "Category Attributes",
                "Item Definition Attributes",
                "Item Attributes"
            }, 0, ShowAttributeTab);
            attributesBox.Add(m_AttributeCollectionsTabToolbar);

            m_AttributeVisualElement = new AttributeVisualElement(m_InventoryMainWindow.Database, true);
            m_AttributeVisualElement.AddToClassList(InventoryManagerStyles.AttributeView_Margin);
            m_AttributesReorderableList = new ReorderableList(null, (VisualElement parent, int index) =>
            {
                var viewName = new AttributeViewNameAndValue();
                viewName.AddManipulator(new ContextualMenuManipulator(
                    evt =>
                    {
                        evt.menu.AppendAction("Duplicate", DuplicateAttribute, DropdownMenuAction.AlwaysEnabled, evt.target);
                    }));
                parent.Add(viewName);
            }, (VisualElement parent, int index) =>
            {
                var attributeViewNameAndValue = parent.ElementAt(0) as AttributeViewNameAndValue;
                var attribute = m_AttributesReorderableList.ItemsSource[index] as AttributeBase;
                attributeViewNameAndValue.Refresh(attribute);
            }, null, (int index) =>
            {
                //Make sure to refresh the attribute in case the object was deserialized again.
                ShowAttributeTab(m_AttributeCollectionsTabToolbar.Selected);
                m_AttributeVisualElement.BindAttribute(m_AttributesReorderableList.ItemsSource[index] as AttributeBase);
            }, () =>
            {
                ItemCategoryEditorUtility.AddAttribute(SelectedObject, c_NewAttributeName, m_AttributeCollectionsTabToolbar.Selected);
                var attributeCollection = GetAttributeCollection(m_AttributeCollectionsTabToolbar.Selected);
                m_AttributesReorderableList.Refresh(attributeCollection.Attributes);
            }, (int index) =>
            {
                var category = SelectedObject;
                if (index < 0 || index > m_AttributesReorderableList.ItemsSource.Count) { return; }
                var attribute = m_AttributesReorderableList.ItemsSource[index] as AttributeBase;

                var removeAttributeMenu = new RemoveAttributeMenu();
                removeAttributeMenu.SetAttribute(attribute);
                removeAttributeMenu.OnRemove += () =>
                {
                    var attributeCollection = GetAttributeCollection(m_AttributeCollectionsTabToolbar.Selected);
                    m_AttributesReorderableList.Refresh(attributeCollection.Attributes);
                    m_AttributeVisualElement.ClearBinding();
                };
                removeAttributeMenu.ShowContext();

            }, (i1, i2) =>
            {
                ItemCategoryEditorUtility.SetItemCategoryDirty(SelectedObject, true);
                Refresh();
            });
            m_AttributeVisualElement.OnValueChanged += (attribute) => m_AttributesReorderableList.Refresh();
            m_AttributeVisualElement.OnAttributeReplaced += (attribute) =>
            {
                ShowAttributeTab(m_AttributeCollectionsTabToolbar.Selected);
                m_AttributesReorderableList.SelectedIndex = m_AttributesReorderableList.ItemsSource.Count - 1;
            };
            attributesBox.Add(m_AttributesReorderableList);
            attributesBox.Add(m_AttributeVisualElement);
            m_ContentPanel.Add(attributesBox);

            var relationshipBox = new VisualElement();
            relationshipBox.name = "box";
            relationshipBox.AddToClassList(ManagerStyles.BoxBackground);

            m_RelationshipsTabToolbar = new TabToolbar(new string[]
            {
                "Ancestors",
                "Descendants",
                "Direct Item Definitions",
                "Inherited Item Definitions",
                "Direct Recipes",
                "Inherited Recipes"
            }, 0, ShowRelationshipTab);
            relationshipBox.Add(m_RelationshipsTabToolbar);

            m_RelationshipsReorderableList = new ReorderableList(null, (VisualElement parent, int index) =>
            {
                //parent.Add(new ViewName());
            }, (VisualElement parent, int index) =>
            {
                var viewName = parent.childCount == 0 ? null : parent.ElementAt(0) as ViewName;
                var obj = m_RelationshipsReorderableList.ItemsSource[index];

                if (viewName == null || viewName.GetType().GetProperty("m_Object")?.PropertyType != obj.GetType()) {
                    viewName = ManagerUtility.CreateViewNameFor(obj);
                    parent.Clear();
                    parent.Add(viewName);
                }

                viewName.Refresh(obj);
            }, null, null, null, null, null);
            relationshipBox.Add(m_RelationshipsReorderableList);
            m_ContentPanel.Add(relationshipBox);
        }

        /// <summary>
        /// Returns true if the category name is unique.
        /// </summary>
        /// <param name="name">The possible name of the category.</param>
        /// <returns>True if the category name is unique.</returns>
        public override bool IsObjectNameValidAndUnique(string name)
        {
            for (int i = 0; i < m_InventoryMainWindow.Database.ItemCategories.Length; ++i) {
                if (m_InventoryMainWindow.Database.ItemCategories[i].name == name) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Refreshes the content for the current database.
        /// </summary>
        public override void Refresh()
        {
            var categories = m_InventoryMainWindow.Database?.ItemCategories;
            
            if (categories == null) {
                return;
            }
            
            for (int i = 0; i < categories.Length; ++i) {
                categories[i].Initialize(false);
            }

            if (m_SelectedIndex == -1) {
                m_SelectedIndex = 0;
            }
            
            OnSelected(m_SelectedIndex);
        }

        /// <summary>
        /// Returns the list that the ReorderableList should use.
        /// </summary>
        /// <returns>The list that the ReorderableList should use.</returns>
        public override IList<ItemCategory> GetList()
        {
            return m_InventoryMainWindow.Database?.ItemCategories;
        }

        /// <summary>
        /// Build the contextual menu when right-clicking a definition.
        /// </summary>
        /// <param name="evt">The event context.</param>
        void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Duplicate", DuplicateObject, DropdownMenuAction.AlwaysEnabled, evt.target);
        }

        /// <summary>
        /// Duplicate the item definition using the right click.
        /// </summary>
        /// <param name="action">The drop down menu action.</param>
        void DuplicateObject(DropdownMenuAction action)
        {
            var categoryViewName = action.userData as ItemCategoryViewName;
            var categoryToDuplicate = categoryViewName?.ItemCategory;

            var newItemCategory = ItemCategoryEditorUtility.DuplicateItemCategory(categoryToDuplicate,
                m_InventoryMainWindow.Database,
                m_InventoryMainWindow.GetDatabaseDirectory());

            Refresh();
            Select(newItemCategory);
        }

        /// <summary>
        /// Duplicate the attribute using the right click.
        /// </summary>
        /// <param name="action">The drop down menu action.</param>
        void DuplicateAttribute(DropdownMenuAction action)
        {
            var viewName = action.userData as AttributeViewNameAndValue;
            var attributeToDuplicate = viewName?.Attribute;

            ItemCategoryEditorUtility.DuplicateAttribute(SelectedObject, attributeToDuplicate);

            Refresh();
        }

        /// <summary>
        /// Creates the new ReorderableList item.
        /// </summary>
        /// <param name="parent">The parent ReorderableList item.</param>
        /// <param name="index">The index of the item.</param>
        public override void MakeListItem(VisualElement parent, int index)
        {
            var viewName = new ItemCategoryViewName();
            viewName.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            parent.Add(viewName);
        }

        /// <summary>
        /// Binds the ReorderableList item to the specified index.
        /// </summary>
        /// <param name="parent">The ReorderableList item that is being bound.</param>
        /// <param name="index">The index of the item.</param>
        public override void BindListItem(VisualElement parent, int index)
        {
            if (m_ListPanel == null) {
                return;
            }
            var itemCategoryViewName = parent.ElementAt(0) as ItemCategoryViewName;
            if (index < 0 || index >= m_ListPanel.SearchableList.ItemList.Count) { return; }
            var itemCategory = m_ListPanel.SearchableList.ItemList[index];
            itemCategoryViewName.Refresh(itemCategory);
        }

        /// <summary>
        /// Update the visual elements to reflect the specified category.
        /// </summary>
        /// <param name="itemCategory">The category that is being displayed.</param>
        protected override void UpdateElements(ItemCategory itemCategory)
        {
            m_Name.SetValueWithoutNotify(itemCategory.name);
            m_IsAbstract.SetValueWithoutNotify(itemCategory.IsAbstract);
            m_MutableAndUniqueContainer.Clear();
            if (itemCategory.IsAbstract == false) {
                m_MutableAndUniqueContainer.Add(m_IsMutable);
                m_MutableAndUniqueContainer.Add(m_IsUnique);
            }
            m_IsMutable.SetValueWithoutNotify(itemCategory.IsMutable);
            m_IsUnique.SetValueWithoutNotify(itemCategory.IsUnique);
            m_Color.SetValueWithoutNotify(itemCategory.m_Color);
            m_IconField.SetValueWithoutNotify(itemCategory.m_EditorIcon);

            itemCategory.Initialize(false);
            m_ParentCategoriesReorderableList.Refresh(itemCategory.Parents);

            if (m_AttributeCollectionsTabToolbar.Selected < 0) { m_AttributeCollectionsTabToolbar.Selected = 0; }
            ShowAttributeTab(m_AttributeCollectionsTabToolbar.Selected);
            if (m_RelationshipsTabToolbar.Selected < 0) { m_RelationshipsTabToolbar.Selected = 0; }
            ShowRelationshipTab(m_RelationshipsTabToolbar.Selected);
        }

        /// <summary>
        /// Shows the attributes that correspond to the tab at the specified index.
        /// </summary>
        /// <param name="index">The attribute tab index.</param>
        private void ShowAttributeTab(int index)
        {
            m_AttributeVisualElement.ClearBinding();
            m_AttributesReorderableList.Refresh(GetAttributeCollection(index).Attributes);

            var attributeIndex = m_AttributesReorderableList.SelectedIndex;
            if (attributeIndex >= 0 && attributeIndex < m_AttributesReorderableList.ItemsSource.Count) {
                m_AttributeVisualElement.BindAttribute(m_AttributesReorderableList.ItemsSource[attributeIndex] as AttributeBase);
            }
        }

        /// <summary>
        /// Returns the AttributeCollection at the specified index.
        /// </summary>
        /// <param name="index">The index to retrieve the AttributeCollection at.</param>
        /// <returns>The AttributeCollection at the specified index.</returns>
        private AttributeCollection GetAttributeCollection(int index)
        {
            var category = SelectedObject;
            if (index == 0) { // Item Category Attributes.
                return category.ItemCategoryAttributeCollection;
            } else if (index == 1) { // Item Definition Attributes.
                return category.ItemDefinitionAttributeCollection;
            } else { // Item Attributes.
                return category.ItemAttributeCollection;
            }
        }

        /// <summary>
        /// Shows the attributes that correspond to the tab at the specified index.
        /// </summary>
        /// <param name="index">The attribute tab index.</param>
        private void ShowRelationshipTab(int index)
        {
            var category = SelectedObject;

            var itemCategories = new ItemCategory[0];
            var itemDefinitions = new ItemDefinition[0];
            IList list;
            switch (index) {
                case 0: list = ListUtility.CreateArrayCopy(category.GetAllParents(ref itemCategories, false), itemCategories); break; //Ancestors
                case 1: list = ListUtility.CreateArrayCopy(category.GetAllChildren(ref itemCategories, false), itemCategories); break; //Descendants
                case 2: list = category.Elements; break; // Direct ItemDefinitions
                case 3: list = ListUtility.CreateArrayCopy(category.GetAllChildrenElements(ref itemDefinitions, true), itemDefinitions); break; // Inherit ItemDefinitions
                case 4: list = InventoryDatabaseUtility.DirectItemCategoryRecipes(m_InventoryMainWindow.Database.CraftingRecipes, category); break; // Direct Recipes
                case 5: list = InventoryDatabaseUtility.InheritedItemCategoryRecipes(m_InventoryMainWindow.Database.CraftingRecipes, category); break; // Inherit Recipes
                default: list = null; break;
            }
            m_RelationshipsReorderableList.Refresh(list);
        }

        /// <summary>
        /// The add button has been pressed.
        /// </summary>
        public override ItemCategory OnAdd(string name)
        {
            return ItemCategoryEditorUtility.AddItemCategory(name, m_InventoryMainWindow.Database, m_InventoryMainWindow.GetDatabaseDirectory());
        }

        /// <summary>
        /// The remove button has been pressed.
        /// </summary>
        /// <param name="index">The index of the selected object.</param>
        public override void OnRemove(int index)
        {
            // No more work needs to be performed if the category is empty.
            if (SelectedIndexOutOfRange) { return; }

            if (SelectedObject == DatabaseValidator.UncategorizedItemCategory) {
                EditorUtility.DisplayDialog("Cannot Be Removed",
                    "For validation reasons the uncategorized category cannot be removed from the database.",
                    "Okay");
                return;
            }

            var warningMessage = SelectedObject.Elements.Count == 0
                ? $"You are trying to remove the Item Category '{SelectedObject}'. This action cannot be undone.\n" +
                  $" Are you sure you would like to remove it?"
                : $"The Item Category '{SelectedObject}' you are trying to remove has direct Item Definitions.\n" +
                  $" Are you sure you would like to remove it?";

            var removeItemCategory = EditorUtility.DisplayDialog("Remove Item Category?",
                warningMessage,
                "Yes",
                "No");

            if (removeItemCategory) {
                ItemCategoryEditorUtility.RemoveItemCategory(SelectedObject, m_InventoryMainWindow.Database);
                OnSelected(-1);
            }
        }

        /// <summary>
        /// Returns a list of options for sorting.
        /// </summary>
        /// <returns>The list of sort options.</returns>
        public override IList<SortOption> GetSortOptions()
        {
            return ItemCategoryEditorUtility.SortOptions();
        }

        /// <summary>
        /// Search filter for the ItemCategory list
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="searchValue">The search string.</param>
        /// <returns>A new filtered list.</returns>
        public override IList<ItemCategory> GetSearchFilter(IList<ItemCategory> list, string searchValue)
        {
            return ItemCategoryEditorUtility.SearchFilter(list, searchValue);
        }
    }
}