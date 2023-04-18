/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Editor.Managers;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Generic ViewName with a colored box.
    /// </summary>
    /// <typeparam name="T">The type of the Object.</typeparam>
    public class ColoredBoxViewName<T> : ViewName<T> where T : class
    {
        protected ColoredBox m_ColoredBox;
        protected VisualElement m_ObjectPreview;

        /// <summary>
        /// Constructor to set up the properties.
        /// </summary>
        public ColoredBoxViewName()
        {
            Clear();
            m_Label = new Label();
            m_Label.style.unityTextAlign = TextAnchor.MiddleLeft;
            m_ColoredBox = new ColoredBox();
            m_ObjectPreview = new VisualElement();

            AddToClassList("box-view-name");
            AddToClassList("flex-grow");
            AddToClassList("horizontal-layout");
            Add(m_ColoredBox);
            Add(m_Label);
        }

        /// <summary>
        /// Set the color of the box.
        /// </summary>
        /// <param name="box">The coloredBox.</param>
        /// <param name="category">The category.</param>
        /// <param name="boxChar">The string written in the colored box.</param>
        /// <param name="newTooltip">The box tooltip.</param>
        protected virtual void SetColoredBox(ColoredBox box, ObjectCategoryBase category, string boxChar, string newTooltip)
        {
            var previewContainer = box.Q("previewContainer");
            previewContainer?.Clear();

            box.RemoveFromClassList(InventoryManagerStyles.ColoredBox_Preview);
            box.style.backgroundImage = null;

            tooltip = newTooltip;

            if (category == null) {
                box.SetColor(Color.red);
                box.AddToClassList(InventoryManagerStyles.ColoredBox_Error);
                box.SetChar("!");
                box.tooltip = $"A category must be specified\n {newTooltip}.";
            } else if (category.name == DatabaseValidator.s_UncategorizedCategoryName) {
                box.AddToClassList(InventoryManagerStyles.ColoredBox_Preview);
                box.style.backgroundImage = new StyleBackground(InventoryManagerStyles.UncategorizedIcon);
                box.SetChar("");
                box.SetColor(Color.clear);
                box.tooltip = newTooltip;
            } else {
                box.RemoveFromClassList(InventoryManagerStyles.ColoredBox_Error);
                box.SetColor(category.m_Color);
                box.SetChar(boxChar);
                box.tooltip = newTooltip;
            }
        }

        /// <summary>
        /// Set the color of the box.
        /// </summary>
        /// <param name="box">The coloredBox.</param>
        /// <param name="objectToPreview">The object that should appear within the box (can be null).</param>
        /// <param name="color">The box color.</param>
        /// <param name="newTooltip">The tooltip when hovering the box.</param>
        protected virtual void SetColoredBox(ColoredBox box, Object objectToPreview, Color color, string newTooltip)
        {
            var previewContainer = box.Q("previewContainer");
            previewContainer?.Clear();
            box.style.backgroundImage = null;

            tooltip = newTooltip;

            if (objectToPreview == null) {
                box.RemoveFromClassList(InventoryManagerStyles.ColoredBox_Preview);
                box.AddToClassList(InventoryManagerStyles.ColoredBox_Error);
                box.SetChar("?");
                box.SetColor(Color.magenta);
                box.tooltip = newTooltip;
            } else {
                box.RemoveFromClassList(InventoryManagerStyles.ColoredBox_Error);
                box.AddToClassList(InventoryManagerStyles.ColoredBox_Preview);

                box.SetColor(color);
                box.SetChar("");

                if (previewContainer == null) {
                    previewContainer = new VisualElement();
                    previewContainer.name = "previewContainer";
                    box.Add(previewContainer);
                }
                ManagerUtility.ObjectPreview(m_ObjectPreview, objectToPreview);
                m_ObjectPreview.AddToClassList(CommonStyles.s_ObjectPreviewSmall);
                previewContainer.Add(m_ObjectPreview);

                box.tooltip = newTooltip;
            }
        }
    }
}