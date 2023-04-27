/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using System;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The colored box.
    /// </summary>
    public class ColoredBox : VisualElement
    {
        protected Label m_Label;
        protected Clickable m_Clickable;
        protected Action m_Action;

        /// <summary>
        /// Create a colored box.
        /// </summary>
        public ColoredBox()
        {
            AddToClassList(InventoryManagerStyles.ColoredBox);
            m_Label = new Label();
            Add(m_Label);

            SetColor(Color.magenta);
            SetChar("");

            m_Clickable = new Clickable((Action)null);
            this.AddManipulator(m_Clickable);
        }

        /// <summary>
        /// Set the color.
        /// </summary>
        /// <param name="color">The color.</param>
        public void SetColor(Color color)
        {
            style.backgroundColor = color;
        }

        /// <summary>
        /// Set the character.
        /// </summary>
        /// <param name="text">The text.</param>
        public void SetChar(string text)
        {
            m_Label.text = text;
        }

        /// <summary>
        /// Set a click action.
        /// </summary>
        /// <param name="action">The action.</param>
        public void SetClickAction(Action action)
        {
            m_Clickable.clicked -= m_Action;
            m_Action = action;
            m_Clickable.clicked += m_Action;
        }
    }
}
