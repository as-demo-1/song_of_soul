/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Interactions
{
    /// <summary>
    /// The interactor interface.
    /// </summary>
    public interface IInteractor
    {
        /// <summary>
        /// Adds an interactable.
        /// </summary>
        /// <param name="interactable">The interactable that should be added.</param>
        void AddInteractable(IInteractable interactable);

        /// <summary>
        /// Remove an interactable.
        /// </summary>
        /// <param name="interactable">The interactable that should be removed.</param>
        void RemoveInteractable(IInteractable interactable);
    }
}
