/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Views
{
    using UnityEngine;

    /// <summary>
    /// Box UI component for changing an image when the box is selected.
    /// </summary>
    public class DragOffView : ViewModule, IViewModuleMovable
    {
        [Tooltip("The image.")]
        [SerializeField] protected GameObject[] m_DeactivateOnDrag;
        [Tooltip("The default sprite.")]
        [SerializeField] protected Behaviour[] m_DisableOnDrag;
        [Tooltip("The selected sprite.")]
        [SerializeField] protected GameObject[] m_DeactivateOnLeftOver;
        [Tooltip("The selected sprite.")]
        [SerializeField] protected Behaviour[] m_DisableOnLeftOver;

        /// <summary>
        /// Clear.
        /// </summary>
        public override void Clear()
        {
            for (int i = 0; i < m_DeactivateOnDrag.Length; i++) {
                m_DeactivateOnDrag[i]?.SetActive(true);
            }
            for (int i = 0; i < m_DisableOnDrag.Length; i++) {
                if (m_DisableOnDrag[i] == null) { continue; }
                m_DisableOnDrag[i].enabled = true;
            }
            for (int i = 0; i < m_DeactivateOnLeftOver.Length; i++) {
                m_DeactivateOnLeftOver[i]?.SetActive(true);
            }
            for (int i = 0; i < m_DisableOnLeftOver.Length; i++) {
                if (m_DisableOnLeftOver[i] == null) { continue; }
                m_DisableOnLeftOver[i].enabled = true;
            }
        }

        /// <summary>
        /// Set the view as moving.
        /// </summary>
        public void SetAsMoving()
        {
            for (int i = 0; i < m_DeactivateOnDrag.Length; i++) {
                m_DeactivateOnDrag[i]?.SetActive(false);
            }
            for (int i = 0; i < m_DisableOnDrag.Length; i++) {
                if (m_DisableOnDrag[i] == null) { continue; }
                m_DisableOnDrag[i].enabled = false;
            }
        }

        /// <summary>
        /// Set the view as the source of a movement.
        /// </summary>
        /// <param name="movingSource">started moving or stopped?</param>
        public void SetAsMovingSource(bool movingSource)
        {
            for (int i = 0; i < m_DeactivateOnLeftOver.Length; i++) {
                m_DeactivateOnLeftOver[i]?.SetActive(!movingSource);
            }
            for (int i = 0; i < m_DisableOnLeftOver.Length; i++) {
                if (m_DisableOnLeftOver[i] == null) { continue; }
                m_DisableOnLeftOver[i].enabled = !movingSource;
            }
        }
    }
}