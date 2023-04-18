/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Audio;

namespace Opsive.UltimateInventorySystem.DropsAndPickups
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Interactions;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// Base class of a pickup interactable behavior.
    /// </summary>
    public abstract class PickupBase : InteractableBehavior
    {
        [Tooltip("On Pickup Success Event.")]
        [SerializeField] protected UnityEvent m_OnPickupSuccess;
        [Tooltip("The audio clip to play when the object is picked up.")]
        [SerializeField] protected AudioClip m_AudioClip;
        [Tooltip("The Audio Config.")]
        [SerializeField] protected AudioConfig m_AudioConfig;
        [Tooltip("On Pickup Fail Event")]
        [SerializeField] protected UnityEvent m_OnPickupFail;
        [Tooltip("The audio clip to play when the object is picked up.")]
        [SerializeField] protected AudioClip m_FailAudioClip;
        [Tooltip("The Audio Config.")]
        [SerializeField] protected AudioConfig m_FailAudioConfig;

        /// <summary>
        /// Deactivate.
        /// </summary>
        public override void Deactivate()
        {
            base.Deactivate();

            if (m_ScheduleReactivationTime <= 0) {
                DestroyPickup();
            }
        }

        /// <summary>
        /// Set the pickup interactable.
        /// </summary>
        private void OnEnable()
        {
            if (m_Interactable == null) {
                m_Interactable = GetComponent<Interactable>();
                if(m_Interactable == null){ return; }
            }
            m_Interactable.SetIsInteractable(true);
        }

        /// <summary>
        /// Successfully picked up the object.
        /// </summary>
        protected virtual void NotifyPickupSuccess()
        {
            m_OnPickupSuccess?.Invoke();
            Shared.Audio.AudioManager.PlayAtPosition(m_AudioClip, m_AudioConfig, transform.position);
        }
        
        /// <summary>
        /// Unsuccessfully picked up the object.
        /// </summary>
        protected virtual void NotifyPickupFailed()
        {
            m_OnPickupFail?.Invoke();
            Shared.Audio.AudioManager.PlayAtPosition(m_FailAudioClip, m_FailAudioConfig, transform.position);
        }

        /// <summary>
        /// Return the pickup to the pool.
        /// </summary>
        protected virtual void DestroyPickup()
        {
            if (ObjectPool.IsPooledObject(gameObject)) {
                ObjectPool.Destroy(gameObject);
            } else {
                Destroy(gameObject);
            }
        }
    }
}