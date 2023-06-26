/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.Shared.Editor.UIElements.Controls.Types;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Reflection;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    /// <summary>
    /// The control type for an item.
    /// </summary>
    [ControlType(typeof(Item))]
    public class ItemControl : TypeControlBase
    {
        protected InventorySystemDatabase m_Database;
        protected VisualElement m_ItemFieldContainer;
        protected ItemField m_ItemField;

        public override bool UseLabel => true;

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
        public override VisualElement GetControl(Object unityObject, object target, FieldInfo field, int arrayIndex, Type type, object value,
            Func<object, bool> onChangeEvent, object userData)
        {
            if (userData is object[] objArray) {
                for (int i = 0; i < objArray.Length; i++) {
                    if (objArray[i] is bool b) {
                        if (b == false) { return null; }
                    }
                    if (objArray[i] is InventorySystemDatabase database) { m_Database = database; }
                }
            } else if (userData is InventorySystemDatabase database) {
                m_Database = database;
            }

            m_ItemFieldContainer = new VisualElement();

            m_ItemField = new ItemField(m_Database);
            m_ItemField.Refresh(value as Item);
            // Ensure the control is kept up to date as the value changes.
            if (field != null) {
                Action<object> onBindingUpdateEvent = (object newValue) => m_ItemField.Refresh(newValue as Item);
                m_ItemField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(field, arrayIndex, target, onBindingUpdateEvent);
                });
                m_ItemField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
            }
            m_ItemField.OnValueChanged += () =>
            {
                if (!(onChangeEvent?.Invoke(m_ItemField.Value) ?? false)) {
                    m_ItemField.Refresh(value as Item);
                }
            };
            return m_ItemFieldContainer;
        }
    }
}