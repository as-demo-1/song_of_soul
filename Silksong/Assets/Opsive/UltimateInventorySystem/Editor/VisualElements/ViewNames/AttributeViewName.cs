/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Attribute view name is used to display the attribute name is a useful way.
    /// </summary>
    public class AttributeViewName : ViewName<AttributeBase>
    {
        protected ColoredBox m_SourceBox;
        protected ColoredBox m_InheritBox;
        private bool m_Header;

        public AttributeBase Attribute => m_Object;

        /// <summary>
        /// Constructor used to setup the properties.
        /// </summary>
        /// <param name="header">Is the VisualElement part of a header?</param>
        public AttributeViewName(bool header)
        {
            m_Header = header;

            m_Label = new Label();

            if (m_Header) {
                m_Label.style.unityFontStyleAndWeight = FontStyle.Bold;
                m_Label.AddToClassList(InventoryManagerStyles.AttributeViewName_LabelSmall);
            } else {
                m_Label.AddToClassList(InventoryManagerStyles.AttributeViewName_Label);
            }
            m_SourceBox = new ColoredBox();
            m_InheritBox = new ColoredBox();

            AddToClassList(InventoryManagerStyles.AttributeViewName);
            Add(m_SourceBox);
            Add(m_InheritBox);
            Add(m_Label);

            RefreshViewName();
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            RefreshViewName();
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        protected void RefreshViewName()
        {
            if (Attribute == null) {
                m_Label.text = "NULL";
                m_SourceBox.SetColor(Color.magenta);
                m_SourceBox.SetChar("?");
                m_InheritBox.SetColor(Color.magenta);
                m_InheritBox.SetChar("?");
                return;
            }

            m_Label.text = Attribute.Name;
            if (!m_Header) {
                m_Label.text += $" ({UnitOptions.GetName(Attribute.GetValueType())})";
            }

            var sourceCategory = Attribute.GetSourceCategory();
            SetBoxColor(m_SourceBox, sourceCategory);
            m_SourceBox.SetChar(GetAttributeVariantTypeChar());
            m_SourceBox.tooltip = $"Source Category is {sourceCategory}.";
            m_SourceBox.SetClickAction(() => Managers.InventoryMainWindow.NavigateTo(sourceCategory));

            var inheritCategory = Attribute.GetInheritCategory();
            SetBoxColor(m_InheritBox, inheritCategory);
            var (inheritChar, inheritTooltip, inheritObject) = GetInheritCharAndTooltip();
            m_InheritBox.SetChar(inheritChar);
            m_InheritBox.tooltip = inheritTooltip;
            m_InheritBox.SetClickAction(() => Managers.InventoryMainWindow.NavigateTo(inheritObject));
        }

        private string GetAttributeVariantTypeChar()
        {
            if (Attribute.VariantType == VariantType.Inherit) { return "I"; }

            if (Attribute.VariantType == VariantType.Override) { return "O"; }

            if (Attribute.VariantType == VariantType.Modify) { return "M"; }

            return "";
        }

        /// <summary>
        /// Get the inherit char that shows the inherit type.
        /// Get the tooltip.
        /// Get the inherit object.
        /// </summary>
        /// <returns>A tuple of information about the inheritance.</returns>
        private (string, string, object) GetInheritCharAndTooltip()
        {
            var inheritItem = Attribute.GetInheritItem();
            if (inheritItem != null) {
                return inheritItem == Attribute.AttachedItem
                    ? ("S", "Does not inherit, the current Item is the source.", inheritItem.ItemDefinition)
                    : ("I", $"Inherits from Default Item {inheritItem.name}.", inheritItem.ItemDefinition);
            }

            var inheritItemDefinition = Attribute.GetInheritItemDefinition();
            if (inheritItemDefinition != null) {
                return inheritItemDefinition == Attribute.AttachedItemDefinition
                    ? ("S", "Does not inherit, the current Item Definition is the source.", inheritItemDefinition)
                    : ("D", $"Inherits from Item Definition {inheritItemDefinition}.", inheritItemDefinition);
            }

            var inheritCategory = Attribute.GetInheritCategory();
            if (inheritCategory != null) {
                return inheritCategory == Attribute.GetAttachedObject()
                    ? ("S", "Does not inherit, the current Item Category is the source.", inheritCategory)
                    : ("C", $"Inherits from Item Category {inheritCategory}.", inheritCategory);
            }

            return ("?", "Error: Something went wrong", null);
        }

        /// <summary>
        /// Set the box color for a category.
        /// </summary>
        /// <param name="box">The colored box.</param>
        /// <param name="itemCategory">The itemCategory.</param>
        protected void SetBoxColor(ColoredBox box, ItemCategory itemCategory)
        {
            if (itemCategory == null) { box.SetColor(Color.magenta); } else { box.SetColor(itemCategory.m_Color); }
        }
    }
}