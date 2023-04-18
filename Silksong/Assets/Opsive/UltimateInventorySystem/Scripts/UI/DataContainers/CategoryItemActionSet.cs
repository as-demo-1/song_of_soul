/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.DataContainers
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.ItemActions;
    using UnityEngine;

    /// <summary>
    /// A scriptable object used to map categories to Item Actions.
    /// </summary>
    [CreateAssetMenu(fileName = "CategoryItemActionSet", menuName = "Ultimate Inventory System/Item Actions/Category Item Action Set", order = 51)]
    public class CategoryItemActionSet : ScriptableObject
    {
        [Tooltip("The item actions for each category.")]
        [SerializeField] protected ItemActionSet[] m_CategoriesItemActions;

        public ItemActionSet[] CategoryItemActionsArray {
            get => m_CategoriesItemActions;
            set => m_CategoriesItemActions = value;
        }

        /// <summary>
        /// Initialize on awake
        /// </summary>
        private void Awake()
        {
            if (m_CategoriesItemActions == null) { return; }
            for (int i = 0; i < m_CategoriesItemActions.Length; i++) {
                if (m_CategoriesItemActions[i] == null) { continue; }
                m_CategoriesItemActions[i].ItemActionCollection?.Initialize(false);
            }
        }

        /// <summary>
        /// Get the item actions for the item specified.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="actions">The item actions, can be resized.</param>
        /// <returns>The number of actions.</returns>
        public ListSlice<ItemAction> GetItemActionsForItem(Item item, ref ItemAction[] actions)
        {
            if (item == null) {
                return (actions, 0, 0);
            }
            
            var count = 0;
            for (int i = 0; i < m_CategoriesItemActions.Length; i++) {

                var categoriesItemAction = m_CategoriesItemActions[i];

                if (categoriesItemAction.MatchItem(item) == false) {
                    continue;
                }

                var isAnException = false;
                for (int j = 0; j < categoriesItemAction.ExceptionCategories.Length; j++) {
                    var exceptionCategory = categoriesItemAction.ExceptionCategories[j];
                    if (exceptionCategory == null) { continue; }

                    if (exceptionCategory.InherentlyContains(item)) {
                        isAnException = true;
                        break;
                    }
                }

                if (isAnException) { continue; }

                var itemActionCollection = categoriesItemAction.ItemActionCollection;
                itemActionCollection.Initialize(false);
                for (int j = 0; j < itemActionCollection.Count; j++) {
                    TypeUtility.ResizeIfNecessary(ref actions, count + 1);

                    actions[count] = itemActionCollection[j];
                    count++;
                }
            }

            return (actions, 0, count);
        }

        /// <summary>
        /// Use the item action specified by the index on the item referenced.
        /// </summary>
        /// <param name="itemInfo">The item info to use.</param>
        /// <param name="itemUser">The item user.</param>
        /// <param name="actionIndex">The item action index to use.</param>
        /// <returns>Invoked the action?.</returns>
        public bool UseItemAction(ItemInfo itemInfo, ItemUser itemUser, int actionIndex)
        {
            var count = 0;

            for (int i = 0; i < m_CategoriesItemActions.Length; i++) {
                if (m_CategoriesItemActions[i].ItemCategory != null && !m_CategoriesItemActions[i].ItemCategory.InherentlyContains(itemInfo.Item)) {
                    continue;
                }

                var itemActionCollection = m_CategoriesItemActions[i].ItemActionCollection;
                itemActionCollection.Initialize(false);
                for (int j = 0; j < itemActionCollection.Count; j++) {

                    if (count == actionIndex) {
                        itemActionCollection[j].InvokeAction(itemInfo, itemUser);
                        return true;
                    }
                    count++;
                }
            }

            return false;
        }

        /// <summary>
        /// Use all the item actions that fits the item category on the item referenced.
        /// </summary>
        /// <param name="itemInfo">The item info to use.</param>
        /// <param name="itemUser">The item user.</param>
        /// <returns>Invoked the action?.</returns>
        public void UseAllItemActions(ItemInfo itemInfo, ItemUser itemUser)
        {
            for (int i = 0; i < m_CategoriesItemActions.Length; i++) {
                if (m_CategoriesItemActions[i].ItemCategory != null && !m_CategoriesItemActions[i].ItemCategory.InherentlyContains(itemInfo.Item)) {
                    continue;
                }

                var itemActionCollection = m_CategoriesItemActions[i].ItemActionCollection;
                itemActionCollection.Initialize(false);
                for (int j = 0; j < itemActionCollection.Count; j++) {
                    itemActionCollection[j].InvokeAction(itemInfo, itemUser);
                }
            }
        }
    }
}
