/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// A Item View UI component that lets you bind the color of an icon to an item attribute.
    /// </summary>
    public class ValueToColorItemView : ItemViewModule
    {
        [Tooltip("The attribute name which holds the integer or enum value.")]
        [SerializeField] protected string m_AttributeName;
        [Tooltip("The icon image.")]
        [SerializeField] protected Image m_Icon;
        [Tooltip("The missing item icon sprite.")]
        [SerializeField] protected Color[] m_Colors;
        [Tooltip("The missing item icon sprite.")]
        [SerializeField] protected Color m_MissingColor;
        [Tooltip("Disable the image component if item is null.")]
        [SerializeField] protected bool m_DisableOnClear;

        /// <summary>
        /// Set the item info.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (info.Item == null || info.Item.IsInitialized == false) {
                Clear();
                return;
            }

            m_Icon.enabled = true;
            if (info.Item.TryGetAttributeValue<int>(m_AttributeName, out var index) &&
                index >= 0 && index < m_Colors.Length) {
                m_Icon.color = m_Colors[index];
                return;
            }
            if (info.Item.TryGetAttribute(m_AttributeName, out var enumAttribute)) {
                if (enumAttribute.GetValueAsObject() is Enum enumValue) {
                    var integerValue = Convert.ToInt32(enumValue);
                    if (integerValue >= 0 && integerValue < m_Colors.Length) {
                        m_Icon.color = m_Colors[integerValue];
                        return;
                    }
                }
            }

            m_Icon.color = m_MissingColor;
        }

        /// <summary>
        /// Clear the component.
        /// </summary>
        public override void Clear()
        {
            m_Icon.color = m_MissingColor;
            if (m_DisableOnClear) { m_Icon.enabled = false; }

        }
    }
}