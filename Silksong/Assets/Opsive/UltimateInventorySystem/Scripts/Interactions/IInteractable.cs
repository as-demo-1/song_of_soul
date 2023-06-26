/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Interactions
{
    using UnityEngine;

    /// <summary>
    /// Interface used to be interacted with by an interactor.
    /// </summary>
    public interface IInteractable
    {
        GameObject gameObject { get; }
        
        /// <summary>
        /// Is the interactable interactable.
        /// </summary>
        bool CanInteract(IInteractor interactor);

        /// <summary>
        /// Interact with the interactable.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        /// <returns>False if not interactable.</returns>
        bool Interact(IInteractor interactor);

        /// <summary>
        /// Select the interactable.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        /// <returns>False if not interactable.</returns>
        bool Select(IInteractor interactor);

        /// <summary>
        /// Deselects the interactable.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        /// <returns>True if the interactable was deselected.</returns>
        bool Deselect(IInteractor interactor);
    }
}
