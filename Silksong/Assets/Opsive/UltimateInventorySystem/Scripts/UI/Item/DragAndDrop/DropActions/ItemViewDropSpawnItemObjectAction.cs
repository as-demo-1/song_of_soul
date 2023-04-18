/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.DragAndDrop.DropActions
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.DropsAndPickups;
    using System;
    using UnityEngine;

    /// <summary>
    /// An Item View Drop Action used to Spawn an item Object at the mouse position using the Item Object Spawner.
    /// </summary>
    [Serializable]
    public class ItemViewDropSpawnItemObjectAction : ItemViewDropAction
    {
        [Tooltip("The item object spawner ID used to identify in the Global Register.")]
        [SerializeField] protected uint m_ItemObjectSpawnerID = 1;
        [Tooltip("Spawn a single Item Object.")]
        [SerializeField] protected bool m_SpawnOne = false;
        [Tooltip("Project the mouse position from the camera.")]
        [SerializeField] protected bool m_ProjectMousePosition = true;
        [Tooltip("The distance from the camera when using the project mouse position option.")]
        [SerializeField] protected float m_CameraDistance = 10;
        [Tooltip("Remove the item from the container.")]
        [SerializeField] protected bool m_RemoveFromContainer;
        [Tooltip("Remove the item from the inventory.")]
        [SerializeField] protected bool m_RemoveFromInventory;

        /// <summary>
        /// Drop Action.
        /// </summary>
        /// <param name="itemViewDropHandler">The Item View Drop Handler.</param>
        public override void Drop(ItemViewDropHandler itemViewDropHandler)
        {
            var itemInfo = itemViewDropHandler.SourceItemInfo;
            if (itemInfo.Item == null || itemInfo.Amount <= 0) {
                return;
            }

            if (m_SpawnOne) {
                itemInfo = (1, itemInfo);
            }

            var itemObjectSpawner = InventorySystemManager.GetGlobal<ItemObjectSpawner>(m_ItemObjectSpawnerID);

            if (itemObjectSpawner == null) {
                Debug.LogWarning("The Item Object Spawner was not found in the scene, please add one (usually next to the Inventory System Manager)");
                return;
            }

            if (m_RemoveFromContainer) {
                itemViewDropHandler.SourceContainer.RemoveItem(itemInfo, itemViewDropHandler.SourceIndex);
            }

            if (m_RemoveFromInventory) {
                itemInfo.ItemCollection?.RemoveItem(itemInfo);
            }

            Vector3 spawnPosition;
            if (m_ProjectMousePosition) {
                var pointerEvent = itemViewDropHandler.DropSlotEventData as ItemViewSlotPointerEventData;
                if (pointerEvent == null) {
                    Debug.LogWarning("Cannot find mouse position");
                    return;
                }

                var screenPosition = pointerEvent.PointerEventData.position;

                spawnPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, m_CameraDistance));
            } else {
                spawnPosition = itemInfo.Inventory.ItemUser.transform.position;
            }

            itemObjectSpawner.Spawn(itemInfo, spawnPosition);
        }
    }
}