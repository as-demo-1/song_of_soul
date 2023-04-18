/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using Opsive.Shared.Editor.Inspectors.Utility;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.ItemActions;
    using System;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// The item action collection field.
    /// </summary>
    public class ItemActionCollectionField : VisualElement
    {
        ReorderableList m_ReorderableList;
        VisualElement m_SelectedContainer;

        UnityEngine.Object m_Target;
        ItemActionCollection m_ItemActions;

        public UnityEngine.Object Target { get { return m_Target; } }
        public ItemActionCollection ItemActions { get { return m_ItemActions; } }
        public ReorderableList ReorderableList { get { return m_ReorderableList; } }

        Action m_OnRefresh;

        /// <summary>
        /// Create the item action collection field.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="itemActions">The item actions.</param>
        /// <param name="title">The title.</param>
        /// <param name="onRefresh">The on refresh action.</param>
        public ItemActionCollectionField(UnityEngine.Object target, ItemActionCollection itemActions, string title, Action onRefresh)
        {
            m_Target = target;
            m_ItemActions = itemActions;
            m_OnRefresh = onRefresh;

            m_ItemActions.Initialize(false);

            var startingSource = new ItemAction[m_ItemActions.Count];
            for (int i = 0; i < startingSource.Length; i++) { startingSource[i] = m_ItemActions[i]; }

            m_ReorderableList = new ReorderableList(
                startingSource,
                (parent, index) => { parent.Add(new ListElement()); }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as ListElement;
                    listElement.SetItemData(m_ItemActions[index], index);
                }, (parent) => { parent.Add(new Label(title)); },
                (index) => 20, (index) =>
                {
                    OnSelectionChanged(index);
                },
                () =>
                {
                    InspectorUtility.AddObjectType(typeof(ItemAction), null,
                        (evt) =>
                        {
                            var newItemAction = Activator.CreateInstance(evt as Type) as ItemAction;

                            var newArrayStruct =
                                ItemActionUtility.GetItemActionResizableArray(m_ItemActions);

                            newArrayStruct.Add(newItemAction);

                            ItemActionUtility.SetItemActions(m_ItemActions, newArrayStruct);
                            Refresh(true);
                        });

                }, (i) =>
                {
                    var newArrayStruct =
                        ItemActionUtility.GetItemActionResizableArray(m_ItemActions);

                    newArrayStruct.RemoveAt(i);

                    ItemActionUtility.SetItemActions(m_ItemActions, newArrayStruct);
                    Refresh(true);
                }, (previousIndex, newIndex) =>
                {
                    var newArrayStruct = ItemActionUtility.GetItemActionResizableArray(m_ItemActions);
                    newArrayStruct.MoveElementIndex(previousIndex, newIndex);
                    ItemActionUtility.SetItemActions(m_ItemActions, newArrayStruct);
                });

            Initialize();
        }

        /// <summary>
        /// Initialize the view.
        /// </summary>
        protected void Initialize()
        {
            m_SelectedContainer = new VisualElement();

            AddToClassList("AttributeCollectionField");
            m_SelectedContainer.AddToClassList("AttributeCollectionField_SelectedContainer");

            Add(m_ReorderableList);
            Add(m_SelectedContainer);

            ItemActionUtility.SerializeItemActionsAndDirty(m_Target, m_ItemActions);

            Undo.UndoRedoCallback onUndoRedo = () =>
            {
                m_ItemActions.Initialize(false);
                Refresh(false);
            };
            RegisterCallback<AttachToPanelEvent>(c => { Undo.undoRedoPerformed += onUndoRedo; });
            RegisterCallback<DetachFromPanelEvent>(c => { Undo.undoRedoPerformed -= onUndoRedo; });
        }

        /// <summary>
        /// Draws the ReorderableList.
        /// </summary>
        /// <param name="triggerSelection">Should the selection change be triggered?</param>
        /// <param name="callOnRefresh">Should the refresh action be called?</param>
        public void Refresh(bool triggerSelection, bool callOnRefresh = true)
        {
            if (callOnRefresh && m_OnRefresh != null) { m_OnRefresh.Invoke(); }
            ItemActionUtility.SerializeItemActionsAndDirty(m_Target, m_ItemActions);

            var newSource = new ItemAction[m_ItemActions.Count];
            for (int i = 0; i < newSource.Length; i++) { newSource[i] = m_ItemActions[i]; }

            m_ReorderableList.Refresh(newSource);

            if (triggerSelection) {
                OnSelectionChanged(m_ReorderableList.SelectedIndex);
            }
        }

        /// <summary>
        /// Refresh the view when a new selection is made.
        /// </summary>
        /// <param name="index">The selected index.</param>
        void OnSelectionChanged(int index)
        {
            m_SelectedContainer.Clear();

            if (index < 0 || index >= m_ReorderableList.ItemsSource.Count) {
                return;
            }

            var item = m_ReorderableList.ItemsSource[m_ReorderableList.SelectedIndex];
            var itemAction = item as ItemAction;

            if (itemAction == null) {
                return;
            }

            var attributeField = new ItemActionField(this, m_ReorderableList.SelectedIndex);
            attributeField.AddToClassList("AttributeCollectionField_AttributeField");

            m_SelectedContainer.Add(attributeField);
        }

        /// <summary>
        /// The list element.
        /// </summary>
        class ListElement : VisualElement
        {
            protected Label nameL;
            protected Label typeL;

            /// <summary>
            /// Create the list element.
            /// </summary>
            public ListElement()
            {
                nameL = new Label();
                typeL = new Label();

                Add(nameL);
                Add(typeL);

                style.flexDirection = FlexDirection.Row;
                style.alignContent = Align.Center;
                style.justifyContent = Justify.SpaceBetween;
            }

            /// <summary>
            /// Set the item action.
            /// </summary>
            /// <param name="itemAction">The item action.</param>
            /// <param name="index">The index.</param>
            public void SetItemData(ItemAction itemAction, int index)
            {
                typeL.text = itemAction.GetType().Name;

                if (itemAction.Name == null) {
                    nameL.text = "No Name";
                } else {
                    nameL.text = itemAction.Name;
                }
            }
        }
    }
}