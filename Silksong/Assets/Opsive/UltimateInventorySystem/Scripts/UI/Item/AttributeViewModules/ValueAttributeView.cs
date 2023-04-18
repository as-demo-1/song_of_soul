/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.AttributeViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using UnityEngine;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// An attribute UI component used to display the attribute name and value as string.
    /// </summary>
    public class ValueAttributeView : AttributeViewModule
    {
        [Tooltip("The value text.")]
        [SerializeField] protected Text m_AttributeText;
        [Tooltip("Hide if the value is null.")]
        [SerializeField] protected bool m_ClearIfNullValue;

        /// <summary>
        /// Set the text.
        /// </summary>
        /// <param name="info">the attribute info.</param>
        public override void SetValue(AttributeInfo info)
        {
            if (info.Attribute == null) {
                Clear();
                return;
            }

            var value = info.Attribute.GetValueAsObject();

            if (m_ClearIfNullValue && (value == null || string.IsNullOrEmpty(value.ToString()))) {
                Clear();
                return;
            }

            m_AttributeText.text = value.ToString();
        }

        /// <summary>
        /// Clear the box.
        /// </summary>
        public override void Clear()
        {
            m_AttributeText.text = "";
        }
    }
}
