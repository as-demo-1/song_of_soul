/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Crafting
{
    /// <summary>
    /// Result from a craft process.
    /// </summary>
    public struct CraftingResult
    {
        private bool m_Success;
        private CraftingOutput m_CraftingOutput;

        public bool Success => m_Success;
        public CraftingOutput CraftingOutput => m_CraftingOutput;

        /// <summary>
        /// Constructor that sets the crafting result.
        /// </summary>
        /// <param name="craftingOutput">The crafting output.</param>
        /// <param name="success">True if the process succeed.</param>
        public CraftingResult(CraftingOutput craftingOutput, bool success)
        {
            m_CraftingOutput = craftingOutput ?? new CraftingOutput();
            m_Success = success;
        }
    }
}