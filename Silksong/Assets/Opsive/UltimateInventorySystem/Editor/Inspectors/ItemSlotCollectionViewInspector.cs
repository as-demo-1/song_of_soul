/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Panels.Hotbar;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The custom inspector for the equipper component.
    /// </summary>
    [CustomEditor(typeof(ItemSlotCollectionView), true)]
    public class ItemSlotCollectionViewInspector : DatabaseInspectorBase
    {

        protected override List<string> ExcludedFields => new List<string>()
        {
            "m_ItemSlotItemViewSlots", "m_ItemSlotSet"
        };

        protected virtual HashSet<string> ItemObjectSlotExclude => new HashSet<string>
            {"m_Name", "m_Category", "m_IsSkinnedEquipment", "m_Transform", "m_ItemObject"};

        protected ItemSlotCollectionView m_ItemSlotCollectionView;
        protected ObjectField m_ItemSetSlotField;
        protected List<ItemViewSlot> m_List;
        protected ReorderableList m_ReorderableList;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_ItemSlotCollectionView = target as ItemSlotCollectionView;

            if (Application.isPlaying == false) {
                m_ItemSlotCollectionView.Initialize(true);
            } else {
                m_ItemSlotCollectionView.Initialize(false);
            }
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            m_ItemSetSlotField = new ObjectField("Item Slot Set");
            m_ItemSetSlotField.objectType = typeof(ItemSlotSet);
            m_ItemSetSlotField.value = m_ItemSlotCollectionView.ItemSlotSet;
            m_ItemSetSlotField.RegisterValueChangedCallback(evt =>
            {
                m_ItemSlotCollectionView.ItemSlotSet = evt.newValue as ItemSlotSet;
                m_ItemSlotCollectionView.Initialize(false);
                m_ItemSlotCollectionView.ResizeItemViewSlotsToItemSlotsSetCount();
                m_List = new List<ItemViewSlot>(m_ItemSlotCollectionView.m_ItemSlotItemViewSlots);
                OnValueChanged();
            });
            container.Add(m_ItemSetSlotField);
            
            m_ItemSlotCollectionView.ResizeItemViewSlotsToItemSlotsSetCount();
            if (m_ItemSlotCollectionView.m_ItemSlotItemViewSlots == null) {
                m_ItemSlotCollectionView.m_ItemSlotItemViewSlots = new ItemViewSlot[0];
            }
            m_List = new List<ItemViewSlot>(m_ItemSlotCollectionView.m_ItemSlotItemViewSlots);
            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var listElement = new ListElement();
                    listElement.OnValueChanged += (newValue) =>
                    {
                        m_List[index] = newValue;
                        OnValueChanged();
                    };

                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as ListElement;
                    if (m_ItemSlotCollectionView.ItemSlotSet == null) {
                        return;
                    }
                    var itemSlot = m_ItemSlotCollectionView.ItemSlotSet.GetSlot(index);
                    if (itemSlot.HasValue == false) {
                        m_List.RemoveAt(index);
                        return;
                    }
                    var itemViewSlot = m_List[index];
                    listElement.Refresh(itemSlot.Value, itemViewSlot);
                }, (parent) =>
                {
                    parent.Add(new Label("Item Slots Item View Slots"));
                },
                (index) => { return 48f; },
                null, null, null, null);

            container.Add(m_ReorderableList);
        }

        /// <summary>
        /// Serialize and update the inspector.
        /// </summary>
        private void OnValueChanged()
        {
            m_ItemSlotCollectionView.m_ItemSlotItemViewSlots = m_List.ToArray();
            m_ReorderableList.Refresh(m_List);
            Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemSlotCollectionView);
        }

        /// <summary>
        /// The list element for the slots.
        /// </summary>
        public class ListElement : VisualElement
        {
            public event Action<ItemViewSlot> OnValueChanged;

            protected Label m_Label;
            protected ItemCategoryViewName m_ItemCategoryViewName;
            protected ObjectField m_ItemViewSlot;

            /// <summary>
            /// The list element.
            /// </summary>
            public ListElement()
            {
                var horizontalContainer = new VisualElement();
                horizontalContainer.AddToClassList("horizontal-layout");
                Add(horizontalContainer);

                m_Label = new Label();
                m_Label.style.width = 125;
                horizontalContainer.Add(m_Label);

                m_ItemCategoryViewName = new ItemCategoryViewName();
                horizontalContainer.Add(m_ItemCategoryViewName);

                m_ItemViewSlot = new ObjectField("Item View slot");
                m_ItemViewSlot.objectType = typeof(ItemViewSlot);
                m_ItemViewSlot.RegisterValueChangedCallback(evt =>
                {
                    OnValueChanged?.Invoke(evt.newValue as ItemViewSlot);
                });

                Add(m_ItemViewSlot);
            }

            /// <summary>
            /// Update the visuals of the element.
            /// </summary>
            /// <param name="itemObjectSlot">The item object slot.</param>
            public void Refresh(ItemSlot itemSlot, ItemViewSlot itemViewSlot)
            {
                m_Label.text = itemSlot.Name;
                m_ItemCategoryViewName.Refresh(itemSlot.Category);
                m_ItemViewSlot.SetValueWithoutNotify(itemViewSlot);
            }
        }

        /// <summary>
        /// Refresh when the database is changed.
        /// </summary>
        protected void Refresh()
        {
            m_ItemSlotCollectionView.Initialize(false);
        }
    }
}