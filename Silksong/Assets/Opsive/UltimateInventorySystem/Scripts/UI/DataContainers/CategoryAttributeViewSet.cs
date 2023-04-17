/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.DataContainers
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// A scriptable object used to map Item Category Attributes to Attribute UI prefabs.
    /// </summary>
    [CreateAssetMenu(fileName = "CategoryAttributeViewSet", menuName = "Ultimate Inventory System/UI/Category Attribute View Set", order = 1)]
    public class CategoryAttributeViewSet : ScriptableObject, IDatabaseSwitcher
    {
        [FormerlySerializedAs("m_CategoriesAttributeBoxes")]
        [Tooltip("The item attribute UIs for each category.")]
        [SerializeField] protected CategoryAttributeViews[] m_CategoriesAttributeViews;

        /// <summary>
        /// The array containing Item categories mapped to attribute UI prefabs.
        /// </summary>
        public CategoryAttributeViews[] CategoriesAttributeBoxArray {
            get => m_CategoriesAttributeViews;
            set => m_CategoriesAttributeViews = value;
        }

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            for (int i = 0; i < CategoriesAttributeBoxArray.Length; i++) {
                var category = CategoriesAttributeBoxArray[i].Category;
                if (database.Contains(category)) { continue; }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Replace any object that is not in the database by an equivalent object in the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>The objects that have been changed.</returns>
        ModifiedObjectWithDatabaseObjects IDatabaseSwitcher.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(InventorySystemDatabase database)
        {
            if (database == null) { return null; }

            for (int i = 0; i < CategoriesAttributeBoxArray.Length; i++) {
                var category = CategoriesAttributeBoxArray[i].Category;
                if (database.Contains(category)) { continue; }

                category = database.FindSimilar(category);

                CategoriesAttributeBoxArray[i] =
                    new CategoryAttributeViews(category,
                        CategoriesAttributeBoxArray[i].AttributeViews);
            }

            return null;
        }
    }

    /// <summary>
    /// A struct of an ItemCategory with an array of string and game object tuples.
    /// </summary>
    [Serializable]
    public struct CategoryAttributeViews
    {
        [Tooltip("The item category.")]
        [SerializeField] private DynamicItemCategory m_Category;

        [FormerlySerializedAs("m_AttributeBoxes")]
        [Tooltip("The attribute UIs.")]
        [SerializeField] private StringKeyGameObject[] m_AttributeViews;

        public ItemCategory Category => m_Category;
        public StringKeyGameObject[] AttributeViews => m_AttributeViews;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="category">The item category.</param>
        /// <param name="attributeViews">The attribute UI array.</param>
        public CategoryAttributeViews(ItemCategory category, StringKeyGameObject[] attributeViews)
        {
            m_Category = category;
            m_AttributeViews = attributeViews;
        }
    }

    /// <summary>
    /// A struct with a name and a game object.
    /// </summary>
    [Serializable]
    public struct StringKeyGameObject
    {
        [Tooltip("The name.")]
        [SerializeField] private string m_Name;
        [Tooltip("The game object.")]
        [SerializeField] private GameObject m_Object;

        public string Name => m_Name;
        public GameObject Object => m_Object;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="o">The object.</param>
        public StringKeyGameObject(string name, GameObject o)
        {
            m_Name = name;
            m_Object = o;
        }
    }
}