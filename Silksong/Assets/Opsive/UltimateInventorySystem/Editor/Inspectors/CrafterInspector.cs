/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Crafting.Processors;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Utility;
    using System;
    using System.Collections.Generic;
    using Opsive.Shared.Utility;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom editor to display the category item actions.
    /// </summary>
    [CustomEditor(typeof(Crafter), true)]
    public class CrafterInspector : DatabaseInspectorBase
    {
        protected override List<string> ExcludedFields => new List<string>() { "m_CraftingProcessorData", "m_MiscellaneousRecipes", "m_CraftingCategories" };

        protected Crafter m_Crafter;
        protected CraftingCategoryReorderableList m_CraftingCategoryReorderableList;
        protected CraftingRecipeReorderableList m_CraftingRecipeReorderableList;

        protected FilterWindowPopupField m_TypeField;
        protected VisualElement m_ProcessorContainer;
        protected CraftingProcessor m_CraftingProcessor;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_Crafter = target as Crafter;

            if (m_Crafter.MiscellaneousRecipes == null) {
                m_Crafter.MiscellaneousRecipes = new CraftingRecipe[0];
                return;
            }

            if (m_Crafter.CraftingCategories == null) {
                m_Crafter.CraftingCategories = new CraftingCategory[0];
                return;
            }

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            if (Application.isPlaying == false) {
                m_Crafter.Deserialize();
            }

            m_CraftingCategoryReorderableList = new CraftingCategoryReorderableList(
                "Crafting Categories",
                m_Database,
                () => m_Crafter.CraftingCategories,
                (newValue) =>
                {
                    m_Crafter.CraftingCategories = newValue;
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_Crafter);
                });
            container.Add(m_CraftingCategoryReorderableList);

            m_CraftingRecipeReorderableList = new CraftingRecipeReorderableList(
                "Miscellaneous Recipes",
                m_Database,
                () => m_Crafter.MiscellaneousRecipes,
                (newValue) =>
                {
                    m_Crafter.MiscellaneousRecipes = newValue;
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_Crafter);
                });
            container.Add(m_CraftingRecipeReorderableList);

            //Crafting Processor.
            if (m_Crafter.Processor == null) {
                m_Crafter.Processor = new SimpleCraftingProcessorWithCurrency();
                ProcessorValueChanged();
            }

            m_CraftingProcessor = m_Crafter.Processor;

            m_TypeField = FilterWindowPopupField.CreateFilterWindowPopupField(typeof(CraftingProcessor),
                FilterWindow.FilterType.Class, "Processor Type", false, m_CraftingProcessor.GetType(),
                (type) =>
                {
                    var changeType = EditorUtility.DisplayDialog("Change Crafting Processor Type?",
                        $"You are trying to change the Type of the Crafting Processor. This action cannot be undone and some values may be lost.\n" +
                        $"Are you sure you would like to change the Type?",
                        "Yes",
                        "No");

                    if (!changeType) { return; }

                    var previous = m_CraftingProcessor;
                    m_CraftingProcessor = (CraftingProcessor)Activator.CreateInstance(type as Type);
                    ReflectionUtility.ObjectCopy(previous, m_CraftingProcessor);
                    m_TypeField.UpdateSelectedObject(type);

                    m_Crafter.Processor = m_CraftingProcessor;

                    ProcessorValueChanged();

                    DrawProcessorFields();

                });
            m_TypeField.label = "Processor";
            container.Add(m_TypeField);

            m_ProcessorContainer = new VisualElement();
            DrawProcessorFields();
            container.Add(m_ProcessorContainer);
        }

        private void DrawProcessorFields()
        {
            m_ProcessorContainer.Clear();
            FieldInspectorView.AddFields(m_Crafter.gameObject, m_CraftingProcessor, Shared.Utility.MemberVisibility.Public,
                m_ProcessorContainer, (object obj) => { ProcessorValueChanged(); }, null, true, null);
        }

        private void ProcessorValueChanged()
        {
            m_Crafter.Serialize();
            Shared.Editor.Utility.EditorUtility.SetDirty(m_Crafter);
        }
    }
}