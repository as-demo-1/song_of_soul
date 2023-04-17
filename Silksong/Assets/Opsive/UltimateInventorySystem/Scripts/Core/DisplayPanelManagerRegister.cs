/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    using Opsive.UltimateInventorySystem.Core.Registers;
    using Opsive.UltimateInventorySystem.UI.Panels;

    /// <summary>
    /// Register for the Display Panel Managers.
    /// </summary>
    public class DisplayPanelManagerRegister : InventoryObjectIDOnlyRegister<DisplayPanelManager>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="register">The register.</param>
        public DisplayPanelManagerRegister(InventorySystemRegister register) : base(register)
        { }
    }
}