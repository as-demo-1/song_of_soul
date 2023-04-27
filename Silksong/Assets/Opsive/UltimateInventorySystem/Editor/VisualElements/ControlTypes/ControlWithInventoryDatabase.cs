/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes
{
    using Opsive.Shared.Editor.UIElements.Controls.Types;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Reflection;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Editor.Managers;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    /// <summary>
    /// The base control for inventory object amounts.
    /// </summary>
    public abstract class ControlWithInventoryDatabase : TypeControlBase
    {
        protected InventorySystemDatabase m_Database;
        protected bool m_BoolUserData = true;

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
            ExtractUserDataAndUseCustomControl(userData);

            var container = new VisualElement();
            if (field == null) {
                var text = value == null ? "NULL" : value.ToString();
                container.Add(new Label(text));
                return container;
            }

            if (m_Database == null) {
                Debug.LogWarning($"The Type Control {GetType().Name} requires the Inventory Database to work properly. But it is missing for object {unityObject}, please contact Opsive.");
            }

            var visualElement = CreateCustomControlVisualElement(value, onChangeEvent, field);
            container.Add(visualElement);

            return container;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userData"></param>
        /// <returns>Returns false if the user Data says not to use the custom control.</returns>
        protected virtual void ExtractUserDataAndUseCustomControl(object userData)
        {
            if (userData is object[] objArray) {
                for (int i = 0; i < objArray.Length; i++) {
                    if (objArray[i] is bool b) {
                        if (b == false) {
                            m_BoolUserData = false;
                        }
                    }

                    if (objArray[i] is InventorySystemDatabase database) {
                        m_Database = database;
                    }
                }
            } else if (userData is InventorySystemDatabase database) {
                m_Database = database;
            }

            if (m_Database == null) {
                if (Application.isPlaying == false || InventorySystemManager.IsNull) {
                    m_Database = Object.FindObjectOfType<InventorySystemManager>()?.Database;
                } else {
                    m_Database = InventorySystemManager.Instance.Database;
                }
            
                if (m_Database == null) {
                    if (Application.isPlaying == false) {
                        m_Database = InventoryMainWindow.InventorySystemDatabase;
                    }
                }
            }
        }

        /// <summary>
        /// Creates an ObjectAmountView for the correct object type.
        /// </summary>
        /// <param name="value">The start value.</param>
        /// <param name="onChangeEvent">The onChangeEvent.</param>
        /// <param name="field">The field info</param>
        /// <returns>The new ObjectAmountsView.</returns>
        public abstract VisualElement CreateCustomControlVisualElement(object value, Func<object, bool> onChangeEvent,
            FieldInfo field);
    }
}