/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers
{
    using Opsive.UltimateInventorySystem.UI.Item;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// The item description panel.
    /// </summary>
    public class ItemDescriptionPanelBinding : DisplayPanelBinding
    {

        [FormerlySerializedAs("m_ItemDescriptionDisplay")]
        [Tooltip("The item description UI.")]
        [SerializeField] protected ItemDescriptionBase m_ItemDescription;

        public ItemDescriptionBase ItemDescription {
            get => m_ItemDescription;
            set => m_ItemDescription = value;
        }

        /// <summary>
        /// Set up the panel.
        /// </summary>
        public override void Initialize(DisplayPanel display, bool force)
        {
            var wasInitialized = m_IsInitialized;
            if (wasInitialized && !force) { return; }
            base.Initialize(display, force);

            if (m_ItemDescription == null) { return; }
            m_ItemDescription.Initialize();
        }
    }
}