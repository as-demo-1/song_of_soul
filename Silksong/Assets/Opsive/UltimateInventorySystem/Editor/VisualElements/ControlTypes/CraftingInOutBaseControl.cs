/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls.Types;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The crafting input and output base control.
    /// </summary>
    public abstract class CraftingInOutBaseControl : TypeControlBase
    {
        protected VisualElement m_TabContent;
        protected Func<object, bool> m_OnChangeEvent;
        protected List<Func<VisualElement>> m_TabContents;
        protected VisualElement m_OtherContent;
        protected InventorySystemDatabase m_Database;

        protected abstract string[] TabNames { get; }
        protected List<Func<VisualElement>> TabContents => m_TabContents;

        public override bool UseLabel => false;

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
            if (userData is object[] objArray) {
                for (int i = 0; i < objArray.Length; i++) {
                    if (objArray[i] is bool b) {
                        if (b == false) { return null; }
                    }
                    if (objArray[i] is InventorySystemDatabase database) { m_Database = database; }
                }
            } else if (userData is bool b) {
                if (b == false) { return null; }
            }

            if (m_Database == null) {
                Debug.LogWarning("Database is null in custom control type.");
            }

            var container = new VisualElement();

            m_OnChangeEvent = onChangeEvent;

            container.name = "box";
            container.AddToClassList(ManagerStyles.BoxBackground);

            var tabToolbar = new TabToolbar(TabNames, 0, HandleSelection);
            container.Add(tabToolbar);

            m_TabContent = new VisualElement();
            m_OtherContent = new VisualElement();
            container.Add(m_TabContent);

            m_TabContents = new List<Func<VisualElement>>();
            m_TabContents.Add(() =>
            {
                m_OtherContent.Clear();
                FieldInspectorView.AddFields(unityObject,
                    target, MemberVisibility.Public, m_OtherContent,
                    (object obj) =>
                    {
                        onChangeEvent?.Invoke(obj);
                    }, null, true, null, false, null, new object[] { false, m_Database });
                return m_OtherContent;
            });

            tabToolbar.Selected = 0;
            HandleSelection(tabToolbar.Selected);

            return container;
        }

        /// <summary>
        /// Show the correct tab content.
        /// </summary>
        /// <param name="index">The tab index.</param>
        protected void HandleSelection(int index)
        {
            m_TabContent.Clear();
            if (index < 0 || index >= m_TabContents.Count) { return; }

            var newContent = TabContents[index].Invoke();
            m_TabContent.Add(newContent);
        }
    }
}