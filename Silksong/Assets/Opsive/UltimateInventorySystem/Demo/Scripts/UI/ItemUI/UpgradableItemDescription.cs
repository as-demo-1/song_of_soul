/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.UI.ItemUI
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Demo.UI.Menus.Upgrade;
    using Opsive.UltimateInventorySystem.UI.Item;
    using UnityEngine;

    /// <summary>
    /// Upgradable item description.
    /// </summary>
    public class UpgradableItemDescription : ItemDescription
    {
        [Tooltip("The item upgrade panel.")]
        [SerializeField] protected ItemUpgradePanel m_ItemUpgradePanel;
        [Tooltip("The item upgrades description UI.")]
        [SerializeField] protected ItemDescriptionBase[] m_UpgradesDescription;

        protected override void Initialize(bool force)
        {
            if (m_Initialized && !force) { return; }

            for (int i = 0; i < m_UpgradesDescription.Length; i++) {
                m_UpgradesDescription[i].Initialize();
            }

            base.Initialize(force);
        }

        /// <summary>
        /// Draw an empty description.
        /// </summary>
        protected override void OnClear()
        {
            base.OnClear();
            for (int i = 0; i < m_UpgradesDescription.Length; i++) {
                m_UpgradesDescription[i].Hide(true);
            }
        }

        /// <summary>
        /// Draw the item description.
        /// </summary>
        protected override void OnSetValue()
        {
            base.OnSetValue();

            for (int i = 0; i < m_UpgradesDescription.Length; i++) {
                if (i < m_ItemUpgradePanel.Slots.Count) {
                    var upgradeItemAmount = m_ItemUpgradePanel.Slots[i];
                    m_UpgradesDescription[i].Hide(upgradeItemAmount.Item == null);
                    m_UpgradesDescription[i].SetValue((ItemInfo)upgradeItemAmount);
                    continue;
                }
                m_UpgradesDescription[i].Hide(true);
            }
        }
    }
}
