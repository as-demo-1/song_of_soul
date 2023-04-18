/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.Inspectors.Utility;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Storage;
    using Opsive.UltimateInventorySystem.UI.Item.DragAndDrop;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom inspector for the item slot set object.
    /// </summary>
    [CustomEditor(typeof(ItemViewSlotDropActionSet), true)]
    public class ItemViewSlotDropActionSetInspector : InspectorBase
    {
        protected const string c_ActionsWithConditions = "m_ActionsWithConditionsData";

        protected override List<string> ExcludedFields => new List<string>() { c_ActionsWithConditions };

        protected ItemViewSlotDropActionSet m_ItemViewSlotDropActionSet;
        protected List<ItemViewDropActionsWithConditions> m_List;
        protected ReorderableList m_ReorderableList;
        protected VisualElement m_Selection;

        protected ItemViewSlotDropActionsConditionsVisualElement<ItemViewDropCondition> m_ConditionsListContainer;
        protected ItemViewSlotDropActionsConditionsVisualElement<ItemViewDropAction> m_ActionsListContainer;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_ItemViewSlotDropActionSet = target as ItemViewSlotDropActionSet;

            m_ItemViewSlotDropActionSet.Initialize(false);

            if (m_ItemViewSlotDropActionSet.ActionsWithConditions == null) {
                m_ItemViewSlotDropActionSet.ActionsWithConditions = new ItemViewDropActionsWithConditions[0];
            }

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            m_List = new List<ItemViewDropActionsWithConditions>(m_ItemViewSlotDropActionSet.ActionsWithConditions);
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
                    parent.Add(new Label("Item View Drop Action Set"));
                },
                (index) => 36,
                (index) => { Select(index); },
                () =>
                {
                    var itemViewSlotDropActionsWithConditions = new ItemViewDropActionsWithConditions();
                    itemViewSlotDropActionsWithConditions.Conditions = new ItemViewDropCondition[0];
                    itemViewSlotDropActionsWithConditions.Actions = new ItemViewDropAction[0];

                    m_List.Add(itemViewSlotDropActionsWithConditions);

                    SaveChanges();

                    m_ReorderableList.SelectedIndex = m_List.Count - 1;
                    //Select(m_List.Count - 1);
                }, (index) =>
                {
                    if (index < 0 || index >= m_List.Count) { return; }

                    m_List.RemoveAt(index);

                    SaveChanges();
                    if (index >= m_List.Count) { index = m_List.Count - 1; }
                    Select(index);
                }, (i1, i2) =>
                {
                    SaveChanges();
                });

            container.Add(m_ReorderableList);

            m_Selection = new VisualElement();

            m_ConditionsListContainer = new ItemViewSlotDropActionsConditionsVisualElement<ItemViewDropCondition>(
                m_ItemViewSlotDropActionSet,
                null,
                "Item View Drop Conditions",
                (list) =>
                {
                    m_ItemViewSlotDropActionSet.ActionsWithConditions[m_ReorderableList.SelectedIndex].Conditions =
                        list.ToArray();
                    SaveChanges();
                    m_ReorderableList.Refresh(m_List);
                });
            m_ActionsListContainer = new ItemViewSlotDropActionsConditionsVisualElement<ItemViewDropAction>(
                m_ItemViewSlotDropActionSet,
                null,
                "Item View Drop Actions",
                (list) =>
                {
                    m_ItemViewSlotDropActionSet.ActionsWithConditions[m_ReorderableList.SelectedIndex].Actions =
                        list.ToArray();
                    SaveChanges();
                    m_ReorderableList.Refresh(m_List);
                });

            container.Add(m_Selection);

            Refresh();
        }

        private void SaveChanges()
        {
            m_ItemViewSlotDropActionSet.ActionsWithConditions = m_List.ToArray();
            m_ItemViewSlotDropActionSet.Serialize();
            m_ReorderableList.Refresh(m_List);
            Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemViewSlotDropActionSet);
        }

        /// <summary>
        /// Refresh when the database is changed.
        /// </summary>
        protected void Refresh()
        {
            m_Selection.Clear();

            Select(m_ReorderableList.SelectedIndex);
        }

        /// <summary>
        /// Select an itemSlot in the list.
        /// </summary>
        /// <param name="index">The slot index.</param>
        private void Select(int index)
        {
            m_Selection.Clear();
            if (index < 0 || index >= m_List.Count) {
                m_Selection.Add(new Label("No item is selected"));
                return;
            }

            m_Selection.Add(m_ConditionsListContainer);
            m_Selection.Add(m_ActionsListContainer);

            m_ConditionsListContainer.Refresh(m_List[index].Conditions);
            m_ActionsListContainer.Refresh(m_List[index].Actions);

            //m_ConditionsListContainer.SetValueWithoutNotify(m_List[index].Conditions);
            //m_ActionsListContainer.SetValueWithoutNotify(m_List[index].Actions);
        }

        /// <summary>
        /// The list element for an item slot.
        /// </summary>
        public class ListElement : VisualElement
        {
            protected VisualElement m_Conditions;
            protected Label m_ConditionLabel;
            protected Label m_ConditionsLabel;

            protected VisualElement m_Actions;
            protected Label m_ActionLabel;
            protected Label m_ActionsLabel;

            /// <summary>
            /// The list element.
            /// </summary>
            public ListElement()
            {
                // Conditions
                m_Conditions = new VisualElement();
                m_Conditions.AddToClassList("horizontal-layout");
                Add(m_Conditions);

                m_ConditionLabel = new Label("Conditions: ");
                m_ConditionLabel.style.minWidth = 80;
                m_ConditionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                m_Conditions.Add(m_ConditionLabel);

                m_ConditionsLabel = new Label();
                m_Conditions.Add(m_ConditionsLabel);

                // Actions
                m_Actions = new VisualElement();
                m_Actions.AddToClassList("horizontal-layout");
                Add(m_Actions);

                m_ActionLabel = new Label("Actions: ");
                m_ActionLabel.style.minWidth = 80;
                m_ActionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                m_Actions.Add(m_ActionLabel);

                m_ActionsLabel = new Label();
                m_Actions.Add(m_ActionsLabel);
            }

            /// <summary>
            /// Redraw the item slot.
            /// </summary>
            /// <param name="dropActionsWithConditions">The item slot.</param>
            public void Refresh(ItemViewDropActionsWithConditions dropActionsWithConditions)
            {
                m_ConditionsLabel.text = NameList(dropActionsWithConditions.Conditions);

                m_ActionsLabel.text = NameList(dropActionsWithConditions.Actions);
            }

            public string NameList<T>(T[] array) where T : class
            {
                if (array == null || array.Length == 0) { return "Empty"; }

                var text = array[0] == null ? "Null" : array[0].ToString();

                for (int i = 1; i < array.Length; i++) {
                    var obj = array[i];
                    if (obj == null) {
                        text += " | Null";
                    } else {
                        text += " | " + obj.ToString();
                    }
                }

                return text;
            }
        }
    }


    public class ItemViewSlotDropActionsConditionsVisualElement<T> : VisualElement where T : class
    {
        protected InventorySystemDatabase m_Database;
        protected ItemViewSlotDropActionSet m_ItemViewSlotDropActionSet;
        protected T m_SelectedObject;
        protected List<T> m_List;
        protected ReorderableList m_ReorderableList;
        protected VisualElement m_Selection;
        protected Action<List<T>> m_UpdateList;

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        public ItemViewSlotDropActionsConditionsVisualElement(ItemViewSlotDropActionSet itemViewSlotDropActionSet,
            InventorySystemDatabase database, string title, Action<List<T>> updateList)
        {
            m_Database = database;
            m_ItemViewSlotDropActionSet = itemViewSlotDropActionSet;
            m_UpdateList = updateList;

            m_List = new List<T>();
            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var listElement = new ListElement();

                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    if (index < 0 || index >= m_List.Count) { return; }

                    var listElement = parent.ElementAt(0) as ListElement;
                    var itemSlot = m_List[index];
                    listElement.Refresh(itemSlot);
                }, (parent) =>
                {
                    parent.Add(new Label(title));
                },
                (index) => { Select(index); },
                () =>
                {

                    InspectorUtility.AddObjectType(typeof(T), null,
                        (evt) =>
                        {
                            var newItemAction = Activator.CreateInstance(evt as Type) as T;

                            m_List.Add(newItemAction);

                            m_UpdateList?.Invoke(m_List);
                            m_ReorderableList.Refresh(m_List);
                            m_ReorderableList.SelectedIndex = m_List.Count - 1;
                            //Select(m_List.Count - 1);
                        });

                }, (index) =>
                {
                    if (index < 0 || index >= m_List.Count) { return; }

                    m_List.RemoveAt(index);

                    m_UpdateList?.Invoke(m_List);
                    m_ReorderableList.Refresh(m_List);
                    if (index >= m_List.Count) { index = m_List.Count - 1; }
                    Select(index);
                }, (i1, i2) =>
                {
                    m_UpdateList?.Invoke(m_List);
                });

            Add(m_ReorderableList);

            m_Selection = new VisualElement();

            Add(m_Selection);

            Refresh();
        }

        /// <summary>
        /// Refresh when the database is changed.
        /// </summary>
        public void Refresh(T[] array)
        {
            m_List.Clear();
            m_List.AddRange(array);
            m_ReorderableList.Refresh(m_List);

            Refresh();
        }

        /// <summary>
        /// Refresh when the database is changed.
        /// </summary>
        protected void Refresh()
        {
            m_Selection.Clear();

            Select(m_ReorderableList.SelectedIndex);
        }

        /// <summary>
        /// Select an itemSlot in the list.
        /// </summary>
        /// <param name="index">The slot index.</param>
        private void Select(int index)
        {
            m_Selection.Clear();
            if (index < 0 || index >= m_List.Count) {
                m_Selection.Add(new Label("No item is selected"));
                return;
            }

            m_SelectedObject = m_List[index];

            if (m_SelectedObject == null) { return; }

            FieldInspectorView.AddField(
                m_ItemViewSlotDropActionSet,
                m_SelectedObject, null, -1, m_SelectedObject.GetType(),
                typeof(T).Name, string.Empty, true,
                m_SelectedObject,
                m_Selection,
                (object obj) =>
                {
                    m_UpdateList?.Invoke(m_List);
                    m_ReorderableList.Refresh(m_List);
                }
                , null, false, null, null, m_Database);
        }

        /// <summary>
        /// The list element for an item slot.
        /// </summary>
        public class ListElement : VisualElement
        {
            protected Label m_Label;

            /// <summary>
            /// The list element.
            /// </summary>
            public ListElement()
            {
                m_Label = new Label();
                Add(m_Label);
            }

            /// <summary>
            /// Redraw the item slot.
            /// </summary>
            /// <param name="dropActionsWithConditions">The item slot.</param>
            public void Refresh(T dropActionsWithConditions)
            {
                if (dropActionsWithConditions == null) {
                    m_Label.text = "NULL";
                    return;
                }

                m_Label.text = dropActionsWithConditions.ToString();
            }
        }
    }
}