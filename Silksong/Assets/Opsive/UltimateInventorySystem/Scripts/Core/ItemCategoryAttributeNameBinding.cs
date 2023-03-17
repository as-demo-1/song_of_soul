/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Storage;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A component that lets you bind attribute names to any string property on a sibling component.
    /// </summary>
    public class ItemCategoryAttributeNameBinding : MonoBehaviour, IDatabaseSwitcher
    {
        [Tooltip("The item category.")]
        [SerializeField] private ItemCategory m_ItemCategory;
        [Tooltip("The attribute name bindings.")]
        [SerializeField] protected AttributeNameBinding[] m_AttributeBindings;

        public ItemCategory ItemCategory => m_ItemCategory;

        public AttributeNameBinding[] AttributeBindings {
            get => m_AttributeBindings;
            internal set => m_AttributeBindings = value;
        }

        /// <summary>
        /// Set the binding.
        /// </summary>
        private void Awake()
        {
            InitializeAttributeBindings();
            var allAttributesCount = m_ItemCategory.GetAttributesCount();
            for (int i = 0; i < allAttributesCount; i++) {
                var attribute = m_ItemCategory.GetAttributesAt(i);
                BindAttribute(attribute);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Apply the binding within the editor.
        /// </summary>
        /// <param name="command">The menu command.</param>
        [MenuItem("CONTEXT/AttributeNameBindingComponent/Apply Values")]
        public static void ApplyValues(MenuCommand command)
        {
            var component = command.context as ItemCategoryAttributeNameBinding;

            component.InitializeAttributeBindings();
            var allAttributesCount = component.ItemCategory.GetAttributesCount();
            for (int i = 0; i < allAttributesCount; i++) {
                var attribute = component.ItemCategory.GetAttributesAt(i);
                component.BindAttribute(attribute);
            }
        }
#endif

        /// <summary>
        /// Initialize the attribute bindings.
        /// </summary>
        private void InitializeAttributeBindings()
        {
            if (m_AttributeBindings == null) {
                m_AttributeBindings = new AttributeNameBinding[0];
                return;
            }

            for (int i = 0; i < m_AttributeBindings.Length; i++) {
                m_AttributeBindings[i].CreatePropertyDelegates();
            }
        }

        /// <summary>
        /// Set the itemCategory.
        /// </summary>
        /// <param name="itemCategory">The new item category.</param>
        public void SetItemCategory(ItemCategory itemCategory)
        {
            m_ItemCategory = itemCategory;
        }

        /// <summary>
        /// Bind the attribute.
        /// </summary>
        /// <param name="attribute">The attribute to bind.</param>
        private void BindAttribute(AttributeBase attribute)
        {
            for (int i = 0; i < m_AttributeBindings.Length; i++) {
                if (m_AttributeBindings[i].AttributeName != attribute.Name) { continue; }
                m_AttributeBindings[i].BindAttribute(attribute);
            }
        }

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            return database.Contains(m_ItemCategory);
        }

        /// <summary>
        /// Replace any object that is not in the database by an equivalent object in the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>The objects that have been changed.</returns>
        ModifiedObjectWithDatabaseObjects IDatabaseSwitcher.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(InventorySystemDatabase database)
        {
            if (database == null) { return null; }

            SetItemCategory(database.FindSimilar(m_ItemCategory));

            return null;
        }
    }
}