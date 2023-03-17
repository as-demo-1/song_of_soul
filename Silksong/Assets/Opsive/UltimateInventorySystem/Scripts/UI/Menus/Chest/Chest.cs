/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Menus.Chest
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.Utility;
    using System;
    using UnityEngine;

    /// <summary>
    /// Chest interface used to open and close a chest.
    /// </summary>
    public interface IChest
    {
        event Action OnClose;
        event Action<Inventory> OnOpen;
        
        Inventory Inventory { get; }
        bool IsOpen { get; }

        /// <summary>
        /// Open the menu and start the animation.
        /// </summary>
        /// <param name="clientInventory">The inventory of the client opening the chest.</param>
        void Open(Inventory clientInventory);

        /// <summary>
        /// Close animation is triggered on close.
        /// </summary>
        void Close();
    }

    /// <summary>
    /// A chest component which allows to open a chest menu on interaction.
    /// </summary>
    public class Chest : MonoBehaviour, IChest
    {
        public event Action OnClose;
        public event Action<Inventory> OnOpen;

        [Tooltip("The panel manager.")]
        [SerializeField] protected DisplayPanelManager m_PanelManager;
        [Tooltip("The chest menu.")]
        [SerializeField] protected ChestMenu m_ChestMenu;
        [Tooltip("The chest inventory.")]
        [SerializeField] protected Inventory m_Inventory;
        [Tooltip("The chest animator component.")]
        [SerializeField] protected Animator m_Animator;

        private static readonly int s_CloseAnim = Animator.StringToHash("Close");
        private static readonly int s_OpenAnim = Animator.StringToHash("Open");

        protected bool m_IsOpen;

        public bool IsOpen => m_IsOpen;

        public Inventory Inventory => m_Inventory;

        public ChestMenu ChestMenu {
            get => m_ChestMenu;
            set => m_ChestMenu = value;
        }

        /// <summary>
        /// Validate by getting the chest menu.
        /// </summary>
        protected virtual void OnValidate()
        {
            if (OnValidateUtility.IsPrefab(this)) { return; }

            if (m_ChestMenu == null) {
                m_ChestMenu = FindObjectOfType<ChestMenu>();
            }

            if (m_PanelManager == null) {
                m_PanelManager = FindObjectOfType<DisplayPanelManager>();
            }

            if (m_Inventory == null) {
                m_Inventory = GetComponent<Inventory>();
            }

            if (m_Animator == null) {
                m_Animator = GetComponent<Animator>();
            }
        }

        /// <summary>
        /// Open the menu and start the animation.
        /// </summary>
        /// <param name="clientInventory">The inventory of the client opening the chest.</param>
        public virtual void Open(Inventory clientInventory)
        {
            m_Animator.SetTrigger(s_OpenAnim);

            if (m_ChestMenu == null) {
                Debug.LogWarning("Chest Menu is null.");
                return;
            }

            m_IsOpen = true;

            m_ChestMenu.BindInventory(clientInventory);
            m_ChestMenu.SetChest(this);

            m_ChestMenu.DisplayPanel.SmartOpen();

            OnOpen?.Invoke(clientInventory);
        }

        /// <summary>
        /// Close animation is triggered on close.
        /// </summary>
        public virtual void Close()
        {
            m_IsOpen = false;
            m_Animator.SetTrigger(s_CloseAnim);
            OnClose?.Invoke();
        }
    }
}