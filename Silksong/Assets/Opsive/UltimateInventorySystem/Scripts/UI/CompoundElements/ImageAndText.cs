/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.CompoundElements
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// A UI element with an image and a text.
    /// </summary>
    [Serializable]
    public class ImageAndText
    {
        [Tooltip("The image.")]
        [SerializeField] protected Image m_Image;
        [Tooltip("The text.")]
        [SerializeField] protected Text m_Text;

        public Image Image => m_Image;
        public Text Text => m_Text;

        /// <summary>
        /// Set the Icon and the text.
        /// </summary>
        /// <param name="icon">The icon.</param>
        /// <param name="text">The text.</param>
        public virtual void SetIconAndText(Sprite icon, string text)
        {
            SetIcon(icon);
            SetText(text);
        }

        /// <summary>
        /// Set the text.
        /// </summary>
        /// <param name="text">The text.</param>
        public virtual void SetText(string text)
        {
            m_Text.text = text;
        }

        /// <summary>
        /// The icon.
        /// </summary>
        /// <param name="icon">The icon.</param>
        public virtual void SetIcon(Sprite icon)
        {
            m_Image.sprite = icon;
        }
    }
}