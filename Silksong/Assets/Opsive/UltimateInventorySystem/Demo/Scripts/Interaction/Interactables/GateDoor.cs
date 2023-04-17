/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.Interaction.Interactables
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Demo.UI;
    using Opsive.UltimateInventorySystem.Interactions;
    using Opsive.UltimateInventorySystem.Storage;
    using UnityEngine;

    /// <summary>
    /// The gate door can only be opened with a key.
    /// </summary>
    public class GateDoor : InteractableBehavior, IDatabaseSwitcher
    {
        [Tooltip("The gate key item definition.")]
        [SerializeField] protected DynamicItemDefinition m_GateKey;
        [Tooltip("The gate animator.")]
        [SerializeField] protected Animator m_Animator;
        [Tooltip("The text panel where the text below will be displayed.")]
        [SerializeField] protected TextPanel m_TextPanel;
        [Tooltip("The text that will be displayed when the interactor does not have the key.")]
        [SerializeField] protected string m_TextIfNoKey;
        [Tooltip("The text that will be displayed when the interactor does have the key.")]
        [SerializeField] protected string m_TextHasKey;
        [Tooltip("The amount of second the text will hte displayed for.")]
        [SerializeField] protected float m_TextDisplayTime;

        private static readonly int s_Open = Animator.StringToHash("Open");
        protected bool m_DoorOpened;

        /// <summary>
        /// Open the gate on interaction if the interactor has the key.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        protected override void OnInteractInternal(IInteractor interactor)
        {
            if (m_DoorOpened) { return; }

            if (!(interactor is IInteractorWithInventory interactorWithInventory)) {
                return;
            }

            if (interactorWithInventory.Inventory.MainItemCollection.HasItem((1, m_GateKey), false)) {
                //Open the Gate with the key.
                m_Animator.SetTrigger(s_Open);

                m_DoorOpened = true;
                m_Interactable.SetIsInteractable(false);

                if (m_TextPanel != null) {
                    m_TextPanel.DisplayText(m_TextHasKey, m_TextDisplayTime);
                }

            } else {
                if (m_TextPanel != null) {
                    m_TextPanel.DisplayText(m_TextIfNoKey, m_TextDisplayTime);
                }
            }
        }

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            return database.Contains(m_GateKey);
        }

        /// <summary>
        /// Replace any object that is not in the database by an equivalent object in the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>The objects that have been changed.</returns>
        ModifiedObjectWithDatabaseObjects IDatabaseSwitcher.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(InventorySystemDatabase database)
        {
            if (database == null) { return null; }

            m_GateKey = database.FindSimilar(m_GateKey);

            return null;
        }
    }
}
