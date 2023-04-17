/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Currency
{
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.UI.Views;
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// The currency UI.
    /// </summary>
    [Serializable]
    public class CurrencyView : View<CurrencyAmount>
    {
        [Tooltip("Hide the component is the currency amount is zero.")]
        [SerializeField] protected bool m_HideIfZero;
        [Tooltip("The image.")]
        [SerializeField] protected Image m_Image;
        [Tooltip("The text.")]
        [SerializeField] protected Text m_Text;

        public Image Image => m_Image;
        public Text Text => m_Text;

        /// <summary>
        /// Set the currency amount.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public override void SetValue(CurrencyAmount newValue)
        {
            base.SetValue(newValue);
            if (m_HideIfZero && newValue.Amount == 0) {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            if (m_Image != null) {
                m_Image.sprite = newValue.Currency?.Icon;
            }
            
            m_Text.text = newValue.Amount.ToString();
        }

        /// <summary>
        /// Clear the view.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            
            if (m_HideIfZero) {
                gameObject.SetActive(false);
                return;
            }

            if (m_Image != null) { m_Image.sprite = null; }

            m_Text.text = "";
        }
    }
}