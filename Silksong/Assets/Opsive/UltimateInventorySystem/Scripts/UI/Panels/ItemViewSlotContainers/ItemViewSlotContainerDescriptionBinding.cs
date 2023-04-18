/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Item;
    using UnityEngine;

    /// <summary>
    /// Bind the item description to an Item View Slot Container to show the description of the selected item.
    /// </summary>
    public class ItemViewSlotContainerDescriptionBinding : ItemViewSlotsContainerBinding
    {
        [Tooltip("The item description panel. Can be null.")]
        [SerializeField] internal ItemDescriptionPanelBinding m_ItemDescriptionPanel;
        [Tooltip("The item description panel. Can be null.")]
        [SerializeField] internal ItemDescriptionBase m_ItemDescription;
        [Tooltip("The item description panel. Can be null.")]
        [SerializeField] protected bool m_HideIfSelectedItemIsNull;
        [Tooltip("Draw the description when an item view slot is selected.")]
        [SerializeField] protected bool m_DrawOnSelect = true;
        [Tooltip("Draw the description when an item view slot is clicked.")]
        [SerializeField] protected bool m_DrawOnClick = false;
        [Tooltip("Draw the description when an item view slot is clicked.")]
        [SerializeField] protected bool m_ClearOnDeselected = false;
        [Tooltip("Draw the description when an item view slot is clicked.")]
        [SerializeField] protected bool m_HideOnDeselected = false;

        public ItemDescriptionBase ItemDescription => m_ItemDescription;

        /// <summary>
        /// On bind.
        /// </summary>
        protected override void OnBindItemViewSlotContainer()
        {
            if (m_ItemDescriptionPanel != null) {
                m_ItemDescription = m_ItemDescriptionPanel.ItemDescription;
            }

            m_ItemViewSlotsContainer.OnItemViewSlotSelected += HandleOnSelect;
            m_ItemViewSlotsContainer.OnItemViewSlotClicked += HandleOnClicked;
            m_ItemViewSlotsContainer.OnItemViewSlotDeselected += HandleOnDeselect;
        }

        /// <summary>
        /// On unbind.
        /// </summary>
        protected override void OnUnbindItemViewSlotContainer()
        {
            m_ItemViewSlotsContainer.OnItemViewSlotSelected -= HandleOnSelect;
            m_ItemViewSlotsContainer.OnItemViewSlotClicked -= HandleOnClicked;
            m_ItemViewSlotsContainer.OnItemViewSlotDeselected -= HandleOnDeselect;
        }

        protected virtual void HandleOnSelect(ItemViewSlotEventData sloteventdata)
        {
            if(m_DrawOnSelect == false){ return; }
            DrawDescription(sloteventdata);
        }

        protected virtual void HandleOnDeselect(ItemViewSlotEventData sloteventdata)
        {
            if (m_ClearOnDeselected) {
                ClearDescription();
            }

            if (m_HideOnDeselected) {
                Hide(true);
            }
        }

        protected virtual void HandleOnClicked(ItemViewSlotEventData sloteventdata)
        {
            if(m_DrawOnClick == false){ return; }
            DrawDescription(sloteventdata);
        }

        /// <summary>
        /// Draw the description.
        /// </summary>
        /// <param name="sloteventdata">The slot event.</param>
        private void DrawDescription(ItemViewSlotEventData sloteventdata)
        {
            var itemInfo = sloteventdata?.ItemView?.ItemInfo;
            if (itemInfo.HasValue == false || itemInfo.Value.Item == null) {
                ClearDescription();
                return;
            }

            DrawDescription(itemInfo.Value);
        }

        /// <summary>
        /// Clear the description on the item.
        /// </summary>
        /// <param name="index">The index.</param>
        private void ClearDescription()
        {
            if (m_ItemDescription == null) { return; }
            m_ItemDescription.Clear();

            if (m_HideIfSelectedItemIsNull) {
                Hide(true);
            }
        }

        private void Hide(bool hide)
        {
            if (m_ItemDescriptionPanel != null) {
                if (hide) {
                    m_ItemDescriptionPanel.DisplayPanel.Close();
                } else {
                    m_ItemDescriptionPanel.DisplayPanel.Open();
                }
                
            } else {
                m_ItemDescription.gameObject.SetActive(hide == false);
            }
        }

        /// <summary>
        /// Draw the description for an item.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="index">The index.</param>
        protected virtual void DrawDescription(ItemInfo itemInfo)
        {
            if (m_ItemDescription == null) { return; }

            m_ItemDescription.SetValue(itemInfo);

            if (m_HideIfSelectedItemIsNull) {
                Hide(false);
            }
        }
    }
}