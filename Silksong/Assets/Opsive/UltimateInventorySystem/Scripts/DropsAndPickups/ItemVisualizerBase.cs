/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.DropsAndPickups
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// The Item pickup visual listener will swap out the ItemPickup mesh gameObject by one specified on the item.
    /// </summary>
    public abstract class ItemVisualizerBase : MonoBehaviour
    {
        [Tooltip("The attribute name for the items visual prefab.")]
        [SerializeField] protected string m_PrefabAttributeName;
        [FormerlySerializedAs("m_ItemObjectParent")]
        [Tooltip("The parent that will hold the item object once spawned.")]
        [SerializeField] internal Transform m_ItemPrefabVisualizerParent;
        [Tooltip("Default visual prefab.")]
        [SerializeField] internal GameObject m_DefaultVisualPrefab;
        [Tooltip("The item UI indicator used to show the item name when an interactor has selected the interactable.")]
        [SerializeField] protected ItemView m_ItemView;

        protected bool m_Initialized;

        public ItemView ItemView {
            get { return m_ItemView; }
            set => m_ItemView = value;
        }

        [ContextMenu("Update Item Visuals")]
        internal virtual void UpdateItemVisuals()
        {
            Initialize(false);
            UpdateVisual();
        }

        /// <summary>
        /// Initialize in awake.
        /// </summary>
        protected virtual void Awake()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force the initialize.</param>
        protected virtual void Initialize(bool force)
        {
            if (m_Initialized && !force) { return; }

            if (string.IsNullOrEmpty(m_PrefabAttributeName)) { m_PrefabAttributeName = "PickupPrefab"; }

            if (m_ItemPrefabVisualizerParent == null) {
                m_ItemPrefabVisualizerParent = transform;
            }

            m_Initialized = true;
        }

        /// <summary>
        /// Register to the events.
        /// </summary>
        protected virtual void OnEnable()
        {
            UpdateVisual();
        }

        public abstract void UpdateVisual();

        /// <summary>
        /// The item pickup item object has changed.
        /// </summary>
        public virtual void UpdateVisual(ItemInfo itemInfo)
        {
            if (itemInfo.Item == null) {
                RemoveVisualInternal();
                return;
            }

            SetVisualInternal(itemInfo);
        }

        /// <summary>
        /// Update the item view.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        protected virtual void UpdateItemView(ItemInfo itemInfo)
        {
            if (m_ItemView == null) { return; }
            m_ItemView.SetValue(itemInfo);
        }

        /// <summary>
        /// Remove the game object that holds the item object visuals
        /// </summary>
        protected virtual void RemoveVisualInternal()
        {
            UpdateItemView(ItemInfo.None);

            if (m_ItemPrefabVisualizerParent.childCount == 0) { return; }

            var child = m_ItemPrefabVisualizerParent.GetChild(m_ItemPrefabVisualizerParent.childCount - 1);
            if (child != null) {
                if (ObjectPool.IsPooledObject(child.gameObject)) {
                    ObjectPool.Destroy(child.gameObject);
                } else {
                    if (Application.isPlaying) {
                        Destroy(child.gameObject);
                    } else {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Set the item pickup visual from the item attribute.
        /// </summary>
        /// <param name="item">The item.</param>
        protected virtual void SetVisualInternal(ItemInfo itemInfo)
        {
            RemoveVisualInternal();

            UpdateItemView(itemInfo);

            var item = itemInfo.Item;
            if (item == null) { return; }

            if (item.ItemDefinition == null) { return; }

            if (!item.TryGetAttributeValue<GameObject>(m_PrefabAttributeName, out var prefab)) {
                prefab = m_DefaultVisualPrefab;
            }

            if (prefab == null) {
                Debug.LogError($"The Prefab attribute value is null, please assign a default value or " +
                               $"in the attribute, make sure the item {item} has a non-null value " +
                               $"for attribute: {m_PrefabAttributeName}.");
                return;
            }

            if (Application.isPlaying) {
                var instance = ObjectPool.Instantiate(prefab, m_ItemPrefabVisualizerParent);
            } else {
                var instance = GameObject.Instantiate(prefab, m_ItemPrefabVisualizerParent);
            }

        }
    }
}