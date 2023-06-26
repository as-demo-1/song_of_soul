/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames
{
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Exchange;
    using UnityEngine;

    /// <summary>
    /// The currency view name.
    /// </summary>
    public class CurrencyViewName : ColoredBoxViewName<Currency>
    {
        public Currency Currency => m_Object;

        /// <summary>
        /// Constructor to setup the object.
        /// </summary>
        public CurrencyViewName() : base()
        {
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            if (Currency == null) {
                m_ColoredBox.visible = false;
                m_Label.text = "None";
                return;
            }

            Object icon = Currency.Icon;
            if (icon == null) {
                icon = InventoryManagerStyles.MissingCurrencyIcon;
            }

            m_ColoredBox.visible = true;
            SetText(Currency.name);
            SetColoredBox(
                m_ColoredBox,
                icon,
                Color.clear,
                $"{Currency.name} <{Currency.GetType().Name}> (ID: {Currency.ID})");
            m_ColoredBox.SetClickAction(() => Managers.InventoryMainWindow.NavigateTo(m_Object));
        }
    }
}