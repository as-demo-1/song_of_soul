/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Storage;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The item binding is used to bind item attributes to other component properties.
    /// Works on an ItemObject Item or on the first item of an Inventory.
    /// </summary>
    public class ItemBinding : MonoBehaviour, IDatabaseSwitcher
    {
        [Tooltip("The attribute bindings serialized data.")]
        [SerializeField] [HideInInspector] protected Serialization[] m_AttributeBindingsData;
        [Tooltip("The item category.")]
        [SerializeField] private ItemCategory m_ItemCategory;

        [System.NonSerialized] protected AttributeBinding[] m_AttributeBindings;
        [System.NonSerialized] protected bool m_Initialized = false;

        protected ItemObject m_ItemObject;
        protected Item m_Item;

        public ItemCategory ItemCategory => m_ItemCategory;
        public ItemObject ItemObject => m_ItemObject;
        public Item Item => m_Item;

        public AttributeBinding[] AttributeBindings {
            get => m_AttributeBindings;
            internal set => m_AttributeBindings = value;
        }

        /// <summary>
        /// Initialize the component.
        /// </summary>
        private void Awake()
        {
            Initialize(true);
        }

        /// <summary>
        /// Initialize the item object.
        /// </summary>
        private void Start()
        {
            if (m_ItemObject != null) { return; }

            var itemObject = GetComponent<ItemObject>();
            if (itemObject != null) {
                SetItemObject(itemObject);
            }
            if (m_ItemObject != null) { return; }

            var inventory = GetComponent<Inventory>();
            if (inventory == null) {
                Debug.LogWarning("ItemObject or inventory could not be found by item binding");
                return;
            }

            var stacks = inventory.MainItemCollection.GetAllItemStacks();
            if (stacks.Count > 0) {
                var item = stacks[0]?.Item;
                SetItem(item);
            }

        }

        /// <summary>
        /// Initialize the binding.
        /// </summary>
        /// <param name="force">Force initialization.</param>
        public void Initialize(bool force)
        {
            if (m_Initialized && !force) { return; }

            Deserialize();
            InitializeAttributeBindings();
            m_Initialized = true;
        }

        /// <summary>
        /// Set the item object.
        /// </summary>
        /// <param name="itemObject">The item object.</param>
        public virtual void SetItemObject(ItemObject itemObject)
        {
            if (m_ItemObject != null) {
                EventHandler.UnregisterEvent(m_ItemObject, EventNames.c_ItemObject_OnItemChanged, SetItem);
                return;
            }

            m_ItemObject = itemObject;
            if (m_ItemObject == null) { return; }

            EventHandler.RegisterEvent(m_ItemObject, EventNames.c_ItemObject_OnItemChanged, SetItem);
            SetItem(m_ItemObject.Item);
        }

        private void SetItem()
        {
            SetItem(m_ItemObject.Item);
        }

        /// <summary>
        /// Deserializes the attributes and stores them in the list and dictionary.
        /// </summary>
        protected void Deserialize()
        {
            if (m_AttributeBindingsData == null) {
                if (m_AttributeBindings == null) { m_AttributeBindings = new AttributeBinding[0]; }
                return;
            }

            m_AttributeBindings = new AttributeBinding[m_AttributeBindingsData.Length];
            for (int i = 0; i < m_AttributeBindingsData.Length; i++) {
                var attribute = m_AttributeBindingsData[i].DeserializeFields(MemberVisibility.Public) as AttributeBinding;
                m_AttributeBindings[i] = attribute;
            }
        }

        /// <summary>
        /// Serializes the attributes in the collection by adding the attributes inside AttributeCollectionData.
        /// </summary>
        public void Serialize()
        {
            if (m_AttributeBindings == null) {
                m_AttributeBindingsData = Serialization.Serialize<AttributeBase>(new List<AttributeBase>());
                return;
            }

            var attributes = new List<AttributeBinding>();
            for (int i = 0; i < m_AttributeBindings.Length; i++) {
                var attributeBinding = m_AttributeBindings[i];
                attributes.Add(attributeBinding);
            }

            m_AttributeBindingsData = Serialization.Serialize<AttributeBinding>(attributes);
        }

        /// <summary>
        /// Initialize the attribute bindings.
        /// </summary>
        private void InitializeAttributeBindings()
        {
            if (m_AttributeBindings == null) {
                m_AttributeBindings = new AttributeBinding[0];
                return;
            }

            for (int i = 0; i < m_AttributeBindings.Length; i++) {
                if (m_AttributeBindings[i] == null) { return; }
                m_AttributeBindings[i].CreatePropertyDelegates();
            }
        }

        /// <summary>
        /// Set the item Category.
        /// </summary>
        /// <param name="itemCategory">The item category.</param>
        public void SetItemCategory(ItemCategory itemCategory)
        {
            m_ItemCategory = itemCategory;
        }

        /// <summary>
        /// Set the item.
        /// </summary>
        /// <param name="item">The item.</param>
        private void SetItem(Item item)
        {
            if (m_Item == item) { return; }

            if (m_ItemCategory == null) {
                Debug.LogError($"Error: A category must be specified on the Item Binding {name}.", this);
                return;
            }
            
            m_Item = item;
            if (m_Item == null || m_Item.IsInitialized == false) {
                m_Item = null;
                UnbindAttributes();
                return;
            }

            if (m_ItemCategory.InherentlyContains(item) == false) {
                Debug.LogWarning($"The item specified ({item}) does not match item category that was set on the binding ({m_ItemCategory}).");
                m_Item = null;
                UnbindAttributes();
                return;
            }

            var allAttributesCount = m_Item.GetAttributeCount();
            for (int i = 0; i < allAttributesCount; i++) {
                var attribute = m_Item.GetAttributeAt(i, true, true);
                BindAttribute(attribute);
            }
        }

        private void UnbindAttributes()
        {
            for (int i = 0; i < m_AttributeBindings.Length; i++) {
                m_AttributeBindings[i].UnBindAttribute();
            }
        }

        /// <summary>
        /// Bind the attribute to a property.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
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

            m_ItemCategory = database.FindSimilar(m_ItemCategory);

            return null;
        }
    }
}