/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Menus.Chest
{
    using Opsive.UltimateInventorySystem.Interactions;
    using UnityEngine;

    /// <summary>
    /// A chest component which allows to open a chest menu on interaction.
    /// </summary>
    public class ChestInteractable : InteractableBehavior
    {
        [Tooltip("The chest game object in case you are using the chest interface instead of the class.")]
        [SerializeField] protected GameObject m_ChestGameObject;
        [Tooltip("The chest.")]
        [SerializeField] protected Chest m_Chest;

        protected IChest m_RealChest;
        
        /// <summary>
        /// Initialize.
        /// </summary>
        protected void Awake()
        {
            if (m_Chest != null) {
                m_RealChest = m_Chest;
            } else {
                m_RealChest = m_ChestGameObject?.GetComponent<IChest>();
                if (m_RealChest == null) {
                    Debug.LogError("A chest component or game object is missing from the chest interactable.",gameObject);
                    return;
                }
            }

            m_RealChest.OnClose += () =>
            {
                m_Interactable.SetIsInteractable(true);
            };
            m_RealChest.OnOpen += (clientInventory) =>
            {
                m_Interactable.SetIsInteractable(false);
            };
        }

        /// <summary>
        /// Make the chest no longer interactable.
        /// </summary>
        public override void Deactivate()
        {
            m_Interactable.SetIsInteractable(false);
        }

        /// <summary>
        /// Use the interactor to reference its inventory in the chest menu.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        protected override void OnInteractInternal(IInteractor interactor)
        {
            if (!(interactor is IInteractorWithInventory interactorWithInventory)) { return; }

            m_RealChest.Open(interactorWithInventory.Inventory);
        }
    }
}