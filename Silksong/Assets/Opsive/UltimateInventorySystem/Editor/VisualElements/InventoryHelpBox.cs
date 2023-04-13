/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Displays a message with a warning icon next to it.
    /// </summary>
    public class InventoryHelpBox : VisualElement
    {
        protected VisualElement m_ContentContainer;

        public VisualElement ContentContainer => m_ContentContainer;

        /// <summary>
        /// The HelpBox constructor.
        /// </summary>
        /// <param name="message">The message that should be displayed.</param>
        public InventoryHelpBox(string message)
        {
            AddToClassList("unity-box");
            AddToClassList("help-box");

            var container = new VisualElement();
            container.AddToClassList("horizontal-layout");
            var image = new Image() { image = EditorGUIUtility.FindTexture("d_console.warnicon"), scaleMode = ScaleMode.ScaleToFit };
            image.style.flexShrink = 0;
            image.style.marginLeft = image.style.marginRight = 6;
            container.Add(image);

            m_ContentContainer = new VisualElement();

            SetMessage(message);

            container.Add(m_ContentContainer);
            Add(container);
        }

        public void SetMessage(string message)
        {
            m_ContentContainer.Clear();

            var label = new Label(message);
            label.style.flexShrink = 1;
            label.style.whiteSpace = WhiteSpace.Normal;

            m_ContentContainer.Add(label);
        }
    }
}