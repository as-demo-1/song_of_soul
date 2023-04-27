/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using System;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class ItemDescriptionDesigner : UIDesignerCreateEditTabContent<
        ItemDescriptionBase,
        ItemDescriptionDesignerCreator,
        ItemDescriptionDesignerEditor>
    {
        public override string Title => "Item Description";

        public override string Description =>
            "Display a description of the item. The Item Description is an Item View with some extra features. The Item View Modules can be used to show more details about the item.\n" +
            "'Category Attribute View Set Item View Module' is automatically added when creating a description but it is not required.\n" +
            "The panel can be converted into a Tooltip for Item View Slots Containers.";

    }

    public class ItemDescriptionDesignerCreator : UIDesignerCreator<ItemDescriptionBase>
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/item-description/";

        private enum ItemDescriptionOption
        {
            Standard,
            Small,
            Big,
        }

        protected EnumField m_Preset;
        protected Toggle m_UsePanel;

        protected override void CreateOptionsContent(VisualElement container)
        {
            m_UsePanel = new Toggle("Give Description a Panel");
            m_UsePanel.value = true;
            container.Add(m_UsePanel);

            m_Preset = new EnumField("Item Description Preset", ItemDescriptionOption.Standard);
            container.Add(m_Preset);
        }

        protected override ItemDescriptionBase BuildInternal()
        {
            ItemDescriptionBase prefab = null;
            switch ((ItemDescriptionOption)m_Preset.value) {
                case ItemDescriptionOption.Standard:
                    prefab = UIDesignerSchema.ItemDescription;
                    break;
                case ItemDescriptionOption.Small:
                    prefab = UIDesignerSchema.ItemDescriptionSmall;
                    break;
                case ItemDescriptionOption.Big:
                    prefab = UIDesignerSchema.ItemDescriptionBig;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            var rectParent = m_ParentTransform.value as RectTransform;
            ItemDescriptionPanelBinding binding = null;

            if (m_UsePanel.value) {
                binding = CreatePanelWithDescriptionBinding(rectParent, (prefab.transform as RectTransform).sizeDelta);
                rectParent = binding.GetComponent<DisplayPanel>().MainContent;
            }

            var description = UIDesignerManager.InstantiateSchemaPrefab<ItemDescriptionBase>(prefab, rectParent);

            if (binding != null) {
                binding.ItemDescription = description;
                SetRectToFullSize(description.RectTransform);
            }

            var categoryAttributeViewSetItemViewModule = description.GetComponent<CategoryAttributeViewSetItemViewModule>();

            if (categoryAttributeViewSetItemViewModule != null) {
                categoryAttributeViewSetItemViewModule.m_CategoryAttributeViewSet =
                    UIDesignerSchema.CategoryAttributeViewSet;
            }

            return description;
        }

        protected ItemDescriptionPanelBinding CreatePanelWithDescriptionBinding(RectTransform panelParent, Vector2 size)
        {
            var rectTransform = CreateRectTransform(panelParent);
            rectTransform.sizeDelta = size;
            rectTransform.gameObject.name = "Item Description Panel";
            var displayPanel = rectTransform.gameObject.AddComponent<DisplayPanel>();

            var panelContent = CreateRectTransform(rectTransform);
            SetRectToFullSize(panelContent);
            panelContent.gameObject.name = "Main Content";
            displayPanel.m_MainContent = panelContent;

            displayPanel.m_StartEnabled = true;
            displayPanel.m_OpenOnStart = true;
            displayPanel.m_IsNonSelectablePanel = true;

            var binding = rectTransform.gameObject.AddComponent<ItemDescriptionPanelBinding>();
            return binding;
        }

        private static void SetRectToFullSize(RectTransform panelContent)
        {
            panelContent.anchorMin = Vector2.zero;
            panelContent.anchorMax = Vector2.one;
            panelContent.pivot = new Vector2(0.5f, 0.5f);
            panelContent.sizeDelta = Vector2.zero;
        }
    }

    public class ItemDescriptionDesignerEditor : UIDesignerEditor<ItemDescriptionBase>
    {
        protected override bool RequireDisplayPanel => false;

        protected DisplayPanelOptions m_DisplayPanelOptions;

        protected ItemViewSlotsContainerItemDescriptionOption m_ItemViewSlotsContainerItemDescriptionOption;

        protected ViewModulesReorderableList<ItemView, ItemViewModule> m_ItemViewModulesReorderableList;

        public ItemViewSlotsContainerItemDescriptionOption
            ItemViewSlotsContainerItemDescriptionOption => m_ItemViewSlotsContainerItemDescriptionOption;

        public ItemDescriptionDesignerEditor()
        {

            m_ItemViewModulesReorderableList = new ViewModulesReorderableList<ItemView, ItemViewModule>();

            m_DisplayPanelOptions = new DisplayPanelOptions();

            m_ItemViewSlotsContainerItemDescriptionOption = new ItemViewSlotsContainerItemDescriptionOption(this);
        }

        protected override void NewValidTargetAssigned()
        {
            base.NewValidTargetAssigned();

            m_ItemViewModulesReorderableList.Refresh(m_Target);
            m_TargetOptionsContainer.Add(m_ItemViewModulesReorderableList);

            m_TargetOptionsContainer.Add(m_DisplayPanelOptions);
            m_DisplayPanelOptions.Refresh(m_DisplayPanel);

            m_ItemViewSlotsContainerItemDescriptionOption.Refresh();
            m_TargetOptionsContainer.Add(m_ItemViewSlotsContainerItemDescriptionOption);
        }
    }

    public abstract class ItemDescriptionOption : UIDesignerBoxBase
    {
        protected ItemDescriptionDesignerEditor m_ItemDescriptionDesignerEditor;

        protected DisplayPanel DisplayPanel => m_ItemDescriptionDesignerEditor.DisplayPanel;

        protected VisualElement m_Container;

        protected ItemDescriptionOption(ItemDescriptionDesignerEditor itemDescriptionDesignerEditor)
        {
            m_ItemDescriptionDesignerEditor = itemDescriptionDesignerEditor;
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

    public class ItemViewSlotsContainerItemDescriptionOption : ItemDescriptionOption
    {
        public override string Title => "Item View Slots Container Binding";

        public override string Description =>
            "Bind the item description to an Item View Slots Container to display the description of the selected Item.\n" +
            "You may also use the description panel as a tooltip.";

        protected ObjectField m_ItemViewSlotsContainerField;
        protected ItemViewSlotsContainerBase m_ItemViewSlotsContainer;

        protected VisualElement m_ItemViewSlotsVisualContainer;

        protected InventoryHelpBox m_HelpBox;

        protected CreateSelectDeleteContainer m_BindSelectUnbindContainer;
        protected CreateSelectDeleteContainer m_ConvertToTooltipButton;

        public ItemViewSlotsContainerItemDescriptionOption(ItemDescriptionDesignerEditor itemDescriptionDesignerEditor) : base(itemDescriptionDesignerEditor)
        {
            m_ItemViewSlotsContainerField = new ObjectField("Item View Slots Container");
            m_ItemViewSlotsContainerField.objectType = typeof(ItemViewSlotsContainerBase);
            m_ItemViewSlotsContainerField.RegisterValueChangedCallback(evt =>
            {
                Refresh();
            });
            Add(m_ItemViewSlotsContainerField);

            m_ItemViewSlotsVisualContainer = new VisualElement();
            Add(m_ItemViewSlotsVisualContainer);

            m_HelpBox = new InventoryHelpBox("OK");

            m_BindSelectUnbindContainer = new CreateSelectDeleteContainer(
                "Item View Slot Container", BindUnBind, BindUnBind,
                () => m_ItemViewSlotsContainer.GetComponent<ItemViewSlotContainerDescriptionBinding>() as Component);
            m_BindSelectUnbindContainer.CreatePrefix = "Bind";
            m_BindSelectUnbindContainer.DeletePrefix = "Unbind";
            m_BindSelectUnbindContainer.SelectPrefix = "Select";

            m_ConvertToTooltipButton = new CreateSelectDeleteContainer(
                "Tooltip", ConvertToTooltip, ConvertToTooltip,
                () => DisplayPanel.GetComponent<ItemViewSlotPanelToTooltip>());
            m_ConvertToTooltipButton.CreatePrefix = "Convert To";
            m_ConvertToTooltipButton.DeletePrefix = "Revert From";
            m_ConvertToTooltipButton.SelectPrefix = "Select";
        }

        protected override void Refresh(VisualElement container)
        {
            m_ItemViewSlotsContainer = m_ItemViewSlotsContainerField.value as ItemViewSlotsContainerBase;
            if (m_ItemViewSlotsContainer == null) {
                m_ItemViewSlotsVisualContainer.Clear();
            } else { HandleValidItemViewSlotsContainer(); }
        }

        public void SetItemViewSlotContainer(ItemViewSlotsContainerBase slotContainer)
        {
            m_ItemViewSlotsContainerField.value = slotContainer;
        }

        private void HandleValidItemViewSlotsContainer()
        {
            m_ItemViewSlotsVisualContainer.Clear();

            m_BindSelectUnbindContainer.Refresh();

            m_ItemViewSlotsVisualContainer.Add(m_BindSelectUnbindContainer);

            if (DisplayPanel == null || DisplayPanel.GetComponent<ItemDescriptionPanelBinding>() == null) {
                m_HelpBox.SetMessage("The Item Description must have a bound Display Panel to be used as a tooltip for an Item View Slots Container.");
                m_ItemViewSlotsVisualContainer.Add(m_HelpBox);
                return;
            }

            m_ConvertToTooltipButton.Refresh();
            m_ItemViewSlotsVisualContainer.Add(m_ConvertToTooltipButton);
        }

        private void BindUnBind()
        {
            var binding = m_ItemViewSlotsContainer.GetComponent<ItemViewSlotContainerDescriptionBinding>();
            if (binding != null) {
                RemoveComponent(binding);
                Refresh();
                return;
            }

            binding = m_ItemViewSlotsContainer.gameObject.AddComponent<ItemViewSlotContainerDescriptionBinding>();
            binding.m_ItemDescription = m_ItemDescriptionDesignerEditor.Target;

            Refresh();
        }

        private void ConvertToTooltip()
        {
            var panelToTooltip = DisplayPanel.GetComponent<ItemViewSlotPanelToTooltip>();
            if (panelToTooltip != null) {
                RemoveComponent(panelToTooltip);
                Refresh();
                return;
            }

            // Add tooltip component

            panelToTooltip = DisplayPanel.gameObject.AddComponent<ItemViewSlotPanelToTooltip>();
            panelToTooltip.m_ItemViewSlotContainer = m_ItemViewSlotsContainer;

            panelToTooltip.m_PlaceOnClick = false;
            panelToTooltip.m_ShowOnClick = false;

            panelToTooltip.m_PlaceOnSelect = true;
            panelToTooltip.m_ShowOnSelect = true;

            panelToTooltip.m_HideShowOnDeselect = true;

            // Canvas group interactable false
            var canvasGroup = m_ItemDescriptionDesignerEditor.Target.GetComponent<CanvasGroup>();
            if (canvasGroup == null) {
                canvasGroup = m_ItemDescriptionDesignerEditor.Target.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Resize panel
            var displayRect = DisplayPanel.transform as RectTransform;
            var descriptionRect = m_ItemDescriptionDesignerEditor.Target.transform as RectTransform;

            displayRect.sizeDelta = descriptionRect.sizeDelta;

            Refresh();
        }
    }
}