/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes
{
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Crafting.IngredientsTypes;
    using System;
    using System.Reflection;
    using UnityEngine.UIElements;

    /// <summary>
    /// The control for crafting ingredients.
    /// </summary>
    [ControlType(typeof(CraftingIngredients))]
    public class CraftingIngredientsControl : CraftingInOutBaseControl
    {
        protected CraftingIngredients m_Ingredients;

        protected override string[] TabNames => new string[]
        {
            "Item Categories",
            "Item Definitions",
            "Items",
            "Others"
        };

        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that should have its fields displayed.</param>
        /// <param name="field">The field responsible for the control (can be null).</param>
        /// <param name="arrayIndex">The index of the object within the array (-1 indicates no array).</param>
        /// <param name="type">The type of control being retrieved.</param>
        /// <param name="value">The value of the control.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="userData">Optional data which can be used by the controls.</param>
        /// <returns>The created control.</returns>
        public override VisualElement GetControl(
            UnityEngine.Object unityObject,
            object target,
            FieldInfo field,
            int arrayIndex,
            System.Type type,
            object value,
            Func<object, bool> onChangeEvent,
            object userData)
        {
            var container = base.GetControl(unityObject, target, field, arrayIndex, type, value, onChangeEvent, userData);
            if (container == null) { return null; }

            m_Ingredients = value as CraftingIngredients;

            m_TabContents.Insert(m_TabContents.Count - 1, () => new ItemCategoryAmountsView(
                  m_Ingredients.ItemCategoryAmounts, m_Database,
                  (x) =>
                  {
                      m_OnChangeEvent?.Invoke(x);
                      return true;
                  }));

            m_TabContents.Insert(m_TabContents.Count - 1, () => new ItemDefinitionAmountsView(
                  m_Ingredients.ItemDefinitionAmounts, m_Database,
                  (x) =>
                  {
                      m_OnChangeEvent?.Invoke(x);
                      return true;
                  }));

            m_TabContents.Insert(m_TabContents.Count - 1, () => new ItemAmountsView(
                  m_Ingredients.ItemAmounts, m_Database,
                  (x) =>
                  {
                      m_OnChangeEvent?.Invoke(x);
                      return true;
                  }));

            HandleSelection(0);
            return container;
        }
    }

    /// <summary>
    /// The control for crafting ingredients with currency.
    /// </summary>
    [ControlType(typeof(CraftingIngredientsWithCurrency))]
    public class CraftingIngredientsWithCurrencyControl : CraftingIngredientsControl
    {

        protected override string[] TabNames => new string[]
        {
            "Item Categories",
            "Item Definitions",
            "Items",
            "Currencies",
            "Others"
        };

        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that should have its fields displayed.</param>
        /// <param name="field">The field responsible for the control (can be null).</param>
        /// <param name="arrayIndex">The index of the object within the array (-1 indicates no array).</param>
        /// <param name="type">The type of control being retrieved.</param>
        /// <param name="value">The value of the control.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="userData">Optional data which can be used by the controls.</param>
        /// <returns>The created control.</returns>
        public override VisualElement GetControl(
            UnityEngine.Object unityObject,
            object target,
            FieldInfo field,
            int arrayIndex,
            System.Type type,
            object value,
            Func<object, bool> onChangeEvent,
            object userData)
        {

            var container = base.GetControl(unityObject, target, field, arrayIndex, type, value, onChangeEvent, userData);
            if (container == null) { return null; }

            var ingredients = m_Ingredients as CraftingIngredientsWithCurrency;
            if (ingredients == null) {
                return container;
            }

            m_TabContents.Insert(m_TabContents.Count - 1, () => new CurrencyAmountsView(
                  ingredients.CurrencyAmounts, m_Database,
                  (x) =>
                  {
                      m_OnChangeEvent?.Invoke(x);
                      return true;
                  }));

            HandleSelection(0);
            return container;
        }
    }
}