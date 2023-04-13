/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Equipping
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The Equipper component is used to equip items by converting them to ItemObjects.
    /// </summary>
    public class Equipper : MonoBehaviour, IEquipper, IDatabaseSwitcher
    {
        [Tooltip("The equippers inventory.")]
        [SerializeField] protected Inventory m_Inventory;
        [Tooltip("The equippers itemCollection within the inventory.")]
        [SerializeField]
        protected ItemCollectionID m_EquipmentItemCollectionID =
            new ItemCollectionID("Equipped", ItemCollectionPurpose.Equipped);
        [Tooltip("The attribute name fo the equipment prefab (visual).")]
        [SerializeField] protected string m_EquipablePrefabAttributeName = "EquipmentPrefab";
        [Tooltip("The attribute name fo the usable item prefab (functional).")]
        [SerializeField] protected string m_UsableItemPrefabAttributeName = "UsableItemPrefab";
        [Tooltip("The item slot set used to restruct the items that can be equipped.")]
        [SerializeField] protected ItemSlotSet m_ItemSlotSet;
        [Tooltip("The item object slots which holds the equipped item.")]
        [SerializeField] protected ItemObjectSlot[] m_Slots;
        [Tooltip("The main root node used to bind Skinned Mesh Renderers at runtime using the bones hiearchy.")]
        [SerializeField] protected Transform m_MainRootNode;

        protected ItemSlotCollection m_EquipmentItemCollection;
        protected Transform[] m_Bones;
        protected HashSet<Transform> m_BonesHashSet;
        protected Dictionary<string, Transform> m_BonesDictionary;

        public ItemSlotSet ItemSlotSet {
            get => m_ItemSlotSet;
            internal set => m_ItemSlotSet = value;
        }
        public ItemObjectSlot[] Slots {
            get => m_Slots;
            internal set => m_Slots = value;
        }

        /// <summary>
        /// Initialize in awake.
        /// </summary>
        protected virtual void Awake()
        {
            m_BonesDictionary = new Dictionary<string, Transform>();
            m_BonesHashSet = new HashSet<Transform>();
            
            if (m_MainRootNode == null) {
                var skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer == null) {
                    Debug.LogWarning("No Root Bone was found for the Equipper.");
                } else {
                    m_MainRootNode = skinnedMeshRenderer.rootBone;
                }
            }

            UpdateBones(m_MainRootNode);
            
            ValidateSlots();
        }

        /// <summary>
        /// Update the character bones.
        /// </summary>
        /// <param name="rootNode">The new root node.</param>
        public virtual void UpdateBones(Transform rootNode)
        {
            if (rootNode == null) {
                Debug.LogWarning("The Root Node for the Equipper cannot be null.");
                return;
            }
            m_MainRootNode = rootNode;
            UpdateBones();
        }
        
        /// <summary>
        /// update the bones.
        /// </summary>
        public virtual void UpdateBones()
        {
            m_Bones = m_MainRootNode.GetComponentsInChildren<Transform>();
            
            m_BonesDictionary.Clear();
            m_BonesHashSet.Clear();
            for (int i = 0; i < m_Bones.Length; i++) {
                var bone = m_Bones[i];

                if (m_BonesDictionary.ContainsKey(bone.name)) {
                    Debug.LogWarning($"Bones in the '{m_MainRootNode.name}' bone hierarchy must all have unique names. The bone name '{bone.name}' is used multiple times.", bone);
                    continue;
                }
                
                m_BonesDictionary.Add(bone.name, bone);
                m_BonesHashSet.Add(bone);
            }
        }

        /// <summary>
        /// Validate the slots by checking the Slot Set.
        /// </summary>
        public virtual void ValidateSlots()
        {
            if (m_Slots == null) { m_Slots = new ItemObjectSlot[0]; }

            var needsRefresh = false;

            if (m_ItemSlotSet == null || m_ItemSlotSet.ItemSlots == null) { return; }

            if (m_Slots.Length != m_ItemSlotSet.ItemSlots.Count) {
                needsRefresh = true;
            } else {
                for (int i = 0; i < m_Slots.Length; i++) {
                    if (m_Slots[i].Name == m_ItemSlotSet.ItemSlots[i].Name &&
                       m_Slots[i].Category == m_ItemSlotSet.ItemSlots[i].Category) { continue; }

                    needsRefresh = true;
                    break;
                }
            }

            if (needsRefresh != true) { return; }

            Array.Resize(ref m_Slots, m_ItemSlotSet.ItemSlots.Count);

            for (int i = 0; i < m_ItemSlotSet.ItemSlots.Count; i++) {
                var itemSlot = m_ItemSlotSet.ItemSlots[i];

                bool foundMatch = false;
                for (int j = 0; j < m_Slots.Length; j++) {
                    if (m_Slots[j] == null || itemSlot.Name != m_Slots[j].Name) { continue; }

                    m_Slots[j] = new ItemObjectSlot(itemSlot.Name, itemSlot.Category, m_Slots[j]);
                    foundMatch = true;

                    if (i != j) {
                        var temp = m_Slots[i];
                        m_Slots[i] = m_Slots[j];
                        m_Slots[j] = temp;
                    }

                    break;
                }

                if (!foundMatch) {
                    m_Slots[i] = new ItemObjectSlot(itemSlot.Name, itemSlot.Category, false, null, null);
                }
            }
        }

        /// <summary>
        /// Initialize the Equiper.
        /// </summary>
        protected virtual void Start()
        {
            if (m_Inventory == null) { m_Inventory = GetComponent<Inventory>(); }
            m_EquipmentItemCollection = m_Inventory.GetItemCollection(m_EquipmentItemCollectionID) as ItemSlotCollection;

            if (m_EquipmentItemCollection == null) {
                Debug.LogWarning("Your inventory does not have an equipment Item Collection.");
                return;
            }

            EventHandler.RegisterEvent<ItemInfo, ItemStack>(m_Inventory, EventNames.c_Inventory_OnAdd_ItemInfo_ItemStack, OnAddedItemToInventory);
            EventHandler.RegisterEvent<ItemInfo>(m_Inventory, EventNames.c_Inventory_OnRemove_ItemInfo, OnRemovedItemFromInventory);

            var equipmentItemAmounts = m_EquipmentItemCollection.GetAllItemStacks();
            if (equipmentItemAmounts == null) {
                Debug.LogWarning("The Equipment Item Collection is null.");
                return;
            }
            for (int i = 0; i < equipmentItemAmounts.Count; i++) {
                Equip(equipmentItemAmounts[i].Item);
            }
        }

        /// <summary>
        /// Make sure to unregister the listener on Destroy.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<ItemInfo, ItemStack>(m_Inventory, EventNames.c_Inventory_OnAdd_ItemInfo_ItemStack, OnAddedItemToInventory);
            EventHandler.UnregisterEvent<ItemInfo>(m_Inventory, EventNames.c_Inventory_OnRemove_ItemInfo, OnRemovedItemFromInventory);
        }

        /// <summary>
        /// Equip item that was added to the equipment collection.
        /// </summary>
        /// <param name="originItemInfo">The origin Item info.</param>
        /// /// <param name="addedItemStack">The added item stack.</param>
        private void OnAddedItemToInventory(ItemInfo originItemInfo, ItemStack addedItemStack)
        {
            if (addedItemStack == null) { return; }
            if (addedItemStack.ItemCollection == m_EquipmentItemCollection) {
                var index = m_EquipmentItemCollection.GetItemSlotIndex(addedItemStack);
                Equip(addedItemStack.Item, index);
            }

        }

        /// <summary>
        /// Unequip an item that was removed from the equipment collection.
        /// </summary>
        /// <param name="removedItemInfo">The removed Item info.</param>
        private void OnRemovedItemFromInventory(ItemInfo removedItemInfo)
        {
            if (removedItemInfo.ItemCollection == m_EquipmentItemCollection) {
                UnEquip(removedItemInfo.Item);
            }
        }

        /// <summary>
        /// Equip an item.
        /// </summary>
        /// <param name="item">The item to equip.</param>
        /// <returns>Return true only if the item equipped successfully.</returns>
        public virtual bool Equip(Item item)
        {
            //Check for available empty slots.
            for (int i = 0; i < m_Slots.Length; i++) {
                if (m_Slots[i].ItemObject != null) { continue; }
                if (m_Slots[i].Category != null && m_Slots[i].Category.InherentlyContains(item) == false) { continue; }

                return Equip(item, i);
            }

            //Check for any slot (even used ones).
            for (int i = 0; i < m_Slots.Length; i++) {
                if (m_Slots[i].Category != null && m_Slots[i].Category.InherentlyContains(item) == false) { continue; }

                return Equip(item, i);
            }

            return false;
        }

        /// <summary>
        /// Equip an item to a specific slot.
        /// </summary>
        /// <param name="item">The item to equip.</param>
        /// <param name="index">The slot to equip to.</param>
        /// <returns>True if equipped successfully.</returns>
        public virtual bool Equip(Item item, int index)
        {
            var slot = m_Slots[index];

            if (slot.Category != null && slot.Category.InherentlyContains(item) == false) { return false; }
            var itemObject = CreateItemObject(item);

            if (itemObject == null) { return false; }

            slot.SetItemObject(itemObject);
            
            if (slot.IsSkinnedEquipment) {
                SkinItemObject(itemObject, slot);
            } else {
                PositionItemObject(itemObject, slot);
            }

            EventHandler.ExecuteEvent(this, EventNames.c_Equipper_OnChange);

            return true;
        }

        /// <summary>
        /// Position the item object after it was spawned.
        /// </summary>
        /// <param name="itemObject">The item object to place.</param>
        /// <param name="slot">The slot in which to place it.</param>
        protected virtual  void PositionItemObject(ItemObject itemObject, ItemObjectSlot slot)
        {
            var itemTransform = itemObject.transform;
            itemTransform.SetParent(slot.Transform);

            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localPosition = Vector3.zero;
            itemTransform.localScale = Vector3.one;
        }

        /// <summary>
        /// Skin an item object with a skinned mesh renderer to the character.
        /// </summary>
        /// <param name="itemObject">The item object ot skin.</param>
        /// <param name="slot">The slot to skin the item to.</param>
        protected virtual  void SkinItemObject(ItemObject itemObject, ItemObjectSlot slot)
        {
            var itemTransform = itemObject.transform;

            var parentTransform = slot.Transform == null  || m_BonesHashSet.Contains(slot.Transform) ? m_MainRootNode.parent : slot.Transform;
            
            itemTransform.SetParent(parentTransform);
            itemTransform.localPosition = Vector3.zero;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
            
            SkinnedMeshRenderer[] equipmentSkinnedMeshRenderers = itemObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            for (var i = 0; i < equipmentSkinnedMeshRenderers.Length; i++) {
                SkinnedMeshRenderer equipmentSkinnedMeshRenderer = equipmentSkinnedMeshRenderers[i];

                // The item might be already linked to the character bones if it was previously spawned,
                // considering items are pooled in a sub pool linked to the Character.
                var equipmentRootNode = equipmentSkinnedMeshRenderer.rootBone;
                if (m_BonesHashSet.Contains(equipmentRootNode)) {
                    continue;
                }
                
                var newRootNode = GetMatchingSubRootNode(equipmentRootNode);
                var newBones = GetMatchingSubNodes(newRootNode, equipmentSkinnedMeshRenderer.bones);

                // Remove the old bones to clean up the hierarchy.
                if (equipmentRootNode != null) {
                    
                    //The Equipment Root Node might not match the master root node, search for it in the parents.
                    var node = equipmentRootNode;
                    var foundMatch = false;
                    while (node != null && node != itemTransform) {
                        if (node.name == m_MainRootNode.name) {
                            foundMatch = true;
                            break;
                        }
                        
                        node = node.parent;
                    }

                    if (foundMatch) {
                        GameObject.Destroy(node.gameObject);
                    } else {
                        GameObject.Destroy(equipmentRootNode.gameObject);
                    }
                }
                
                equipmentSkinnedMeshRenderer.rootBone = newRootNode;
                equipmentSkinnedMeshRenderer.bones = newBones;
            }
        }

        /// <summary>
        /// Get the matching sub root node.
        /// </summary>
        /// <param name="otherRootBone">The other root node.</param>
        /// <returns>The new matching root node.</returns>
        protected virtual Transform GetMatchingSubRootNode(Transform otherRootBone)
        {
            if (otherRootBone == null) {
                Debug.LogWarning("The equipment root node is null, the system will try to bind the equipment to the master root node.", gameObject);
                return m_MainRootNode;
            }

            if (m_BonesDictionary.TryGetValue(otherRootBone.name, out var subRootNode)) {
                return subRootNode;
            }
            
            Debug.LogError("No Matching root node found for the equipment. Looking for "+otherRootBone.name, gameObject);
            
            return m_MainRootNode;
        }

        /// <summary>
        /// Get the matching sub nodes within the new bone hiearchy.
        /// </summary>
        /// <param name="newRootNode">The new root node.</param>
        /// <param name="originalItemBones">The original bones.</param>
        /// <returns>The new bones.</returns>
        protected virtual Transform[] GetMatchingSubNodes(Transform newRootNode, Transform[] originalItemBones)
        {
            for (int i = 0; i < originalItemBones.Length; i++) {
                var originalItemBone = originalItemBones[i];
                if(m_BonesDictionary.TryGetValue(originalItemBone.name, out var newBone)) {
                    originalItemBones[i] = newBone;
                }
            }

            return originalItemBones;
        }

        /// <summary>
        /// Check if the item is equipped.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>True if equipped.</returns>
        public bool IsEquipped(Item item)
        {
            if (item == null) { return false; }
            for (int i = 0; i < m_Slots.Length; i++) {
                if (m_Slots[i].ItemObject == null) { continue; }
                if (m_Slots[i].ItemObject.Item != item) { continue; }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the slot has an item equipped already.
        /// </summary>
        /// <param name="index">The slot.</param>
        /// <returns>True if an item is equipped in that slot.</returns>
        public bool IsEquipped(int index)
        {
            if (m_Slots[index].ItemObject == null) { return false; }
            return m_Slots[index].ItemObject.Item != null;
        }

        /// <summary>
        /// Get the item Object slot by name.
        /// </summary>
        /// <param name="slotName">The slot name.</param>
        /// <returns>The item Object slot.</returns>
        public virtual ItemObjectSlot GetItemObjectSlot(string slotName)
        {
            for (int i = 0; i < m_Slots.Length; i++) {
                if (m_Slots[i].Name == slotName) {
                    return m_Slots[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Get the item equipped in the slot provided.
        /// </summary>
        /// <param name="index">The slot.</param>
        /// <returns>The item equipped in that slot.</returns>
        public virtual Item GetEquippedItem(int index)
        {
            if (m_Slots[index].ItemObject == null) { return null; }
            return m_Slots[index].ItemObject.Item;
        }

        /// <summary>
        /// Get the item equipped in the slot provided.
        /// </summary>
        /// <param name="slotName">The slot name.</param>
        /// <returns>The item equipped in that slot.</returns>
        public virtual Item GetEquippedItem(string slotName)
        {
            var slotIndex = m_ItemSlotSet.GetIndexOf(slotName);
            if (slotIndex == -1) {
                return null;
            }

            return GetEquippedItem(slotIndex);
        }

        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>The total amount for the attribute.</returns>
        public virtual int GetEquipmentStatInt(string attributeName)
        {
            return (int)GetEquipmentStatFloat(attributeName);
        }

        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>The total amount for the attribute.</returns>
        public virtual float GetEquipmentStatFloat(string attributeName)
        {
            var stat = 0f;
            for (int i = 0; i < m_Slots.Length; i++) {
                if (m_Slots[i].ItemObject == null) { continue; }
                var item = m_Slots[i].ItemObject.Item;

                if (item.TryGetAttributeValue<int>(attributeName, out var intAttributeValue)) {
                    stat += intAttributeValue;
                }
                if (item.TryGetAttributeValue<float>(attributeName, out var floatAttributeValue)) {
                    stat += floatAttributeValue;
                }
            }

            return stat;
        }

        /// <summary>
        /// Get a preview stat total by simulating adding a new item.
        /// </summary>
        /// <param name="attributeName">The attribute.</param>
        /// <param name="itemPreview">The item to preview.</param>
        /// <returns>The total attribute value.</returns>
        public int GetEquipmentStatPreviewAdd(string attributeName, Item itemPreview)
        {
            var stat = 0f;
            for (int i = 0; i < m_Slots.Length; i++) {

                Item item = null;

                if (itemPreview == null) {
                    if (m_Slots[i].ItemObject == null) { continue; }
                    item = m_Slots[i].ItemObject.Item;
                } else if (m_Slots[i].Category.InherentlyContains(itemPreview.Category)) {
                    item = itemPreview;
                } else {
                    if (m_Slots[i].ItemObject == null) { continue; }
                    item = m_Slots[i].ItemObject.Item;
                }

                if (item.TryGetAttributeValue<int>(attributeName, out var intAttributeValue)) {
                    stat += intAttributeValue;
                }
                if (item.TryGetAttributeValue<float>(attributeName, out var floatAttributeValue)) {
                    stat += floatAttributeValue;
                }
            }

            return (int)stat;
        }

        /// <summary>
        /// Preview the attribute stat by simulating removing an item.
        /// </summary>
        /// <param name="attributeName">The attribute.</param>
        /// <param name="itemPreview">The item to preview remove.</param>
        /// <returns>The total attribute value.</returns>
        public int GetEquipmentStatPreviewRemove(string attributeName, Item itemPreview)
        {
            var stat = 0f;
            for (int i = 0; i < m_Slots.Length; i++) {

                if (m_Slots[i].ItemObject == null) { continue; }
                var item = m_Slots[i].ItemObject.Item;
                if (item == itemPreview) { continue; }

                if (item.TryGetAttributeValue<int>(attributeName, out var intAttributeValue)) {
                    stat += intAttributeValue;
                }
                if (item.TryGetAttributeValue<float>(attributeName, out var floatAttributeValue)) {
                    stat += floatAttributeValue;
                }
            }

            return (int)stat;
        }

        /// <summary>
        /// UnEquip an item.
        /// </summary>
        /// <param name="item">The item to unequip.</param>
        public virtual void UnEquip(Item item)
        {
            for (int i = 0; i < m_Slots.Length; i++) {
                if (m_Slots[i].ItemObject == null || m_Slots[i].ItemObject.Item != item) { continue; }

                UnEquip(i);
                return;
            }
        }

        /// <summary>
        /// UnEquip the item at the slot.
        /// </summary>
        /// <param name="index">The slot.</param>
        public virtual void UnEquip(int index)
        {
            var itemObject = m_Slots[index].ItemObject;

            m_Slots[index].SetItemObject(null);

            if (ObjectPoolBase.IsPooledObject(itemObject.gameObject)) {
                ReturnItemObjectToPool(itemObject);
            } else {
                Destroy(itemObject.gameObject);
            }

            EventHandler.ExecuteEvent(this, EventNames.c_Equipper_OnChange);
        }

        /// <summary>
        /// Return the Item Object to the pool.
        /// </summary>
        /// <param name="itemObject">The itemObject to return.</param>
        protected virtual void ReturnItemObjectToPool(ItemObject itemObject)
        {
            // The skinned mesh bones will stay linked to the character, since the pool is a sub the item will never to bound to other characters.
            
            // The itemObject could have children that are pooled object such as the equipment model.
            for (int i = itemObject.transform.childCount - 1; i >= 0; i--) {
                var child = itemObject.transform.GetChild(i);
                if ((ObjectPoolBase.IsPooledObject(child.gameObject) == false)) { continue; }

                ObjectPoolBase.Destroy(child.gameObject);
            }

            ObjectPoolBase.Destroy(itemObject.gameObject);
        }

        /// <summary>
        /// Create an item Object from a pool.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The ItemObject.</returns>
        public virtual ItemObject CreateItemObject(Item item)
        {
            if (item.TryGetAttributeValue(m_EquipablePrefabAttributeName, out GameObject itemPrefab) == false) {
                Debug.LogError($"Prefab Attribute is undefined for Attribute {m_EquipablePrefabAttributeName}.", gameObject);
                return null;
            }

            if (itemPrefab == null) {
                Debug.LogError($"Prefab Attribute value is null for Attribute {m_EquipablePrefabAttributeName}.", gameObject);
                return null;
            }

            if (item.TryGetAttributeValue(m_UsableItemPrefabAttributeName, out GameObject usablePrefab) == false) {
                return CreateItemObjectInternal(item, itemPrefab);
            }

            var usableItemGameObject = CreateItemObjectInternal(item, usablePrefab);

            if (usableItemGameObject == null) {
                Debug.LogError($"The Usable Item GameObject is null for Attribute {m_UsableItemPrefabAttributeName}.");
                return null;
            }

            var characterID = gameObject.GetInstanceID();
            var equipmentGameObject = ObjectPoolBase.Instantiate(itemPrefab, characterID, usableItemGameObject.transform);
            equipmentGameObject.transform.localPosition = Vector3.zero;
            equipmentGameObject.transform.localRotation = Quaternion.identity;
            equipmentGameObject.transform.localScale = itemPrefab.transform.localScale;

            return usableItemGameObject;
        }

        /// <summary>
        /// Create an ItemObject.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="itemPrefab">The ItemObjectPrefab.</param>
        /// <returns>The item Object.</returns>
        protected virtual ItemObject CreateItemObjectInternal(Item item, GameObject itemPrefab)
        {
            if (itemPrefab == null) {
                Debug.LogWarning("The item prefab is null.");
                return null;
            }

            var characterID = gameObject.GetInstanceID();
            var itemGameObject = ObjectPoolBase.Instantiate(itemPrefab, characterID);

            var itemObject = itemGameObject.GetComponent<ItemObject>();
            if (itemObject == null) {
                itemObject = itemGameObject.AddComponent<ItemObject>();
            }

            itemObject.SetItem(item);
            return itemObject;
        }

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            return (m_ItemSlotSet as IDatabaseSwitcher)?.IsComponentValidForDatabase(database) ?? true;
        }

        /// <summary>
        /// Replace any object that is not in the database by an equivalent object in the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>The objects that have been changed.</returns>
        ModifiedObjectWithDatabaseObjects IDatabaseSwitcher.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(InventorySystemDatabase database)
        {
            if (database == null) { return null; }

            (m_ItemSlotSet as IDatabaseSwitcher)?.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(database);

            return new UnityEngine.Object[] { m_ItemSlotSet };
        }
    }
}