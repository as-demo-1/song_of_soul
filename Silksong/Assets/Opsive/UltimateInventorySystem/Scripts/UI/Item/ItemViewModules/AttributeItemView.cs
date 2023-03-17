/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using UnityEngine;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// Item View component displaying attribute value.
    /// </summary>
    public class AttributeItemView : ItemViewModule
    {
        [Tooltip("The attribute name. Must be an integer.")]
        [SerializeField] protected string m_AttributeName;
        [Tooltip("The attribute value text.")]
        [SerializeField] protected Text m_AttributeValueText;
        [Tooltip("Default text value.")]
        [SerializeField] protected string m_DefaultTextValue;
        [Tooltip("Disable If Attribute Missing.")]
        [SerializeField] protected GameObject m_DisableOnClear;

        public string AttributeName {
            get => m_AttributeName;
            set => m_AttributeName = value;
        }

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (info.Item == null || info.Item.IsInitialized == false) {
                Clear();
                return;
            }
            
            var attribute = info.Item.GetAttribute(m_AttributeName);
            if (attribute == null) {
                Clear();
                return;
            }
            
            if (m_DisableOnClear != null) {
                m_DisableOnClear.SetActive(true);
            }

            m_AttributeValueText.text = attribute.GetValueAsObject()?.ToString();
        }

        /// <summary>
        /// Clear the value.
        /// </summary>
        public override void Clear()
        {
            m_AttributeValueText.text = m_DefaultTextValue;
            if (m_DisableOnClear != null) {
                m_DisableOnClear.SetActive(false);
            }
        }
    }
}