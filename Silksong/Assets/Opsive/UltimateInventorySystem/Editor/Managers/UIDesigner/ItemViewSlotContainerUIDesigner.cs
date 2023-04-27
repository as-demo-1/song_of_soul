/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.Shared.Editor.Inspectors.Utility;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Editor.Inspectors;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Item.DragAndDrop;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ActionPanels;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    public abstract class ItemViewSlotsContainerOption : UIDesignerBoxBase
    {
        protected ItemViewSlotsContainerOptions m_ItemViewContainerEditor;

        protected ItemViewSlotsContainerBase ItemViewContainer => m_ItemViewContainerEditor.ItemViewSlotsContainer;
        protected DisplayPanel DisplayPanel => m_ItemViewContainerEditor.DisplayPanel;

        protected VisualElement m_Container;

        protected ItemViewSlotsContainerOption(ItemViewSlotsContainerOptions itemViewContainerEditor)
        {
            m_ItemViewContainerEditor = itemViewContainerEditor;
            m_Container = new VisualElement();
            Add(m_Container);
        }

        public void Refresh()
        {
            m_Container.Clear();
            Refresh(m_Container);
        }

        protected abstract void Refresh(VisualElement container);

    }

    public class ItemViewSlotsContainerOptions : UIDesignerBoxBase
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/item-view-slots-container/";
        public override string Title => "Item View Slots Container Common Options";
        public override string Description => "Use the options below to add and edit functionality to your item view slots container";
        public override Func<Component> SelectTargetGetter => () => m_ItemViewSlotsContainer;

        protected ItemViewSlotsContainerBase m_ItemViewSlotsContainer;
        protected DisplayPanel m_DisplayPanel;

        protected VisualElement m_Container;

        public ItemViewSlotsContainerBase ItemViewSlotsContainer => m_ItemViewSlotsContainer;
        public DisplayPanel DisplayPanel => m_DisplayPanel;

        private ItemViewDrawerOptions m_ItemViewDrawerOptions;
        private ItemActionsOptions m_ItemActionsOptions;
        private ItemDescriptionOptions m_ItemDescriptionOptions;
        private MovingItemsOptions m_MovingItemsOptions;

        public ItemViewSlotsContainerOptions()
        {
            m_ItemViewDrawerOptions = new ItemViewDrawerOptions(this);

            m_ItemActionsOptions = new ItemActionsOptions(this);

            m_ItemDescriptionOptions = new ItemDescriptionOptions(this);

            m_MovingItemsOptions = new MovingItemsOptions(this);

            m_Container = new VisualElement();
            Add(m_Container);
        }

        public void Refresh(ItemViewSlotsContainerBase itemViewSlotsContainer)
        {
            m_Container.Clear();

            m_ItemViewSlotsContainer = itemViewSlotsContainer;

            if (m_ItemViewSlotsContainer == null) {
                m_Container.Add(new Label("Item View Slots Container not found for target"));
                return;
            }

            m_DisplayPanel = m_ItemViewSlotsContainer.gameObject.GetComponentInParent<DisplayPanel>(true);
            if (m_DisplayPanel == null) {
                Debug.LogWarning("Display panel is null");
            }

            m_ItemViewDrawerOptions.Refresh();
            m_ItemActionsOptions.Refresh();
            m_ItemDescriptionOptions.Refresh();
            m_MovingItemsOptions.Refresh();

            m_Container.Add(m_ItemViewDrawerOptions);
            m_Container.Add(m_ItemActionsOptions);
            m_Container.Add(m_ItemDescriptionOptions);
            m_Container.Add(m_MovingItemsOptions);

        }
    }

    public class ItemActionsOptions : ItemViewSlotsContainerOption
    {
        public override string Title => "Item Actions";
        public override string Description => "You may use an item action or open an item action panel when clicking an item (or using one of the inventory system input)\n" +
                                              "Do not forget to assign the Category Item Actions (Set) asset to the Item Action Binding once created.";

        protected ItemActionPanel m_ItemActionPanel;
        protected ItemViewSlotsContainerItemActionBindingBase m_ItemActionBinding;
        protected ItemViewSlotPanelToTooltip m_PanelToTooltip;

        protected List<ItemViewSlotsContainerItemActionBindingBase> m_ItemActionBindings;
        protected ReorderableList m_ReorderableList;
        protected VisualElement m_SelectedContainer;

        protected ComponentSelectionButton m_SelectBinding;

        protected ObjectFieldWithNestedInspector<CategoryItemActionSet, CategoryItemActionSetInspector>
            m_CategoryItemActionSet;
        protected ObjectFieldWithNestedInspector<ItemActionSet, ItemActionSetInspector>
            m_CategoryItemActions;

        protected CreateSelectDeleteContainer m_ItemActionPanelContainer;
        protected CreateSelectDeleteContainer m_PanelToTooltipContainer;

        public ItemActionsOptions(ItemViewSlotsContainerOptions itemViewContainerEditor) : base(itemViewContainerEditor)
        {
            m_ItemActionBindings = new List<ItemViewSlotsContainerItemActionBindingBase>();
            m_SelectedContainer = new VisualElement();


            m_SelectBinding = new ComponentSelectionButton("Select Item Action Binding", () => m_ItemActionBinding);

            m_ItemActionPanelContainer = new CreateSelectDeleteContainer("Item Action Panel",
                CreateItemActionPanel,
                DeleteItemActionPanel,
                () => m_ItemActionPanel);

            m_PanelToTooltipContainer = new CreateSelectDeleteContainer("Panel To Tooltip",
                CreatePanelToTooltip,
                DeletePanelToTooltip,
                () => m_PanelToTooltip);


            m_ReorderableList = new ReorderableList(m_ItemActionBindings,
                (container, index) =>
                {
                    container.Add(new Label("Missing"));
                },
                (container, index) =>
                {
                    var label = container.ElementAt(0) as Label;
                    label.text = m_ItemActionBindings[index]?.ToString() ?? "NULL";
                },
                (container) =>
                {
                    container.Add(new Label("Item Action Bindings"));
                },
                HandleItemActionSelect,
                () =>
                {
                    InspectorUtility.AddObjectType(typeof(ItemViewSlotsContainerItemActionBindingBase),
                        null,
                        (evt) =>
                        {
                            var target = ItemViewContainer;
                            if (target == null) { return; }

                            var newObj = target.gameObject.AddComponent(evt as Type) as ItemViewSlotsContainerItemActionBindingBase;

                            m_ItemActionBindings.Add(newObj);

                            Refresh();
                        });
                }, (index) =>
                {
                    var element = m_ItemActionBindings[index];

                    m_ItemActionBindings.RemoveAt(index);

                    RemoveComponent(element);

                    Refresh();
                }, null
            );

        }

        protected override void Refresh(VisualElement container)
        {
            m_SelectedContainer.Clear();

            m_ItemActionBindings.Clear();

            m_ItemActionBindings.AddRange(ItemViewContainer.GetComponents<ItemViewSlotsContainerItemActionBindingBase>());

            m_ReorderableList.Refresh();
            container.Add(m_ReorderableList);
            container.Add(m_SelectedContainer);

            var selectedIndex = m_ReorderableList.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= m_ItemActionBindings.Count) {
                return;
            }

            HandleItemActionSelect(selectedIndex);
        }

        private void HandleItemActionSelect(int index)
        {
            m_ItemActionBinding = m_ItemActionBindings[index];
            m_ItemActionPanel = m_ItemActionBinding.m_ActionPanel;
            m_PanelToTooltip = m_ItemActionPanel?.GetComponent<ItemViewSlotPanelToTooltip>();

            ComponentSelection.Select(m_ItemActionBindings[index]);

            m_SelectedContainer.Clear();

            m_SelectedContainer.Add(m_SelectBinding);

            SetNestedObjectActionField();

            m_SelectedContainer.Add(new SubTitleLabel("Item Action Panel"));

            m_ItemActionPanelContainer.Refresh();
            m_SelectedContainer.Add(m_ItemActionPanelContainer);

            if (m_ItemActionPanel == null) {
                return;
            }

            m_SelectedContainer.Add(new SubTitleLabel("Panel To Tooltip"));

            m_PanelToTooltipContainer.Refresh();
            m_SelectedContainer.Add(m_PanelToTooltipContainer);
        }

        private void SetNestedObjectActionField()
        {
            if (m_ItemActionBinding is ItemViewSlotsContainerItemActionBinding itemActionsBinding) {

                m_CategoryItemActions = new ObjectFieldWithNestedInspector
                    <ItemActionSet, ItemActionSetInspector>(
                        "Category Item Action Set",
                        itemActionsBinding.m_ItemActionSet,
                        "The categories item actions. Specifies the actions that can be performed on each item. Can be null.",
                        (newValue) =>
                        {
                            itemActionsBinding.m_ItemActionSet = newValue;
                            Shared.Editor.Utility.EditorUtility.SetDirty(newValue);
                            Shared.Editor.Utility.EditorUtility.SetDirty(itemActionsBinding);
                        });
                m_SelectedContainer.Add(m_CategoryItemActions);
            } else if (m_ItemActionBinding is ItemViewSlotsContainerCategoryItemActionSetBinding itemActionsSetBinding) {
                m_CategoryItemActionSet = new ObjectFieldWithNestedInspector
                    <CategoryItemActionSet, CategoryItemActionSetInspector>(
                        "Category Item Action Set",
                        itemActionsSetBinding.m_CategoryItemActionSet,
                        "The categories item actions. Specifies the actions that can be performed on each item. Can be null.",
                        (newValue) =>
                        {
                            itemActionsSetBinding.m_CategoryItemActionSet = newValue;
                            Shared.Editor.Utility.EditorUtility.SetDirty(newValue);
                            Shared.Editor.Utility.EditorUtility.SetDirty(itemActionsSetBinding);
                        });
                m_SelectedContainer.Add(m_CategoryItemActionSet);
            }
        }

        private void CreateItemActionPanel()
        {
            var itemActionPanel =
                UIDesignerManager.InstantiateSchemaPrefab<ItemActionPanel>(
                    UIDesignerManager.UIDesignerSchema.ItemActionPanel, DisplayPanel.MainContent);
            itemActionPanel.m_ActionButtonPrefab = UIDesignerManager.UIDesignerSchema.ItemActionButton.gameObject;

            m_ItemActionBinding.m_ActionPanel = itemActionPanel;

            Refresh();
        }

        private void DeleteItemActionPanel()
        {
            DestroyGameObject(m_ItemActionPanel);
        }

        private void CreatePanelToTooltip()
        {
            var tooltipPanel = m_ItemActionPanel.gameObject.AddComponent<ItemViewSlotPanelToTooltip>();
            tooltipPanel.m_ItemViewSlotContainer = ItemViewContainer;

            tooltipPanel.m_PlaceOnClick = true;
            tooltipPanel.m_ShowOnClick = true;

            tooltipPanel.m_PlaceOnSelect = false;
            tooltipPanel.m_ShowOnSelect = false;

            tooltipPanel.m_HideShowOnDeselect = false;

            Refresh();
        }

        private void DeletePanelToTooltip()
        {
            RemoveComponent(m_PanelToTooltip);
            Refresh();
        }
    }

    public class ItemDescriptionOptions : ItemViewSlotsContainerOption
    {
        public override string Title => "Item Description";
        public override string Description => "Display a description of the item. The Item Description is an ItemView with some extra features, use ItemViewModules to show more details about the item.\n" +
                                              "'Category Attribute View Set Item View Module' is automatically added when creating a description but it is not required\n" +
                                              "Additionally you may transform the panel into a Tooltip using choose if the description works as a tooltip, a fixed panel, ect...";
        public override Func<Component> SelectTargetGetter => () => m_ItemDescriptionBinding;

        protected ItemViewSlotContainerDescriptionBinding m_ItemDescriptionBinding;
        protected ComponentSelectionButton m_SelectDescription;

        protected ViewModulesReorderableList<ItemView, ItemViewModule> m_ItemViewModulesReorderableList;

        public ItemDescriptionOptions(ItemViewSlotsContainerOptions itemViewContainerEditor) : base(itemViewContainerEditor)
        {
            m_SelectDescription = new ComponentSelectionButton("Select Item Description", () => m_ItemDescriptionBinding?.ItemDescription);

            m_ItemViewModulesReorderableList = new ViewModulesReorderableList<ItemView, ItemViewModule>();
        }

        protected override void Refresh(VisualElement container)
        {
            m_ItemDescriptionBinding = ItemViewContainer.GetComponent<ItemViewSlotContainerDescriptionBinding>();

            CreateButton("Go To Item Description Designer to Create or Edit", m_Container, () =>
            {
                var descriptionDesigner = UIDesignerManager.GetTab<ItemDescriptionDesigner>();
                UIDesignerManager.ChangeTab(descriptionDesigner);

                var description = m_ItemDescriptionBinding?.ItemDescription;

                descriptionDesigner.DesignerEditor.SetTarget(description);

                if (description == null) {
                    descriptionDesigner.DesignerCreator.ParentTransformField.value =
                        m_ItemViewContainerEditor.DisplayPanel.MainContent;
                    return;
                }

                descriptionDesigner.DesignerEditor.ItemViewSlotsContainerItemDescriptionOption.SetItemViewSlotContainer(
                    m_ItemViewContainerEditor.ItemViewSlotsContainer);
            });

            if (m_ItemDescriptionBinding == null) { return; }

            m_Container.Add(m_SelectDescription);
        }
    }

    public class ItemViewDrawerOptions : ItemViewSlotsContainerOption
    {
        public override string Title => "Item View Drawer";

        public override string Description =>
            "The Item View Drawer is used to spawn the appropriate item views for the item to display.\n" +
            "The Item Views can be set as prefabs and are organized by item categories. The order and inheritance are important to find the best match for the item to display.\n";

        public override Func<Component> SelectTargetGetter => () => m_ItemViewDrawer;

        protected ItemViewDrawer m_ItemViewDrawer;
        protected ObjectFieldWithNestedInspector<CategoryItemViewSet, CategoryItemViewSetInspector> m_CategoryItemViewSetField;

        public ItemViewDrawerOptions(ItemViewSlotsContainerOptions itemViewContainerEditor) : base(itemViewContainerEditor)
        {
            m_CategoryItemViewSetField = new ObjectFieldWithNestedInspector<CategoryItemViewSet, CategoryItemViewSetInspector>(
                "Category Item View Set",
                null,
                "The category item view set is used to define the link between item categories and item views",
                (newValue) =>
                {
                    m_ItemViewDrawer.CategoryItemViewSet = newValue;
                    Shared.Editor.Utility.EditorUtility.SetDirty(newValue);
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemViewDrawer);
                    Refresh();
                }, true);
        }

        protected override void Refresh(VisualElement container)
        {
            m_ItemViewDrawer = ItemViewContainer.GetComponent<ItemViewDrawer>();

            if (m_ItemViewDrawer == null) {
                container.Add(new Label("No Item View Drawer was found"));
                return;
            }

            m_CategoryItemViewSetField.value = m_ItemViewDrawer.CategoryItemViewSet;

            container.Add(m_CategoryItemViewSetField);
        }
    }

    public class MovingItemsOptions : ItemViewSlotsContainerOption
    {
        public override string Title => "Moving Items";
        public override string Description => "Choose whether items can be moved using an item action, using drag & drop, etc.\n" +
                                              "The Item View Drop Handler component is required for both Move and drag & drop.\n" +
                                              "The Drop Handler references the Drop Action Set which has the logic for the decision making when an itemView is dropped or moved to that slot";

        protected ItemViewSlotCursorManager m_CursorManager;
        protected ItemViewDropHandler m_DropHandler;
        protected ItemViewSlotDragHandler m_DragHandler;
        protected ItemViewSlotMoveCursor m_MoveCursor;

        protected CursorManagerOption m_CursorManagerOption;
        protected DropHandlerOption m_DropHandlerOption;
        protected DragHandlerOption m_DragHandlerOption;
        protected MoveCursorOption m_MoveCursorOption;

        public MovingItemsOptions(ItemViewSlotsContainerOptions itemViewContainerEditor) : base(itemViewContainerEditor)
        {
            m_CursorManagerOption = new CursorManagerOption(this);
            m_DropHandlerOption = new DropHandlerOption(this);
            m_DragHandlerOption = new DragHandlerOption(this);
            m_MoveCursorOption = new MoveCursorOption(this);
        }

        protected override void Refresh(VisualElement container)
        {
            if (ItemViewContainer == null) {
                return;
            }

            m_CursorManager = ItemViewContainer.gameObject.GetComponentInParent<ItemViewSlotCursorManager>(true);
            m_DropHandler = ItemViewContainer.GetComponent<ItemViewDropHandler>();
            m_DragHandler = ItemViewContainer.GetComponent<ItemViewSlotDragHandler>();
            m_MoveCursor = ItemViewContainer.GetComponent<ItemViewSlotMoveCursor>();

            m_CursorManagerOption.Refresh(m_CursorManager);
            m_DropHandlerOption.Refresh(m_DropHandler);
            m_DragHandlerOption.Refresh(m_DragHandler);
            m_MoveCursorOption.Refresh(m_MoveCursor);

            container.Add(m_CursorManagerOption);
            container.Add(m_DropHandlerOption);
            container.Add(m_DragHandlerOption);
            container.Add(m_MoveCursorOption);
        }

        public abstract class MovingItemsOption<T> : VisualElement where T : Component
        {
            public abstract string Title { get; }
            public abstract string Description { get; }

            protected T m_Target;
            protected MovingItemsOptions m_MovingItemOptions;

            protected CreateSelectDeleteContainer m_CreateSelectDeleteContainer;

            public MovingItemsOption(MovingItemsOptions movingItemsOptions)
            {
                m_MovingItemOptions = movingItemsOptions;

                Add(new SubTitleLabel(Title));
                Add(new SubDescriptionLabel(Description));

                m_CreateSelectDeleteContainer = new CreateSelectDeleteContainer(Title, Create, Delete, Get);
                Add(m_CreateSelectDeleteContainer);
            }

            protected abstract void Create();

            protected abstract void Delete();

            protected Component Get()
            {
                return m_Target;
            }

            public virtual void Refresh(T target)
            {
                m_Target = target;
                m_CreateSelectDeleteContainer.Refresh();
            }
        }

        public class CursorManagerOption : MovingItemsOption<ItemViewSlotCursorManager>
        {
            public override string Title => "Cursor Manager";

            public override string Description =>
                "The Cursor manager is set next to the Canvas manager components. It dictates how the item is shown while moving on the screen\n" +
                "This component is required for any of the drag/drop/move components to work.";

            protected ObjectFieldWithNestedInspector<CategoryItemViewSet, CategoryItemViewSetInspector>
                m_CategoryItemViewSetField;

            protected VisualElement m_OptionContainer;

            public CursorManagerOption(MovingItemsOptions movingItemsOptions) : base(movingItemsOptions)
            {
                m_CategoryItemViewSetField = new ObjectFieldWithNestedInspector<CategoryItemViewSet, CategoryItemViewSetInspector>(
                    "Category Item View Set",
                    null,
                    "The Category Item View Set is optional and may be used to show the Item being dragged on screen.",
                    (newValue) =>
                    {
                        if (m_Target == null) { return; }

                        m_Target.CategoryItemViewSet = newValue;
                    }, true);

                m_OptionContainer = new VisualElement();
                Add(m_OptionContainer);
            }

            protected override void Create()
            {
                var canvas = m_MovingItemOptions.ItemViewContainer.gameObject.GetComponentInParent<Canvas>(true);
                m_Target = canvas.gameObject.AddComponent<ItemViewSlotCursorManager>();
                m_MovingItemOptions.Refresh();
            }

            protected override void Delete()
            {
                m_MovingItemOptions.RemoveComponent(m_Target);
                m_MovingItemOptions.Refresh();
            }

            public override void Refresh(ItemViewSlotCursorManager target)
            {
                base.Refresh(target);

                m_OptionContainer.Clear();

                m_CategoryItemViewSetField.value = target?.CategoryItemViewSet;

                if (target == null) { return; }

                m_OptionContainer.Add(m_CategoryItemViewSetField);
            }
        }

        public class DropHandlerOption : MovingItemsOption<ItemViewDropHandler>
        {
            public override string Title => "Drop Handler";

            public override string Description =>
                "The drop handler is required for dropping or moving items\n" +
                "Don't forget the to set the Drop Action Set";

            protected ObjectFieldWithNestedInspector<ItemViewSlotDropActionSet, ItemViewSlotDropActionSetInspector>
                m_ItemViewSlotDropActionSetField;

            protected VisualElement m_OptionContainer;

            public DropHandlerOption(MovingItemsOptions movingItemsOptions) : base(movingItemsOptions)
            {
                m_ItemViewSlotDropActionSetField = new ObjectFieldWithNestedInspector<ItemViewSlotDropActionSet, ItemViewSlotDropActionSetInspector>(
                    "Item View Slot Drop Action Set",
                    null,
                    "The Item View Slot Drop Action Set allows you to choose exactly what happens when an item is dropped/moved",
                    (newValue) =>
                    {
                        if (m_Target == null) { return; }

                        m_Target.ItemViewSlotDropActionSet = newValue;
                    }, true);

                m_OptionContainer = new VisualElement();
                Add(m_OptionContainer);
            }

            protected override void Create()
            {
                m_Target = m_MovingItemOptions.ItemViewContainer.gameObject.AddComponent<ItemViewDropHandler>();
                m_MovingItemOptions.Refresh();
            }

            protected override void Delete()
            {
                m_MovingItemOptions.RemoveComponent(m_Target);
                m_MovingItemOptions.Refresh();
            }

            public override void Refresh(ItemViewDropHandler target)
            {
                base.Refresh(target);

                m_OptionContainer.Clear();

                m_ItemViewSlotDropActionSetField.value = target?.ItemViewSlotDropActionSet;

                if (target == null) { return; }

                m_OptionContainer.Add(m_ItemViewSlotDropActionSetField);
            }
        }

        public class DragHandlerOption : MovingItemsOption<ItemViewSlotDragHandler>
        {
            public override string Title => "Drag Handler";

            public override string Description =>
                "The drag Handler allows you to drag an item using the mouse";

            public DragHandlerOption(MovingItemsOptions movingItemsOptions) : base(movingItemsOptions)
            { }

            protected override void Create()
            {
                m_Target = m_MovingItemOptions.ItemViewContainer.gameObject.AddComponent<ItemViewSlotDragHandler>();
                m_Target.m_ItemViewSlotCursorManager = m_MovingItemOptions.m_CursorManager;
                m_MovingItemOptions.Refresh();
            }

            protected override void Delete()
            {
                m_MovingItemOptions.RemoveComponent(m_Target);
                m_MovingItemOptions.Refresh();
            }
        }

        public class MoveCursorOption : MovingItemsOption<ItemViewSlotMoveCursor>
        {
            public override string Title => "Move Cursor";

            public override string Description =>
                "The move cursor allows you to move items without a mouse, it is used by the 'Move' Item Action\n" +
                "It is recommended to add the item action binding components to the 'Unbind while moving' field such that clicking to place the item won't trigger the item action";

            public MoveCursorOption(MovingItemsOptions movingItemsOptions) : base(movingItemsOptions)
            { }

            protected override void Create()
            {
                m_Target = m_MovingItemOptions.ItemViewContainer.gameObject.AddComponent<ItemViewSlotMoveCursor>();
                m_Target.m_DropHandler = m_MovingItemOptions.m_DropHandler;

                var rectTransform = m_MovingItemOptions.CreateRectTransform(m_MovingItemOptions.DisplayPanel.MainContent);
                rectTransform.name = "Move Cursor Panel";
                var displayPanel = rectTransform.gameObject.AddComponent<DisplayPanel>();

                m_Target.m_MoveDisplayPanel = displayPanel;
                m_MovingItemOptions.Refresh();
            }

            protected override void Delete()
            {
                m_MovingItemOptions.RemoveComponent(m_Target);
                m_MovingItemOptions.Refresh();
            }
        }
    }
}