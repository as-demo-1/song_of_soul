/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The crafting category manager.
    /// </summary>
    [OrderedEditorItem("Crafting Categories", 40)]
    [RequireDatabase]
    [System.Serializable]
    public class CraftingCategoryManager : InventorySystemObjectBaseManager<CraftingCategory>
    {
        private Toggle m_IsAbstract;
        private ColorField m_Color;
        private UnityObjectFieldWithPreview m_IconField;
        private VisualElement m_RecipeTypeContainer;
        private FilterWindowPopupField m_RecipeType;
        private ReorderableList m_ParentCategoriesReorderableList;
        private TabToolbar m_RelationshipsTabToolbar;
        private ReorderableList m_RelationshipsReorderableList;

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            base.BuildVisualElements();

            m_IsAbstract = new Toggle("Abstract");
            m_IsAbstract.tooltip =
                "An abstract category cannot have direct crafting recipes. And therefore will not be shown in the dropdown when selecting a category for a recipe.";
            m_IsAbstract.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                var setAbstract = (SelectedObject.IsAbstract || SelectedObject.Elements.Count == 0) ||
                                  EditorUtility.DisplayDialog("Set the Crafting Category as Abstract?",
                                      $"The Crafting Category '{SelectedObject}' you are trying to set Abstract has direct Crafting Recipes." +
                                      $" Setting it abstract will set all direct Crafting Recipes to a child or null category.\n" +
                                      $"Are you sure you would like to set it to Abstract",
                                      "Yes",
                                      "Cancel");

                if (setAbstract) {
                    CraftingCategoryEditorUtility.SetIsAbstract(SelectedObject, evt.newValue);
                    m_RecipeTypeContainer.Clear();
                    if (SelectedObject.IsAbstract == false) {
                        m_RecipeTypeContainer.Add(m_RecipeType);
                    }
                }

            });
            m_ContentPanel.Add(m_IsAbstract);

            m_RecipeTypeContainer = new VisualElement();
            m_ContentPanel.Add(m_RecipeTypeContainer);

            m_RecipeType = FilterWindowPopupField.CreateFilterWindowPopupField(typeof(CraftingRecipe), FilterWindow.FilterType.Class, "Crafting Recipe", false, SelectedObject?.RecipeType,
                (type) =>
                {
                    var changeType = SelectedObject.Elements.Count == 0 || EditorUtility.DisplayDialog("Change Crafting Category?",
                                         $"You are trying to change the Recipe Type of the Crafting Category '{SelectedObject}'. This action cannot be undone and some values may be lost.\n" +
                                         $"Are you sure you would like to change the recipeType?",
                                         "Yes",
                                         "Cancel");

                    if (changeType) {
                        CraftingCategoryEditorUtility.SetRecipeType(SelectedObject, type as Type);
                        m_RecipeType.UpdateSelectedObject(type);
                    }

                });
            m_RecipeType.label = "Recipe Type";
            m_RecipeType.tooltip =
                "The recipe type defines the class type of its recipe, any class inheriting the CraftingRecipe class will show up in the dropdown.";
            m_RecipeTypeContainer.Add(m_RecipeType);

            m_Color = new ColorField("Editor Color");
            m_Color.tooltip = "The color of the box next to the name of the category.";
            m_Color.RegisterCallback<ChangeEvent<Color>>(evt =>
            {
                CraftingCategoryEditorUtility.SetColor(SelectedObject, evt.newValue);
                Refresh();
            });
            m_ContentPanel.Add(m_Color);

            m_IconField = new UnityObjectFieldWithPreview();
            m_IconField.label = "Editor Icon";
            m_IconField.tooltip = "The icon used in the custom editors, displayed next to the object name.";
            m_IconField.objectType = typeof(Sprite);
            m_IconField.RegisterValueChangedCallback(evt =>
            {
                CraftingCategoryEditorUtility.SetIcon(SelectedObject, evt.newValue as Sprite);
                Refresh();
            });
            m_ContentPanel.Add(m_IconField);

            var craftingCategoryParentPicker = new CraftingCategoryField("", m_InventoryMainWindow.Database,
                new (string, Action<CraftingCategory>)[]
                {
                    ("Add Parent", c =>
                    {
                        CraftingCategoryEditorUtility.AddCraftingCategoryParent(SelectedObject, c);
                        Refresh();
                    })
                },
                (cat) => SelectedObject?.AddParentCondition(cat) ?? false);
            craftingCategoryParentPicker.IncludeNullOption = false;
            m_ParentCategoriesReorderableList = new ReorderableList(null, (VisualElement parent, int index) =>
                {
                    parent.Add(new CraftingCategoryViewName());
                },
                (VisualElement parent, int index) =>
                {
                    var craftingCategoryViewName = parent.ElementAt(0) as CraftingCategoryViewName;
                    var category = m_ParentCategoriesReorderableList.ItemsSource[index] as CraftingCategory;

                    craftingCategoryViewName.Refresh(category);
                },
                (VisualElement header) =>
                {
                    header.Add(new Label("Parents"));
                }, null,
                () =>
                {
                    craftingCategoryParentPicker.SearchableListWindow.OpenPopUpWindow(
                        m_ParentCategoriesReorderableList.AddButton.worldBound.position,
                        m_ParentCategoriesReorderableList.AddButton.worldBound.size);
                },
                (int index) =>
                {
                    CraftingCategoryEditorUtility.RemoveCraftingCategoryParent(
                        SelectedObject,
                        m_ParentCategoriesReorderableList.ItemsSource[index] as CraftingCategory);
                    Refresh();
                }, null);
            m_ContentPanel.Add(m_ParentCategoriesReorderableList);

            var relationshipBox = new VisualElement();
            relationshipBox.name = "box";
            relationshipBox.AddToClassList(ManagerStyles.BoxBackground);

            m_RelationshipsTabToolbar = new TabToolbar(new string[]
            {
                "Ancestors",
                "Descendants",
                "Direct Recipes",
                "Inherited Recipes",
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
        /// Check if the name is unique.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>True if unique.</returns>
        public override bool IsObjectNameValidAndUnique(string name)
        {
            for (int i = 0; i < m_InventoryMainWindow.Database.CraftingCategories.Length; ++i) {
                if (m_InventoryMainWindow.Database.CraftingCategories[i].name == name) {
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
            var categories = m_InventoryMainWindow.Database?.CraftingCategories;
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
        public override IList<CraftingCategory> GetList()
        {
            return m_InventoryMainWindow.Database?.CraftingCategories;
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
            var viewName = action.userData as CraftingCategoryViewName;
            var objectToDuplicate = viewName?.CraftingCategory;

            var newObject = CraftingCategoryEditorUtility.DuplicateCraftingCategory(objectToDuplicate,
                m_InventoryMainWindow.Database,
                m_InventoryMainWindow.GetDatabaseDirectory());

            Refresh();
            Select(newObject);
        }

        /// <summary>
        /// Creates the new ReorderableList item.
        /// </summary>
        /// <param name="parent">The parent ReorderableList item.</param>
        /// <param name="index">The index of the item.</param>
        public override void MakeListItem(VisualElement parent, int index)
        {
            var viewName = new CraftingCategoryViewName();
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
            var categoryViewName = parent.ElementAt(0) as CraftingCategoryViewName;
            if (index < 0 || index >= m_ListPanel.SearchableList.ItemList.Count) { return; }
            var category = m_ListPanel.SearchableList.ItemList[index];
            categoryViewName.Refresh(category);
        }

        /// <summary>
        /// Update the visual elements to reflect the specified category.
        /// </summary>
        /// <param name="craftingCategory">The category that is being displayed.</param>
        protected override void UpdateElements(CraftingCategory craftingCategory)
        {
            m_Name.SetValueWithoutNotify(craftingCategory.name);
            m_IsAbstract.SetValueWithoutNotify(craftingCategory.IsAbstract);
            m_Color.SetValueWithoutNotify(craftingCategory.m_Color);
            m_IconField.SetValueWithoutNotify(craftingCategory.m_EditorIcon);
            m_RecipeType.UpdateSelectedObject(craftingCategory.RecipeType);

            m_RecipeTypeContainer.Clear();
            if (craftingCategory.IsAbstract == false) {
                m_RecipeTypeContainer.Add(m_RecipeType);
            }

            craftingCategory.Initialize(false);
            m_ParentCategoriesReorderableList.Refresh(craftingCategory.Parents);

            if (m_RelationshipsTabToolbar.Selected < 0) { m_RelationshipsTabToolbar.Selected = 0; }
            ShowRelationshipTab(m_RelationshipsTabToolbar.Selected);
        }

        /// <summary>
        /// Shows the attributes that correspond to the tab at the specified index.
        /// </summary>
        /// <param name="index">The attribute tab index.</param>
        private void ShowRelationshipTab(int index)
        {
            var category = SelectedObject;

            var craftingCategories = new CraftingCategory[0];
            var craftingRecipes = new CraftingRecipe[0];
            IList list;
            switch (index) {
                case 0: list = Shared.Utility.ListUtility.CreateArrayCopy(category.GetAllParents(ref craftingCategories, false), craftingCategories); break; //Ancestors
                case 1: list = Shared.Utility.ListUtility.CreateArrayCopy(category.GetAllChildren(ref craftingCategories, false), craftingCategories); break; //Descendants
                case 2: list = category.Elements; break; // Direct Recipes
                case 3: list = Shared.Utility.ListUtility.CreateArrayCopy(category.GetAllChildrenElements(ref craftingRecipes, true), craftingRecipes); break; // Inherit Recipes
                default: list = null; break;
            }
            m_RelationshipsReorderableList.Refresh(list);
        }

        /// <summary>
        /// The add button has been pressed.
        /// </summary>
        public override CraftingCategory OnAdd(string name)
        {
            return CraftingCategoryEditorUtility.AddCraftingCategory(name, m_InventoryMainWindow.Database, m_InventoryMainWindow.GetDatabaseDirectory());
        }

        /// <summary>
        /// The remove button has been pressed.
        /// </summary>
        /// <param name="index">The index of the selected object.</param>
        public override void OnRemove(int index)
        {
            // No more work needs to be performed if the category is empty.
            if (SelectedIndexOutOfRange) { return; }

            if (SelectedObject == DatabaseValidator.UncategorizedCraftingCategory) {
                EditorUtility.DisplayDialog("Cannot Be Removed",
                    "For validation reasons the uncategorized category cannot be removed from the database.",
                    "Okay");
                return;
            }

            var warningMessage = SelectedObject.Elements.Count == 0
                ? $"You are trying to remove the Crafting Category '{SelectedObject}'. This action cannot be undone.\n" +
                  $" Are you sure you would like to remove it?"
                : $"The Crafting Category '{SelectedObject}' you are trying to remove has direct Crafting Recipes.\n" +
                  $" Are you sure you would like to remove it?";

            var removeCategory = EditorUtility.DisplayDialog("Remove Crafting Category?",
                warningMessage,
                "Yes",
                "No");

            if (removeCategory) {
                CraftingCategoryEditorUtility.RemoveCraftingCategory(SelectedObject, m_InventoryMainWindow.Database);
            }
        }

        /// <summary>
        /// Search filter for the Category list
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="searchValue">The search string.</param>
        /// <returns>A new filtered list.</returns>
        public override IList<CraftingCategory> GetSearchFilter(IList<CraftingCategory> list, string searchValue)
        {
            return CraftingCategoryEditorUtility.SearchFilter(list, searchValue);
        }

        /// <summary>
        /// Returns a list of options for sorting.
        /// </summary>
        /// <returns>The list of sort options.</returns>
        public override IList<SortOption> GetSortOptions()
        {
            return CraftingCategoryEditorUtility.SortOptions();
        }
    }
}