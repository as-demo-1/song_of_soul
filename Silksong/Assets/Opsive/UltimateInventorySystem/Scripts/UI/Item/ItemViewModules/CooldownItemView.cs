/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.UI.Views;
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using EventHandler = Opsive.Shared.Events.EventHandler;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// Cooldown Item View is used with an item Action that sends an event of Used Item Action With Cooldown.
    /// </summary>
    public class CooldownItemView : ItemViewModule
    {
        // Start is called before the first frame update
        [Tooltip("The Item User Inventory ID to get the ItemUser.")]
        [SerializeField] protected uint m_ItemUserInventoryID=1;
        [Tooltip("The GameObject that shows the timer overlay.")]
        [SerializeField] protected GameObject m_OverlayGameObject;
        [Tooltip("The Overlay Image, this uses the fill amount.")]
        [SerializeField] protected Image m_OverlayImage;
        [Tooltip("The time Text.")]
        [SerializeField] protected Text m_TimeText;
        [Tooltip("The Time text format.")]
        [SerializeField] protected string m_TextFormat = "{0:0.00}";
        [Tooltip("The Cooldown attribute.")]
        [SerializeField] protected string m_CooldownAttributeName = "Cooldown";
        [Tooltip("The cooldown item user data name must be unique.")]
        [SerializeField] protected string m_CooldownItemUserDataName = "ItemCooldown";

        protected ItemUser m_ItemUser;
        private float m_OverlayFillAmount;
        private float m_CooldownNextTime;
        private float m_CooldownTime;
        private float m_OverlayTimer;

        /// <summary>
        /// Initialize the item view modules.
        /// </summary>
        /// <param name="view">The view.</param>
        public override void Initialize(View view)
        {
            base.Initialize(view);
            var inventoryIdentifier = InventorySystemManager.GetInventoryIdentifier(m_ItemUserInventoryID);
            if (inventoryIdentifier == null) {
                Debug.LogWarning("The cooldown Item View could not find the specific inventory identifier");
                return;
            }

            RegisterItemUser(inventoryIdentifier.gameObject.GetCachedComponent<ItemUser>());
        }

        /// <summary>
        /// Register with the item user.
        /// </summary>
        /// <param name="itemUser">The item user.</param>
        public void RegisterItemUser(ItemUser itemUser)
        {
            m_ItemUser = itemUser;
            if(m_ItemUser == null){return;}
            
            EventHandler.RegisterEvent<ItemAction, ItemInfo, float>(m_ItemUser.gameObject,
                EventNames.c_CharacterGameObject_UsedItemActionWithCooldown_ItemAction_ItemInfo_Float,
                HandleItemActionWithCooldownUsed);
        }

        /// <summary>
        /// Remove the linked item user on destroy.
        /// </summary>
        public void OnDestroy()
        {
            if(m_ItemUser == null){return;}
            
            EventHandler.UnregisterEvent<ItemAction, ItemInfo, float>(m_ItemUser.gameObject,
                EventNames.c_CharacterGameObject_UsedItemActionWithCooldown_ItemAction_ItemInfo_Float,
                HandleItemActionWithCooldownUsed);
        }

        /// <summary>
        /// Handle the event of the item action with cooldown.
        /// </summary>
        /// <param name="itemAction">The item action.</param>
        /// <param name="itemInfo">The iteminfo.</param>
        /// <param name="cooldown">The cooldown.</param>
        private void HandleItemActionWithCooldownUsed(ItemAction itemAction, ItemInfo itemInfo, float cooldown)
        {
            if(itemInfo.Item != ItemInfo.Item){ return; }

            SetValue(ItemInfo);
        }

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            var item = info.Item;
            if (item == null) {
                Clear();
                return;
            }

            if (m_ItemUser == null) {
                return;
            }

            item.TryGetAttributeValue(m_CooldownAttributeName, out m_CooldownTime);
            m_ItemUser.TryGetData(m_CooldownItemUserDataName, item, out m_CooldownNextTime);

            var timeLeft = GetCooldownTimeLeft();
            if (m_OverlayGameObject != null) {
                m_OverlayGameObject.SetActive(timeLeft > 0);
            }

        }

        /// <summary>
        /// Clear the value.
        /// </summary>
        public override void Clear()
        {
            if (m_OverlayGameObject != null) {
                m_OverlayGameObject.SetActive(false);
            }
            m_TimeText.text = string.Empty;

            m_CooldownTime = 0;
            m_CooldownNextTime = 0;
        }


        // Update is called once per frame
        protected virtual void Update()
        {
            if(m_OverlayGameObject.activeSelf == false){return;}
            
            var timeLeft = GetCooldownTimeLeft();
            if (timeLeft < 0) {
                m_OverlayGameObject.SetActive(false);
                return;
            }

            var fillAmount = timeLeft / m_CooldownTime;

            m_OverlayImage.fillAmount = fillAmount;
            m_TimeText.text = timeLeft.ToString(m_TextFormat);
        }

        /// <summary>
        /// Get the cooldown time left.
        /// </summary>
        /// <returns></returns>
        protected virtual float GetCooldownTimeLeft()
        {
            var time = Time.time;
            var timeLeft = m_CooldownNextTime - time;
            return timeLeft;
        }
    }
}