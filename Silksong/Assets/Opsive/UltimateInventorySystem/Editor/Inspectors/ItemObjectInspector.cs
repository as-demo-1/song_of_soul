/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Storage;
    using Opsive.UltimateInventorySystem.Utility;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The custom inspector for the item object component.
    /// </summary>
    [CustomEditor(typeof(ItemObject), true)]
    public class ItemObjectInspector : DatabaseInspectorBase
    {
        protected override List<string> ExcludedFields => new List<string>() { c_ItemPropertyName };

        protected const string c_ItemPropertyName = "m_ItemInfo";

        protected VisualElement m_SelectedContainer;

        protected IntegerField m_AmountField;
        protected ItemField m_ItemField;
        protected ItemObject m_ItemObject;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_ItemObject = target as ItemObject;

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {

            m_AmountField = new IntegerField("Amount");
            m_AmountField.value = m_ItemObject.ItemInfo.Amount;
            m_AmountField.RegisterValueChangedCallback(evt =>
            {
                m_ItemObject.SetAmount(evt.newValue);
                Serialize();
            });
            container.Add(m_AmountField);

            m_SelectedContainer = new VisualElement();
            container.Add(m_SelectedContainer);

            var query = container.Query<BindableElement>();
            var scheduledAction = container.schedule.Execute(() =>
            {
                //Update the ItemObject ItemField in case the item attributes are changed externally (i.e ItemBinding).
                if (m_ItemObject == null) { return; }
                if (m_ItemObject.Item == null) { return; }
                if (OnValidateUtility.IsPrefab(m_ItemObject)) { return; }
                if (!Application.isPlaying) { return; }

                var focusedElement = container.focusController.focusedElement;
                var containerIsFocused = false;
                query.ForEach(x =>
                {
                    if (x == focusedElement) { containerIsFocused = true; }
                });
                if (containerIsFocused) { return; }

                m_ItemField.Refresh(m_ItemObject.Item);

            });
            scheduledAction.Every(500);

            Refresh();
        }

        /// <summary>
        /// Refresh when the database is changed.
        /// </summary>
        protected void Refresh()
        {
            m_SelectedContainer.Clear();

            var database = m_DatabaseField.value as InventorySystemDatabase;

            if (database == null) {
                m_SelectedContainer.Add(new Label("The database is null."));
                return;
            }
            database.Initialize(false);

            m_ItemField = new ItemField(m_DatabaseField.value as InventorySystemDatabase);
            m_ItemObject.ValidateItem();
            m_ItemField.Refresh(m_ItemObject.Item);
            m_ItemField.OnValueChanged += () =>
            {
                if (m_AmountField.value <= 0) { m_AmountField.SetValueWithoutNotify(1); }
                m_ItemObject.SetItem((ItemInfo)(m_AmountField.value, m_ItemField.Value));
                Serialize();
            };

            m_SelectedContainer.Add(m_ItemField);
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        private void Serialize()
        {
            m_ItemObject.Item?.Serialize();
            Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemObject);
        }
    }
}
