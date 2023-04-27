/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    using Opsive.UltimateInventorySystem.Core.Registers;

    /// <summary>
    /// Inventory system register compiles all the registers for the Inventory system manager.
    /// </summary>
    public class InventorySystemRegister
    {
        protected IInventorySystemManager m_Manager;

        protected GlobalRegister m_GlobalRegister;

        protected ItemCategoryRegister m_ItemCategoryRegister;
        protected ItemDefinitionRegister m_ItemDefinitionRegister;
        protected ItemRegister m_ItemRegister;

        protected CurrencyRegister m_CurrencyRegister;

        protected CraftingCategoryRegister m_CraftingCategoryRegister;
        protected CraftingRecipeRegister m_CraftingRecipeRegister;

        protected InventoryIdentifierRegister m_InventoryIdentifierRegister;
        protected DisplayPanelManagerRegister m_DisplayPanelManagerRegister;
        protected ItemViewSlotCursorManagerRegister m_ItemViewSlotCursorManagerRegister;

        public IInventorySystemManager Manager => m_Manager;

        public GlobalRegister GlobalRegister => m_GlobalRegister;

        public ItemCategoryRegister ItemCategoryRegister => m_ItemCategoryRegister;
        public ItemDefinitionRegister ItemDefinitionRegister => m_ItemDefinitionRegister;
        public ItemRegister ItemRegister => m_ItemRegister;

        public CurrencyRegister CurrencyRegister => m_CurrencyRegister;

        public CraftingCategoryRegister CraftingCategoryRegister => m_CraftingCategoryRegister;
        public CraftingRecipeRegister CraftingRecipeRegister => m_CraftingRecipeRegister;
        public InventoryIdentifierRegister InventoryIdentifierRegister => m_InventoryIdentifierRegister;
        public DisplayPanelManagerRegister DisplayPanelManagerRegister => m_DisplayPanelManagerRegister;

        public ItemViewSlotCursorManagerRegister ItemViewSlotCursorManagerRegister =>
            m_ItemViewSlotCursorManagerRegister;

        /// <summary>
        /// Create the Inventory System Register.
        /// </summary>
        /// <param name="manager">The Inventory system manager.</param>
        public InventorySystemRegister(IInventorySystemManager manager)
        {
            m_Manager = manager;

            m_GlobalRegister = new GlobalRegister();
            m_ItemCategoryRegister = new ItemCategoryRegister(this);
            m_ItemDefinitionRegister = new ItemDefinitionRegister(this);
            m_ItemRegister = new ItemRegister(this);
            m_CurrencyRegister = new CurrencyRegister(this);
            m_CraftingCategoryRegister = new CraftingCategoryRegister(this);
            m_CraftingRecipeRegister = new CraftingRecipeRegister(this);
            m_InventoryIdentifierRegister = new InventoryIdentifierRegister(this);
            m_DisplayPanelManagerRegister = new DisplayPanelManagerRegister(this);
            m_ItemViewSlotCursorManagerRegister = new ItemViewSlotCursorManagerRegister(this);
        }
    }
}