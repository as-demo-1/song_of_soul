/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom inspector for the item slot set object.
    /// </summary>
    [CustomEditor(typeof(ItemSlotSet), true)]
    public class ItemSlotSetInspector : DatabaseInspectorBase
    {
        protected const string c_ItemSlots = "m_ItemSlots";

        protected override List<string> ExcludedFields => new List<string>() { c_ItemSlots };

        protected ItemSlotSet m_ItemSlotSet;
        protected List<ItemSlot> m_List;
        protected ReorderableList m_ReorderableList;
        protected VisualElement m_Selection;

        protected UnicodeTextField m_NameField;
        protected ItemCategoryField m_ItemCategoryField;

        protected int m_PreviousSelected = -1;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_ItemSlotSet = target as ItemSlotSet;

            if (m_ItemSlotSet.ItemSlotsArray == null) {
                m_ItemSlotSet.ItemSlotsArray = new ItemSlot[0];
            }

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            m_List = new List<ItemSlot>(m_ItemSlotSet.ItemSlotsArray);
            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var listElement = new ListElement();

                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as ListElement;
                    var itemSlot = m_List[index];
                    listElement.Refresh(itemSlot);
                }, (parent) =>
                {
                    parent.Add(new Label("Item Slot Set"));
                },
                (index) => { Select(index); },
                () =>
                {
                    m_List.Add(default);

                    m_ItemSlotSet.ItemSlotsArray = m_List.ToArray();
                    m_ReorderableList.Refresh(m_List);
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemSlotSet);
                    Select(m_List.Count - 1);
                }, (index) =>
                {
                    if (index < 0 || index >= m_List.Count) { return; }

                    m_List.RemoveAt(index);

                    m_ItemSlotSet.ItemSlotsArray = m_List.ToArray();
                    m_ReorderableList.Refresh(m_List);
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemSlotSet);
                    if (index >= m_List.Count) { index = m_List.Count - 1; }
                    Select(index);
                }, (i1, i2) =>
                {
                    m_ItemSlotSet.ItemSlotsArray = m_List.ToArray();
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemSlotSet);
                });

            container.Add(m_ReorderableList);

            m_Selection = new VisualElement();

            m_NameField = new UnicodeTextField("Name");
            m_NameField.RegisterValueChangedCallback(evt => { NameChanged(evt.newValue); });

            container.Add(m_Selection);

            Refresh();
        }
        /// <summary>
        /// Refresh when the database is changed.
        /// </summary>
        protected void Refresh()
        {
            m_Selection.Clear();
            m_ItemCategoryField = new ItemCategoryField("Item Category",
                m_DatabaseField.value as InventorySystemDatabase,
                new (string, Action<ItemCategory>)[]
                {
                    ("Set Category", (x) =>
                    {
                        ItemCategoryChange(x);
                    })
                },
                (x) => true);

            Select(m_PreviousSelected);
        }

        /// <summary>
        /// Select an itemSlot in the list.
        /// </summary>
        /// <param name="index">The slot index.</param>
        private void Select(int index)
        {
            if (m_ReorderableList.SelectedIndex == index && m_PreviousSelected == index) {

                if (index < 0 || index >= m_List.Count) {
                    m_Selection.Add(new Label("No item is selected"));
                    return;
                }

                m_NameField.SetValueWithoutNotify(m_List[index].Name);
                m_ItemCategoryField.Refresh(m_List[index].Category);
                return;
            }

            m_PreviousSelected = index;
            m_ReorderableList.SelectedIndex = index;

            m_Selection.Clear();

            if (index < 0 || index >= m_List.Count) {
                m_Selection.Add(new Label("No item is selected"));
                return;
            }

            m_Selection.Add(m_NameField);
            m_Selection.Add(m_ItemCategoryField);

            m_NameField.SetValueWithoutNotify(m_List[index].Name);
            m_ItemCategoryField.Refresh(m_List[index].Category);
        }

        /// <summary>
        /// The slot name changed.
        /// </summary>
        /// <param name="slotName">The slot name.</param>
        private void NameChanged(string slotName)
        {
            var index = m_ReorderableList.SelectedIndex;
            OnValueChanged(index,
                new ItemSlot(slotName, m_List[index].Category));
        }

        /// <summary>
        /// The size limit changed.
        /// </summary>
        /// <param name="sizeLimit">The size limit.</param>
        private void SizeLimitChanged(int sizeLimit)
        {
            var index = m_ReorderableList.SelectedIndex;
            OnValueChanged(index,
                new ItemSlot(m_List[index].Name, m_List[index].Category));
        }

        /// <summary>
        /// The item category changed.
        /// </summary>
        /// <param name="category">The new item category.</param>
        private void ItemCategoryChange(ItemCategory category)
        {
            var index = m_ReorderableList.SelectedIndex;
            OnValueChanged(index,
                new ItemSlot(m_List[index].Name, category));
        }

        /// <summary>
        /// Serialize and refresh the view when an value changes.
        /// </summary>
        /// <param name="index">The slot index.</param>
        /// <param name="value">The new Item slot value.</param>
        private void OnValueChanged(int index, ItemSlot value)
        {
            m_List[index] = value;
            m_ItemSlotSet.ItemSlotsArray = m_List.ToArray();
            m_ReorderableList.Refresh(m_List);
            Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemSlotSet);
            Select(index);
        }

        /// <summary>
        /// The list element for an item slot.
        /// </summary>
        public class ListElement : VisualElement
        {
            protected Label m_Label;
            protected ItemCategoryViewName m_ItemCategoryViewName;

            /// <summary>
            /// The list element.
            /// </summary>
            public ListElement()
            {
                AddToClassList("horizontal-layout");
                m_Label = new Label();
                m_Label.style.width = 125;
                Add(m_Label);

                m_ItemCategoryViewName = new ItemCategoryViewName();
                Add(m_ItemCategoryViewName);
            }

            /// <summary>
            /// Redraw the item slot.
            /// </summary>
            /// <param name="itemSlot">The item slot.</param>
            public void Refresh(ItemSlot itemSlot)
            {
                m_Label.text = itemSlot.Name;
                m_ItemCategoryViewName.Refresh(itemSlot.Category);
            }
        }


    }
}