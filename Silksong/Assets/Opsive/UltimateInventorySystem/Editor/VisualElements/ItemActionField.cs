/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.Utility;
    using System;
    using Opsive.Shared.Utility;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// Item Action Field visual element.
    /// </summary>
    public class ItemActionField : VisualElement
    {
        protected ItemAction m_ItemAction;

        protected FilterWindowPopupField m_TypeField;
        protected ItemActionCollectionField m_ItemActionCollectionField;
        protected VisualElement m_OptionsContainer;
        protected int m_IndexInList;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="itemActionCollectionField">The ItemActionCollectionField.</param>
        /// <param name="indexInList">The index in the list.</param>
        public ItemActionField(ItemActionCollectionField itemActionCollectionField = null, int indexInList = -1)
        {
            m_ItemActionCollectionField = itemActionCollectionField;
            m_IndexInList = indexInList;

            if (itemActionCollectionField != null) {
                m_ItemAction = m_ItemActionCollectionField.ItemActions[m_IndexInList];
            } else {
                m_ItemAction = new DebugItemAction();
            }

            m_OptionsContainer = new VisualElement();

            SetLayout();
        }

        /// <summary>
        /// Record before any changes.
        /// </summary>
        protected void BeforeRefresh()
        {
            var randomString = UnityEngine.Random.value.ToString();
            var recordName = string.Format("Record {0} : {1} ", m_ItemActionCollectionField.Target.name, randomString);
            Undo.RecordObject(m_ItemActionCollectionField.Target, recordName);
        }

        /// <summary>
        /// Refresh after any changes.
        /// </summary>
        protected void Refresh()
        {
            m_ItemActionCollectionField.Refresh(true);
            Clear();
            SetLayout();
        }

        /// <summary>
        /// Set the layout of the field.
        /// </summary>
        protected void SetLayout()
        {
            Add(DefaultHeader());
            Add(TypeSpecificFields());
        }

        /// <summary>
        /// The header of the field.
        /// </summary>
        /// <returns>The visual element.</returns>
        protected VisualElement DefaultHeader()
        {
            var fullHeader = new VisualElement();
            var header = new VisualElement();

            m_TypeField = FilterWindowPopupField.CreateFilterWindowPopupField(typeof(ItemAction), FilterWindow.FilterType.Class, "Item Action Type", false, m_ItemAction.GetType(),
                (type) =>
                {
                    var changeType = EditorUtility.DisplayDialog("Change Item Action Type?",
                        $"You are trying to change the type of the Item Action. This action cannot be undone and some values may be lost.\n" +
                        $"Are you sure you would like to change the type?",
                        "Yes",
                        "No");

                    if (changeType) {
                        BeforeRefresh();

                        var previousItemAction = m_ItemAction;
                        m_ItemAction = (ItemAction)Activator.CreateInstance(type as Type);
                        ReflectionUtility.ObjectCopy(previousItemAction, m_ItemAction);
                        m_TypeField.UpdateSelectedObject(type);

                        m_ItemActionCollectionField.ItemActions.ItemActions[m_IndexInList] = m_ItemAction;
                        m_ItemActionCollectionField.ItemActions.Serialize();

                        Refresh();
                        m_ItemActionCollectionField.ReorderableList.SelectedIndex = m_IndexInList;
                    }
                });
            m_TypeField.label = "Item Action";
            header.Add(m_TypeField);
            fullHeader.Add(header);
            return fullHeader;
        }

        /// <summary>
        /// The components specific to the Item Action type selected.
        /// </summary>
        /// <returns>The field.</returns>
        protected VisualElement TypeSpecificFields()
        {
            UnityEngine.Object target = null;
            if (m_ItemActionCollectionField != null) {
                target = m_ItemActionCollectionField.Target;
            }

            m_OptionsContainer.Clear();

            FieldInspectorView.AddFields(target, m_ItemAction, Shared.Utility.MemberVisibility.Public, m_OptionsContainer,
                (object obj) =>
                {
                    BeforeRefresh();
                    m_ItemActionCollectionField.ItemActions.Serialize();
                    m_ItemActionCollectionField.Refresh(false);
                });

            return m_OptionsContainer;
        }
    }
}