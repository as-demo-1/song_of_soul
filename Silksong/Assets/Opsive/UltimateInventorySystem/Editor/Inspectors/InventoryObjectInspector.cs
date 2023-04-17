/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Editor.Managers;
    using Opsive.UltimateInventorySystem.Exchange;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// The base inspector for the Inventory Objects. 
    /// </summary>
    public class InventoryObjectInspector : InspectorBase
    {
        /// <summary>
        /// Do not draw the basic visual elements.
        /// </summary>
        /// <param name="parent">The parent container.</param>
        /// <param name="nested">Is the inspector nested?.</param>
        public override void DrawElements(VisualElement parent, bool nested = false)
        {
            ShowFooterElements(parent);
        }

        /// <summary>
        /// Creates the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            var button = new Button();
            button.clickable.clicked += () =>
            {
                InventoryMainWindow.ShowWindow();
                InventoryMainWindow.NavigateTo(target);
            };
            button.text = $"Open Inventory Manager";
            container.Add(button);
        }
    }

    /// <summary>
    /// The item category inspector.
    /// </summary>
    [CustomEditor(typeof(ItemCategory))]
    public class ItemCategoryInspector : InventoryObjectInspector { }

    /// <summary>
    /// The item definition inspector.
    /// </summary>
    [CustomEditor(typeof(ItemDefinition))]
    public class ItemDefinitionInspector : InventoryObjectInspector { }

    /// <summary>
    /// The crafting category inspector.
    /// </summary>
    [CustomEditor(typeof(CraftingCategory))]
    public class CraftingCategoryInspector : InventoryObjectInspector { }

    /// <summary>
    /// The crafting recipe inspector.
    /// </summary>
    [CustomEditor(typeof(CraftingRecipe))]
    public class CraftingRecipeInspector : InventoryObjectInspector { }

    /// <summary>
    /// The currency inspector.
    /// </summary>
    [CustomEditor(typeof(Currency))]
    public class CurrencyInspector : InventoryObjectInspector { }
}
