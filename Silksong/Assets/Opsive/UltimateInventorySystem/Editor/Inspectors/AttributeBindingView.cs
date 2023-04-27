/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using System;
    using System.Collections.Generic;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Attribute binding view is used to display an Item Binding in the editor.
    /// </summary>
    public class AttributeBindingView : VisualElement
    {
        public event Action OnValueChange;
        protected AttributeViewName m_AttributeViewName;
        protected VisualElement m_BindingContainer;
        protected ObjectField m_ObjectField;
        protected PopupField<string> m_PropertyPathField;

        protected AttributeBinding m_AttributeBinding;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AttributeBindingView(Type objectType)
        {
            AddToClassList(InventoryManagerStyles.AttributeBinding);

            m_AttributeViewName = new AttributeViewName(true);
            Add(m_AttributeViewName);

            m_BindingContainer = new VisualElement();
            m_BindingContainer.AddToClassList("horizontal-layout");
            Add(m_BindingContainer);

            m_ObjectField = new ObjectField();
            m_ObjectField.AddToClassList(InventoryManagerStyles.AttributeBindingObject);
            m_ObjectField.objectType = objectType;
            m_ObjectField.RegisterValueChangedCallback(evt =>
            {
                m_AttributeBinding.BoundComponent = evt.newValue;
                if (evt.newValue == null) {
                    m_AttributeBinding.PropertyPath = string.Empty;
                }
                ValueChanged();
                Refresh();
            });

            m_BindingContainer.Add(m_ObjectField);
            var choices = new List<string>();
            choices.Add("(none)");
            m_PropertyPathField = new PopupField<string>(choices, 0,
                propertyPath =>
                {
                    return propertyPath;
                },
                propertyPath => { return propertyPath; });
            m_PropertyPathField.RegisterValueChangedCallback(evt =>
            {
                var propertyPath = evt.newValue;
                
                if (m_AttributeBinding == null || m_AttributeBinding.PropertyPath == propertyPath ||
                    m_AttributeBinding.PropertyPath == string.Empty && propertyPath == "(none)") { return; }

                m_AttributeBinding.PropertyPath = m_PropertyPathField.index != 0 ? propertyPath : string.Empty;
                ValueChanged();
            });
            m_PropertyPathField.AddToClassList(InventoryManagerStyles.AttributeBindingPopup);
            m_BindingContainer.Add(m_PropertyPathField);
        }

        /// <summary>
        /// Setup the attribute and attribute binding.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="binding">The attribute binding.</param>
        public void Setup(AttributeBase attribute, AttributeBinding binding)
        {
            m_AttributeViewName.Refresh(attribute);
            m_AttributeBinding = binding;

            Refresh();
        }

        /// <summary>
        /// Refresh the view to show the corresponding visual elements.
        /// </summary>
        protected void Refresh()
        {
            if (m_AttributeBinding == null) { return; }

            m_ObjectField.value = m_AttributeBinding.BoundComponent;
            var choices = CreatePropertyPathChoices();
            var selectedIndex = 0;
            for (int i = 0; i < choices.Count; i++) {
                if (choices[i] == m_AttributeBinding.PropertyPath) {
                    selectedIndex = i;
                    break;
                }
            }
            Shared.Editor.UIElements.UIElementsUtility.SetPopupFieldChoices(m_PropertyPathField, choices);
            m_PropertyPathField.SetValueWithoutNotify(choices[selectedIndex]);
        }

        /// <summary>
        /// Create the property path choices.
        /// </summary>
        /// <returns>A list of paths.</returns>
        protected virtual List<string> CreatePropertyPathChoices()
        {
            var choices = new List<string>();
            choices.Add("(none)");
            var properties = m_AttributeBinding?.BoundComponent?.GetType().GetProperties();
            if (!(properties?.Length > 0)) { return choices; }

            for (int i = 0; i < properties.Length; i++) {
                if (!m_AttributeViewName.Object.GetValueType().IsAssignableFrom(properties[i].PropertyType)) { continue; }

                choices.Add(properties[i].Name);
            }

            return choices;
        }

        /// <summary>
        /// Send event of a change.
        /// </summary>
        protected void ValueChanged()
        {
            OnValueChange?.Invoke();
        }
    }

    /// <summary>
    /// Attribute name binding view is used to view an attribute name binding in the editor.
    /// </summary>
    public class AttributeNameBindingView : AttributeBindingView
    {
        protected bool m_HidePropertyPath = false;

        public AttributeNameBindingView(Type objectType) : base(objectType)
        {
        }

        /// <summary>
        /// Hide the property path.
        /// </summary>
        /// <param name="hide">Should the property path be hidden?</param>
        public void HidePropertyPath(bool hide)
        {
            m_HidePropertyPath = hide;
            if (m_BindingContainer == null) { return; }
            Refresh();

            m_BindingContainer.Clear();
            m_BindingContainer.Add(m_ObjectField);
            if (!m_HidePropertyPath) {
                m_BindingContainer.Add(m_PropertyPathField);
            }
        }

        /// <summary>
        /// Creat the property path choices.
        /// </summary>
        /// <returns>List of paths.</returns>
        protected override List<string> CreatePropertyPathChoices()
        {
            var choices = new List<string>();
            choices.Add("(none)");
            var properties = m_AttributeBinding?.BoundComponent?.GetType().GetProperties();
            if (!(properties?.Length > 0)) { return choices; }

            for (int i = 0; i < properties.Length; i++) {
                if (!typeof(string).IsAssignableFrom(properties[i].PropertyType)) { continue; }

                choices.Add(properties[i].Name);
            }

            return choices;
        }
    }
}