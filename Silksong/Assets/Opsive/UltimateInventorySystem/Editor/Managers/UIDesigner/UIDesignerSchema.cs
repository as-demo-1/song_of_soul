/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Demo.UI.Menus.Storage;
    using Opsive.UltimateInventorySystem.UI;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using Opsive.UltimateInventorySystem.UI.Currency;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Item.AttributeViewModules;
    using Opsive.UltimateInventorySystem.UI.Item.DragAndDrop;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using Opsive.UltimateInventorySystem.UI.Menus.Chest;
    using Opsive.UltimateInventorySystem.UI.Menus.Crafting;
    using Opsive.UltimateInventorySystem.UI.Menus.Shop;
    using Opsive.UltimateInventorySystem.UI.Monitors;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ActionPanels;
    using Opsive.UltimateInventorySystem.UI.Panels.Hotbar;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers.GridFilterSorters.InventoryGridFilters;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers.GridFilterSorters.InventoryGridSorters;
    using Opsive.UltimateInventorySystem.UI.Panels.Save;
    using System;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.Serialization;
    using Object = UnityEngine.Object;

    [Serializable]
    [CreateAssetMenu(fileName = "New Schema", menuName = "Ultimate Inventory System/UI/UI Designer Schema", order = 101)]
    public class UIDesignerSchema : ScriptableObject
    {
        protected (bool isValid, string message) m_ValidationResult = (false, "Not Yet Checked");
        public (bool isValid, string message) ValidationResult => m_ValidationResult;

        [Header("Schema Info")]
        [Tooltip("If set to false the prefabs will simply be instantiated, if set to true the prefab will keep the link as if they were dropped in the scene from the project view.")]
        [SerializeField] internal bool m_KeepPrefabLink = true;
        [SerializeField] internal string m_Description = "A New Schema used to create UI using the UI Designer Editor";
        [SerializeField] internal Texture[] m_Images = null;

        public bool KeepPrefabLink => m_KeepPrefabLink;


        [Header("Full Schema")]
        [UIDesignerSchemaFullLayoutValidation]
        [SerializeField] protected GameObject m_FullSchemaLayout;

        public GameObject FullSchemaLayout => m_FullSchemaLayout;

        [Header("Panels")]
        [SerializeField] protected DisplayPanel m_SimplePanel;
        [SerializeField] protected DisplayPanel m_FloatingPanel;
        [SerializeField] protected DisplayPanel m_MainMenuInnerPanel;

        public DisplayPanel MainMenuInnerPanel => m_MainMenuInnerPanel;
        public DisplayPanel SimplePanel => m_SimplePanel;
        public DisplayPanel FloatingPanel => m_FloatingPanel;

        [Header("Main Menu")]
        [SerializeField] protected MainMenu m_MainMenuVertical;
        [SerializeField] protected MainMenu m_MainMenuHorizontal;
        [SerializeField] protected ActionButton m_MainMenuTabButton;

        public MainMenu MainMenuVertical => m_MainMenuVertical;
        public MainMenu MainMenuHorizontal => m_MainMenuHorizontal;
        public ActionButton MainMenuTabButton => m_MainMenuTabButton;

        [Header("Inventory Grid")]
        [UIDesignerSchemaValidation(new[] { typeof(ItemViewDrawer), typeof(ItemInfoGrid) })]
        [SerializeField] protected InventoryGrid m_InventoryGrid;

        public InventoryGrid InventoryGrid => m_InventoryGrid;


        [Header("Item Shape Grid")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_ItemShapeInventoryGrid")]
        [UIDesignerSchemaValidation(new[] { typeof(ItemViewDrawer) })]
        [SerializeField] protected ItemShapeGrid m_ItemShapeGrid;
        [SerializeField] protected ItemViewSlot m_ItemShapeViewSlot;

        public ItemShapeGrid ItemShapeGrid => m_ItemShapeGrid;
        public ItemViewSlot ItemShapeViewSlot => m_ItemShapeViewSlot;

        [Header("Equipment")]
        [UIDesignerSchemaValidation(new[] { typeof(ItemViewDrawer) })]
        [SerializeField] protected ItemSlotCollectionView m_EquipmentItemViewSlotContainer;

        public ItemSlotCollectionView EquipmentItemViewSlotContainer => m_EquipmentItemViewSlotContainer;

        [Header("Item Hotbar")]
        [UIDesignerSchemaValidation(new[] { typeof(ItemHotbar) })]
        [SerializeField] protected DisplayPanel m_ItemHotbarPanel;
        [SerializeField] protected ItemView m_ItemViewForHotbar;
        [SerializeField] protected CategoryItemViewSet m_ItemHotbarViewSet;
        [SerializeField] protected CategoryItemActionSet m_ItemHotbarActionSet;

        public DisplayPanel ItemHotbarPanel => m_ItemHotbarPanel;
        public ItemView ItemViewForHotbar => m_ItemViewForHotbar;
        public CategoryItemViewSet ItemHotbarViewSet => m_ItemHotbarViewSet;
        public CategoryItemActionSet ItemHotbarActionSet => m_ItemHotbarActionSet;

        [Header("Shop")]
        [UIDesignerSchemaValidation(new[] { typeof(DisplayPanel) })]
        [SerializeField] protected ShopMenu m_ShopMenu;
        [SerializeField] protected QuantityPickerPanel m_QuantityPickerPanel;

        public ShopMenu ShopMenu => m_ShopMenu;
        public QuantityPickerPanel QuantityPickerPanel => m_QuantityPickerPanel;

        [Header("Crafting")]
        [UIDesignerSchemaValidation(new[] { typeof(DisplayPanel) })]
        [SerializeField] protected CraftingMenu m_CraftingMenu;
        [SerializeField] protected RecipePanel m_RecipePanel;

        public CraftingMenu CraftingMenu => m_CraftingMenu;
        public RecipePanel RecipePanel => m_RecipePanel;

        [Header("Save")]
        [UIDesignerSchemaValidation(new[] { typeof(DisplayPanel) })]
        [SerializeField] protected SaveMenu m_SaveMenu;
        [SerializeField] protected SaveView m_SaveView;

        public SaveMenu SaveMenu => m_SaveMenu;
        public SaveView SaveView => m_SaveView;

        [Header("Storage")]
        [UIDesignerSchemaValidation(new[] { typeof(DisplayPanel) })]
        [SerializeField] protected StorageMenu m_StorageMenu;

        public StorageMenu StorageMenu => m_StorageMenu;

        [Header("Chest")]
        [UIDesignerSchemaValidation(new[] { typeof(DisplayPanel) })]
        [SerializeField] protected ChestMenu m_ChestMenu;

        public ChestMenu ChestMenu => m_ChestMenu;

        [Header("Item Description")]
        [SerializeField] protected DisplayPanel m_ItemDescriptionPanel;
        [SerializeField] protected ItemDescriptionBase m_ItemDescription;
        [SerializeField] protected ItemDescriptionBase m_ItemDescriptionSmall;
        [SerializeField] protected ItemDescriptionBase m_ItemDescriptionBig;

        public DisplayPanel ItemDescriptionPanel => m_ItemDescriptionPanel;
        public ItemDescriptionBase ItemDescription => m_ItemDescription;
        public ItemDescriptionBase ItemDescriptionSmall => m_ItemDescriptionSmall;
        public ItemDescriptionBase ItemDescriptionBig => m_ItemDescriptionBig;

        [Header("Currency Views")]
        [SerializeField] protected MultiCurrencyView m_MultiCurrencyView;
        [SerializeField] protected MultiCurrencyView m_MultiCurrencyViewSmall;
        [UIDesignerSchemaValidation(new[] { typeof(MultiCurrencyView) })]
        [SerializeField] protected CurrencyOwnerMonitor m_CurrencyOwnerMonitor;
        [SerializeField] protected CurrencyView m_CurrencyView;

        public MultiCurrencyView MultiCurrencyView => m_MultiCurrencyView;
        public MultiCurrencyView MultiCurrencyViewSmall => m_MultiCurrencyViewSmall;
        public CurrencyOwnerMonitor CurrencyOwnerMonitor => m_CurrencyOwnerMonitor;
        public CurrencyView CurrencyView => m_CurrencyView;

        [Header("Inventory Monitor")]
        [SerializeField] protected InventoryMonitor m_InventoryMonitor;

        public InventoryMonitor InventoryMonitor => m_InventoryMonitor;

        [Header("Item Views, Slots and Sets")]
        [SerializeField] protected ItemViewSlot m_ItemViewSlot;
        [SerializeField] protected ItemView m_ItemViewForGrid;
        [SerializeField] protected ItemView m_ItemViewForList;
        [SerializeField] protected ItemView m_ItemViewForShop;
        [SerializeField] protected ItemView m_ItemViewForItemShape;
        [SerializeField] protected ItemView m_ItemViewForIngredient;
        [SerializeField] protected ItemView m_ItemViewForInventoryMonitor;
        [SerializeField] protected CategoryItemViewSet m_GridCategoryItemViewSet;
        [SerializeField] protected CategoryItemViewSet m_ListCategoryItemViewSet;
        [SerializeField] protected CategoryItemViewSet m_ShopCategoryItemViewSet;
        [SerializeField] protected CategoryItemViewSet m_ItemShapeCategoryItemViewSet;

        public ItemViewSlot ItemViewSlot => m_ItemViewSlot;
        public ItemView ItemViewForGrid => m_ItemViewForGrid;
        public ItemView ItemViewForList => m_ItemViewForList;
        public ItemView ItemViewForShop => m_ItemViewForShop;
        public ItemView ItemViewForItemShape => m_ItemViewForItemShape;
        public ItemView ItemViewForIngredient => m_ItemViewForIngredient;
        public ItemView ItemViewForInventoryMonitor => m_ItemViewForInventoryMonitor;
        public CategoryItemViewSet GridCategoryItemViewSet => m_GridCategoryItemViewSet;
        public CategoryItemViewSet ListCategoryItemViewSet => m_ListCategoryItemViewSet;
        public CategoryItemViewSet ShopCategoryItemViewSet => m_ShopCategoryItemViewSet;
        public CategoryItemViewSet ItemShapeCategoryItemViewSet => m_ItemShapeCategoryItemViewSet;

        [Header("Attribute Views and Sets")]
        [SerializeField] protected AttributeView m_AttributeView;
        [SerializeField] protected CategoryAttributeViewSet m_CategoryAttributeViewSet;

        public AttributeView AttributeView => m_AttributeView;
        public CategoryAttributeViewSet CategoryAttributeViewSet => m_CategoryAttributeViewSet;

        [Header("Item Actions")]
        [SerializeField] protected ItemActionPanel m_ItemActionPanel;
        [SerializeField] protected ActionButton m_ItemActionButton;
        [SerializeField] protected CategoryItemActionSet m_CategoryItemActionSet;

        public ItemActionPanel ItemActionPanel => m_ItemActionPanel;
        public ActionButton ItemActionButton => m_ItemActionButton;
        public CategoryItemActionSet CategoryItemActionSet => m_CategoryItemActionSet;

        [Header("Tab")]
        [SerializeField] protected TabToggle m_TabToggle;
        [SerializeField] protected UnityEngine.UI.Button m_ButtonRight;

        public TabToggle TabToggle => m_TabToggle;
        public UnityEngine.UI.Button ButtonRight => m_ButtonRight;

        [Header("Filters & Sorters")]
        [SerializeField] protected InventoryGridSorterDropDown m_GridSortDropDown;
        [SerializeField] protected InventorySearchFilter m_GridSearchBar;

        public InventoryGridSorterDropDown GridSortDropDown => m_GridSortDropDown;
        public InventorySearchFilter GridSearchBar => m_GridSearchBar;

        [Header("Scrollbar")]
        [SerializeField] protected ScrollbarWithButtons m_ScrollbarVertical;

        public ScrollbarWithButtons ScrollbarVertical => m_ScrollbarVertical;

        [Header("Drag and Drop")]
        [SerializeField] protected ItemViewSlotDropActionSet m_ItemViewSlotDropActionSet;

        public ItemViewSlotDropActionSet ItemViewSlotDropActionSet => m_ItemViewSlotDropActionSet;


        public DisplayPanel GetPanelPrefab(InventoryGridDisplayPanelOption panelOption)
        {
            switch (panelOption) {
                case InventoryGridDisplayPanelOption.Floating:
                    return m_FloatingPanel;
                case InventoryGridDisplayPanelOption.Simple:
                    return m_SimplePanel;
                case InventoryGridDisplayPanelOption.MainMenu:
                    return m_MainMenuInnerPanel;
                case InventoryGridDisplayPanelOption.Basic:
                    var rect = UIDesignerUtility.CreateRectTransform(null);
                    var panel = rect.gameObject.AddComponent<DisplayPanel>();
                    var mainContent = UIDesignerUtility.CreateRectTransform(rect);
                    mainContent.gameObject.name = "Main Content";
                    panel.m_MainContent = mainContent;
                    return panel;
            }

            return null;
        }

        public (bool isValid, string message) CheckIfValid()
        {
            var result = CheckIfValidInternal();
            //Cache the result.
            m_ValidationResult = result;
            return result;
        }


        private (bool isValid, string message) CheckIfValidInternal()
        {
            var valid = true;
            var message = "";

            //Use Reflection to validate all fields.

            var schemaType = typeof(UIDesignerSchema);

            var fields = schemaType.GetFields(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int i = 0; i < fields.Length; i++) {
                var field = fields[i];
                var fieldName = field.Name;

                if (field.FieldType.IsSubclassOf(typeof(Object)) == false) {
                    continue;
                }

                var fieldValue = field.GetValue(this) as Object;
                if (ReferenceEquals(fieldValue, null)) {
                    message += $"The field '{fieldName}' is null.\n";
                    valid = false;
                    continue;
                }

                if (TypeUtility.GetAttribute(field, typeof(UIDesignerSchemaValidationAttribute)) != null) {
                    var gameobject = fieldValue as GameObject;
                    if (gameobject == null) {
                        var component = fieldValue as Component;
                        if (component != null) {
                            gameobject = component.gameObject;
                        } else {
                            gameobject = null;
                        }
                    }

                    var validationAttribute = field.GetCustomAttribute<UIDesignerSchemaValidationAttribute>();
                    var result = validationAttribute.ValidateField(fieldName, gameobject);

                    message += result.message;
                    if (result.isValid == false) {
                        valid = false;
                    }
                }
            }

            return (valid, message);
        }
    }

    /// <summary>
    ///   <para>Use this PropertyAttribute to check if the schema field is valid.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class UIDesignerSchemaValidationAttribute : PropertyAttribute
    {
        private Type[] m_RequiredTypes;

        /// <summary>
        ///   <para>Check that the field is valid.</para>
        /// </summary>
        public UIDesignerSchemaValidationAttribute()
        {
            m_RequiredTypes = null;
        }

        /// <summary>
        ///   <para>Check that the field is valid.</para>
        /// </summary>
        /// <param name="requiredComponentTypes">The header text.</param>
        public UIDesignerSchemaValidationAttribute(Type[] requiredComponentTypes)
        {
            m_RequiredTypes = requiredComponentTypes;
        }

        public virtual (bool isValid, string message) ValidateField(string fieldName, GameObject gameObject)
        {
            var valid = true;
            var message = "";

            if (gameObject == null) {
                message += $"The field '{fieldName}' is null.\n";
                valid = false;
                return (valid, message);
            }

            if (m_RequiredTypes != null) {
                for (int i = 0; i < m_RequiredTypes.Length; i++) {
                    var requiredType = m_RequiredTypes[i];
                    var component = gameObject.GetComponentsInChildren(requiredType);
                    if (component == null) {
                        message += $"The field '{fieldName}' is missing a required component '{requiredType.Name}'.\n";
                        valid = false;
                    }
                }
            }

            return (valid, message);
        }
    }

    /// <summary>
    ///   <para>Use this PropertyAttribute to check if the schema field is valid.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class UIDesignerSchemaFullLayoutValidationAttribute : UIDesignerSchemaValidationAttribute
    {

        public override (bool isValid, string message) ValidateField(string fieldName, GameObject gameObject)
        {
            var result = base.ValidateField(fieldName, gameObject);

            var valid = result.isValid;
            var message = result.message;

            var foundGameplayPanel = false;
            var foundMainMenu = false;

            for (int i = 0; i < gameObject.transform.childCount; i++) {
                var child = gameObject.transform.GetChild(i);

                if (child.name == "Gameplay Panel") {
                    foundGameplayPanel = child.GetComponent<DisplayPanel>() != null;
                }

                if (child.name == "Main Menu") {
                    foundMainMenu = child.GetComponent<DisplayPanel>() != null;
                }
            }

            if (foundGameplayPanel == false) {
                message += $"The '{fieldName}' is missing a child panel called 'Gameplay Panel'";
                valid = false;
            }

            if (foundMainMenu == false) {
                message += $"The '{fieldName}' is missing a child panel called 'Main Menu'";
                valid = false;
            }

            return (valid, message);
        }


    }
}