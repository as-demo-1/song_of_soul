/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.UI.VisualStructures.AttributeUIs
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Demo.CharacterControl.Player;
    using Opsive.UltimateInventorySystem.Equipping;
    using Opsive.UltimateInventorySystem.UI.Item.AttributeViewModules;
    using UnityEngine;
    using UnityEngine.UI;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// Compare an attribute value to the matching character equipper item attribute value.
    /// </summary>
    public class DemoCompareAttributeView : AttributeViewModule
    {
        [Tooltip("The stat name.")]
        [SerializeField] protected string m_StatName;
        [Tooltip("The current value text.")]
        [SerializeField] protected Text m_CurrentValueText;
        [Tooltip("The new potential value text.")]
        [SerializeField] protected Text m_NewValueText;
        [Tooltip("The arrow image that can change color.")]
        [SerializeField] protected Image m_ArrowImage;

        protected PlayerCharacter m_PlayerCharacter;
        protected Equipper m_Equipper;

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

            var item = info.ItemInfo.Item;

            if (m_PlayerCharacter == null) { m_PlayerCharacter = info.ItemInfo.Inventory?.gameObject?.GetComponent<PlayerCharacter>(); }
            if (m_PlayerCharacter == null) { m_PlayerCharacter = FindObjectOfType<PlayerCharacter>(); }

            if (m_PlayerCharacter == null) {
                m_Equipper = FindObjectOfType<Equipper>();
            } else {
                m_Equipper = m_PlayerCharacter.Equipper as Equipper;
            }
            
            if (m_Equipper == null) {
                Debug.LogWarning("Player character or it's equipper was not found for attribute stat comparision.", gameObject);
                return;
            }

            var currentValue = m_Equipper.GetEquipmentStatInt(m_StatName);

            var previewValue = m_Equipper.IsEquipped(item)
                ? m_Equipper.GetEquipmentStatPreviewRemove(m_StatName, item)
                : m_Equipper.GetEquipmentStatPreviewAdd(m_StatName, item);

            m_CurrentValueText.text = currentValue.ToString();

            m_ArrowImage.color = previewValue > currentValue ? Color.green : previewValue < currentValue ? Color.red : Color.white;
            m_NewValueText.text = previewValue.ToString();
        }

        /// <summary>
        /// Clear the box.
        /// </summary>
        public override void Clear()
        {
            m_CurrentValueText.text = "?";
            m_ArrowImage.color = Color.grey;
            m_NewValueText.text = "?";
        }
    }
}