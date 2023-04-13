/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    using Opsive.UltimateInventorySystem.Core.Registers;
    using Opsive.UltimateInventorySystem.UI.Item;

    /// <summary>
    /// Register for the ItemV iew Slot Cursor Manager.
    /// </summary>
    public class ItemViewSlotCursorManagerRegister : InventoryObjectIDOnlyRegister<ItemViewSlotCursorManager>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="register">The register.</param>
        public ItemViewSlotCursorManagerRegister(InventorySystemRegister register) : base(register)
        { }
    }
}