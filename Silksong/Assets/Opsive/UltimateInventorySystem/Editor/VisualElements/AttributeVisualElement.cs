/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using RadioButtonGroup = Opsive.Shared.Editor.UIElements.RadioButtonGroup;

    /// <summary>
    /// The AttributeView displays the options for the bound attribute.
    /// </summary>
    public class AttributeVisualElement : VisualElement
    {
        public event Action<AttributeBase> OnValueChanged;
        public event Action<AttributeBase> OnAttributeReplaced;

        private AttributeBase m_Attribute;
        private InventorySystemDatabase m_Database;

        private AttributeViewName m_AttributeViewName;
        private UnicodeTextField m_NameField;
        private FilterWindowPopupField m_TypePopup;
        private RadioButtonGroup m_VariantType;
        private UnicodeTextField m_Expression;
        private VisualElement m_Value;
        private VisualElement m_InheritValue;
        private VisualElement m_OverrideValue;

        private object[] m_AttributeControlUserData;

        /// <summary>
        /// Two parameter constructor.
        /// </summary>
        /// <param name="database">A reference to the selected database.</param>
        /// <param name="advancedEditOptions">Specify if the attributes can be edited by type and more.</param>
        public AttributeVisualElement(InventorySystemDatabase database, bool advancedEditOptions = false)
        {
            m_Database = database;
            m_AttributeControlUserData = new object[] { m_Database, new MultilineAttribute() };

            AddToClassList(InventoryManagerStyles.SubMenu);

            m_AttributeViewName = new AttributeViewName(true);
            m_AttributeViewName.Label.style.width = -1;

            var moreOptionsButton = new IconOptionButton(IconOption.Cog);
            moreOptionsButton.clicked += () =>
            {
                ShowMoreOptionsMenu(advancedEditOptions);
            };
            Add(moreOptionsButton);

            // The type button and nameview are on the same row.
            var horizontalContainer = new VisualElement();
            horizontalContainer.AddToClassList("horizontal-layout");
            horizontalContainer.AddToClassList(CommonStyles.s_AlignChildrenCenter);
            var leftContainer = new VisualElement();
            leftContainer.AddToClassList("horizontal-layout");
            leftContainer.Add(m_AttributeViewName);
            horizontalContainer.Add(leftContainer);
            horizontalContainer.Add(moreOptionsButton);
            Add(horizontalContainer);

            m_NameField = new UnicodeTextField();
            m_NameField.label = "Name";
            m_NameField.isDelayed = true;
            m_NameField.RegisterValueChangedCallback(evt =>
            {
                AttributeEditorUtility.RenameAttribute(m_Attribute, evt.newValue);

                m_AttributeViewName.Refresh();
                AttributeChanged();
            });
            m_NameField.AddToClassList("flex-grow");
            Add(m_NameField);

            if (advancedEditOptions) {
                m_TypePopup = FilterWindowPopupField.CreateFilterWindowPopupField(typeof(Attribute<>), FilterWindow.FilterType.AvailableTypes, "Attributes", true, null, OnNewAttributeTypeSelection);
                m_TypePopup.label = "Type";
                m_TypePopup.style.maxWidth = 350f;
                Add(m_TypePopup);
            }

            m_VariantType = new RadioButtonGroup("Variant", new string[] { "Inherit", "Override", "Modify" }, 0, (int selectedIndex) =>
            {
                var newVariant = (VariantType)selectedIndex;
                if (m_Attribute.VariantType != newVariant) {
                    AttributeEditorUtility.SetVariantType(m_Attribute, (VariantType)selectedIndex);
                    AttributeChanged();
                }
                m_Expression.style.display = m_Attribute.VariantType == VariantType.Modify ? DisplayStyle.Flex : DisplayStyle.None;
            });
            Add(m_VariantType);

            m_Expression = new UnicodeTextField();
            m_Expression.label = "Expression";
            m_Expression.isDelayed = true;
            m_Expression.RegisterValueChangedCallback(c =>
            {
                AttributeEditorUtility.SetModifyExpression(m_Attribute, c.newValue);
                AttributeChanged();
            });
            m_Expression.style.display = DisplayStyle.None; // The expression is shown when the variant type is modify.
            Add(m_Expression);

            m_Value = new VisualElement();
            m_Value.AddToClassList(InventoryManagerStyles.AttributeValueField);
            m_Value.SetEnabled(false);
            m_InheritValue = new VisualElement();
            m_InheritValue.AddToClassList(InventoryManagerStyles.AttributeValueField);
            m_InheritValue.SetEnabled(false);
            m_OverrideValue = new VisualElement();
            m_OverrideValue.AddToClassList(InventoryManagerStyles.AttributeValueField);
            m_OverrideValue.AddToClassList(InventoryManagerStyles.AttributeOverrideValueField);
            Add(m_Value);
            Add(m_InheritValue);
            Add(m_OverrideValue);
        }

        /// <summary>
        /// Binds the view to the specified attribute.
        /// </summary>
        /// <param name="attribute">The attribute that should be displayed.</param>
        public void BindAttribute(AttributeBase attribute)
        {
            ClearBinding();

            style.display = DisplayStyle.Flex;
            m_Attribute = attribute;
            if (Application.isPlaying == false) {
                m_Attribute.Unbind(false);
            }

            DrawValueAndInheritValue();

            DrawOverrideValue();

            Refresh();
        }

        /// <summary>
        /// Populates and shows the More Options Generic Menu.
        /// </summary>
        /// <param name="showMoveToOption">Should the Move To option be shown?</param>
        private void ShowMoreOptionsMenu(bool showMoveToOption)
        {
            var moreOptions = new GenericMenu();
            moreOptions.AddItem(new GUIContent((m_Attribute.IsPreEvaluated ? "Disable " : "Enable ") + "PreEvaluate"), false, () =>
            {
                AttributeEditorUtility.SetIsPreEvaluated(m_Attribute, !m_Attribute.IsPreEvaluated);
                AttributeChanged();
            });

            if (showMoveToOption) {
                var sourceCategory = m_Attribute.GetSourceCategory();
                if (m_Attribute.AttributeCollectionType != typeof(RequiredItemAttributeCollection)) {
                    moreOptions.AddItem(new GUIContent("Move To/Item"), false, () =>
                    {
                        ItemCategoryEditorUtility.RegisterUndoItemCategoryFamilyConnections(sourceCategory, "Move To Item");
                        sourceCategory.MoveAttributeToItems(m_Attribute);
                        ItemCategoryEditorUtility.SerializeCategoryFamilyConnections(sourceCategory);
                        OnAttributeReplaced?.Invoke(null);
                        Refresh();
                    });
                }

                if (m_Attribute.AttributeCollectionType != typeof(RequiredItemDefinitionAttributeCollection)) {
                    moreOptions.AddItem(new GUIContent("Move To/Item Definition"), false, () =>
                    {
                        ItemCategoryEditorUtility.RegisterUndoItemCategoryFamilyConnections(sourceCategory, "Move To Item Definition");
                        sourceCategory.MoveAttributeToItemDefinitions(m_Attribute);
                        ItemCategoryEditorUtility.SerializeCategoryFamilyConnections(sourceCategory);
                        OnAttributeReplaced?.Invoke(null);
                        Refresh();
                    });
                }

                if (m_Attribute.AttributeCollectionType != typeof(ItemCategoryAttributeCollection)) {
                    moreOptions.AddItem(new GUIContent("Move To/Item Category"), false, () =>
                    {
                        ItemCategoryEditorUtility.RegisterUndoItemCategoryFamilyConnections(sourceCategory, "Move To Item Category");
                        sourceCategory.MoveAttributeToItemCategories(m_Attribute);
                        ItemCategoryEditorUtility.SerializeCategoryFamilyConnections(sourceCategory);
                        OnAttributeReplaced?.Invoke(null);
                        Refresh();
                    });
                }
            }
            moreOptions.ShowAsContext();
        }

        /// <summary>
        /// Draw the fields when the the attribute changed.
        /// </summary>
        public void Refresh()
        {
            m_AttributeViewName.Refresh(m_Attribute);
            m_NameField.SetValueWithoutNotify(m_Attribute.Name);

            if (m_TypePopup != null) {
                m_TypePopup.UpdateSelectedObject(m_Attribute.GetValueType());
            }

            m_VariantType.SelectedIndex = (int)m_Attribute.VariantType;
            m_Expression.style.display = m_Attribute.VariantType == VariantType.Modify ? DisplayStyle.Flex : DisplayStyle.None;
            m_Expression.SetValueWithoutNotify(m_Attribute.ModifyExpression);

            DrawValueAndInheritValue();
        }

        /// <summary>
        /// Draw the value and inherit value fields.
        /// </summary>
        private void DrawValueAndInheritValue()
        {
            m_Attribute.ReevaluateValue(false);

            m_Value.Clear();
            m_InheritValue.Clear();

            var attributeValueType = m_Attribute.GetValueType();

            FieldInspectorView.AddField(
                m_Attribute.GetAttachedObject(),
                m_Attribute, null, -1, attributeValueType,
                "Value", string.Empty, true,
                m_Attribute.GetValueAsObject(),
                m_Value,
                (object obj) => { }
                , null, false, null, null, m_AttributeControlUserData);

            FieldInspectorView.AddField(
                m_Attribute.GetAttachedObject(),
                m_Attribute, null, -1, attributeValueType,
                "Inherited Value", string.Empty, true,
                m_Attribute.GetInheritValueAsObject(),
                m_InheritValue,
                (object obj) => { }
                , null, false, null, null, m_AttributeControlUserData);
        }

        /// <summary>
        /// Draw the override value.
        /// </summary>
        private void DrawOverrideValue()
        {
            m_OverrideValue.Clear();

            var fieldInfo = m_Attribute.GetType().GetField("m_OverrideValue",
                BindingFlags.NonPublic | BindingFlags.Default | BindingFlags.Instance);
            FieldInspectorView.AddField(
                m_Attribute.GetAttachedObject(),
                m_Attribute, fieldInfo,
                -1, m_Attribute.GetValueType(),
                "Override Value", string.Empty, true,
                m_Attribute.GetOverrideValueAsObject(),
                m_OverrideValue,
                (object obj) =>
                {
                    AttributeEditorUtility.SetOverrideValueAsObject(m_Attribute, obj);
                    
                    //In some cases the displayed Attribute is no longer the one on the object
                    
                    AttributeChanged();
                }, null, false, null, null, m_AttributeControlUserData);
        }

        /// <summary>
        /// Clears the attribute binding.
        /// </summary>
        public void ClearBinding()
        {
            style.display = DisplayStyle.None;
        }

        /// <summary>
        /// A new attribute type has been selected.
        /// </summary>
        /// <param name="selectedObject">The selected object type.</param>
        private void OnNewAttributeTypeSelection(object selectedObject)
        {
            var newAttribute = AttributeEditorUtility.ChangeType(m_Attribute, (Type)selectedObject);

            if (newAttribute == null) {
                //The attribute failed to change type.
                return;
            }
            BindAttribute(newAttribute);
            OnAttributeReplaced?.Invoke(newAttribute);
            AttributeChanged();
        }

        /// <summary>
        /// Saves the changes and triggers the value changed event.
        /// </summary>
        private void AttributeChanged()
        {
            //ItemCategoryEditorUtility.SetItemCategoryDirty(m_Attribute.AttachedItemCategory, true);
            AttributeEditorUtility.SerializeAttributes(m_Attribute.AttachedItemCategory);
            if (m_Attribute.AttachedItemDefinition != null) {
                //ItemDefinitionEditorUtility.SetItemDefinitionDirty(m_Attribute.AttachedItemDefinition, true);
                AttributeEditorUtility.SerializeAttributes(m_Attribute.AttachedItemDefinition);
            }
            if (m_Attribute.AttachedItem != null) {
                AttributeEditorUtility.SerializeAttributes(m_Attribute.AttachedItem);
            }

            Refresh();
            OnValueChanged?.Invoke(m_Attribute);

        }
    }
}