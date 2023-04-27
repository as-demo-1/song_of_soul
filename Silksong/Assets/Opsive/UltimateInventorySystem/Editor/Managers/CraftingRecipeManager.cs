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
    using Opsive.UltimateInventorySystem.Crafting.IngredientsTypes;
    using Opsive.UltimateInventorySystem.Crafting.RecipeTypes;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using Opsive.Shared.Utility;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The crafting recipe manager.
    /// </summary>
    [OrderedEditorItem("Crafting Recipes", 50)]
    [RequireDatabase]
    [System.Serializable]
    public class CraftingRecipeManager : InventorySystemObjectBaseManager<CraftingRecipe>
    {
        protected CraftingCategoryField m_CategoryField;
        protected UnityObjectFieldWithPreview m_IconField;
        protected TabToolbar m_RecipeInOutTabToolbar;
        protected VisualElement m_RecipeInOutTabContent;
        protected VisualElement m_Ingredients;
        protected VisualElement m_DefaultOutputs;
        protected ScriptableObjectInspectorViewWithDatabase m_Other;
        protected CraftingRecipeView m_CraftingRecipeView;
        protected TabToolbar m_RelationshipsTabToolbar;
        protected ReorderableList m_RelationshipsReorderableList;

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            var list = GetList();
            if (list != null) {
                for (int i = 0; i < list.Count; i++) { list[i].Initialize(false); }
            }

            base.BuildVisualElements();

            m_CategoryField = new CraftingCategoryField(
                "Category",
                m_InventoryMainWindow.Database,
                new (string, Action<CraftingCategory>)[]
                {
                    ("Set category", c =>
                    {
                        var setCategory = c.RecipeType == SelectedObject.GetType() || EditorUtility.DisplayDialog("Set new Crafting Category?",
                                             $"You are trying to set a new category which does not have the same Recipe Type as this recipe. This action cannot be undone and some values may be lost.\n" +
                                             $"Are you sure you would like to change the recipeType?",
                                             "Yes",
                                             "Cancel");

                        if (setCategory) {
                            var newRecipe = CraftingRecipeEditorUtility.SetCraftingRecipeCategory(SelectedObject, c);
                            m_ListPanel.SearchableList.Refresh(GetList());
                            m_ListPanel.SearchableList.SelectObject(newRecipe);
                        }
                    }),
                },
                (cat) => cat.IsAbstract == false && cat != SelectedObject?.Category);

            m_CategoryField.OnClose += () =>
            {
                Refresh();
            };
            m_CategoryField.tooltip = "The crafting category will define the recipes type.";
            m_ContentPanel.Add(m_CategoryField);

            m_IconField = new UnityObjectFieldWithPreview();
            m_IconField.label = "Editor Icon";
            m_IconField.tooltip = "The icon used in the custom editors, displayed next to the object name.";
            m_IconField.objectType = typeof(Sprite);
            m_IconField.RegisterValueChangedCallback(evt =>
            {
                CraftingRecipeEditorUtility.SetIcon(SelectedObject, evt.newValue as Sprite);
                Refresh();
            });
            m_ContentPanel.Add(m_IconField);

            var recipeContentBox = new VisualElement();
            recipeContentBox.name = "box";
            recipeContentBox.AddToClassList(ManagerStyles.BoxBackground);

            m_RecipeInOutTabToolbar = new TabToolbar(new string[]
            {
                "Ingredients",
                "Outputs",
                "Other"
            }, 0, ShowRecipeInOutTab);
            recipeContentBox.Add(m_RecipeInOutTabToolbar);
            m_RecipeInOutTabContent = new VisualElement();
            recipeContentBox.Add(m_RecipeInOutTabContent);

            var inputOutputBox = new VisualElement();
            inputOutputBox.name = "box";
            inputOutputBox.AddToClassList(ManagerStyles.BoxBackground);
            inputOutputBox.style.marginTop = 5;

            m_Ingredients = new VisualElement();
            m_DefaultOutputs = new VisualElement();
            m_Other = new ScriptableObjectInspectorViewWithDatabase(m_InventoryMainWindow.Database);
            m_Other.OnValueChanged += () =>
            {
                m_CraftingRecipeView.Refresh();
                CraftingRecipeEditorUtility.SetCraftingRecipeDirty(SelectedObject, true);
            };
            m_ContentPanel.Add(recipeContentBox);

            m_CraftingRecipeView = new CraftingRecipeView();
            inputOutputBox.Add(m_CraftingRecipeView);
            m_ContentPanel.Add(inputOutputBox);

            var relationshipBox = new VisualElement();
            relationshipBox.name = "box";
            relationshipBox.AddToClassList(ManagerStyles.BoxBackground);
            relationshipBox.style.marginTop = 5;

            m_RelationshipsTabToolbar = new TabToolbar(new string[]
            {
                "Crafting Categories"
            }, 0, ShowRelationshipTab);
            relationshipBox.Add(m_RelationshipsTabToolbar);

            m_RelationshipsReorderableList = new ReorderableList(null, (VisualElement parent, int index) =>
            {

            }, (VisualElement parent, int index) =>
            {
                var viewName = parent.childCount == 0 ? null : parent.ElementAt(0) as ViewName;
                var obj = m_RelationshipsReorderableList.ItemsSource[index];
                if (obj == null) {
                    Debug.LogError("A Relation Object is null.");
                    return;
                }

                if (viewName == null || viewName.GetType().GetProperty("m_Object")?.PropertyType != obj.GetType()) {
                    viewName = ManagerUtility.CreateViewNameFor(obj);
                    parent.Clear();
                    parent.Add(viewName);
                }

                viewName.Refresh(obj);
            }, null, null, null, null, null);
            relationshipBox.Add(m_RelationshipsReorderableList);
            m_ContentPanel.Add(relationshipBox);

            m_ListPanel.Refresh(-1);
        }

        /// <summary>
        /// Show the recipe Ingredient or outputs
        /// </summary>
        /// <param name="index">The index.</param>
        private void ShowRecipeInOutTab(int index)
        {
            m_RecipeInOutTabContent.Clear();
            if (index == 0) {
                m_RecipeInOutTabContent.Add(m_Ingredients);
            } else if (index == 1) {
                m_RecipeInOutTabContent.Add(m_DefaultOutputs);
            } else {
                m_RecipeInOutTabContent.Add(m_Other);
            }
        }

        /// <summary>
        /// Shows the crafting categories that correspond to the tab at the specified index.
        /// </summary>
        /// <param name="index">The attribute tab index.</param>
        private void ShowRelationshipTab(int index)
        {
            var recipe = SelectedObject;

            if (recipe == null) {
                m_RelationshipsReorderableList.Refresh(null);
                return;
            }

            var craftingCategories = new CraftingCategory[0];
            IList list;
            switch (index) {
                case 0: list = Shared.Utility.ListUtility.CreateArrayCopy(recipe.Category.GetAllParents(ref craftingCategories, true), craftingCategories); break; // Crafting Categories.
                default: list = null; break;
            }

            m_RelationshipsReorderableList.Refresh(list);
        }

        /// <summary>
        /// Returns true if the definition name is unique.
        /// </summary>
        /// <param name="name">The possible name of the definition.</param>
        /// <returns>True if the definition name is unique.</returns>
        public override bool IsObjectNameValidAndUnique(string name)
        {
            for (int i = 0; i < m_InventoryMainWindow.Database.CraftingRecipes.Length; ++i) {
                if (m_InventoryMainWindow.Database.CraftingRecipes[i].name == name) {
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
            var recipes = m_InventoryMainWindow.Database?.CraftingRecipes;
            m_Other.Database = m_InventoryMainWindow.Database;
            if (recipes == null) {
                return;
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
        public override IList<CraftingRecipe> GetList()
        {
            return m_InventoryMainWindow.Database?.CraftingRecipes;
        }

        /// <summary>
        /// Returns a list of options for sorting.
        /// </summary>
        /// <returns>The list of sort options.</returns>
        public override IList<SortOption> GetSortOptions()
        {
            return CraftingRecipeEditorUtility.SortOptions();
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
            var viewName = action.userData as CraftingRecipeViewName;
            var objectToDuplicate = viewName?.CraftingRecipe;

            var newObject = CraftingRecipeEditorUtility.DuplicateCraftingRecipe(objectToDuplicate,
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
            var viewName = new CraftingRecipeViewName();
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
            var craftingRecipeViewName = parent.ElementAt(0) as CraftingRecipeViewName;
            if (index < 0 || index >= m_ListPanel.SearchableList.ItemList.Count) { return; }
            var recipe = m_ListPanel.SearchableList.ItemList[index];
            craftingRecipeViewName.Refresh(recipe);
        }

        /// <summary>
        /// Update the visual elements to reflect the specified category.
        /// </summary>
        /// <param name="recipe">The recipe that is being displayed.</param>
        protected override void UpdateElements(CraftingRecipe recipe)
        {
            if (recipe == null) { return; }
            recipe.Initialize(false);

            m_Name.SetValueWithoutNotify(recipe.name);
            m_CategoryField.Refresh(recipe.Category);
            m_IconField.SetValueWithoutNotify(recipe.m_EditorIcon);

            m_Ingredients.Clear();
            var fieldInfo = recipe.GetType().GetField("m_Ingredients", BindingFlags.NonPublic | BindingFlags.Default | BindingFlags.Instance);
            var ingredientOverrideAttribute = recipe.GetType().GetCustomAttribute<OverrideCraftingIngredients>();
            var ingredientType = ingredientOverrideAttribute?.m_OverrideType ?? recipe.Ingredients.GetType();
            if (recipe.Ingredients.GetType() != ingredientType) {
                var previous = recipe.Ingredients;
                recipe.Ingredients = Activator.CreateInstance(ingredientType) as CraftingIngredients;
                ReflectionUtility.ObjectCopy(previous, recipe.Ingredients);
            }
            FieldInspectorView.AddField(
                recipe,
                recipe.Ingredients, fieldInfo, -1, ingredientType,
                "Ingredients", string.Empty, true,
                recipe.Ingredients,
                m_Ingredients,
                (object obj) =>
                {
                    m_CraftingRecipeView.Refresh();
                    CraftingRecipeEditorUtility.SetCraftingRecipeDirty(SelectedObject, true);
                }, null, false, null, null, new object[] { true, m_InventoryMainWindow.Database });

            m_DefaultOutputs.Clear();
            fieldInfo = recipe.GetType().GetField("m_DefaultOutput", BindingFlags.NonPublic | BindingFlags.Default | BindingFlags.Instance);
            var outputOverrideAttribute = recipe.GetType().GetCustomAttribute<OverrideCraftingOutput>();
            var outputType = outputOverrideAttribute?.m_OverrideType ?? recipe.DefaultOutput.GetType();
            if (recipe.DefaultOutput.GetType() != outputType) {
                var previous = recipe.DefaultOutput;
                recipe.DefaultOutput = Activator.CreateInstance(outputType) as CraftingOutput;
                ReflectionUtility.ObjectCopy(previous, recipe.DefaultOutput);
            }
            FieldInspectorView.AddField(
                recipe,
                recipe.DefaultOutput, fieldInfo, -1, outputType,
                "Outputs", string.Empty, true,
                recipe.DefaultOutput,
                m_DefaultOutputs,
                (object obj) =>
                {
                    m_CraftingRecipeView.Refresh();
                    CraftingRecipeEditorUtility.SetCraftingRecipeDirty(SelectedObject, true);
                }, null, false, null, null, new object[] { true, m_InventoryMainWindow.Database });

            m_Other.Refresh(recipe);

            m_CraftingRecipeView.Refresh(recipe);

            if (m_RecipeInOutTabToolbar.Selected < 0) { m_RecipeInOutTabToolbar.Selected = 0; }
            ShowRecipeInOutTab(m_RecipeInOutTabToolbar.Selected);
            if (m_RelationshipsTabToolbar.Selected < 0) { m_RelationshipsTabToolbar.Selected = 0; }
            ShowRelationshipTab(m_RelationshipsTabToolbar.Selected);
        }

        /// <summary>
        /// The add button has been pressed.
        /// </summary>
        public override CraftingRecipe OnAdd(string name)
        {
            return CraftingRecipeEditorUtility.AddCraftingRecipe(name,
                DatabaseValidator.UncategorizedCraftingCategory,
                m_InventoryMainWindow.Database,
                m_InventoryMainWindow.GetDatabaseDirectory());
        }

        /// <summary>
        /// The remove button has been pressed.
        /// </summary>
        /// <param name="index">The index of the selected object.</param>
        public override void OnRemove(int index)
        {
            // No more work needs to be performed if the object is empty.
            if (SelectedIndexOutOfRange) { return; }

            if (EditorUtility.DisplayDialog("Remove Crafting Recipe?",
                $"Are you sure you want to remove the Crafting Recipe '{SelectedObject}'? This action cannot be undone.",
                "Yes", "No")) {
                CraftingRecipeEditorUtility.RemoveCraftingRecipe(SelectedObject, m_InventoryMainWindow.Database);
                OnSelected(-1);
            }
        }

        /// <summary>
        /// Search filter for the ItemDefinition list
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="searchValue">The search string.</param>
        /// <returns>A new filtered list.</returns>
        public override IList<CraftingRecipe> GetSearchFilter(IList<CraftingRecipe> list, string searchValue)
        {
            return CraftingRecipeEditorUtility.SearchFilter(list, searchValue);
        }
    }

    /// <summary>
    /// The view for a crafting recipe.
    /// </summary>
    public class CraftingRecipeView : VisualElement
    {
        protected CraftingRecipe m_Recipe;

        protected ReorderableList m_Ingredients;
        protected Label m_Label;
        protected ReorderableList m_Outputs;

        /// <summary>
        /// Create the fields for a crafting recipe.
        /// </summary>
        public CraftingRecipeView()
        {
            AddToClassList(InventoryManagerStyles.CraftingRecipeView);

            m_Ingredients = new ReorderableList(null, (VisualElement parent, int index) =>
            {
            }, (VisualElement parent, int index) =>
            {
                var viewName = parent.childCount == 0 ? null : parent.ElementAt(0) as ViewName;
                var obj = m_Ingredients.ItemsSource[index];
                if (obj == null) {
                    Debug.LogError("A Relation Object is null.");
                    return;
                }

                if (viewName == null || viewName.GetType().GetProperty("m_Object")?.PropertyType != obj.GetType()) {
                    viewName = ManagerUtility.CreateViewNameFor(obj);
                    viewName.SetShowType(true);
                    parent.Clear();
                    parent.Add(viewName);
                }

                viewName.Refresh(obj);
            }, (parent) =>
            {
                parent.Add(new Label("Ingredients"));
            }, null, null, null, null);
            m_Ingredients.HighlightSelectedItem = false;
            Add(m_Ingredients);

            m_Label = new Label("=>");
            Add(m_Label);

            m_Outputs = new ReorderableList(null, (VisualElement parent, int index) =>
            {
            }, (VisualElement parent, int index) =>
            {
                var viewName = parent.childCount == 0 ? null : parent.ElementAt(0) as ViewName;
                var obj = m_Outputs.ItemsSource[index];
                if (obj == null) {
                    Debug.LogError("A Relation Object is null.");
                    return;
                }

                if (viewName == null || viewName.GetType().GetProperty("m_Object")?.PropertyType != obj.GetType()) {
                    viewName = ManagerUtility.CreateViewNameFor(obj);
                    viewName.SetShowType(true);
                    parent.Clear();
                    parent.Add(viewName);
                }

                viewName.Refresh(obj);
            }, (parent) =>
            {
                parent.Add(new Label("Output"));
            }, null, null, null, null);
            m_Outputs.HighlightSelectedItem = false;
            Add(m_Outputs);
        }

        /// <summary>
        /// Change the crafting recipe to view.
        /// </summary>
        /// <param name="recipe">The new recipe to view.</param>
        public void Refresh(CraftingRecipe recipe)
        {
            m_Recipe = recipe;
            Refresh();
        }

        /// <summary>
        /// Redraw the fields.
        /// </summary>
        public void Refresh()
        {
            if (m_Recipe == null) {
                m_Ingredients.Refresh(null);
                m_Outputs.Refresh(null);
                return;
            }
            var ingredients = new List<object>();
            for (int i = 0; i < m_Recipe.Ingredients.ItemCategoryAmounts.Count; i++) {
                ingredients.Add(m_Recipe.Ingredients.ItemCategoryAmounts[i]);
            }
            for (int i = 0; i < m_Recipe.Ingredients.ItemDefinitionAmounts.Count; i++) {
                ingredients.Add(m_Recipe.Ingredients.ItemDefinitionAmounts[i]);
            }
            for (int i = 0; i < m_Recipe.Ingredients.ItemAmounts.Count; i++) {
                ingredients.Add(m_Recipe.Ingredients.ItemAmounts[i]);
            }

            if (m_Recipe.Ingredients is CraftingIngredientsWithCurrency ingredientsWithCurrency) {
                for (int i = 0; i < ingredientsWithCurrency.CurrencyAmounts.Count; i++) {
                    ingredients.Add(ingredientsWithCurrency.CurrencyAmounts[i]);
                }
            }
            m_Ingredients.Refresh(ingredients);

            var outputs = new List<object>();
            for (int i = 0; i < m_Recipe.DefaultOutput.ItemAmounts.Count; i++) {
                outputs.Add(m_Recipe.DefaultOutput.ItemAmounts[i]);
            }

            m_Outputs.Refresh(outputs);
        }
    }
}