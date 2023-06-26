/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Serialization;

    /// <summary>
    /// An Item View Slot Action event contains information about an event which was triggered on an Item View Slot.
    /// </summary>
    public class ItemViewSlotActionEvent
    {
        protected bool m_StopPropagation;

        public ItemViewSlotEventHandler Action { get; protected set; }
        public bool Enabled { get; protected set; }

        public bool StopPropagation => Enabled && m_StopPropagation;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="action">The action to call when the event is triggered.</param>
        /// <param name="stopPropagation">Should this event prevent other events from being triggered?</param>
        public ItemViewSlotActionEvent(ItemViewSlotEventHandler action, bool stopPropagation)
        {
            Action = action;
            m_StopPropagation = stopPropagation;
            Enabled = true;
        }

        /// <summary>
        /// Invoke the action by setting the event data.
        /// </summary>
        /// <param name="slotEventData">The slot event data.</param>
        public void Invoke(ItemViewSlotEventData slotEventData)
        {
            if (Enabled == false) { return; }

            Action?.Invoke(slotEventData);
        }

        /// <summary>
        /// Enable or Disable the event.
        /// </summary>
        /// <param name="enable">Enable?</param>
        public void Enable(bool enable)
        {
            Enabled = enable;
        }
    }

    /// <summary>
    /// An Item View Slot Container, is a group of item view slots, used for grids, hotbars, etc...
    /// </summary>
    public abstract class ItemViewSlotsContainerBase : MonoBehaviour
    {
        public event Action OnEnableEvent;
        public event Action OnDisableEvent;
        
        public event Action<Inventory> OnBindInventory;
        public event Action<Inventory> OnUnBindInventory;

        public event ItemViewSlotEventHandler OnItemViewSlotSelected;
        public event ItemViewSlotEventHandler OnItemViewSlotDeselected;
        public event ItemViewSlotEventHandler OnItemViewSlotClicked;
        public event ItemViewSlotPointerEventHandler OnItemViewSlotPointerDownE;
        public event ItemViewSlotPointerEventHandler OnItemViewSlotBeginDragE;
        public event ItemViewSlotPointerEventHandler OnItemViewSlotEndDragE;
        public event ItemViewSlotPointerEventHandler OnItemViewSlotDragE;
        public event ItemViewSlotPointerEventHandler OnItemViewSlotDropE;

        public event Action OnDraw;
        public event Action OnResetDraw;

        [Tooltip("The name of the container, it should be unique.")]
        [SerializeField] protected string m_ContainerName;
        [FormerlySerializedAs("m_CursorManager")]
        [Tooltip("The parent of all the itemBoxSlots.")]
        [SerializeField] protected ItemViewSlotCursorManager m_SlotCursor;
        [Tooltip("Use an ID instead of a the Slot Cursor field above is null.")]
        [SerializeField] protected uint m_SlotCursorID = 1;
        [Tooltip("The inventory bound to the Item View Slot Container.")]
        [SerializeField] protected Inventory m_Inventory;
        [Tooltip("If true the inventory will draw when enabled (leave false when using panels).")]
        [SerializeField] protected bool m_DrawOnEnable = false;
        [Tooltip("Redraw the inventory grid each time the inventory updates.")]
        [SerializeField] protected bool m_DrawOnInventoryUpdate = true;

        protected ItemViewSlot m_SelectedSlot;
        protected ItemViewSlot[] m_ItemViewSlots;

        protected bool m_IsInitialized;
        protected bool m_IsRegisteredToInventoryUpdate = false;
        protected bool m_IsInventorySet = false;
        protected ItemViewSlotEventData m_ItemViewSlotEventData;
        protected ItemViewSlotPointerEventData m_ItemViewSlotPointerDownEventData;
        protected ItemViewSlotPointerEventData m_ItemViewSlotDragEventData;
        protected ItemViewSlotPointerEventData m_ItemViewSlotDropEventData;
        protected DisplayPanel m_DisplayPanel;
        protected ItemViewSlotActionEvent m_OneTimeClick;
        protected bool m_HasToDraw;

        public int SlotCount => m_ItemViewSlots.Length;
        public IReadOnlyList<ItemViewSlot> ItemViewSlots => m_ItemViewSlots;
        public ItemViewSlotCursorManager ItemViewSlotCursor => m_SlotCursor;
        public DisplayPanel Panel => m_DisplayPanel;
        public string ContainerName => m_ContainerName;

        public Inventory Inventory => m_Inventory;


        /// <summary>
        /// Set the name of the Item View Slot Container.
        /// </summary>
        /// <param name="containerName">The container name.</param>
        public void SetName(string containerName)
        {
            m_ContainerName = containerName;
        }

        /// <summary>
        /// Initlialize.
        /// </summary>
        protected virtual void Awake()
        {
            Initialize(false);
        }

        /// <summary>
        /// Listen to the inventory update event.
        /// </summary>
        protected virtual void OnEnable()
        {
            Initialize(false);
            RegisterToInventoryUpdate();
            if (m_DrawOnEnable) {
                Draw();
            }
            OnEnableEvent?.Invoke();
        }

        /// <summary>
        /// Stop listening to the inventory update event.
        /// </summary>
        protected virtual void OnDisable()
        {
            UnregisterFromInventoryUpdate();
            OnDisableEvent?.Invoke();
        }

        /// <summary>
        /// Set up the item hot bar slots
        /// </summary>
        public virtual void Initialize(bool force)
        {
            if (m_IsInitialized && !force) { return; }

            if (Application.isPlaying == false) {
                m_IsInitialized = true;
                return;
            }
            
            if (m_SlotCursor == null && Application.isPlaying) {

                m_SlotCursor = InventorySystemManager.GetItemViewSlotCursorManager(m_SlotCursorID);
                if (m_SlotCursor == null) {
                    Debug.LogWarning("The Item View Slot Cursor Manager is missing, please add one on your canvas.");
                }

            }

            m_ItemViewSlotEventData = new ItemViewSlotEventData();
            m_ItemViewSlotPointerDownEventData = new ItemViewSlotPointerEventData();
            m_ItemViewSlotDropEventData = new ItemViewSlotPointerEventData();
            m_ItemViewSlotDragEventData = new ItemViewSlotPointerEventData();

            for (int i = 0; i < m_ItemViewSlots.Length; i++) {
                var itemViewSlot = m_ItemViewSlots[i];

                var localIndex = i;

                itemViewSlot.AssignIndex(i);

                itemViewSlot.OnSubmitE += () =>
                {
                    m_ItemViewSlotEventData.SetValues(this, localIndex);

                    if (m_OneTimeClick != null) {
                        var oneTimeClick = m_OneTimeClick;
                        m_OneTimeClick = null;

                        oneTimeClick.Invoke(m_ItemViewSlotEventData);
                        if (oneTimeClick.StopPropagation) {
                            return;
                        }
                    }

                    OnItemViewSlotClicked?.Invoke(m_ItemViewSlotEventData);
                };
                itemViewSlot.OnSelectE += () =>
                {
                    m_SelectedSlot = GetItemViewSlot(localIndex);
                    m_ItemViewSlotEventData.SetValues(this, localIndex);
                    OnItemViewSlotSelected?.Invoke(m_ItemViewSlotEventData);
                };
                itemViewSlot.OnDeselectE += () =>
                {
                    if (GetItemViewSlot(localIndex) != m_SelectedSlot) { return; }

                    m_SelectedSlot = null;
                    m_ItemViewSlotEventData.SetValues(this, localIndex);
                    OnItemViewSlotDeselected?.Invoke(m_ItemViewSlotEventData);
                };
                itemViewSlot.OnPointerDownE += (pointerEventData) =>
                {
                    m_ItemViewSlotPointerDownEventData.SetValues(this, localIndex);
                    m_ItemViewSlotPointerDownEventData.PointerEventData = pointerEventData;
                    OnItemViewSlotPointerDownE?.Invoke(m_ItemViewSlotPointerDownEventData);
                };
                itemViewSlot.OnBeginDragE += (pointerEventData) =>
                {
                    if (m_ItemViewSlotDragEventData.PointerEventData != null) {
                        //The drop event is being used, so it cannot be used again until the drag ends.
                        return;
                    }
                    
                    m_ItemViewSlotDragEventData.SetValues(this, localIndex);
                    m_ItemViewSlotDragEventData.PointerEventData = pointerEventData;
                    OnItemViewSlotBeginDragE?.Invoke(m_ItemViewSlotDragEventData);
                };
                itemViewSlot.OnEndDragE += (pointerEventData) =>
                {
                    if (m_ItemViewSlotDragEventData.PointerEventData == null || 
                        pointerEventData?.pointerId != m_ItemViewSlotDragEventData.PointerEventData.pointerId) {
                        return;
                    }
                    //m_ItemBoxPointerEventData.SetValues(this, GetItemBoxAt(index),index);
                    m_ItemViewSlotDragEventData.PointerEventData = pointerEventData;
                    OnItemViewSlotEndDragE?.Invoke(m_ItemViewSlotDragEventData);
                    
                    m_ItemViewSlotDragEventData.Reset();
                };
                itemViewSlot.OnDragE += (pointerEventData) =>
                {
                    if (m_ItemViewSlotDragEventData.PointerEventData == null || 
                        pointerEventData?.pointerId != m_ItemViewSlotDragEventData.PointerEventData.pointerId) {
                        return;
                    }
                    //m_ItemBoxPointerEventData.SetValues(this, GetItemBoxAt(index),index);
                    m_ItemViewSlotDragEventData.PointerEventData = pointerEventData;
                    OnItemViewSlotDragE?.Invoke(m_ItemViewSlotDragEventData);
                };
                itemViewSlot.OnDropE += (pointerEventData) =>
                {
                    m_ItemViewSlotDropEventData.SetValues(this, localIndex);
                    m_ItemViewSlotDropEventData.PointerEventData = pointerEventData;
                    OnItemViewSlotDropE?.Invoke(m_ItemViewSlotDropEventData);
                };
            }

            var bindings = GetComponents<ItemViewSlotsContainerBinding>();
            for (int i = 0; i < bindings.Length; i++) {
                bindings[i].BindItemViewSlotContainer(this);
            }

            OnInitializeBeforeSettingInventory();

            if (m_Inventory != null) {
                SetInventory(m_Inventory);
            }

            m_IsInitialized = true;
        }

        /// <summary>
        /// Cancel the dragEvent.
        /// </summary>
        public void CancelDragEvent()
        {
            if (m_ItemViewSlotDragEventData.PointerEventData == null) {
                return;
            }
            OnItemViewSlotEndDragE?.Invoke(m_ItemViewSlotDragEventData);
                    
            m_ItemViewSlotDragEventData.Reset();
        }
        
        /// <summary>
        /// This method is called before the Inventory is set to the Item View Slots Container.
        /// </summary>
        protected virtual void OnInitializeBeforeSettingInventory() { }

        /// <summary>
        /// Set the inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        public void SetInventory(Inventory inventory)
        {
            SetInventory(inventory, true);
        }

        /// <summary>
        /// Set the inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        public virtual void SetInventory(Inventory inventory, bool handleChange)
        {
            if (m_Inventory == inventory && m_IsInventorySet) { return; }

            m_IsInventorySet = true;

            UnregisterFromInventoryUpdate();

            var previousInventory = inventory;
            m_Inventory = inventory;
            OnInventoryChanged(previousInventory, m_Inventory);

            RegisterToInventoryUpdate();

            if (handleChange) {
                HandleInventoryUpdate();
            }
        }

        /// <summary>
        /// A new Inventory was set.
        /// </summary>
        protected virtual void OnInventoryChanged(Inventory previousInventory, Inventory newInventory) { }

        /// <summary>
        /// Register to the inventory update event.
        /// </summary>
        protected virtual void RegisterToInventoryUpdate()
        {
            if (m_IsRegisteredToInventoryUpdate || m_Inventory == null || gameObject.activeInHierarchy == false) { return; }
            Shared.Events.EventHandler.RegisterEvent(m_Inventory, EventNames.c_Inventory_OnUpdate, HandleInventoryUpdate);

            m_IsRegisteredToInventoryUpdate = true;
            OnInventoryBound();
            OnBindInventory?.Invoke(m_Inventory);
        }

        /// <summary>
        /// A new Inventory was bound to the container.
        /// </summary>
        protected virtual void OnInventoryBound() { }

        /// <summary>
        /// Unregister from the inventory update event.
        /// </summary>
        protected virtual void UnregisterFromInventoryUpdate()
        {
            if (m_Inventory == null || !m_IsRegisteredToInventoryUpdate) { return; }
            Shared.Events.EventHandler.UnregisterEvent(m_Inventory, EventNames.c_Inventory_OnUpdate, HandleInventoryUpdate);

            m_IsRegisteredToInventoryUpdate = false;
            OnInventoryUnbound();
            OnUnBindInventory?.Invoke(m_Inventory);
        }

        /// <summary>
        /// The inventory was unbound from the container.
        /// </summary>
        protected virtual void OnInventoryUnbound() { }

        /// <summary>
        /// Draw the UI whenever the inventory changes.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        protected virtual void HandleInventoryUpdate()
        {
            if (gameObject.activeInHierarchy == false || m_DrawOnInventoryUpdate == false) { return; }

            Draw();
        }

        /// <summary>
        /// Set a one time click action.
        /// </summary>
        /// <param name="action">The action to trigger once only.</param>
        public void SetOneTimeClickAction(ItemViewSlotActionEvent action)
        {
            m_OneTimeClick = action;
        }

        /// <summary>
        /// Assign an item to a slot.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="slot">The item slot.</param>
        protected virtual void AssignItemToSlot(ItemInfo itemInfo, int slot)
        {
            m_ItemViewSlots[slot].SetItemInfo(itemInfo);
        }

        /// <summary>
        /// Unassign an item to a slot.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        public virtual void UnassignItemFromSlots(ItemInfo itemInfo)
        {
            int slot;

            while ((slot = GetItemIndex(itemInfo)) != -1) {
                AssignItemToSlot(ItemInfo.None, slot);
            }
        }

        /// <summary>
        /// Toggle whether to assign or unassign the item to the slot.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="slot">The item slot.</param>
        public virtual void ToggleAssignItemToSlot(ItemInfo itemInfo, int slot)
        {
            if (GetItemAt(slot) == itemInfo) {
                AssignItemToSlot(ItemInfo.None, slot);
            } else {
                AssignItemToSlot(itemInfo, slot);
            }
        }

        /// <summary>
        /// Reset the items in the hot bar when destroyed (Use for domain reload disabled)
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (m_ItemViewSlots == null) { return; }

            for (int i = 0; i < m_ItemViewSlots.Length; i++) {
                if (m_ItemViewSlots[i]?.ItemView == null) { continue; }
                //TODO this causes a problem with Dynamic ItemCategory, is there another solution? 
                //m_ItemViewSlots[i].SetItemInfo(new ItemInfo());
            }
        }

        /// <summary>
        /// Remove the item from an index.
        /// </summary>
        /// <param name="itemInfo">The item info to remove.</param>
        /// <param name="index">The index to remove the item from.</param>
        /// <returns>Returns the item info removed.</returns>
        public virtual ItemInfo RemoveItem(ItemInfo itemInfo, int index)
        {
            var removedItem = GetItemAt(index);

            AssignItemToSlot(new ItemInfo((null, 0)), index);

            return removedItem;
        }

        /// <summary>
        /// Can the item be added to the index?
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="index">The index.</param>
        /// <returns>True if the item can be added.</returns>
        public virtual bool CanAddItem(ItemInfo itemInfo, int index)
        {
            if (itemInfo.Amount <= 0 || itemInfo.Item == null) { return false; }

            if (index < 0 || index >= m_ItemViewSlots.Length) { return false; }

            if (m_ItemViewSlots[index].CanContain(itemInfo) == false) { return false; }

            return true;
        }

        /// <summary>
        /// Add the item to the slot.
        /// </summary>
        /// <param name="itemInfo">The item info to add.</param>
        /// <param name="index">The index to add the item to.</param>
        /// <returns>The item info that was actually added.</returns>
        public virtual ItemInfo AddItem(ItemInfo itemInfo, int index)
        {
            if (CanAddItem(itemInfo, index) == false) {
                return (0, null, null);
            }

            AssignItemToSlot(itemInfo, index);
            return itemInfo;
        }

        /// <summary>
        /// Can the item be moved from one index to another.
        /// </summary>
        /// <param name="sourceIndex">The source index.</param>
        /// <param name="destinationIndex">The destination index.</param>
        /// <returns>True if the item can be moved.</returns>
        public virtual bool CanMoveItem(int sourceIndex, int destinationIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= m_ItemViewSlots.Length) { return false; }
            if (destinationIndex < 0 || destinationIndex >= m_ItemViewSlots.Length) { return false; }

            return m_ItemViewSlots[sourceIndex].CanContain(m_ItemViewSlots[destinationIndex].ItemInfo)
                && m_ItemViewSlots[destinationIndex].CanContain(m_ItemViewSlots[sourceIndex].ItemInfo);
        }

        /// <summary>
        /// Move the item from one index to another.
        /// </summary>
        /// <param name="sourceIndex">The source index.</param>
        /// <param name="destinationIndex">The destination index.</param>
        public virtual void MoveItem(int sourceIndex, int destinationIndex)
        {
            if (CanMoveItem(sourceIndex, destinationIndex) == false) { return; }

            var itemAtSource = m_ItemViewSlots[sourceIndex].ItemInfo;
            AssignItemToSlot(m_ItemViewSlots[destinationIndex].ItemInfo, sourceIndex);
            AssignItemToSlot(itemAtSource, destinationIndex);
        }

        /// <summary>
        /// Can the container give away the item specified.
        /// </summary>
        /// <param name="itemInfo">The item info to give.</param>
        /// <param name="slotIndex">The slot where the item is located.</param>
        /// <returns>True if the item can be given away.</returns>
        public virtual bool CanGiveItem(ItemInfo itemInfo, int slotIndex)
        {
            return true;
        }

        /// <summary>
        /// Returns the number of items within the container.
        /// </summary>
        /// <returns>The number of items.</returns>
        public virtual int GetItemCount()
        {
            return m_ItemViewSlots.Length;
        }

        /// <summary>
        /// Get the item at the index provided.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item info.</returns>
        public virtual ItemInfo GetItemAt(int index)
        {
            return m_ItemViewSlots[index].ItemInfo;
        }

        /// <summary>
        /// Return the number of item view slots.
        /// </summary>
        /// <returns>Number of item view slots.</returns>
        public virtual int GetItemViewSlotCount()
        {
            return m_ItemViewSlots.Length;
        }

        /// <summary>
        /// Get the item view slot at the index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item view slot.</returns>
        public ItemViewSlot GetItemViewSlot(int index)
        {
            return m_ItemViewSlots[index];
        }

        /// <summary>
        /// Get the Item View at the index provided.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item view.</returns>
        public virtual ItemView GetItemViewAt(int index)
        {
            return m_ItemViewSlots[index].ItemView;
        }

        /// <summary>
        /// Get the index of the item.
        /// </summary>
        /// <param name="itemInfo">The item info to find the index for.</param>
        /// <returns>The index where the item info is located.</returns>
        public int GetItemIndex(ItemInfo itemInfo)
        {
            for (int i = 0; i < m_ItemViewSlots.Length; i++) {
                if (itemInfo == m_ItemViewSlots[i].ItemInfo) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Select the slot at the index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void SelectSlot(int index)
        {
            if (index < 0 || index >= m_ItemViewSlots.Length) { return; }
            m_ItemViewSlots[index].Select();
        }

        /// <summary>
        /// Get the selected slot.
        /// </summary>
        /// <returns>Get the selected slot.</returns>
        public ItemViewSlot GetSelectedSlot()
        {
            return m_SelectedSlot;
        }

        /// <summary>
        /// Set the display panel.
        /// </summary>
        /// <param name="display">The display panel.</param>
        public virtual void SetDisplayPanel(DisplayPanel display)
        {
            m_DisplayPanel = display;
        }

        protected virtual void DrawInternal()
        {
            for (int i = 0; i < m_ItemViewSlots.Length; i++) {
                AssignItemToSlot(m_ItemViewSlots[i].ItemInfo, i);
            }
        }

        /// <summary>
        /// Draw the item view slots.
        /// </summary>
        public void Draw()
        {
            m_HasToDraw = true;
        }

        /// <summary>
        /// Get the Box prefab for the item specified.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>The box prefab game object.</returns>
        public abstract GameObject GetViewPrefabFor(ItemInfo itemInfo);

        /// <summary>
        /// Reset Draw, resets the container to its original state before drawing.
        /// </summary>
        public virtual void ResetDraw()
        {
            ResetDrawInternal();
            OnResetDraw?.Invoke();
        }
        
        /// <summary>
        /// Reset Draw, resets the container to its original state before drawing.
        /// </summary>
        protected virtual void ResetDrawInternal()
        {
            Draw();
        }

        /// <summary>
        /// Draw is late update if necessary.
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (m_HasToDraw) {
                DrawInternal();
                OnDraw?.Invoke();
                m_HasToDraw = false;
            }
        }
    }
}