/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Monitors
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Shared.Events.EventHandler;

    /// <summary>
    /// The inventory monitor.
    /// </summary>
    public class InventoryMonitor : MonoBehaviour
    {
        [Tooltip("The inventory ID to monitor, does not monitor if zero.")]
        [SerializeField] protected uint m_InventoryID = 1;
        [UnityEngine.Serialization.FormerlySerializedAs("m_MonitoredInventory")]
        [Tooltip("The monitored inventory.")]
        [SerializeField] protected Inventory m_StartingMonitoredInventory;
        [Tooltip("The names of the monitored item collections.")]
        [SerializeField] protected string[] m_MonitoredItemCollections;
        [Tooltip("The parent rect transform for the pop ups.")]
        [SerializeField] protected RectTransform m_MonitorContent;

        [UnityEngine.Serialization.FormerlySerializedAs("m_ItemBoxPrefab")]
        [Tooltip("The Item View prefab.")]
        [SerializeField] internal GameObject m_ItemViewPrefab;
        [Tooltip("The maximum number of Item Views displayed at once.")]
        [SerializeField] protected int m_MaxDisplays;
        [Tooltip("Combine amounts of similar items.")]
        [SerializeField] protected bool m_CombineSimilarItems;
        [Tooltip("The maximum time a Item View should be displayed for before fading.")]
        [SerializeField] protected float m_RectMaxDisplayTime;
        [Tooltip("The minimum time an Item View should be displayed for before fading.")]
        [SerializeField] protected float m_RectMinDisplayTime;
        [Tooltip("The transition time between Item Views popping up.")]
        [SerializeField] protected float m_RectTransitionTime;

        protected ResizableArray<ItemView> m_ItemDisplays;
        protected List<ItemView> m_ActiveItemViews;

        protected WaitForSeconds m_WaitMinDisplayTime;
        protected WaitForSeconds m_WaitMaxDisplayTime;

        protected ResizableArray<ItemInfo> m_ItemAmountBuffer;
        protected Dictionary<ItemView, Coroutine> m_ItemViewCoroutines;

        protected bool m_Poping;
        protected Action m_PopAction;
        protected Func<IEnumerator> m_Transition;
        protected Func<ItemView, IEnumerator> m_FadeInOut;
        protected Func<ItemView, IEnumerator> m_FadeIn;
        protected Func<ItemView, IEnumerator> m_FadeOut;

        protected float m_ItemDisplayHeight;
        protected bool m_IsListening;

        protected bool m_IsLoading;
        protected bool m_StartListeningWhenFinishedLoading;

        protected Inventory m_MonitoredInventory;

        public Inventory MonitoredInventory {
            get => m_MonitoredInventory;
            internal set => m_MonitoredInventory = value;
        }

        /// <summary>
        /// Initialize the component on awake.
        /// </summary>
        private void Awake()
        {
            if (m_MonitorContent == null) { m_MonitorContent = transform as RectTransform; }

            if (m_ItemViewPrefab == null) {
                Debug.LogWarning("Display Prefab is null.");
                return;
            }

            var prefabItemDisplay = m_ItemViewPrefab.GetComponent<ItemView>();
            if (prefabItemDisplay == null) {
                Debug.LogWarning("Display Prefab does not have an Item Display component.");
                return;
            }

            m_ItemDisplayHeight = prefabItemDisplay.RectTransform.sizeDelta.y;

            m_ItemDisplays = new ResizableArray<ItemView>();
            m_ItemDisplays.Initialize(m_MaxDisplays);
            for (int i = 0; i < m_MaxDisplays; i++) {
                var itemView = Instantiate(m_ItemViewPrefab, m_MonitorContent).GetComponent<ItemView>();
                itemView.CanvasGroup.alpha = 0;
                m_ItemDisplays.Add(itemView);
            }

            m_ActiveItemViews = new List<ItemView>();
            m_ItemViewCoroutines = new Dictionary<ItemView, Coroutine>();
            
            m_ItemAmountBuffer = new ResizableArray<ItemInfo>();

            m_WaitMinDisplayTime = new WaitForSeconds(m_RectMinDisplayTime);
            m_WaitMaxDisplayTime = new WaitForSeconds(m_RectMaxDisplayTime);

            m_PopAction = () =>
            {
                m_Poping = false;
                Pop();
            };

            m_Transition = Transition;
            m_FadeInOut = FadeInOut;
            m_FadeIn = FadeIn;
            m_FadeOut = FadeOut;
        }

        /// <summary>
        /// Start listening to events.
        /// </summary>
        void Start()
        {
            EventHandler.RegisterEvent<int>(EventNames.c_WillStartLoadingSave_Index, HandleWillStartLoadingSave);
            EventHandler.RegisterEvent<int>(EventNames.c_LoadingSaveComplete_Index, HandleCompleteLoadingSave);

            if (m_MonitoredInventory == null) {
                if (m_StartingMonitoredInventory == null) {
                    if (RetrieveAndSetMonitoredInventoryUsingID() == false) {
                        Debug.LogWarning("No Inventory was found to be monitored.", gameObject);
                        return;
                    }
                } else {
                    SetMonitoredInventory(m_StartingMonitoredInventory);
                }
            }

            StartListening();
        }

        /// <summary>
        /// Handle the save starting to load by disabling the monitor.
        /// </summary>
        /// <param name="saveIndex">The save index.</param>
        private void HandleWillStartLoadingSave(int saveIndex)
        {
            m_StartListeningWhenFinishedLoading = m_IsListening;
            StopListening();
            m_IsLoading = true;
        }
        
        /// <summary>
        /// Handle the save being completely loaded by re-enabling the monitor.
        /// </summary>
        /// <param name="saveIndex">The save index.</param>
        private void HandleCompleteLoadingSave(int saveIndex)
        {
            m_IsLoading = false;
            if (m_StartListeningWhenFinishedLoading) {
                StartListening();
            }
        }

        /// <summary>
        /// Set the Inventory to monitor.
        /// </summary>
        /// <param name="monitoredInventory">The monitored Inventory.</param>
        public void SetMonitoredInventory(Inventory monitoredInventory)
        {
            if (m_MonitoredInventory != null) {
                EventHandler.UnregisterEvent<bool>(m_MonitoredInventory.gameObject, EventNames.c_InventoryGameObject_InventoryMonitorListen_Bool, Listen);
            }
            if (monitoredInventory != null) {
                EventHandler.RegisterEvent<bool>(monitoredInventory.gameObject, EventNames.c_InventoryGameObject_InventoryMonitorListen_Bool, Listen);
            }
            
            if (isActiveAndEnabled) {
                StopListening();
                m_MonitoredInventory = monitoredInventory;
                StartListening();
            } else {
                m_MonitoredInventory = monitoredInventory;
            }
        }

        /// <summary>
        /// Retrieve the monitored Inventory.
        /// </summary>
        /// <returns>True if the monitored inventory was found.</returns>
        protected virtual bool RetrieveAndSetMonitoredInventoryUsingID()
        {
            if (m_InventoryID == 0) { return false; }

            if (InventorySystemManager.InventoryIdentifierRegister.TryGetValue(m_InventoryID,
                out var inventoryIdentifier)) {
                if (inventoryIdentifier == null || inventoryIdentifier.Inventory == null) {
                    return false;
                }
                SetMonitoredInventory(inventoryIdentifier.Inventory);
                return true;
            }

            return false;

        }

        /// <summary>
        /// Listen or stop listening to the event.
        /// </summary>
        /// <param name="listen">listen or stop listening?</param>
        private void Listen(bool listen)
        {
            if (listen) {
                StartListening();
            } else {
                StopListening();
            }
        }

        /// <summary>
        /// Start listening to events.
        /// </summary>
        private void OnEnable()
        {
            StartListening();
        }

        /// <summary>
        /// Start listening to events.
        /// </summary>
        public void StartListening()
        {
            if (m_IsLoading) {
                m_StartListeningWhenFinishedLoading = true;
            }
            if (m_MonitoredInventory == null) {
                if (RetrieveAndSetMonitoredInventoryUsingID() == false) { return; }
            }
            
            if (m_IsListening) { return; }

            EventHandler.RegisterEvent<ItemInfo, ItemStack>(m_MonitoredInventory,
                EventNames.c_Inventory_OnAdd_ItemInfo_ItemStack,
                OnItemAmountAdded);

            m_IsListening = true;
        }

        /// <summary>
        /// Stop listening to events.
        /// </summary>
        public void StopListening()
        {
            if (m_IsLoading) {
                m_StartListeningWhenFinishedLoading = false;
            }
            if (m_IsListening == false) { return; }

            if (m_MonitoredInventory != null) {
                EventHandler.UnregisterEvent<ItemInfo, ItemStack>(m_MonitoredInventory,
                    EventNames.c_Inventory_OnAdd_ItemInfo_ItemStack,
                    OnItemAmountAdded);
            }

            m_IsListening = false;
        }

        /// <summary>
        /// An item amount was added to the inventory.
        /// </summary>
        /// <param name="addedItemInfo">The item info.</param>
        /// <param name="newStack">The item origin.</param>
        private void OnItemAmountAdded(ItemInfo addedItemInfo, ItemStack newStack)
        {
            if (newStack == null) { return;}
            if (IsItemStackMonitored(newStack) == false) { return; }
            
            var originCollection = addedItemInfo.ItemCollection;
            if (originCollection != null) {
                if (originCollection.Purpose == ItemCollectionPurpose.Hide
                    || originCollection.Purpose == ItemCollectionPurpose.Loadout
                    || originCollection.Inventory == newStack.Inventory) {
                    return;
                }
            }

            if (m_CombineSimilarItems == false) {
                m_ItemAmountBuffer.Add(addedItemInfo);
                Pop();
            } else {
                var foundMatch = false;

                for (int i = 0; i < m_ActiveItemViews.Count; i++) {
                    var activeItemView = m_ActiveItemViews[i];
                    var activeItemInfo = activeItemView.ItemInfo;
                    if (activeItemInfo.Item.StackableEquivalentTo(newStack.Item)) {
                        activeItemInfo = (activeItemInfo.Amount + addedItemInfo.Amount, activeItemInfo);
                        activeItemView.SetValue(activeItemInfo);
                        foundMatch = true;
                        break;
                    }
                }

                if (foundMatch == false) {
                    for (int i = 0; i < m_ItemAmountBuffer.Count; i++) {
                        var bufferItemInfo = m_ItemAmountBuffer[i];
                        if (bufferItemInfo.Item.StackableEquivalentTo(newStack.Item)) {
                            m_ItemAmountBuffer[i] = (bufferItemInfo.Amount + addedItemInfo.Amount, bufferItemInfo);
                            foundMatch = true;
                            break;
                        }
                    }
                }

                if (foundMatch == false) {
                    m_ItemAmountBuffer.Add(addedItemInfo);
                    Pop();
                }
            }
            
        }

        /// <summary>
        /// Should the item be monitored by the Inventory Monitor?
        /// </summary>
        /// <param name="newStack">The item stack with the item to check whether it should be monitored.</param>
        /// <returns>True if the item should be monitored.</returns>
        protected virtual bool IsItemStackMonitored(ItemStack newStack)
        {
            if (m_MonitoredItemCollections.Length == 0 && newStack.ItemCollection == m_MonitoredInventory.MainItemCollection) {
                return true;
            }

            for (int i = 0; i < m_MonitoredItemCollections.Length; i++) {
                if (m_MonitoredItemCollections[i] == newStack.ItemCollection?.Name) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Pop a Item View to show the next item amount that was added.
        /// </summary>
        protected void Pop()
        {
            if (m_Poping == true || m_ItemAmountBuffer.Count == 0) {
                return;
            }

            m_Poping = true;
            var ItemAmount = m_ItemAmountBuffer[0];
            m_ItemAmountBuffer.RemoveAt(0);

            m_ItemDisplays.MoveElementIndex(m_MaxDisplays - 1, 0);

            var itemDisplay = m_ItemDisplays[0];
            //itemDisplay.gameObject.SetActive(true);
            itemDisplay.RectTransform.anchoredPosition = Vector2.zero;
            itemDisplay.SetValue(ItemAmount);
            itemDisplay.CanvasGroup.alpha = 0;
            
            m_ActiveItemViews.Add(itemDisplay);

            //Transition
            //Fade
            if (gameObject.activeInHierarchy) {
                if (m_ItemViewCoroutines.TryGetValue(itemDisplay, out var coroutine)) {
                    if (coroutine != null) {
                        StopCoroutine(coroutine);
                    }
                }
                m_ItemViewCoroutines[itemDisplay] = StartCoroutine(m_FadeInOut(itemDisplay));
                StartCoroutine(m_Transition());
            } else {
                for (int i = 1; i < m_ItemDisplays.Count; i++) {
                    m_ItemDisplays[i].RectTransform.anchoredPosition = new Vector2(0, i * m_ItemDisplayHeight);
                }
            }
            Scheduler.Schedule(m_RectMinDisplayTime + m_RectTransitionTime, m_PopAction);
        }

        /// <summary>
        /// Fade in and then out the Item View.
        /// </summary>
        /// <param name="itemView">The Item View.</param>
        /// <returns>The IEnumerator.</returns>
        protected IEnumerator FadeInOut(ItemView itemView)
        {
            yield return m_FadeIn(itemView);
            yield return m_WaitMaxDisplayTime;
            m_ActiveItemViews.Remove(itemView);
            yield return m_FadeOut(itemView);
            //itemDisplay.gameObject.SetActive(false);
        }

        /// <summary>
        /// Fade in the Item View.
        /// </summary>
        /// <param name="itemView">The Item View.</param>
        /// <returns>The IEnumerator.</returns>
        protected IEnumerator FadeIn(ItemView itemView)
        {
            var transitionStep = Time.unscaledDeltaTime / m_RectTransitionTime;
            while (itemView.CanvasGroup.alpha < 1) {
                itemView.CanvasGroup.alpha += transitionStep;
                yield return null;
            }
        }

        /// <summary>
        /// Fade out the Item View.
        /// </summary>
        /// <param name="itemView">The Item View.</param>
        /// <returns>The IEnumerator.</returns>
        protected IEnumerator FadeOut(ItemView itemView)
        {
            var transitionStep = Time.unscaledDeltaTime / m_RectTransitionTime;
            while (itemView.CanvasGroup.alpha > 0) {
                itemView.CanvasGroup.alpha -= transitionStep;
                yield return null;
            }
        }

        /// <summary>
        /// Translate the Item View with a nice Lerp.
        /// </summary>
        /// <returns>The IEnumarator.</returns>
        protected IEnumerator Transition()
        {
            //Move existing boxes up
            var transitionStep = m_ItemDisplayHeight * Time.unscaledDeltaTime / m_RectTransitionTime;
            var positionYDelta = 0f;
            while (positionYDelta < m_ItemDisplayHeight) {
                for (int i = 1; i < m_ItemDisplays.Count; i++) {
                    m_ItemDisplays[i].RectTransform.anchoredPosition = new Vector2(0, (i - 1) * m_ItemDisplayHeight + positionYDelta);
                }
                positionYDelta += transitionStep;
                yield return null;
            }

            for (int i = 1; i < m_ItemDisplays.Count; i++) {
                m_ItemDisplays[i].RectTransform.anchoredPosition = new Vector2(0, i * m_ItemDisplayHeight);
            }
        }

        /// <summary>
        /// Stop listening to events.
        /// </summary>
        private void OnDestroy()
        {
            StopListening();
        }

        /// <summary>
        /// Stop listening to events.
        /// </summary>
        private void OnDisable()
        {
            if (m_ItemDisplays == null) { return; }

            for (int i = 0; i < m_ItemDisplays.Count; i++) {
                m_ItemDisplays[i].CanvasGroup.alpha = 0;
            }

            StopListening();
        }
    }
}
