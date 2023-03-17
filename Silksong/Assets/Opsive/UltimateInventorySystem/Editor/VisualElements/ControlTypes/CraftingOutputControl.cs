/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes
{
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateInventorySystem.Crafting;
    using System;
    using System.Reflection;
    using UnityEngine.UIElements;

    /// <summary>
    /// The control for crafting output control.
    /// </summary>
    [ControlType(typeof(CraftingOutput))]
    public class CraftingOutputControl : CraftingInOutBaseControl
    {
        protected CraftingOutput m_Output;

        protected override string[] TabNames => new string[]
        {
            "Outputs",
            "Other"
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

            m_Output = value as CraftingOutput;

            m_TabContents.Insert(m_TabContents.Count - 1, () =>
              {
                  return new ItemAmountsView(
                      m_Output.ItemAmounts, m_Database,
                      (x) =>
                      {
                          m_OnChangeEvent?.Invoke(x);
                          return true;
                      });
              });

            HandleSelection(0);
            return container;
        }
    }
}