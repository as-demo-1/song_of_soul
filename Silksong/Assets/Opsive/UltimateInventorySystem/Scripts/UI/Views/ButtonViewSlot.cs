/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Views
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The box Ui parent with reference to a button.
    /// </summary>
    public class ButtonViewSlot : ViewSlot
    {
        [Tooltip("The selectable button.")]
        [SerializeField] protected Selectable m_Button;

        public Selectable Button => m_Button;

        /// <summary>
        /// Set the box child.
        /// </summary>
        /// <param name="child">The child box.</param>
        public override void SetView(View child)
        {
            m_View = child;
            m_Button.targetGraphic = child != null ? child.TargetGraphic : null;
        }
    }
}