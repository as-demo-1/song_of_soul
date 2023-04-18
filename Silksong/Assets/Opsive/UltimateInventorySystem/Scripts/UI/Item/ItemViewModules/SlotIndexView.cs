/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using Opsive.UltimateInventorySystem.UI.Views;
    using UnityEngine;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// An Item View component to display an amount.
    /// </summary>
    public class SlotIndexView : ViewModule, IViewModuleWithSlot
    {
        [Tooltip("The amount text.")]
        [SerializeField] protected Text m_IndexText;
        [Tooltip("Start the index at 1 or at 0?")]
        [SerializeField] protected bool m_StartIndexAt1 = true;

        private IViewSlot m_ViewSlot;
        public IViewSlot ViewSlot => m_ViewSlot;

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void Initialize(View view)
        {
            base.Initialize(view);
            if (view.ViewSlot == null) { return; }

            SetViewSlot(view.ViewSlot);
        }

        /// <summary>
        /// Set the view slot.
        /// </summary>
        /// <param name="viewSlot">The view slot.</param>
        public void SetViewSlot(IViewSlot viewSlot)
        {
            m_ViewSlot = viewSlot;

            var index = m_StartIndexAt1 ? m_ViewSlot.Index + 1 : m_ViewSlot.Index;

            m_IndexText.text = index.ToString();
        }

        /// <summary>
        /// Clear.
        /// </summary>
        public override void Clear()
        {
            //nothing.
        }
    }
}