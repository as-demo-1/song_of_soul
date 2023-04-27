/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// Tab toggle is used by a TabController to create tabs.
    /// </summary>
    public class TabToggle : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("The name text.")]
        [SerializeField] protected Text m_NameText;
        [Tooltip("The icon image.")]
        [SerializeField] protected Image m_Icon;
        [Tooltip("The icons displaying the page index and count.")]
        [SerializeField] protected Image[] m_PageIcons;
        [Tooltip("The objects to disable when the tab is off.")]
        [SerializeField] protected GameObject[] m_DisableWhenOff;
        [Tooltip("The color when the tab is on.")]
        [SerializeField] protected Color m_ColorOn = Color.white;
        [Tooltip("The color when the tab is off.")]
        [SerializeField] protected Color m_ColorOff = Color.gray;

        protected bool m_IsOn;
        protected int m_PageCount;
        protected int m_SelectedPageIndex;
        protected int m_TabIndex;
        protected ITabControl m_TabControl;

        public bool IsOn => m_IsOn;
        public int PageCount => m_PageCount;
        public int SelectedPageIndex => m_SelectedPageIndex;

        /// <summary>
        /// Set the tab control that controls this tab toggle.
        /// </summary>
        /// <param name="tabControl">The tab control.</param>
        /// <param name="index">The index of the tab within the tab control.</param>
        public virtual void SetControl(ITabControl tabControl, int index)
        {
            m_TabControl = tabControl;
            m_TabIndex = index;

            ToggleOff(true);
        }

        /// <summary>
        /// Toggle the tab on.
        /// </summary>
        public virtual void ToggleOn()
        {
            if (m_IsOn) { return; }

            m_Icon.color = m_ColorOn;

            for (int i = 0; i < m_DisableWhenOff.Length; i++) {
                m_DisableWhenOff[i].SetActive(true);
            }

            m_IsOn = true;
        }

        /// <summary>
        /// Toogle the tab off.
        /// </summary>
        public virtual void ToggleOff(bool force = false)
        {
            if (m_IsOn == false && !force) { return; }

            m_Icon.color = m_ColorOff;

            for (int i = 0; i < m_DisableWhenOff.Length; i++) {
                m_DisableWhenOff[i].SetActive(false);
            }

            m_IsOn = false;
        }

        /// <summary>
        /// Set the page count.
        /// </summary>
        /// <param name="pageCount">The page count.</param>
        public virtual void SetPageCount(int pageCount)
        {
            m_PageCount = pageCount;

            if (m_PageCount > m_PageIcons.Length) {
                Debug.LogWarning("There are not enough page icons available.");
            }

            for (int i = 0; i < m_PageIcons.Length; i++) {
                m_PageIcons[i].gameObject.SetActive(i < m_PageCount);
            }
        }

        /// <summary>
        /// Set the page index.
        /// </summary>
        /// <param name="index">The index.</param>
        public virtual void SetPageIndex(int index)
        {
            if (index < 0 || index >= m_PageCount && index != 0) {
            }

            m_SelectedPageIndex = index;

            for (int i = 0; i < m_PageIcons.Length; i++) {
                if (i == index) {
                    m_PageIcons[i].color = m_ColorOn;
                } else {
                    m_PageIcons[i].color = m_ColorOff;
                }

            }
        }

        /// <summary>
        /// Set the text to display.
        /// </summary>
        /// <param name="text">The text.</param>
        public virtual void SetText(string text)
        {
            m_NameText.text = text;
        }

        /// <summary>
        /// Set the text to display.
        /// </summary>
        /// <param name="text">The text.</param>
        public virtual string GetText()
        {
            return m_NameText.text;
        }

        /// <summary>
        /// Set the icon to display.
        /// </summary>
        /// <param name="icon">The icon.</param>
        public virtual void SetIcon(Sprite icon)
        {
            m_Icon.sprite = icon;
        }

        /// <summary>
        /// Toggle the tab on when it is clicked.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            m_TabControl?.SetTabOn(m_TabIndex);
        }
    }
}
