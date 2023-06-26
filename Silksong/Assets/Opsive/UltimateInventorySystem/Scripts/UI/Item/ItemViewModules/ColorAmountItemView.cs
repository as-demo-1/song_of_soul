/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using UnityEngine;
    using UnityEngine.UI;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// Item View component that shows the item name.
    /// </summary>
    public class ColorAmountItemView : ItemViewModule
    {
        [Tooltip("The text to color.")]
        [SerializeField] protected Text m_Text;
        [Tooltip("The image to color.")]
        [SerializeField] protected Image m_Image;
        [Tooltip("The threshold that will decide which color is set (Below Inclusive).")]
        [SerializeField] protected int m_Threshold;
        [Tooltip("The Color that will be set when the amount is above the threshold.")]
        [SerializeField] protected Color m_AboveThresholdColor;
        [Tooltip("The Color that will be set when the amount is below the threshold.")]
        [SerializeField] protected Color m_BelowThresholdColor;

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (info.Item == null) {
                Clear();
                return;
            }

            if (info.Amount > m_Threshold) {
                m_Text.color = m_AboveThresholdColor;
                if (m_Image != null) { m_Image.color = m_AboveThresholdColor; }
            } else {
                m_Text.color = m_BelowThresholdColor;
                if (m_Image != null) { m_Image.color = m_BelowThresholdColor; }
            }
        }

        /// <summary>
        /// Clear the value.
        /// </summary>
        public override void Clear()
        {
            m_Text.color = m_AboveThresholdColor;
            if (m_Image != null) { m_Image.color = m_AboveThresholdColor; }
        }
    }
}