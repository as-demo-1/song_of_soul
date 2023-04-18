/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.AttributeSystem
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;

    /// <summary>
    /// The attribute for an Item. It requires a custom attribute to serialize and deserialize the item.
    /// </summary>
    [System.Serializable]
    public class AttributeItem : Attribute<Item>
    {
        public AttributeItem() : base() { }

        /// <summary>
        /// Constructor is required.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="overrideValue">The override value.</param>
        /// <param name="variantType">The variant type.</param>
        /// <param name="modifyExpression">The modify expression.</param>
        public AttributeItem(string name, Item overrideValue = null,
            VariantType variantType = VariantType.Override, string modifyExpression = "")
            : base(name, overrideValue, variantType, modifyExpression)
        {
            Deserialize();
        }

        /// <summary>
        /// Deserialize the item.
        /// </summary>
        public override void Deserialize()
        {
            base.Deserialize();

            if (m_OverrideValue != null && m_OverrideValue.ItemDefinition != null) {
                m_OverrideValue.Initialize(false);
            }
            if (m_PreEvaluatedValue != null && m_PreEvaluatedValue.ItemDefinition != null) {
                m_PreEvaluatedValue.Initialize(false);
            }
        }

        /// <summary>
        /// Serialize the item.
        /// </summary>
        public override void Serialize()
        {
            base.Serialize();
            if (m_OverrideValue != null) {
                m_OverrideValue.Serialize();
            }
            if (m_PreEvaluatedValue != null) {
                m_PreEvaluatedValue.Serialize();
            }
        }
    }

    /// <summary>
    /// The attribute for an Item Array. It requires a custom attribute to serialize and deserialize the item.
    /// </summary>
    [System.Serializable]
    public class AttributeItemArray : Attribute<Item[]>
    {
        public AttributeItemArray() : base() { }

        /// <summary>
        /// Constructor is required.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="overrideValue">The override value.</param>
        /// <param name="variantType">The variant type.</param>
        /// <param name="modifyExpression">The modify expression.</param>
        public AttributeItemArray(string name, Item[] overrideValue = null,
            VariantType variantType = VariantType.Override, string modifyExpression = "")
            : base(name, overrideValue, variantType, modifyExpression)
        {
            Deserialize();
        }

        /// <summary>
        /// Deserialize the item array.
        /// </summary>
        public override void Deserialize()
        {
            base.Deserialize();

            if (m_OverrideValue != null) {
                for (int i = 0; i < m_OverrideValue.Length; i++) {
                    if (m_OverrideValue[i] == null || m_OverrideValue[i].ItemDefinition == null) { continue; }

                    m_OverrideValue[i].Initialize(false);
                }
            }
            if (m_PreEvaluatedValue != null) {
                for (int i = 0; i < m_PreEvaluatedValue.Length; i++) {
                    if (m_PreEvaluatedValue[i] == null || m_PreEvaluatedValue[i].ItemDefinition == null) { continue; }

                    m_PreEvaluatedValue[i].Initialize(false);
                }
            }
        }

        /// <summary>
        /// Serialize the item array.
        /// </summary>
        public override void Serialize()
        {
            base.Serialize();
            if (m_OverrideValue != null) {
                for (int i = 0; i < m_OverrideValue.Length; i++) {
                    if (m_OverrideValue[i] == null) { continue; }

                    m_OverrideValue[i].Serialize();
                }
            }
            if (m_PreEvaluatedValue != null) {
                for (int i = 0; i < m_PreEvaluatedValue.Length; i++) {
                    if (m_PreEvaluatedValue[i] == null) { continue; }

                    m_PreEvaluatedValue[i].Serialize();
                }
            }
        }
    }

    /// <summary>
    /// The attribute for an Item Collection. It requires a custom attribute to serialize and deserialize the item.
    /// </summary>
    [System.Serializable]
    public class AttributeItemCollection : Attribute<ItemCollection>
    {
        public AttributeItemCollection() : base() { }

        /// <summary>
        /// Constructor is required.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="overrideValue">The override value.</param>
        /// <param name="variantType">The variant type.</param>
        /// <param name="modifyExpression">The modify expression.</param>
        public AttributeItemCollection(string name, ItemCollection overrideValue = null,
            VariantType variantType = VariantType.Override, string modifyExpression = "")
            : base(name, overrideValue, variantType, modifyExpression)
        {
            Deserialize();
        }

        /// <summary>
        /// Deserialize the itemCollection.
        /// </summary>
        public override void Deserialize()
        {
            base.Deserialize();

            if (m_OverrideValue != null) {
                m_OverrideValue.Initialize(null, false);
            }
            if (m_PreEvaluatedValue != null) {
                m_PreEvaluatedValue.Initialize(null, false);
            }
        }

        /// <summary>
        /// Serialize the itemCollection.
        /// </summary>
        public override void Serialize()
        {
            base.Serialize();
            if (m_OverrideValue != null) {
                var itemCollection = m_OverrideValue;

                var allItemData = itemCollection.GetAllItemStacks();

                for (int j = 0; j < allItemData.Count; j++) {

                    var itemAmount = allItemData[j];
                    var item = itemAmount.Item;
                    item.Serialize();

                }
            }
            if (m_PreEvaluatedValue != null) {
                var itemCollection = m_PreEvaluatedValue;

                var allItemData = itemCollection.GetAllItemStacks();

                for (int j = 0; j < allItemData.Count; j++) {

                    var itemAmount = allItemData[j];
                    var item = itemAmount.Item;
                    item.Serialize();

                }
            }
        }
    }
}
