/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.UI.Menus.Upgrade
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.DatabaseNames;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    /// <summary>
    /// Item upgrade panel used to add upgrade items to item slots.
    /// </summary>
    public class ItemUpgradePanel : DisplayPanel
    {
        public event Action OnItemChanged;
        public event Action<Item, int> OnUpgradeSlotClicked;

        [Tooltip("The upgradable item view.")]
        [SerializeField] protected ItemView m_UpgradableItemView;
        [FormerlySerializedAs("m_UpgradeItemBoxes")]
        [Tooltip("The upgrade item view slots.")]
        [SerializeField] protected ItemViewSlot[] m_UpgradeItemViewSlots;
        [Tooltip("The upgrade item removers.")]
        [SerializeField] protected Button[] m_UpgradeRemoverButtons;
        [Tooltip("The slot count attribute name.")]
        [SerializeField] protected string m_SlotCountAttributeName;
        [Tooltip("The slots attribute name.")]
        [SerializeField] protected string m_SlotsAttributeName;

        protected ItemInfo m_UpgradeableItemInfo;
        protected int m_SlotCount;
        protected ItemAmounts m_Slots;
        protected List<ItemAmount> m_BeforePreviewSlots;

        public ItemAmounts Slots => m_Slots;

        public Inventory UpgradeItemsInventory { get; set; }

        public virtual int MaxSlotCount => m_UpgradeItemViewSlots.Length;

        /// <summary>
        /// Set up the panel.
        /// </summary>
        public override void Setup(DisplayPanelManager manager, bool force)
        {
            if (m_IsSetup) { return; }
            base.Setup(manager, force);

            if (m_Slots == null) { m_Slots = new ItemAmounts(MaxSlotCount); }

            if (m_BeforePreviewSlots == null) { m_BeforePreviewSlots = new List<ItemAmount>(); }

            if (string.IsNullOrWhiteSpace(m_SlotsAttributeName)) { m_SlotsAttributeName = "Slots"; }
            if (string.IsNullOrWhiteSpace(m_SlotCountAttributeName)) { m_SlotCountAttributeName = "SlotCount"; }

            for (int i = 0; i < m_UpgradeItemViewSlots.Length; i++) {
                int localI = i;
                m_UpgradeItemViewSlots[i].OnSubmitE += () => ItemButtonClicked(localI);
                m_UpgradeItemViewSlots[i].ItemView.Clear();
            }

            for (int i = 0; i < m_UpgradeRemoverButtons.Length; i++) {
                int localI = i;
                m_UpgradeRemoverButtons[i].onClick.AddListener(() => RemoveButtonClicked(localI));
            }
        }

        /// <summary>
        /// Remove an upgrade from a slot.
        /// </summary>
        /// <param name="index">The slot index.</param>
        private void RemoveButtonClicked(int index)
        {
            SetUpgradeInSlot(null, index);
        }

        /// <summary>
        /// Get the slot button.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The button.</returns>
        public ItemViewSlot GetSlot(int index)
        {
            return m_UpgradeItemViewSlots[index];
        }

        /// <summary>
        /// The item was clicked.
        /// </summary>
        /// <param name="index">The index.</param>
        private void ItemButtonClicked(int index)
        {
            var upgradeBox = m_UpgradeItemViewSlots[index];

            OnUpgradeSlotClicked?.Invoke(upgradeBox.ItemInfo.Item, index);
        }

        /// <summary>
        /// Set the upgradable item.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        public void SetUpgradeableItemInfo(ItemInfo itemInfo)
        {
            m_UpgradeableItemInfo = itemInfo;
            m_UpgradableItemView.SetValue(m_UpgradeableItemInfo);

            if (RetrieveSlots() == false) {
                return;
            }

            DrawSlots();
        }

        /// <summary>
        /// Set hte upgrade item in the slot.
        /// </summary>
        /// <param name="item">The upgrade item.</param>
        /// <param name="index">The slot.</param>
        public void SetUpgradeInSlot(Item item, int index)
        {
            if (index < 0 || index >= m_Slots.Count) {
                return;
            }

            ResetFromPreview();

            var previousSlottedItem = m_Slots[index];
            m_Slots[index] = (1, item);

            //return item
            var addedItem = UpgradeItemsInventory.MainItemCollection.AddItem((previousSlottedItem, null));

            //remove item
            var removeItem = UpgradeItemsInventory.MainItemCollection.RemoveItem((ItemInfo)m_Slots[index]);

            ApplyUpgradeStatChanges();
            Draw();
            OnItemChanged?.Invoke();
        }

        /// <summary>
        /// Preview the upgrade by simulating the add.
        /// </summary>
        /// <param name="item">The upgrade item.</param>
        /// <param name="index">The slot index.</param>
        public void SetUpgradePreviewInSlot(Item item, int index)
        {
            if (index < 0 || index >= m_Slots.Count) {
                return;
            }

            m_BeforePreviewSlots.Clear();
            for (int i = 0; i < m_Slots.Count; i++) {
                m_BeforePreviewSlots.Add(m_Slots[i]);
            }

            m_Slots[index] = (1, item);
            ApplyUpgradeStatChanges();
        }

        /// <summary>
        /// Reset the preview.
        /// </summary>
        public void ResetFromPreview()
        {
            for (int i = 0; i < m_BeforePreviewSlots.Count; i++) {
                m_Slots[i] = m_BeforePreviewSlots[i];
            }
            m_BeforePreviewSlots.Clear();
            
            ApplyUpgradeStatChanges();
        }

        /// <summary>
        /// Draw the panel.
        /// </summary>
        public void Draw()
        {
            m_UpgradableItemView.Refresh();
            DrawSlots();
        }

        /// <summary>
        /// Draw the upgrade slots.
        /// </summary>
        public virtual void DrawSlots()
        {

            for (int i = 0; i < m_UpgradeItemViewSlots.Length; i++) {
                if (m_SlotCount <= i) {
                    m_UpgradeItemViewSlots[i].gameObject.SetActive(false);
                    m_UpgradeRemoverButtons[i].gameObject.SetActive(false);
                    continue;
                }

                m_UpgradeRemoverButtons[i].gameObject.SetActive(true);
                m_UpgradeItemViewSlots[i].gameObject.SetActive(true);
                m_UpgradeItemViewSlots[i].SetItemInfo((m_Slots[i], null));
            }
        }

        /// <summary>
        /// Get the item slots from the item attributes.
        /// </summary>
        /// <returns>True if there was no error.</returns>
        protected virtual bool RetrieveSlots()
        {
            if (m_UpgradeableItemInfo.Item.TryGetAttributeValue(m_SlotCountAttributeName, out m_SlotCount) ==
                false) {
                Debug.LogWarning($"SlotCount attribute not found with name: {m_SlotCountAttributeName}.");
                return false;
            }

            var slotsAttribute = m_UpgradeableItemInfo.Item.GetAttribute<Attribute<ItemAmounts>>(m_SlotsAttributeName);

            if (slotsAttribute == null) {
                Debug.LogWarning($"Slot attribute not found with name: {m_SlotsAttributeName}.");
                return false;
            }

            if (slotsAttribute.VariantType != VariantType.Override) { slotsAttribute.SetOverrideValue(new ItemAmounts(m_SlotCount)); }
            m_Slots = slotsAttribute.GetValue();

            if (m_Slots != null && m_Slots.Count == m_SlotCount) { return true; }

            if (m_Slots == null) {
                m_Slots = new ItemAmounts(m_SlotCount);
            } else {
                m_Slots.Resize(m_SlotCount);
            }

            slotsAttribute.SetOverrideValue(m_Slots);

            return true;
        }

        /// <summary>
        /// Apply the upgrade stat changes to the attributes.
        /// </summary>
        protected virtual void ApplyUpgradeStatChanges()
        {
            if (m_UpgradeableItemInfo.Item == null) { return; }

            m_UpgradeableItemInfo.Item.name = GetFullItemWithSlotsName(m_UpgradeableItemInfo.Item, m_Slots);

            int constBoost = 0;
            float multBoost = 0;
            for (int i = 0; i < m_Slots.Count; i++) {
                constBoost += m_Slots[i].Item?.GetAttribute<Attribute<int>>("BoostConstant")?.GetValue() ?? 0;
                multBoost += m_Slots[i].Item?.GetAttribute<Attribute<float>>("BoostMultiplier")?.GetValue() ?? 0;
            }

            var attackAttribute = m_UpgradeableItemInfo.Item.GetAttribute<Attribute<int>>("Attack");
            if (attackAttribute != null) {
                var baseAttack = m_UpgradeableItemInfo.Item.GetAttribute<Attribute<int>>("BaseAttack").GetValue();

                attackAttribute.SetOverrideValue(baseAttack + constBoost + (int)(baseAttack * multBoost));
            }

            var defenseAttribute = m_UpgradeableItemInfo.Item.GetAttribute<Attribute<int>>("Defense");
            if (defenseAttribute != null) {
                var baseDefense = m_UpgradeableItemInfo.Item.GetAttribute<Attribute<int>>("BaseDefense").GetValue();

                defenseAttribute.SetOverrideValue(baseDefense + constBoost + (int)(baseDefense * multBoost));
            }
        }

        /// <summary>
        /// Returns the full name for an item with upgrades.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="itemSlots">The item slots.</param>
        /// <returns>The full item name.</returns>
        public static string GetFullItemWithSlotsName(Item item, ItemAmounts itemSlots)
        {
            if (itemSlots == null || itemSlots.Count == 0) { return item.ItemDefinition.name; }

            string prefix = "";
            string suffix = "";
            var count = 0;
            for (int i = 0; i < itemSlots.Count; i++) {
                if (itemSlots[i].Item == null) { continue; }

                count++;
                var itemPrefix = itemSlots[i].Item.GetAttribute<Attribute<string>>(DemoInventoryDatabaseNames.UpgradeItem.prefix).GetValue();
                if (!string.IsNullOrWhiteSpace(itemPrefix)) {
                    prefix += itemPrefix + " ";
                }
                var itemSuffix = itemSlots[i].Item.GetAttribute<Attribute<string>>(DemoInventoryDatabaseNames.UpgradeItem.suffix).GetValue();
                if (!string.IsNullOrWhiteSpace(itemSuffix)) {
                    suffix += itemSuffix + " ";
                }
            }

            if (count == 0) { return item.ItemDefinition.name; }

            return String.Format("{0} {1} {2} (+{3})", prefix, item?.ItemDefinition?.name, suffix, count);
        }
    }
}