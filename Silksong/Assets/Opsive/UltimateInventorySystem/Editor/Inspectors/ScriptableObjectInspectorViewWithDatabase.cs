/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Opsive.UltimateInventorySystem.Storage;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// A view that lets you embed a scriptable object inspector in another inspector.
    /// </summary>
    public class ScriptableObjectInspectorViewWithDatabase : VisualElement
    {
        public event Action OnValueChanged;

        private ScriptableObject m_Value;
        private Label m_Label;
        private List<VisualElement> m_VisualElements;

        public InventorySystemDatabase Database { get; set; }

        /// <summary>
        /// Create the scriptable object inspector.
        /// </summary>
        public ScriptableObjectInspectorViewWithDatabase(InventorySystemDatabase database)
        {
            Database = database;
            m_VisualElements = new List<VisualElement>();
            m_Label = new Label();
            Add(m_Label);

            Refresh();
        }

        /// <summary>
        /// Change the scriptable object to watch.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void Refresh(ScriptableObject value)
        {
            m_Value = value;
            Refresh();
        }

        /// <summary>
        /// Draw the view again.
        /// </summary>
        public void Refresh()
        {
            Clear();
            if (m_Value == null) {
                m_Label.text = "The Object is NULL";
                Add(m_Label);
                return;
            }

            m_Label.text = $"{m_Value.name} ({m_Value.GetType().Name})";
            Add(m_Label);

            var fields = new List<FieldInfo>(m_Value.GetType()
                .GetFields(BindingFlags.Default | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic));

            fields.RemoveAll(x =>
            {
                foreach (var attribute in x.CustomAttributes) {
                    if (attribute.AttributeType == typeof(HideInInspector)) { return true; }
                }

                return false;
            });

            for (int i = 0; i < fields.Count; i++) {
                if (m_VisualElements.Count <= i) { m_VisualElements.Add(new VisualElement()); }

                Add(m_VisualElements[i]);
                m_VisualElements[i].Clear();

                var field = fields[i];
                var fieldValue = field.GetValue(m_Value);
                FieldInspectorView.AddField(
                    m_Value,
                    fieldValue, field, -1, field.FieldType,
                    field.Name, string.Empty, true,
                    fieldValue,
                    m_VisualElements[i],
                    (object obj) =>
                    {
                        if (field.GetValue(m_Value) == obj) { return; }
                        field.SetValue(m_Value, obj);
                        OnValueChanged?.Invoke();
                    }, null, false, null, null, Database);
            }
        }
    }
}