/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.AttributeViewModules
{
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using UnityEngine;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// An attribute UI component used to display a float value.
    /// </summary>
    public class FloatValueAttributeView : AttributeViewModule
    {
        [Tooltip("The value text.")]
        [SerializeField] protected Text m_ValueText;
        [Tooltip("Clear the value if it is under a certain value.")]
        [SerializeField] protected float m_ClearBelowThreshold;
        [Tooltip("The format for the float value.")]
        [SerializeField] protected string m_Format = "{0:0.00}";

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
            if (info.Attribute is Attribute<float> floatAttribute) {

                var value = floatAttribute.GetValue();

                if (value < m_ClearBelowThreshold) {
                    Clear();
                    return;
                }

                m_ValueText.text = string.Format(m_Format, value);
            } else { m_ValueText.text = "?"; }
        }

        /// <summary>
        /// Clear the box.
        /// </summary>
        public override void Clear()
        {
            m_ValueText.text = "";
        }
    }
}