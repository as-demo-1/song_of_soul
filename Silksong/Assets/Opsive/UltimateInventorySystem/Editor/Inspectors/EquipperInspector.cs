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
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Equipping;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The custom inspector for the equipper component.
    /// </summary>
    [CustomEditor(typeof(Equipper), true)]
    public class EquipperInspector : DatabaseInspectorBase
    {
        protected const string c_Slots = "m_Slots";

        protected override List<string> ExcludedFields => new List<string>() { c_Slots, "m_ItemSlotSet" };

        protected virtual HashSet<string> ItemObjectSlotExclude => new HashSet<string>
            {"m_Name", "m_Category", "m_IsSkinnedEquipment", "m_Transform", "m_ItemObject"};

        protected Equipper m_Equipper;
        protected ObjectField m_ItemSetSlotField;
        protected List<ItemObjectSlot> m_List;
        protected ReorderableList m_ReorderableList;
        protected VisualElement m_Selection;

        protected Toggle m_SkinnedField;
        protected ObjectField m_TransformField;
        protected ObjectField m_ItemObjectField;
        protected VisualElement m_HideObjectsField;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_Equipper = target as Equipper;

            m_Equipper.ValidateSlots();
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {

            m_ItemSetSlotField = new ObjectField("Item Slot Set");
            m_ItemSetSlotField.objectType = typeof(ItemSlotSet);
            m_ItemSetSlotField.value = m_Equipper.ItemSlotSet;
            m_ItemSetSlotField.RegisterValueChangedCallback(evt =>
            {
                m_Equipper.ItemSlotSet = evt.newValue as ItemSlotSet;
                m_Equipper.ValidateSlots();
                m_List = new List<ItemObjectSlot>(m_Equipper.Slots);
                OnValueChanged();
            });
            container.Add(m_ItemSetSlotField);

            if (m_Equipper.Slots == null) {
                m_Equipper.Slots = new ItemObjectSlot[0];
            }

            m_List = new List<ItemObjectSlot>(m_Equipper.Slots);
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
                    parent.Add(new Label("Item Object Slots"));
                },
                (index) => { Select(index); },
                null, null, null);

            container.Add(m_ReorderableList);

            m_Selection = new VisualElement();
            container.Add(m_Selection);

            m_SkinnedField = new Toggle("Skinned Equipment");
            m_SkinnedField.RegisterValueChangedCallback(evt =>
            {
                m_List[m_ReorderableList.SelectedIndex].IsSkinnedEquipment = evt.newValue;
                OnValueChanged();
            });

            m_TransformField = new ObjectField("Transform");
            m_TransformField.objectType = typeof(Transform);
            m_TransformField.RegisterValueChangedCallback(evt =>
            {
                m_List[m_ReorderableList.SelectedIndex].Transform = evt.newValue as Transform;
                OnValueChanged();
            });

            m_ItemObjectField = new ObjectField("Item Object");
            m_ItemObjectField.objectType = typeof(ItemObject);
            m_ItemObjectField.RegisterValueChangedCallback(evt =>
            {
                m_List[m_ReorderableList.SelectedIndex].ItemObject = evt.newValue as ItemObject;
                OnValueChanged();
            });

            m_HideObjectsField = new VisualElement();

            Select(m_ReorderableList.SelectedIndex);
        }

        /// <summary>
        /// Select a equipment slot.
        /// </summary>
        /// <param name="index">The slot index selected.</param>
        private void Select(int index)
        {
            m_Selection.Clear();
            if (index < 0 || index >= m_List.Count) {
                m_Selection.Add(new Label("No item is selected"));
                return;
            }

            m_Selection.Add(m_SkinnedField);
            m_Selection.Add(m_TransformField);
            m_Selection.Add(m_ItemObjectField);
            m_Selection.Add(m_HideObjectsField);

            m_SkinnedField.SetValueWithoutNotify(m_List[index].IsSkinnedEquipment);
            m_TransformField.SetValueWithoutNotify(m_List[index].Transform);
            m_ItemObjectField.SetValueWithoutNotify(m_List[index].ItemObject);

            m_HideObjectsField.Clear();

            FieldInspectorView.AddField(
                m_Equipper,
                m_List, null, -1, m_List[index].GetType(),
                "More Options", string.Empty, true,
                m_List[index],
                m_HideObjectsField,
                (object obj) => { OnValueChanged(); }
                , null, false, null, ItemObjectSlotExclude);
        }

        /// <summary>
        /// Serialize and update the inspector.
        /// </summary>
        private void OnValueChanged()
        {
            m_Equipper.Slots = m_List.ToArray();
            m_ReorderableList.Refresh(m_List);
            Shared.Editor.Utility.EditorUtility.SetDirty(m_Equipper);
            Select(m_ReorderableList.SelectedIndex);
        }

        /// <summary>
        /// The list element for the slots.
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
            /// Update the visuals of the element.
            /// </summary>
            /// <param name="itemObjectSlot">The item object slot.</param>
            public void Refresh(ItemObjectSlot itemObjectSlot)
            {
                m_Label.text = itemObjectSlot.Name;
                m_ItemCategoryViewName.Refresh(itemObjectSlot.Category);
            }
        }

        /// <summary>
        /// Refresh when the database is changed.
        /// </summary>
        protected void Refresh()
        {
            m_Equipper.ValidateSlots();
        }
    }
}