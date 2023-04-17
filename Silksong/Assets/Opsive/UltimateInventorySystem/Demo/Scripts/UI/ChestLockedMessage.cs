/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.UI
{
    using UnityEngine;

    /// <summary>
    /// The chest locked message displays a message whether the character has the key or not.
    /// </summary>
    public class ChestLockedMessage : MonoBehaviour
    {
        [Tooltip("The text that will be displayed when the interactor does not have the key.")]
        [SerializeField] protected string m_TextIfNoKey;
        [Tooltip("The text that will be displayed when the interactor does have the key.")]
        [SerializeField] protected string m_TextHasKey;
        [Tooltip("The amount of second the text will hte displayed for.")]
        [SerializeField] protected float m_TextDisplayTime;
        [Tooltip("The text panel component.")]
        [SerializeField] protected TextPanel m_TextPanel;

        /// <summary>
        /// Initialize.
        /// </summary>
        private void Awake()
        {
            if (m_TextPanel == null) {
                m_TextPanel = FindObjectOfType<TextPanel>();
            }
        }

        /// <summary>
        /// No key to open.
        /// </summary>
        public void OnNoKey()
        {
            if (m_TextPanel != null) {
                m_TextPanel.DisplayText(m_TextIfNoKey, m_TextDisplayTime);
            }
        }

        /// <summary>
        /// Has key to open.
        /// </summary>
        public void HasKey()
        {
            if (m_TextPanel != null) {
                m_TextPanel.DisplayText(m_TextHasKey, m_TextDisplayTime);
            }
        }
    }
}