/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Input;
    using Opsive.UltimateInventorySystem.UI.Views;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Box UI component for changing an image when the box is selected.
    /// </summary>
    public class SelectImageView : ViewModule, IViewModuleSelectable
    {
        [Tooltip("The image.")]
        [SerializeField] protected Image m_Image;
        [Tooltip("The default sprite.")]
        [SerializeField] protected Sprite m_Default;
        [Tooltip("The selected sprite.")]
        [SerializeField] protected Sprite m_Selected;

        /// <summary>
        /// Clear.
        /// </summary>
        public override void Clear()
        {
            var view = m_BaseView;
            if (view == null) {
                Select(false);
                return;
            }

            if (view.ViewSlot == null) {
                Select(false);
                return;
            }

            var viewSlotGameObject = view.ViewSlot.gameObject;
            var @select = EventSystemManager.GetEvenSystemFor(viewSlotGameObject).currentSelectedGameObject == viewSlotGameObject;
            Select(@select);
        }
        
        /// <summary>
        /// Select the box.
        /// </summary>
        /// <param name="select">select or unselect.</param>
        public void Select(bool select)
        {
            m_Image.sprite = select ? m_Selected : m_Default;
        }
    }
}