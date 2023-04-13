/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item
{
    using UnityEngine;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// The Item description UI.
    /// </summary>
    public class ItemDescription : ItemDescriptionBase
    {
        [Tooltip("The attribute name for the description.")]
        [SerializeField] protected string m_DescriptionAttributeName = "Description";
        [Tooltip("The item name.")]
        [SerializeField] protected Text m_ItemNameText;
        [Tooltip("The description text.")]
        [SerializeField] protected Text m_DescriptionText;
        [Tooltip("The text displayed when no item is not selected.")]
        [SerializeField] protected string m_NoItemSelectedMessage = "No Item selected";
        [Tooltip("The text displayed when the item does not have a description.")]
        [SerializeField] protected string m_NoDescriptionMessage = "The item does not have a description";

        /// <summary>
        /// Draw the description
        /// </summary>
        protected override void OnSetValue()
        {
            m_ItemNameText.text = ItemInfo.Item.name;

            if (ItemInfo.Item.TryGetAttributeValue<string>(m_DescriptionAttributeName,
                out var descriptionValue)) {
                m_DescriptionText.text = descriptionValue;
            } else {
                m_DescriptionText.text = m_NoDescriptionMessage;
            }
        }

        /// <summary>
        /// Draw an empty description.
        /// </summary>
        protected override void OnClear()
        {
            m_ItemNameText.text = m_NoItemSelectedMessage;

            m_DescriptionText.text = "";
        }
    }
}
