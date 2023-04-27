/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using System.Collections.Generic;
    using UnityEngine.UI;

    public class ItemShapeGrid : ItemViewSlotsContainerBase
    {
        public enum ItemShapeNavigation
        {
            PerItem, //Navigate the shape grid item by item.
            PerSlot, //Navigate the shape grid slot by slot.
            Custom   //Doesn't change the navigation.
        }
        
        [Tooltip("The item shape grid data ID, used to get the grid data from the inventory at runtime.")]
        [SerializeField] protected int m_ItemShapeGridDataID;
        [Tooltip("The Grid size in units.")]
        [SerializeField] protected Vector2Int m_GridSize = new Vector2Int(8, 8);
        [Tooltip("The item shape size or cell size (in pixel).")]
        [SerializeField] protected Vector2 m_ItemShapeSize = new Vector2(100, 100);
        [Tooltip("The Item View Slot Prefab is used to spawn the correct amount of item view slot at runtime.")]
        [SerializeField] protected ItemViewSlot m_ItemViewSlotPrefab;
        [Tooltip("The item view drawe is used to draw the item views in the slots.")]
        [SerializeField] protected ItemViewDrawer m_ItemViewDrawer;
        [Tooltip("The item shape view content, where the foreground Item views will be drawn.")]
        [SerializeField] protected RectTransform m_ItemShapeViewContent;
        [Tooltip("The Grid layout group for the item view slots.")]
        [SerializeField] protected GridLayoutGroup m_GridLayoutGroup;
        [Tooltip("Default keyboard or gamepad Navigation.")]
        [SerializeField] protected ItemShapeNavigation m_NavigationType;

        protected ItemShapeGridData m_ItemShapeGridData;
        protected List<ItemView> m_ForegroundItemViews;

        public int ItemShapeGridDataID => m_ItemShapeGridDataID;
        public int GridFullSize => m_GridSize.x * m_GridSize.y;
        public Vector2Int GridSize => m_GridSize;
        public ItemShapeGridData ItemShapeGridData => m_ItemShapeGridData;
        public Vector2 ItemShapeSize => m_ItemShapeSize;

        public ItemViewSlot ItemViewSlotPrefab {
            get => m_ItemViewSlotPrefab;
            set => m_ItemViewSlotPrefab = value;
        }
        
        public ItemShapeNavigation NavigationType
        {
            get => m_NavigationType;
            set
            {
                m_NavigationType = value;
                UpdateNavigation();
            }
        }

        /// <summary>
        /// Initialize the grid.
        /// </summary>
        /// <param name="force">Force the initialize.</param>
        public override void Initialize(bool force)
        {
            if (m_IsInitialized && !force) { return; }

            m_ForegroundItemViews = new List<ItemView>();
            
            if (m_ItemViewDrawer == null) {
                m_ItemViewDrawer = GetComponent<ItemViewDrawer>();
            }
            m_ItemViewDrawer.Initialize(force);

            if (m_GridLayoutGroup == null) {
                m_GridLayoutGroup = m_ItemViewDrawer.Content.GetComponent<GridLayoutGroup>();
            }

            var itemViewSlotAmount = m_ItemViewDrawer.ViewSlots.Length;

            if (itemViewSlotAmount != GridFullSize) {
                Debug.LogError(
                    $"The number of Item View Slots in the Content '{itemViewSlotAmount}', does not match the grid size '{GridFullSize}'");
            }

            m_ItemViewSlots = new ItemViewSlot[itemViewSlotAmount];
            for (int i = 0; i < itemViewSlotAmount; i++) {
                var boxSlot = m_ItemViewDrawer.ViewSlots[i] as ItemViewSlot;
                if (boxSlot == null) {
                    Debug.LogWarning("The item view slot container must use ItemViewSlots and not IViewSlots");
                }

                m_ItemViewSlots[i] = boxSlot;
            }
            
            var dragHandler = GetComponent<ItemViewSlotDragHandler>();
            if (dragHandler != null) { dragHandler.OnDragStarted += HandleItemViewSlotBeginDrag; }

            base.Initialize(force);
        }

        /// <summary>
        /// A new Inventory was bound to the container.
        /// </summary>
        protected override void OnInventoryBound()
        {
            if (m_ItemShapeGridData == null && m_Inventory != null) {
                var gridsShapeController = m_Inventory.GetComponent<ItemShapeGridController>();

                if (gridsShapeController != null) {
                    m_ItemShapeGridData = gridsShapeController.GetGridDataWithID(m_ItemShapeGridDataID);
                } else { Debug.LogWarning("The inventory is missing a ItemShapesGridsController component."); }
            }

            if (m_ItemShapeGridData == null) {
                Debug.LogError("The Item Shape Grid data is null.");
                return;
            }

            // Make sure the grid size is the same TODO resize the UI grid to match!?
            if (m_GridSize != m_ItemShapeGridData.GridSize) {
                Debug.LogError(
                    "The grid size of the Inventory Grid (on the Grid gameobject) and the Inventory Grid Item Shape Data (on the Inventory gameobject) does not match!",
                    gameObject);
            }
        }

        /// <summary>
        /// The inventory was unbound from the container.
        /// </summary>
        protected override void OnInventoryUnbound()
        {
            m_ItemShapeGridData = null;
        }

        /// <summary>
        /// Change the grid size.
        /// </summary>
        /// <param name="newSize">The new grid size.</param>
        /// <param name="spawnMissingSlots">Spawn/remove the missing/additional slots?</param>
        public void SetGridSize(Vector2Int newSize, bool spawnMissingSlots)
        {
            m_GridSize = newSize;

            if (spawnMissingSlots == false) { return; }

            m_GridLayoutGroup.constraintCount = newSize.x;

            var gridContentTransform = m_GridLayoutGroup.transform;
            var childCount = gridContentTransform.childCount;
            for (int i = childCount - 1; i >= 0; i--) {
                var child = gridContentTransform.GetChild(i).gameObject;
                if (Application.isPlaying) {
                    Destroy(child);
                } else {
                    DestroyImmediate(child);
                }
            }

            var fullSize = GridFullSize;
            for (int i = 0; i < fullSize; i++) {
                Instantiate(m_ItemViewSlotPrefab, Vector3.zero, Quaternion.identity, gridContentTransform);
            }
        }

        /// <summary>
        /// Set the item shape size and optionally the grid layout cell size.
        /// </summary>
        /// <param name="newSize">The new cell size.</param>
        /// <param name="updateGridLayout">Update the grid layout cell size?</param>
        public void SeItemShapeSize(Vector2 newSize, bool updateGridLayout)
        {
            m_ItemShapeSize = newSize;

            if (updateGridLayout == false || m_GridLayoutGroup == null) { return; }

            m_GridLayoutGroup.cellSize = newSize;
        }

        /// <summary>
        /// Remove an item from the index.
        /// </summary>
        /// <param name="itemInfo">The item info to remove.</param>
        /// <param name="index">The index to remove the item from.</param>
        /// <returns>The item info removed.</returns>
        public override ItemInfo RemoveItem(ItemInfo itemInfo, int index)
        {
            return Inventory.RemoveItem(itemInfo);
        }

        /// <summary>
        /// Can the item be added without specifying a specific position.
        /// </summary>
        /// <param name="itemInfo">The item Info to add.</param>
        /// <param name="receivingItemCollection">The item collection to add the item to (can be null).</param>
        /// <returns></returns>
        public bool CanAddItem(ItemInfo itemInfo, ItemCollection receivingItemCollection = null)
        {
            return m_ItemShapeGridData.CanAddItem(itemInfo, receivingItemCollection);
        }

        /// <summary>
        /// Add an item to any position.
        /// </summary>
        /// <param name="itemInfo">The itemInfo to add.</param>
        /// <param name="receivingItemCollection">The item collection to add the item to (can be null).</param>
        /// <returns></returns>
        public ItemInfo AddItem(ItemInfo itemInfo, ItemCollection receivingItemCollection = null)
        {
            return m_ItemShapeGridData.AddItem(itemInfo, receivingItemCollection);
        }

        /// <summary>
        /// Can the item be added at index?
        /// </summary>
        /// <param name="itemInfo">The item info to add.</param>
        /// <param name="index">The index to add to item to.</param>
        /// <returns>True if the item can be added.</returns>
        public override bool CanAddItem(ItemInfo itemInfo, int index)
        {
            var canAddBase = base.CanAddItem(itemInfo, index);
            if (canAddBase == false) { return false; }

            if (Inventory.CanAddItem(itemInfo, null) == null) { return false; }

            var itemPos = m_ItemShapeGridData.OneDTo2D(index);
            if (m_ItemShapeGridData.IsPositionAvailable(itemInfo, itemPos) == false) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Add the item at the index.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="index">The index of the item.</param>
        /// <returns>The added item.</returns>
        public override ItemInfo AddItem(ItemInfo itemInfo, int index)
        {
            if (CanAddItem(itemInfo, index) == false) { return ItemInfo.None; }

            return m_ItemShapeGridData.AddItemToPosition(itemInfo, index);
        }

        /// <summary>
        /// Can the item be moved from one index to another.
        /// </summary>
        /// <param name="sourceIndex">The source index.</param>
        /// <param name="destinationIndex">The destination index.</param>
        /// <returns>True if the item can be moved.</returns>
        public override bool CanMoveItem(int sourceIndex, int destinationIndex)
        {
            if (base.CanMoveItem(sourceIndex, destinationIndex) == false) { return false; }

            return m_ItemShapeGridData.CanMoveIndex(
                m_ItemShapeGridData.OneDTo2D(sourceIndex),
                m_ItemShapeGridData.OneDTo2D(destinationIndex));
        }

        /// <summary>
        /// Move the item from one index to another.
        /// </summary>
        /// <param name="sourceIndex">The source index.</param>
        /// <param name="destinationIndex">The destination index.</param>
        public override void MoveItem(int sourceIndex, int destinationIndex)
        {
            if (CanMoveItem(sourceIndex, destinationIndex) == false) { return; }

            var sourceItemIndex = sourceIndex;
            var destinationItemIndex = destinationIndex;

            var result = m_ItemShapeGridData.TryMoveIndex(m_ItemShapeGridData.OneDTo2D(sourceItemIndex),
                m_ItemShapeGridData.OneDTo2D(destinationItemIndex));
        }

        /// <summary>
        /// Draw the item view slots.
        /// </summary>
        protected override void DrawInternal()
        {
            var foregroundViewCount = 0;
            var gridFullSize = m_GridSize.x * m_GridSize.y;
            for (int i = 0; i < gridFullSize; i++) {
                var gridElementData = m_ItemShapeGridData.GetElementAt(i);

                //Set/Draw Items in the background. Used to know which items view slot are set.
                var itemInfo = (ItemInfo)gridElementData.ItemStack;

                m_ItemViewDrawer.DrawView(i, i, itemInfo);
                AssignItemToSlot(itemInfo, i);

                var itemView = GetItemViewAt(i);

                ItemShapeItemView backgroundItemShapeView = itemView.gameObject.GetCachedComponent<ItemShapeItemView>();

                if (backgroundItemShapeView == null) {
                    Debug.LogError("The Item Views must have an Item Shape Item View component when using a Item Shape Grid.", gameObject);
                    continue;
                }

                backgroundItemShapeView.SetGridInfo(this, i, false);

                //Draw Item in the foreground. Visual item which overflows on multiple slots.
                if (gridElementData.IsAnchor == false) {
                    continue;
                }

                DrawInViewForeground(foregroundViewCount, i, itemInfo, backgroundItemShapeView);
                foregroundViewCount++;
            }

            // Remove all the excessive Foreground Item Views.
            for (int i = foregroundViewCount; i < m_ForegroundItemViews.Count; i++) {
                if (m_ForegroundItemViews[i] != null) {
                    ObjectPool.Destroy(m_ForegroundItemViews[i]);
                    m_ForegroundItemViews[i] = null;
                }
            }

            UpdateNavigation();
        }

        /// <summary>
        /// Draw the Item view in the foreground.
        /// </summary>
        /// <param name="viewIndex">The foreground view index.</param>
        /// <param name="gridIndex">The grid index.</param>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="backgroundItemShapeView">The background item shape view.</param>
        protected virtual void DrawInViewForeground(int viewIndex, int gridIndex, ItemInfo itemInfo, ItemShapeItemView backgroundItemShapeView)
        {
            var gridPos = m_ItemShapeGridData.OneDTo2D(gridIndex);

            ItemView view = null;
            var viewPrefab = GetViewPrefabFor(itemInfo);
            
            if (viewIndex >= 0 && viewIndex < m_ForegroundItemViews.Count && m_ForegroundItemViews[viewIndex] != null) {
                // A view already exists.
                var currentView = m_ForegroundItemViews[viewIndex];
                if (viewPrefab == ObjectPool.GetOriginalObject(currentView.gameObject)) {
                    view = currentView;
                } else {
                    ObjectPool.Destroy(currentView.gameObject);
                    m_ForegroundItemViews[viewIndex] = null;
                }
            }

            if (view == null) {
                var viewGO = ObjectPool.Instantiate(viewPrefab, m_ItemShapeViewContent);
                view = viewGO.GetComponent<ItemView>();

                if (view == null) {
                    Debug.LogWarning("The Box Drawer BoxUI Prefab does not have a BoxUI component or it is not of the correct Type");
                    return;
                }
                
                view.RectTransform.anchorMax = new Vector2(0, 1);
                view.RectTransform.anchorMin = new Vector2(0, 1);
                view.RectTransform.pivot = new Vector2(0, 1);
                view.RectTransform.localScale = Vector3.one;
            }

            view.RectTransform.sizeDelta = m_ItemShapeSize;
            view.RectTransform.anchoredPosition =
                new Vector2(gridPos.x * m_ItemShapeSize.x, -gridPos.y * m_ItemShapeSize.y);

            view.SetValue(itemInfo);
            var foregroundItemShapeView = view.GetViewModule<ItemShapeItemView>();
            foregroundItemShapeView.SetGridInfo(this, gridIndex, true);
            backgroundItemShapeView.ForegroundItemView = foregroundItemShapeView;

            if (viewIndex >= m_ForegroundItemViews.Count) {
                m_ForegroundItemViews.Insert(viewIndex, view);
            } else {
                m_ForegroundItemViews[viewIndex] = view;
            }
        }

        /// <summary>
        /// Get the Box prefab for the item specified.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>The view prefab game object.</returns>
        public override GameObject GetViewPrefabFor(ItemInfo itemInfo)
        {
            return m_ItemViewDrawer.GetViewPrefabFor(itemInfo);
        }

        /// <summary>
        /// Handle an item view slot beginning a drag event.
        /// </summary>
        /// <param name="eventdata">The event data.</param>
        private void HandleItemViewSlotBeginDrag(ItemViewSlotPointerEventData eventdata)
        {
            if (m_ItemShapeGridData == null) { return; }

            if (m_ItemShapeGridData.TryFindAnchorForItem(eventdata.ItemView.ItemInfo, out var anchorPos) == false) {
                return;
            }

            var itemSlotPos = m_ItemShapeGridData.OneDTo2D(eventdata.Index);

            var modules = m_SlotCursor.FloatingItemView.Modules;

            for (int i = 0; i < modules.Count; i++) {
                if (modules[i] is ItemShapeItemView itemShapeItemView) {
                    itemShapeItemView.DiscreteOffsetImage(anchorPos - itemSlotPos);
                }
            }
        }

        /// <summary>
        /// Set the navigation type.
        /// </summary>
        public void SetNavigationTypePerItem()
        {
            NavigationType = ItemShapeNavigation.PerItem;
        }
        
        /// <summary>
        /// Set the navigation type.
        /// </summary>
        public void SetNavigationTypePerSlot()
        {
            NavigationType = ItemShapeNavigation.PerSlot;
        }
        
        /// <summary>
        /// Set the navigation type.
        /// </summary>
        public void SetNavigationTypeCustom()
        {
            NavigationType = ItemShapeNavigation.Custom;
        }

        /// <summary>
        /// Update the navigation.
        /// </summary>
        protected virtual void UpdateNavigation()
        {
            if (m_NavigationType == ItemShapeNavigation.Custom) {
                return;
            }

            if (m_NavigationType == ItemShapeNavigation.PerSlot) {
                SetGridLayoutGroupNavigation(m_GridLayoutGroup, false);
                return;
            }

            if (m_NavigationType == ItemShapeNavigation.PerItem) {
                SetNavigationPerItem(m_GridLayoutGroup);
                return;
            }
        }
        
        /// <summary>
        /// Set the grid layout group navigation.
        /// </summary>
        /// <param name="gridLayoutGroup">The grid layout group.</param>
        /// <param name="gridWrap">Should the navigation wrap around?</param>
        public void SetNavigationPerItem(GridLayoutGroup gridLayoutGroup)
        {
            Vector2Int gridSize = Vector2Int.zero;

            if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.Flexible) {
                return;
            }

            var gridParent = gridLayoutGroup.transform;
            
            if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount) {
                gridSize = new Vector2Int(gridLayoutGroup.constraintCount,
                    gridParent.childCount / gridLayoutGroup.constraintCount);
            }
            if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedRowCount) {
                gridSize = new Vector2Int(gridParent.childCount / gridLayoutGroup.constraintCount,
                    gridLayoutGroup.constraintCount);
            }

            var horizontalStartAxis = gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal;
            
            // Go through all the anchors first
            for (int i = 0; i < gridSize.y; i++) {
                for (int j = 0; j < gridSize.x; j++) {
                   
                    var index = horizontalStartAxis ?
                        i * gridSize.x + j :
                        j * gridSize.y + i;

                    var currentItemViewSlot = m_ItemViewSlots[index];
                    if (IsSlotAnchor(currentItemViewSlot) == false) { continue; }

                    var navigation = currentItemViewSlot.navigation;
                    navigation.mode = Navigation.Mode.Explicit;
                    
                    navigation.selectOnUp =  GetAnchorSlotInDirection(Vector2Int.up,  j, i, horizontalStartAxis, gridSize, currentItemViewSlot);
                    navigation.selectOnDown =  GetAnchorSlotInDirection(Vector2Int.down, j, i, horizontalStartAxis, gridSize, currentItemViewSlot);
                    navigation.selectOnLeft = GetAnchorSlotInDirection(Vector2Int.left, j, i, horizontalStartAxis, gridSize, currentItemViewSlot);
                    navigation.selectOnRight = GetAnchorSlotInDirection(Vector2Int.right, j, i, horizontalStartAxis, gridSize, currentItemViewSlot);

                    currentItemViewSlot.navigation = navigation;
                }
            }

            // Go through all the non-anchors and copy the anchors navigation
            for (int i = 0; i < gridSize.y; i++) {
                for (int j = 0; j < gridSize.x; j++) {
                   
                    var index = horizontalStartAxis ?
                        i * gridSize.x + j :
                        j * gridSize.y + i;
                    
                    var currentItemViewSlot = m_ItemViewSlots[index];
                    var itemShapeView = currentItemViewSlot.ItemView.gameObject.GetCachedComponent<ItemShapeItemView>();
                    if (itemShapeView.IsAnchor) { continue; }
                    
                    var anchorIndex = itemShapeView.AnchorIndex;

                    if (anchorIndex < 0 || anchorIndex >= m_ItemViewSlots.Length) {
                        continue;
                    }

                    var anchorItemViewSlot = m_ItemViewSlots[anchorIndex];

                    currentItemViewSlot.navigation = anchorItemViewSlot.navigation;
                }
            }
        }

        protected static bool IsSlotAnchor(ItemViewSlot currentItemViewSlot)
        {
            var itemShapeView = currentItemViewSlot.ItemView.gameObject.GetCachedComponent<ItemShapeItemView>();
            return itemShapeView.IsAnchor;
        }

        /// <summary>
        /// Get the Anchor slot above the specified position.
        /// </summary>
        /// <param name="direction">The direction in which to search</param>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="startHorizontal">Does the grid start in horizontal axis?</param>
        /// <param name="gridSize">The grid size.</param>
        /// <param name="exception">Ingore that item view slot.</param>
        /// <returns>The anchor Item View Slot above the current position.</returns>
        private ItemViewSlot GetAnchorSlotInDirection(Vector2Int direction, int x, int y, bool startHorizontal, Vector2Int gridSize, ItemViewSlot exception)
        {
            if (direction == Vector2Int.zero) {
                Debug.LogWarning("Searching in not direction is not allowed");
                return null;
            }
            
            while ( y >= 0 && y < gridSize.y && x >= 0 && x < gridSize.x) {
                
                var index = startHorizontal?
                    y * gridSize.x + x:
                    x * gridSize.y + y;
                
                if (index < 0 || index >= m_ItemViewSlots.Length) {
                    continue;
                }
                
                if (TryGetAnchorItemViewSlot(index, out var viewSlot) && viewSlot != exception) {
                    return viewSlot;
                }

                if (startHorizontal) {
                    x += direction.x;
                    y -= direction.y;
                } else {
                    x += direction.x;
                    y -= direction.y;
                }
                
            }

            return null;
        }

        /// <summary>
        /// Try to get the selectable at the index provided.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="onlyAnchor">Only return true if the index is the anchor index.</param>
        /// <param name="itemViewSlot">The selectable output.</param>
        /// <returns>True if it exists.</returns>
        private bool TryGetAnchorItemViewSlot(int index, out ItemViewSlot itemViewSlot)
        {
            if (index < 0 || index >= m_ItemViewSlots.Length) {
                itemViewSlot = null;
                return false;
            }

            itemViewSlot = m_ItemViewSlots[index];
            var itemShapeView = itemViewSlot.ItemView.gameObject.GetCachedComponent<ItemShapeItemView>();

            var anchorIndex = itemShapeView.AnchorIndex;

            if (anchorIndex < 0 || anchorIndex >= m_ItemViewSlots.Length) {
                itemViewSlot = null;
                return false;
            }

            itemViewSlot = m_ItemViewSlots[anchorIndex];
            return true;
        }

        /// <summary>
        /// Set the grid layout group navigation.
        /// </summary>
        /// <param name="gridLayoutGroup">The grid layout group.</param>
        /// <param name="gridWrap">Should the navigation wrap around?</param>
        public void SetGridLayoutGroupNavigation(GridLayoutGroup gridLayoutGroup, bool gridWrap)
        {
            Vector2Int gridSize = Vector2Int.zero;

            if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.Flexible) {
                return;
            }

            var gridParent = gridLayoutGroup.transform;
            
            if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount) {
                gridSize = new Vector2Int(gridLayoutGroup.constraintCount,
                    gridParent.childCount / gridLayoutGroup.constraintCount);
            }
            if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedRowCount) {
                gridSize = new Vector2Int(gridParent.childCount / gridLayoutGroup.constraintCount,
                    gridLayoutGroup.constraintCount);
            }

            for (int i = 0; i < gridSize.y; i++) {
                for (int j = 0; j < gridSize.x; j++) {

                    var nextChildIndex = -1;
                    var previousXIndex = -1;
                    var previousYIndex = -1;
                    bool noWrapX = false;
                    bool noWrapY = false;

                    if (gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal) {
                        nextChildIndex = i * gridSize.x + j;
                        previousXIndex = i * gridSize.x + (j - 1);
                        previousYIndex = (i - 1) * gridSize.x + j;
                        noWrapX = !gridWrap && j == 0;
                        noWrapY = false;
                    } else {
                        nextChildIndex = j * gridSize.y + i;
                        previousXIndex = (j - 1) * gridSize.y + i;
                        previousYIndex = j * gridSize.y + (i - 1);
                        noWrapX = false;
                        noWrapY = !gridWrap && i == 0;
                    }


                    if (TryGetSelectableChild(gridParent, nextChildIndex, out var selectable)) {

                        TryGetSelectableChild(gridParent, previousXIndex, out var previousX);
                        if (noWrapX) { previousX = null; }

                        TryGetSelectableChild(gridParent, previousYIndex, out var previousY);
                        if (noWrapY) { previousY = null; }

                        var navigation = selectable.navigation;
                        navigation.mode = Navigation.Mode.Explicit;
                        navigation.selectOnUp = previousY;
                        navigation.selectOnDown = null;
                        navigation.selectOnLeft = previousX;
                        navigation.selectOnRight = null;

                        selectable.navigation = navigation;

                        if (previousX != null) {
                            var previousNavigation = previousX.navigation;
                            previousNavigation.selectOnRight = selectable;
                            previousX.navigation = previousNavigation;
                        }

                        if (previousY != null) {
                            var previousNavigation = previousY.navigation;
                            previousNavigation.selectOnDown = selectable;
                            previousY.navigation = previousNavigation;
                        }
                    }
                }
            }
        }

         /// <summary>
         /// Try to get the selectable at the index provided.
         /// </summary>
         /// <param name="gridParent">The grid parent transform.</param>
         /// <param name="index">The index.</param>
         /// <param name="selectable">The selectable output.</param>
         /// <returns>True if it exists.</returns>
         private bool TryGetSelectableChild(Transform gridParent, int index, out Selectable selectable)
         {
             if (index < 0 || index >= gridParent.childCount) {
                 selectable = null;
                 return false;
             }

             var child = gridParent.GetChild(index);
             if (child.gameObject.activeSelf == false) {
                 selectable = null;
                 return false;
             }

             selectable = gridParent.GetChild(index).GetComponent<Selectable>();
             return selectable != null;
         }

    }
}