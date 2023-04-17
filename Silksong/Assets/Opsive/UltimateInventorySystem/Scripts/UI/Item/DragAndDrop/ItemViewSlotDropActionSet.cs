/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------


namespace Opsive.UltimateInventorySystem.UI.Item.DragAndDrop
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.UI.Item.DragAndDrop.DropActions;
    using Opsive.UltimateInventorySystem.UI.Item.DragAndDrop.DropConditions;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A scriptable object used to define the possible item view drop actions.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemViewSlotDropActionSet",
        menuName = "Ultimate Inventory System/UI/Item View Slot Drop Action Set", order = 1)]
    public class ItemViewSlotDropActionSet : ScriptableObject
    {
        [Tooltip("The serialized data of actions with conditions.")]
        [SerializeField] protected Serialization[] m_ActionsWithConditionsData;

        protected ItemViewDropActionsWithConditions[] m_ActionsWithConditions;

        protected bool m_Initialized;

        public ItemViewDropActionsWithConditions[] ActionsWithConditions {
            get => m_ActionsWithConditions;
            set => m_ActionsWithConditions = value;
        }

        /// <summary>
        /// Default constructor adds the basics.
        /// </summary>
        public ItemViewSlotDropActionSet()
        {
            ResetActionsConditionsToDefault();
        }

        /// <summary>
        /// Reset the Actions and Conditions to their default value.
        /// </summary>
        public void ResetActionsConditionsToDefault()
        {
            m_ActionsWithConditions = new ItemViewDropActionsWithConditions[2];

            // Same container move.
            m_ActionsWithConditions[0] = new ItemViewDropActionsWithConditions();
            m_ActionsWithConditions[0].Conditions = new ItemViewDropCondition[1];
            m_ActionsWithConditions[0].Actions = new ItemViewDropAction[1];

            m_ActionsWithConditions[0].Conditions[0] = new ItemViewDropContainerCanMoveCondition();
            m_ActionsWithConditions[0].Actions[0] = new ItemViewDropMoveIndexAction();

            // Container Exchange
            m_ActionsWithConditions[1] = new ItemViewDropActionsWithConditions();
            m_ActionsWithConditions[1].Conditions = new ItemViewDropCondition[1];
            m_ActionsWithConditions[1].Actions = new ItemViewDropAction[1];

            m_ActionsWithConditions[1].Conditions[0] = new ItemViewDropContainerCanSmartExchangeCondition();
            m_ActionsWithConditions[1].Actions[0] = new ItemViewDropContainerSmartExchangeAction();

            Serialize();
        }

        /// <summary>
        /// Initializes the scriptable object to deserialize the abstract arrays.
        /// </summary>
        /// <param name="force">Force initialize the object.</param>
        public virtual void Initialize(bool force)
        {
            if (m_Initialized && !force) { return; }

            Deserialize();
        }


        /// <summary>
        /// Deserialize all the properties of the ItemDefinition.
        /// </summary>
        internal void Deserialize()
        {
            if (m_ActionsWithConditionsData == null) {
                if (m_ActionsWithConditions == null) { m_ActionsWithConditions = new ItemViewDropActionsWithConditions[0]; }
                return;
            }

            m_ActionsWithConditions = new ItemViewDropActionsWithConditions[m_ActionsWithConditionsData.Length];
            for (int i = 0; i < m_ActionsWithConditionsData.Length; i++) {
                var actionsWithConditions = m_ActionsWithConditionsData[i].DeserializeFields(MemberVisibility.Public) as ItemViewDropActionsWithConditions;
                m_ActionsWithConditions[i] = actionsWithConditions;
            }
        }

        /// <summary>
        /// Serialize all the properties of the itemDefinition.
        /// </summary>
        public void Serialize()
        {
            m_ActionsWithConditionsData = Serialization.Serialize(m_ActionsWithConditions as IList<ItemViewDropActionsWithConditions>);
        }

        /// <summary>
        /// Handle Item View Slot drop.
        /// </summary>
        /// <param name="itemViewDropHandler">The item view drop handler.</param>
        public void HandleItemViewSlotDrop(ItemViewDropHandler itemViewDropHandler)
        {
            var index = GetFirstPassingConditionIndex(itemViewDropHandler);
            if (index == -1) { return; }

            m_ActionsWithConditions[index].Drop(itemViewDropHandler);
        }

        /// <summary>
        /// Get the first passing condition index within the list of actions conditions.
        /// </summary>
        /// <param name="itemViewDropHandler">The item view drop handler.</param>
        /// <returns>The index, -1 if non-found.</returns>
        public int GetFirstPassingConditionIndex(ItemViewDropHandler itemViewDropHandler)
        {
            for (int i = 0; i < m_ActionsWithConditions.Length; i++) {
                if (m_ActionsWithConditions[i].CanDrop(itemViewDropHandler)) { return i; }
            }

            return -1;
        }

        /// <summary>
        /// Get the first passing action condition.
        /// </summary>
        /// <param name="itemViewDropHandler">The item view drop handler.</param>
        /// <returns>The passing actions conditions.</returns>
        public ItemViewDropActionsWithConditions GetFirstPassingCondition(ItemViewDropHandler itemViewDropHandler)
        {
            for (int i = 0; i < m_ActionsWithConditions.Length; i++) {
                if (m_ActionsWithConditions[i].CanDrop(itemViewDropHandler)) { return m_ActionsWithConditions[i]; }
            }

            return null;
        }
    }

    /// <summary>
    /// Base class for an Item View Drop Action.
    /// </summary>
    [Serializable]
    public abstract class ItemViewDropAction
    {
        /// <summary>
        /// Action Invoked when an Item View is dropped.
        /// </summary>
        /// <param name="itemViewDropHandler">The item view drop handler.</param>
        public abstract void Drop(ItemViewDropHandler itemViewDropHandler);

        /// <summary>
        /// Custom string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return GetType().Name.Replace("ItemViewDrop", "").Replace("Action", "");
        }
    }

    /// <summary>
    /// Base class for an Item View Drop Condition.
    /// </summary>
    [Serializable]
    public abstract class ItemViewDropCondition
    {
        /// <summary>
        /// The condition used to know whether or not an drop action should be executed.
        /// </summary>
        /// <param name="itemViewDropHandler">The item view drop handler.</param>
        /// <returns>Should the drop be executed?</returns>
        public abstract bool CanDrop(ItemViewDropHandler itemViewDropHandler);

        /// <summary>
        /// Custom string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return GetType().Name.Replace("ItemViewDrop", "").Replace("Condition", "");
        }
    }

    /// <summary>
    /// Contains a list of Actions and a list of conditions.
    /// </summary>
    [Serializable]
    public class ItemViewDropActionsWithConditions
    {
        [Tooltip("Item View Drop conditions.")]
        [SerializeField] protected ItemViewDropCondition[] m_Conditions;
        [Tooltip("Item View drop actions..")]
        [SerializeField] protected ItemViewDropAction[] m_Actions;


        public ItemViewDropCondition[] Conditions {
            get => m_Conditions;
            set => m_Conditions = value;
        }

        public ItemViewDropAction[] Actions {
            get => m_Actions;
            set => m_Actions = value;
        }

        /// <summary>
        /// Do the conditions pass for the drop.
        /// </summary>
        /// <param name="itemViewDropHandler">The item view drop handler.</param>
        /// <returns>True if the conditions pass.</returns>
        public bool CanDrop(ItemViewDropHandler itemViewDropHandler)
        {
            for (int i = 0; i < m_Conditions.Length; i++) {
                if (m_Conditions[i].CanDrop(itemViewDropHandler) == false) { return false; }
            }

            return true;
        }

        /// <summary>
        /// Invoke the actions.
        /// </summary>
        /// <param name="itemViewDropHandler">The item view drop handler.</param>
        public void Drop(ItemViewDropHandler itemViewDropHandler)
        {
            for (int i = 0; i < m_Actions.Length; i++) { m_Actions[i].Drop(itemViewDropHandler); }
        }
    }
}