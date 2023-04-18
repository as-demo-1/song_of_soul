/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes;
    using Opsive.UltimateInventorySystem.Storage;
    using Opsive.UltimateInventorySystem.Utility;
    using System;
    using System.Collections.Generic;
    using Opsive.Shared.Utility;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The item collection field.
    /// </summary>
    public class ItemCollectionField : VisualElement
    {
        public event Action<ItemCollection> OnItemCollectionTypeChange;
        public event Action OnCollectionContentChange;

        protected ItemCollection m_ItemCollection;
        protected FilterWindowPopupField m_TypeField;
        protected VisualElement m_Container;
        protected InventorySystemDatabase m_Database;
        protected ItemAmountsView m_ItemAmountsView;

        protected const string c_NameProperty = "m_Name";
        protected const string c_OnUpdateCollectionProperty = "m_OnUpdateCollection";
        protected const string c_DefaultLoadoutProperty = "m_DefaultLoadout";

        protected HashSet<string> m_PropertiesToExclude
            = new HashSet<string> { c_NameProperty, c_OnUpdateCollectionProperty, c_DefaultLoadoutProperty };

        public ItemCollection ItemCollection => m_ItemCollection;

        /// <summary>
        /// Create the item collection field.
        /// </summary>
        /// <param name="itemCollection">The item collection.</param>
        /// <param name="database">The database.</param>
        public ItemCollectionField(ItemCollection itemCollection, InventorySystemDatabase database)
        {
            m_Database = database;
            m_ItemCollection = itemCollection;

            var nameField = new UnicodeTextField("Name");
            nameField.value = itemCollection.Name;
            nameField.isDelayed = true;
            nameField.RegisterValueChangedCallback(evt =>
            {
                itemCollection.SetName(evt.newValue);
                ValueChanged();
            });
            Add(nameField);

            m_TypeField = FilterWindowPopupField.CreateFilterWindowPopupField(typeof(ItemCollection), FilterWindow.FilterType.Class, "Collection Type", false, itemCollection.GetType(),
                (type) =>
                {
                    var changeType = EditorUtility.DisplayDialog("Change Item Collection Type?",
                                         $"You are trying to change the Type of the Item Collection. This action cannot be undone and some values may be lost.\n" +
                                         $"Are you sure you would like to change the Type?",
                                         "Yes",
                                         "No");

                    if (changeType) {
                        var previousItemCollection = m_ItemCollection;
                        m_ItemCollection = (ItemCollection)Activator.CreateInstance(type as Type);
                        ReflectionUtility.ObjectCopy(previousItemCollection, m_ItemCollection);
                        m_TypeField.UpdateSelectedObject(type);
                        OnItemCollectionTypeChange?.Invoke(m_ItemCollection);
                        ValueChanged();
                    }
                });
            m_TypeField.label = "Collection";
            Add(m_TypeField);

            m_Container = new VisualElement();

            FieldInspectorView.AddFields(m_ItemCollection?.Inventory?.gameObject, m_ItemCollection, Shared.Utility.MemberVisibility.Public, m_Container,
                                            (object obj) => { ValueChanged(); }, null, true, null, false, m_PropertiesToExclude, null);
            Add(m_Container);

            ItemAmounts itemAmounts = null;
            if (!Application.isPlaying) {
                if (itemCollection.DefaultLoadout == null) {
                    itemCollection.DefaultLoadout = new ItemAmount[0];
                }

                itemAmounts = itemCollection.DefaultLoadout;
            } else {
                void PlayTimeRefresh()
                {
                    m_ItemAmountsView.Refresh(itemCollection.ItemStacks.ToItemAmountArray());
                }
                itemCollection.OnItemCollectionUpdate -= PlayTimeRefresh;
                itemCollection.OnItemCollectionUpdate += PlayTimeRefresh;
                itemAmounts = itemCollection.ItemStacks.ToItemAmountArray();
            }

            m_ItemAmountsView = new ItemAmountsView(
                itemAmounts, m_Database,
                (obj) =>
                {
                    var previousItemAmounts = obj as ItemAmounts;
                    var itemCount = previousItemAmounts.Count;
                 
                    var newArray = new ItemAmount[itemCount];
                    Array.Copy(previousItemAmounts.Array, newArray, itemCount);
                    var newItemAmounts = new ItemAmounts(newArray);
                    itemCollection.DefaultLoadout = newItemAmounts;
                    ValueChanged();
                    return true;
                });

            Add(m_ItemAmountsView);
        }

        /// <summary>
        /// Notify that a value changed.
        /// </summary>
        public void ValueChanged()
        {
            OnCollectionContentChange?.Invoke();
        }
    }
}