/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Item.DragAndDrop;
    using Opsive.UltimateInventorySystem.UI.Views;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// A Item View UI component that lets you bind an icon to the item icon attribute.
    /// </summary>
    public class DropHoverIconPreviewItemView : ItemViewModuleSelectable, IItemViewSlotDropHoverSelectable
    {
        [Tooltip("The icon image.")]
        [SerializeField] protected Image m_ItemIcon;
        [Tooltip("The icon image.")]
        [SerializeField] protected Image m_ColorFilter;
        [Tooltip("The color when not condition have passed.")]
        [SerializeField] protected Color m_NoConditionsPassed;
        [Tooltip("The preview color when at least one condition has passed.")]
        [SerializeField] protected Color m_ConditionsPassed;
        [Tooltip("Should the Item View Source show the item within the destination Item View")]
        [SerializeField] protected bool m_DoNotPreviewInSource;

        /// <summary>
        /// Set the item info.
        /// </summary>
        /// <param name="info">The item .</param>
        public override void SetValue(ItemInfo info)
        {
            if (info.Item == null || info.Item.IsInitialized == false) {
                Clear();
                return;
            }
        }

        /// <summary>
        /// Clear the component.
        /// </summary>
        public override void Clear()
        {
            m_ItemIcon.enabled = false;
            m_ColorFilter.enabled = false;
        }

        /// <summary>
        /// Select with a drop handler.
        /// </summary>
        /// <param name="dropHandler">The drop handler.</param>
        public virtual void SelectWith(ItemViewDropHandler dropHandler)
        {
            var dropIndex =
                dropHandler.ItemViewSlotDropActionSet.GetFirstPassingConditionIndex(dropHandler);
            m_ColorFilter.color = dropIndex == -1 ? m_NoConditionsPassed : m_ConditionsPassed;

            var sourceItemInfo = dropHandler.SlotCursorManager.SourceItemViewSlot.ItemInfo;

            PreviewIcon(sourceItemInfo);

            if (m_DoNotPreviewInSource == false) {
                var sourceItemViewModules = dropHandler.SlotCursorManager.SourceItemViewSlot.ItemView.Modules;

                for (int i = 0; i < sourceItemViewModules.Count; i++) {
                    if (sourceItemViewModules[i] is ItemViewModule itemViewModule) {
                        itemViewModule.SetValue(ItemInfo);
                    }
                }
            }

            m_ColorFilter.enabled = true;
        }

        /// <summary>
        /// Preview the icon.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        protected virtual void PreviewIcon(ItemInfo itemInfo)
        {
            if (itemInfo.Item != null && itemInfo.Item.TryGetAttributeValue<Sprite>("Icon", out var icon)) {
                m_ItemIcon.sprite = icon;
                m_ItemIcon.enabled = true;
                return;
            }

            m_ItemIcon.enabled = false;
        }

        /// <summary>
        /// Deselect with the item view drop handler.
        /// </summary>
        /// <param name="dropHandler">The drop handler.</param>
        public virtual void DeselectWith(ItemViewDropHandler dropHandler)
        {
            var sourceBoxModules = dropHandler.SlotCursorManager.SourceItemViewSlot.ItemView.Modules;
            
            for (int i = 0; i < sourceBoxModules.Count; i++) {
                if (sourceBoxModules[i] is ItemViewModule itemViewModule) {
                    itemViewModule.SetValue(ItemInfo.None);
                }
            }

            Select(false);
        }

        /// <summary>
        /// Simple select/deselect.
        /// </summary>
        /// <param name="select">Select?</param>
        public override void Select(bool select)
        {
            if (select) { return; }
            if (ItemInfo.Item == null || ItemInfo.Item.IsInitialized == false) {
                Clear();
                return;
            }

            m_ColorFilter.enabled = false;
            if (ItemInfo.Item.TryGetAttributeValue<Sprite>("Icon", out var icon)) {
                m_ItemIcon.sprite = icon;
                m_ItemIcon.enabled = true;
                return;
            }

            m_ItemIcon.enabled = false;
        }
    }
}