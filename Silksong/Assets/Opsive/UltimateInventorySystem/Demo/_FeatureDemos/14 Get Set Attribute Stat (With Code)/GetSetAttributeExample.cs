/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.FeatureDemos
{
    using System.Collections.Generic;
    using Opsive.Shared.Events;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Utility;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using Text = Opsive.Shared.UI.Text;
    
    /// <summary>
    /// This script is an example on how to get and set attribute values.
    /// </summary>
    public class GetSetAttributeExample : MonoBehaviour
    {
        [Tooltip("The Character Inventory ID.")]
        [SerializeField] protected uint m_CharacterInventoryID = 1;
#if TEXTMESH_PRO_PRESENT
        [Tooltip("The attribute name field, for the attribute to get/set.")]
        [SerializeField] protected TMPro.TMP_InputField m_AttributeNameField;
        [Tooltip("The item collection Dropdown used to select the itemCollection to get the sum from.")]
        [SerializeField] protected TMPro.TMP_Dropdown m_ItemCollectionDropdown;
        [Tooltip("The item name field, for the item to get or set the attribute for.")]
        [SerializeField] protected TMPro.TMP_InputField m_ItemNameField;
        [Tooltip("The attribute value field, for the attribute to set.")]
        [SerializeField] protected TMPro.TMP_InputField m_SetAttributeValueField;
#else
    [Tooltip("The attribute name field, for the attribute to get/set.")]
    [SerializeField] protected InputField m_AttributeNameField;
    [Tooltip("The item collection Dropdown used to select the itemCollection to get the sum from.")]
    [SerializeField] protected Dropdown m_ItemCollectionDropdown;
    [Tooltip("The item name field, for the item to get or set the attribute for.")]
    [SerializeField] protected InputField m_ItemNameField;
    [Tooltip("The attribute value field, for the attribute to set.")]
    [SerializeField] protected InputField m_SetAttributeValueField;
#endif
        [Tooltip("The button to add items (Optional).")]
        [SerializeField] protected Button m_SetItemAttributeButton;
        [Tooltip("The item name field, for the item to add or remove.")]
        [SerializeField] protected Text m_SetItemAttributeResult;
        [Tooltip("The button to add items (Optional).")]
        [SerializeField] protected Button m_GetItemAttributeButton;
        [Tooltip("The item name field, for the item to add or remove.")]
        [SerializeField] protected Text m_GetItemAttributeResult;
        [Tooltip("The amount to remove items (Optional).")]
        [SerializeField] protected Button m_GetCollectionSumButton;
        [Tooltip("The item name field, for the item to add or remove.")]
        [SerializeField] protected Text m_CollectionSumAttributeResult;
        [Tooltip("Update the result automatically when an Item is added to the collection or Inventory.")]
        [SerializeField] protected Toggle m_AutoUpdateGetResultOnInventoryUpdate;

        protected Inventory m_Inventory;
        protected ItemCollection m_MainItemCollection;
        protected ItemSlotCollection m_EquipmentItemCollection;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Start()
        {
            // Setting up the collection dropdown in code.
#if TEXTMESH_PRO_PRESENT
            m_ItemCollectionDropdown.options = new List<TMP_Dropdown.OptionData>()
            {
                new TMP_Dropdown.OptionData("Full Inventory"),
                new TMP_Dropdown.OptionData("Main Collection"),
                new TMP_Dropdown.OptionData("Equipment Collection"),
            };
#else
        m_ItemCollectionDropdown.options = new List<Dropdown.OptionData>()
        {
            new Dropdown.OptionData("Full Inventory"),
            new Dropdown.OptionData("Main Collection"),
            new Dropdown.OptionData("Equipment Collection"),
        };
#endif

            // Getting the Inventory using the Unique ID.
            m_Inventory = InventorySystemManager.GetInventoryIdentifier(m_CharacterInventoryID).Inventory;
            // Get the ItemCollections by name
            m_MainItemCollection = m_Inventory.GetItemCollection("Main");
            // When the collection is of a specific type you can simply cast it to that type.
            // Equipment Collection are usually of type ItemSlotCollection
            m_EquipmentItemCollection = m_Inventory.GetItemCollection("Equipment") as ItemSlotCollection;
        
            // Register to the inventory update event to know when something changed in the inventory.
            EventHandler.RegisterEvent(m_Inventory,EventNames.c_Inventory_OnUpdate, HandleInventoryUpdated);
        
            // Register to items being added and removed. This is useful if you wish to know exactly what item was added or removed.
            EventHandler.RegisterEvent<ItemInfo, ItemStack>(m_Inventory,EventNames.c_Inventory_OnAdd_ItemInfo_ItemStack, HandleItemAdded);
            EventHandler.RegisterEvent<ItemInfo>(m_Inventory,EventNames.c_Inventory_OnRemove_ItemInfo, HandleItemRemoved);
        
            // In some cases for stats, all that matters is the contents on the Equipment collection in that case you may do this
            m_EquipmentItemCollection.OnItemCollectionUpdate += ComputeStat;

            // Set the button events to call the relevent functions
            m_SetItemAttributeButton.onClick.AddListener(HandleSetItemAttributeButtonClicked);
            m_GetItemAttributeButton.onClick.AddListener(HandleGetItemAttributeButtonClicked);
            m_GetCollectionSumButton.onClick.AddListener(HandleGetCollectionSumButtonClicked);
        }
    
        /// <summary>
        /// Set the Item Attribute when the button is clicked.
        /// </summary>
        private void HandleSetItemAttributeButtonClicked()
        {
            // Get the ItemDefinition by name.
            var itemName = m_ItemNameField.text;
            var itemDefinition = InventorySystemManager.GetItemDefinition(itemName);
            if (itemDefinition == null) {
                m_SetItemAttributeResult.text = $"The Item with the name '{itemName}' does not exist in the database";
                return;
            }
        
            if (itemDefinition.IsMutable == false) {
                m_SetItemAttributeResult.text = $"The Item is not Mutable, so the attributes cannot be set at runtime";
                return;
            }
        
            Item item;

            // Here we get the first item we find within the Inventory that matches the item name.
            // IMPORTANT: setting attributes at runtime it usually done on Unique items.
            // Therefore most of the time you will wish to specify the exact item to change.
            // The best way to do so is with an ItemAction, such that you can specify the item to change within the UI. 
            var itemInfo = m_Inventory.GetItemInfo(itemDefinition);
            if (itemInfo.HasValue == false) {
                m_SetItemAttributeResult.text = $"The Item '{itemName}' is not contained within the inventory";
            
                // Even if the item is not contained in the inventory we can always get the attribute value from the default item.
                // This is not really recommended in most cases, especially when setting the attribute value.
                // That's because all unique items derive from the default item. So setting values to it at runtime is not allowed.
                item = itemDefinition.DefaultItem;
                return;
            } else {
                item = itemInfo.Value.Item;
            }

            // Check this function to see how to set an attribute value without knowing its type.
            // And make sure to check the other SetItemAttributeAs(String/Float/Int) functions to learn how it can be done when the type is known in advance.
            SetItemAttributeAsObject(item, m_AttributeNameField.text, m_SetAttributeValueField.text);
        }

        /// <summary>
        /// Get the item attribute when the button is clicked.
        /// </summary>
        private void HandleGetItemAttributeButtonClicked()
        {
            // Get the ItemDefinition by name.
            var itemName = m_ItemNameField.text;
            var itemDefinition = InventorySystemManager.GetItemDefinition(itemName);
            if (itemDefinition == null) {
                m_GetItemAttributeResult.text = $"The Item with the name '{itemName}' does not exist in the database";
                return;
            }

            Item item;
        
            // Here we get the first item we find within the Inventory that matches the item name.
            // IMPORTANT: setting attributes at runtime it usually done on Unique items.
            // Therefore most of the time you will wish to specify the exact item to change.
            // The best way to do so is with an ItemAction, such that you can specify the item to change within the UI. 
            var itemInfo = m_Inventory.GetItemInfo(itemDefinition);
            if (itemInfo.HasValue == false) {
                m_GetItemAttributeResult.text = $"The Item '{itemName}' is not contained within the inventory";
            
                // Even if the item is not contained in the inventory we can always get the attribute value from the default item.
                // Of course if the item is unique and mutable the value can be different on other items.
                // So it is important to be precise on the item selected.
                item = itemDefinition.DefaultItem;
            } else {
                item = itemInfo.Value.Item;
            }
        
            // Check this function to see how to set an attribute value without knowing its type.
            // And make sure to check the other GetItemAttributeAs(String/Float/Int) functions to learn how it can be done when the type is known in advance.
            GetItemAttributeAsObject(item, m_AttributeNameField.text);
        
        }

        /// <summary>
        /// Get the sum of the attributes with the same name within an item collection or the inventory.
        /// </summary>
        private void HandleGetCollectionSumButtonClicked()
        {
            // The sum of attributes works with int and float attributes.
            // This is a great shortcut for computing stats for your character.
        
            var attributeName = m_AttributeNameField.text;
        
            if (m_ItemCollectionDropdown.value == 0) {
                // Check the Sum in the Inventory
                var sum = m_Inventory.GetFloatSum(attributeName);
                m_CollectionSumAttributeResult.text = $"The sum of '{attributeName}' in the Inventory is '{sum}'";
            } else if(m_ItemCollectionDropdown.value == 1){
                // Check the Sum in the Main Collection
                var sum = m_MainItemCollection.GetFloatSum(attributeName);
                m_CollectionSumAttributeResult.text = $"The sum of '{attributeName}' in the Main Collection is '{sum}'";
            }else if(m_ItemCollectionDropdown.value == 2){
                // Check the Sum in the Equipment Collection
                var sum = m_EquipmentItemCollection.GetFloatSum(attributeName);
                m_CollectionSumAttributeResult.text = $"The sum of '{attributeName}' in the Equipment is '{sum}'";
            }
        
            /* EXAMPLE */
            // Internally the Inventory and ItemCollection Float Sum uses the AttributeUtility class to get the sum of the attribute in a list of items.
            var itemInfos = new List<ItemInfo>();
            // Add items in that list, it can be a list of Items, ItemStacks, ItemInfos or ItemDefinitions.
            // And cast the list to a listSlice if necessary
            var sumExample = AttributeUtility.GetFloatSum("MyAttributeName", (ListSlice<ItemInfo>)itemInfos);
        }


        /// <summary>
        /// An Item was removed from the Inventory.
        /// </summary>
        /// <param name="itemInfoRemoved">The item Info that was removed.</param>
        private void HandleItemRemoved(ItemInfo itemInfoRemoved)
        {
            //To know where the item was removed from check to the collection
            var removeFromItemCollection = itemInfoRemoved.ItemCollection;
        
            // For example you may choose to update the stats of your character only if the item was removed from the equipment Collection.
            if (removeFromItemCollection == m_EquipmentItemCollection) {
                // Update the character stats.
            }
        }

        /// <summary>
        /// An Item was added to the inventory.
        /// </summary>
        /// <param name="originalItemInfo">The original Item Info that was added.</param>
        /// <param name="itemStack">The item Stack where the item was added to.</param>
        private void HandleItemAdded(ItemInfo originalItemInfo, ItemStack itemStack)
        {
            // To know the amount that was added check the originalItemInfo
            var amountAdded = originalItemInfo.Amount;
            // To know the new amount, if there was already some on that stack check the itemStack
            var newAmount = itemStack.Amount;
            // Simply do the difference to know the previous amount
            var previousAmount = newAmount - amountAdded;
        
            // Here you can know where the item came from
            var itemOriginCollection = originalItemInfo.ItemCollection;
            // and where it was added
            var itemNewCollection = itemStack.ItemCollection;
        
            // For example you may choose to update the stats of your character only if the item was added to the equipment Collection.
            if (itemNewCollection == m_EquipmentItemCollection) {
                // Update the character stats.
            }
        }
    
        /// <summary>
        /// The Inventory has updated.
        /// </summary>
        private void HandleInventoryUpdated()
        {
            // The Inventory Update can be called multiple times within a single frame.

            // Lets automatically get the result if the auto update result on change field is true.
            if (m_AutoUpdateGetResultOnInventoryUpdate) {
                HandleGetItemAttributeButtonClicked();
                HandleGetCollectionSumButtonClicked();
            }
        }

        /*
     * Get Attribute values
     */
    
        /// <summary>
        /// Get the item Attribute if it is a float.
        /// </summary>
        /// <param name="item">The item with the attribute to get.</param>
        /// <param name="attributeName">The attribute name.</param>
        private float GetItemAttributeAsFloat(Item item, string attributeName)
        {
            var floatAttribute = item.GetAttribute<Attribute<float>>(attributeName);

            if (floatAttribute == null) {
                Debug.LogWarning($"The Item '{item.name}' does not have a float attribute with the name '{attributeName}'");
                return float.NaN;
            }
        
            return floatAttribute.GetValue();
        }
    
        /// <summary>
        /// Get the item Attribute if it is a string.
        /// </summary>
        /// <param name="item">The item with the attribute to get.</param>
        /// <param name="attributeName">The attribute name.</param>
        private string GetItemAttributeAsString(Item item, string attributeName)
        {
            var stringAttribute = item.GetAttribute<Attribute<string>>(attributeName);

            if (stringAttribute == null) {
                Debug.LogWarning($"The Item '{item.name}' does not have a string attribute with the name '{attributeName}'");
                return null;
            }
        
            return stringAttribute.GetValue();
        }
    
        /// <summary>
        /// Get the item Attribute if it is an int.
        /// </summary>
        /// <param name="item">The item with the attribute to get.</param>
        /// <param name="attributeName">The attribute name.</param>
        private void GetItemAttributeAsInt(Item item, string attributeName)
        {
            var intAttribute = item.GetAttribute<Attribute<int>>(attributeName);

            if (intAttribute == null) {
                Debug.LogWarning($"The Item '{item.name}' does not have a int attribute with the name '{attributeName}'");
                return;
            }
        
            intAttribute.GetValue();
        }
    
        /// <summary>
        /// Get the item Attribute if it is an Object.
        /// </summary>
        /// <param name="item">The item with the attribute to get.</param>
        /// <param name="attributeName">The attribute name.</param>
        private object GetItemAttributeAsObject(Item item, string attributeName)
        {
            // In this case we aree getting the attribute without actually knowing its type.
            // We are getting it as a generic object.
        
            var attribute = item.GetAttribute(attributeName);

            if (attribute == null) {
                Debug.LogWarning($"The Item '{item.name}' does not have an attribute with the name '{attributeName}'");
                m_GetItemAttributeResult.text = $"The Item '{item.name}' does not have an attribute with the name '{attributeName}'";
                return null;
            }

            // Get the attribute value as an object when the types is not known.
            var attributeValue = attribute.GetValueAsObject();

            m_GetItemAttributeResult.text = $"{attributeValue}";

            return attributeValue;
        }

        /*
     * Set Attribute values
     */

        /// <summary>
        /// Set the item Attribute if it is a float.
        /// </summary>
        /// <param name="item">The item with the attribute to set.</param>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="attributeValue">The attribute value as a float.</param>
        private void SetItemAttributeAsFloat(Item item, string attributeName, float attributeValue)
        {
            var floatAttribute = item.GetAttribute<Attribute<float>>(attributeName);

            if (floatAttribute == null) {
                Debug.LogWarning($"The Item '{item.name}' does not have a float attribute with the name '{attributeName}'");
                return;
            }
        
            floatAttribute.SetOverrideValue(attributeValue);
        }
    
        /// <summary>
        /// Set the item Attribute if it is a string.
        /// </summary>
        /// <param name="item">The item with the attribute to set.</param>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="attributeValue">The attribute value as a string.</param>
        private void SetItemAttributeAsString(Item item, string attributeName, string attributeValue)
        {
            var stringAttribute = item.GetAttribute<Attribute<string>>(attributeName);

            if (stringAttribute == null) {
                Debug.LogWarning($"The Item '{item.name}' does not have a string attribute with the name '{attributeName}'");
                return;
            }
        
            stringAttribute.SetOverrideValue(attributeValue);
        }
    
        /// <summary>
        /// Set the item Attribute if it is an int.
        /// </summary>
        /// <param name="item">The item with the attribute to set.</param>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="attributeValue">The attribute value as a int.</param>
        private void SetItemAttributeAsInt(Item item, string attributeName, int attributeValue)
        {
            var intAttribute = item.GetAttribute<Attribute<int>>(attributeName);

            if (intAttribute == null) {
                Debug.LogWarning($"The Item '{item.name}' does not have a int attribute with the name '{attributeName}'");
                return;
            }
        
            intAttribute.SetOverrideValue(attributeValue);
        }
    
        /// <summary>
        /// Set the item Attribute without knowing the attribute type in advance.
        /// </summary>
        /// <param name="item">The item with the attribute to set.</param>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="attributeValueAsStringObject">The attribute value as a string.</param>
        private void SetItemAttributeAsObject(Item item, string attributeName, string attributeValueAsStringObject)
        {
            // Usually when setting an item attribute you would know the attribute type.
            // Please refer to SetItemAttributeInt/Float/String for examples of that.
            // In this case we do not know the attribute type so we simply try to set it as an object.
            // This only works if the attributeValueAsString can by automatically converted to the attribute type.

            if (item.IsMutable == false) {
                m_SetItemAttributeResult.text = $"The Item is not Mutable, so the attributes cannot be set at runtime";
                return;
            }
        
            var itemAttribute = item.GetAttribute(attributeName);
            if (itemAttribute == null) {
                m_SetItemAttributeResult.text = $"The Item '{item.name}' does not have any attribute called '{attributeName}'";
                return;
            }

            if (itemAttribute.AttachedItem == null) {
                m_SetItemAttributeResult.text = $"The attribute'{attributeName}', is not an Item Attribute. It cannot be set at runtime";
                return;
            }
        
            // Here we set the attribute value as an object because we do not know the attribute type.
            // Please refer to SetItemAttributeInt/Float/String for examples of how to set the attribute if the type is known in advance.
            itemAttribute.SetOverrideValueAsObject(attributeValueAsStringObject);

            m_SetItemAttributeResult.text = $"The item Attribute was set to '{itemAttribute.GetValueAsObject()}'/n" +
                                            $"(Please check the console for warnings or errors, if the result is unexpected)";
        }
    
        /*
     *  Stat Examples
     */

        /// <summary>
        /// An example on how you could compute some stats for your character using item attributes.
        /// </summary>
        public void ComputeStat()
        {
            // When computing stats it is very important to always recompute from scratch.
            // Adding/removing values from a saved value with add float rounding errors which can pile up over time and cause bugs.
            // Funny anecdote, this happened in an MMO once, and some players took advantage of it to create weapons with all stats maxed out.
        
            // In this example we'll show how you could compute an attack stat.
        
            // In most cases you'll want a base stat
            float baseAttack = 5;
            // By default the attack will be equal to the baseAttack
            float attack = baseAttack;
        
            // Now lets get the sum of the "Attack" attribute from the equipped items.
            var equipmentAttackSum = m_EquipmentItemCollection.GetFloatSum("Attack");
        
            // so the new attack value is
            attack = baseAttack + equipmentAttackSum;
            // DO NOT do:
            //attack += equipmentAttackSum;
            // as explained above this can cause issues over time.
        
            // You can go a step further and do sums, multiplications, etc...
            // For example lets take the Weapon weapon and sum attack multiplier attribute from it
            var slotName = "Weapon";
            var weaponItemInfo = m_EquipmentItemCollection.GetItemInfoAtSlot(slotName);
            if (weaponItemInfo.Item != null) {
                if(weaponItemInfo.Item.TryGetAttributeValue("AttackMultiplier", out float multiplier))
                {
                    attack = baseAttack + (equipmentAttackSum * multiplier);
                }
            }
        
            // You can take this as far as you wish for all the character stats.
        
            // Simply recompute the attack stat whenever the Inventory or the Equipment Collection Updates. (Example in Stat)
            // It is also possible to use the C# event instead if you prefer, simply do this in Awake:
            // m_EquipmentItemCollection.OnItemCollectionUpdate += ComputeStat;
        }
    }
}
