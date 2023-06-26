/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Core.Registers;

    /// <summary>
    /// Register for the Inventory Identifier.
    /// </summary>
    public class InventoryIdentifierRegister : InventoryObjectIDOnlyRegister<InventoryIdentifier>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="register">The register.</param>
        public InventoryIdentifierRegister(InventorySystemRegister register) : base(register)
        { }
    }
}