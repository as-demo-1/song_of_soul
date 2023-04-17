/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Interactions
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core;
    using UnityEngine;

    /// <summary>
    /// An abstract class for interactable behaviors.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public abstract class InteractableBehavior : MonoBehaviour
    {
        [Tooltip("Deactivate the object when interacted with.")]
        [SerializeField] protected bool m_DeactivateOnInteract = true;
        [Tooltip("A negative or 0 scheduled reactivation time will disable schedule reactivation.")]
        [SerializeField] protected float m_ScheduleReactivationTime = -1;
        [Tooltip("Enable the game objects when the interactable is selected.")]
        [SerializeField] internal GameObject[] m_SelectIndicators;

        protected Interactable m_Interactable;
        protected System.Action m_ReActivateAction;

        public Interactable Interactable => m_Interactable;

        /// <summary>
        /// Initialize.
        /// </summary>
        public void Initialize(Interactable interactable)
        {
            m_Interactable = interactable;
            m_ReActivateAction = Reactivate;
        }

        /// <summary>
        /// Set the listeners. 
        /// </summary>
        protected virtual void Start()
        {
            EventHandler.RegisterEvent<IInteractor>(gameObject, EventNames.c_Interactable_OnInteract_IInteractor, OnInteract);
            EventHandler.RegisterEvent<IInteractor>(gameObject, EventNames.c_Interactable_OnSelect_IInteractor, OnSelect);
            EventHandler.RegisterEvent<IInteractor>(gameObject, EventNames.c_Interactable_OnDeselect_IInteractor, OnDeselect);
            OnDeselect(null);
        }

        /// <summary>
        /// Enable the object and set interactable.
        /// </summary>
        public virtual void Reactivate()
        {
            gameObject.SetActive(true);
            m_Interactable.SetIsInteractable(true);
        }

        /// <summary>
        /// can the interactor interact with this compoenent.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        /// <returns>Returns true if it can interact.</returns>
        public virtual bool CanInteract(IInteractor interactor)
        {
            return true;
        }

        /// <summary>
        /// The event called when the interactable is selected by an interactor.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        public virtual void OnSelect(IInteractor interactor)
        {
            for (int i = 0; i < m_SelectIndicators.Length; i++) { m_SelectIndicators[i].SetActive(true); }
        }

        /// <summary>
        /// The event when the interactable is no longer selected.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        public virtual void OnDeselect(IInteractor interactor)
        {
            for (int i = 0; i < m_SelectIndicators.Length; i++) { m_SelectIndicators[i].SetActive(false); }
        }

        /// <summary>
        /// The event when the interactable is interacted with by an interactor.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        public virtual void OnInteract(IInteractor interactor)
        {
            OnInteractInternal(interactor);

            if (!m_DeactivateOnInteract) { return; }
            interactor.RemoveInteractable(m_Interactable);
            Deactivate();

        }

        /// <summary>
        /// Do something when interacted with.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        protected abstract void OnInteractInternal(IInteractor interactor);

        /// <summary>
        /// Deactivate the component.
        /// </summary>
        public virtual void Deactivate()
        {
            gameObject.SetActive(false);
            m_Interactable.SetIsInteractable(false);

            if (m_ScheduleReactivationTime > 0) {
                ScheduleReactivation();
            }
        }

        /// <summary>
        /// Schedule to be reactivated later.
        /// </summary>
        protected virtual void ScheduleReactivation()
        {
            Scheduler.Schedule(m_ScheduleReactivationTime, m_ReActivateAction);
        }

        /// <summary>
        /// Remove the listeners.
        /// </summary>
        protected virtual void OnDestroy()
        {
            EventHandler.UnregisterEvent<IInteractor>(gameObject, EventNames.c_Interactable_OnInteract_IInteractor, OnInteract);
            EventHandler.UnregisterEvent<IInteractor>(gameObject, EventNames.c_Interactable_OnSelect_IInteractor, OnSelect);
            EventHandler.UnregisterEvent<IInteractor>(gameObject, EventNames.c_Interactable_OnDeselect_IInteractor, OnDeselect);
        }
    }
}