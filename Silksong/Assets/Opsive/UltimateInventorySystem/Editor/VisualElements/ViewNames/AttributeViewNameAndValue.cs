/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames
{
    using Opsive.UltimateInventorySystem.Editor.Managers;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using UnityEngine.UIElements;

    /// <summary>
    /// Attribute view name that also displays the attribute value.
    /// </summary>
    public class AttributeViewNameAndValue : AttributeViewName
    {
        protected VisualElement m_Value;

        /// <summary>
        /// Constructor used to setup the properties.
        /// </summary>
        public AttributeViewNameAndValue() : base(false)
        {
            m_Value = new VisualElement();
            m_Value.AddToClassList(InventoryManagerStyles.AttributeViewNameAndValue_Value);
            Add(m_Value);

            RefreshValue();
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            RefreshValue();
        }

        /// <summary>
        /// Refresh the value.
        /// </summary>
        protected void RefreshValue()
        {
            m_Value.Clear();
            if (Attribute == null) { return; }

            Attribute.ReevaluateValue(false);
            var value = Attribute.GetValueAsObject();

            if (value is UnityEngine.Object obj) {
                var container = ManagerUtility.ObjectFieldWithPreview(
                    Attribute.GetAttachedObject(),
                    Attribute, obj);
                container.ElementAt(1).SetEnabled(false);
                m_Value.Add(container);
            } else {
                var text = value == null ? "(none)" : value.ToString();
                m_Value.Add(new Label(text));
            }
        }
    }
}