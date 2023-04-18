/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.Storage;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The custom inspector for a currency Owner
    /// </summary>
    [CustomEditor(typeof(CurrencyOwner), true)]
    public class CurrencyOwnerInspector : DatabaseInspectorBase
    {
        protected override List<string> ExcludedFields => new List<string>() { c_CurrencyAmountPropertyName };

        protected const string c_CurrencyAmountPropertyName = "m_CurrencyAmount";

        protected VisualElement m_SelectedContainer;

        private CurrencyAmountsView m_CurrencyAmountsField;
        protected CurrencyOwner m_CurrencyOwner;

        protected UQueryBuilder<BindableElement> m_FocusQuery;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_CurrencyOwner = target as CurrencyOwner;

            if (Application.isPlaying == false) { m_CurrencyOwner.CurrencyAmount.Initialize(null, true); }

            base.InitializeInspector();
        }

        /// <summary>
        /// Creat the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {

            m_SelectedContainer = new VisualElement();

            container.Add(m_SelectedContainer);

            m_FocusQuery = container.Query<BindableElement>();

            EventHandler.UnregisterEvent(
                m_CurrencyOwner,
                EventNames.c_CurrencyOwner_OnUpdate, CurrencyOwnerChanged);

            EventHandler.RegisterEvent(
                m_CurrencyOwner,
                EventNames.c_CurrencyOwner_OnUpdate, CurrencyOwnerChanged);

            Refresh();
        }

        /// <summary>
        /// Refresh when the currency owner changes.
        /// </summary>
        private void CurrencyOwnerChanged()
        {
            // Prevent refresh if the inspector is focused.
            if (m_CurrencyOwner == null) { return; }

            var containerIsFocused = false;
            m_FocusQuery.ForEach(x =>
            {
                if (x == null || x.focusController == null) { return; }
                var focusedElement = x.focusController.focusedElement;
                if (x == focusedElement) { containerIsFocused = true; }
            });
            if (containerIsFocused) { return; }

            Refresh();
        }

        /// <summary>
        /// Refresh the view to display the updated data.
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

            if (Application.isPlaying == false) { m_CurrencyOwner.CurrencyAmount.Initialize(null, false); }

            var currencyAmounts = new CurrencyAmounts(m_CurrencyOwner.CurrencyAmount.GetCurrencyAmounts().ToArray());
            m_CurrencyAmountsField = new CurrencyAmountsView(
                currencyAmounts,
                database,
                (newValue) =>
                {
                    var newCurrencyAmounts = newValue as CurrencyAmounts;
                    m_CurrencyOwner.CurrencyAmount.SetCurrency(newCurrencyAmounts);
                    m_CurrencyOwner.CurrencyAmount.Serialize();
                    Serialize();
                    return false;
                });

            m_SelectedContainer.Add(m_CurrencyAmountsField);
        }

        /// <summary>
        /// Serialize and flags the object as dirty.
        /// </summary>
        protected void Serialize()
        {
            if (Application.isPlaying) { return; }
            m_CurrencyOwner.CurrencyAmount.Serialize();
            Shared.Editor.Utility.EditorUtility.SetDirty(m_CurrencyOwner);
        }

        /// <summary>
        /// Unregister the listeners on destroy.
        /// </summary>
        private void OnDestroy()
        {
            if (m_CurrencyOwner == null) { return; }
            EventHandler.UnregisterEvent(m_CurrencyOwner, EventNames.c_CurrencyOwner_OnUpdate, CurrencyOwnerChanged);
        }
    }
}