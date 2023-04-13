/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Interactions
{
    using System;
    using Opsive.UltimateInventorySystem.Core;
    using UnityEngine;
    using UnityEngine.Events;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    public delegate bool InteractionCondition(IInteractor interactor);

    /// <summary>
    /// The interactable class allows you to easily select, unselect and interact with an object.
    /// </summary>
    public class Interactable : MonoBehaviour, IInteractable
    {
        [Tooltip("The interactor layers.")]
        [SerializeField] protected LayerMask m_InteractorLayerMask = -1;
        [Tooltip("Is the interactable interactable.")]
        [SerializeField] protected bool m_IsInteractable = true;
        [Tooltip("Automatically interact with the interactor.")]
        [SerializeField] protected bool m_AutoInteract = false;
        [Tooltip("Check for 2D trigger events.")]
        [SerializeField] protected bool m_2D = true;
        [Tooltip("Check for 3D trigger events.")]
        [SerializeField] protected bool m_3D = true;
        [Tooltip("The unity event for on interact.")]
        [SerializeField] protected UnityEvent m_OnInteract;
        [Tooltip("The unity event for on select.")]
        [SerializeField] protected UnityEvent m_OnSelect;
        [Tooltip("The unity event for on delect.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_OnUnselect")]
        [SerializeField] protected UnityEvent m_OnDeselect;

        protected InteractableBehavior m_InteractableBehavior;
        public virtual InteractableBehavior InteractableBehavior => m_InteractableBehavior;

        protected IInteractor m_LastInteractor;
        public IInteractor LastInteractor => m_LastInteractor;

        /// <summary>
        /// Initialize.
        /// </summary>
        private void Awake()
        {
            m_InteractableBehavior = GetComponent<InteractableBehavior>();
            if (m_InteractableBehavior != null) {
                m_InteractableBehavior.Initialize(this);
            }
        }

        /// <summary>
        /// On trigger enter 3D.
        /// </summary>
        /// <param name="other">The other collider.</param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!m_3D) { return; }
            OnTriggerEnterInternal(other.gameObject);
        }

        /// <summary>
        /// On trigger exit 3D.
        /// </summary>
        /// <param name="other">The other collider.</param>
        protected virtual void OnTriggerExit(Collider other)
        {
            if (!m_3D) { return; }
            OnTriggerExitInternal(other.gameObject);
        }

        /// <summary>
        /// On trigger enter 2D.
        /// </summary>
        /// <param name="other">The other collider.</param>
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!m_2D) { return; }
            OnTriggerEnterInternal(other.gameObject);
        }

        /// <summary>
        /// On trigger exit 2D.
        /// </summary>
        /// <param name="other">The other collider.</param>
        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (!m_2D) { return; }
            OnTriggerExitInternal(other.gameObject);
        }

        /// <summary>
        /// On trigger enter internal.
        /// </summary>
        /// <param name="other">The other game object.</param>
        protected virtual void OnTriggerEnterInternal(GameObject other)
        {
            if (!m_IsInteractable || !m_InteractorLayerMask.Contains(other)) { return; }

            var interactor = other.GetComponentInParent<IInteractor>();
            interactor?.AddInteractable(this);
        }

        /// <summary>
        /// On trigger exit internal.
        /// </summary>
        /// <param name="other">The other game object.</param>
        protected virtual void OnTriggerExitInternal(GameObject other)
        {
            if (!m_InteractorLayerMask.Contains(other)) { return; }

            var interactor = other.GetComponentInParent<IInteractor>();
            interactor?.RemoveInteractable(this);
        }

        /// <summary>
        /// Can the interactor interact with this object.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        /// <returns>True if it can interact.</returns>
        public virtual bool CanInteract(IInteractor interactor)
        {
            if (m_IsInteractable == false) { return false; }

            if (m_InteractableBehavior == null) { return true; }

            return m_InteractableBehavior.CanInteract(interactor);
        }

        /// <summary>
        /// Select the interactable.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        /// <returns>False if not interactable.</returns>
        public virtual bool Select(IInteractor interactor)
        {
            if (!CanInteract(interactor)) { return false; }

            m_LastInteractor = interactor;

            m_OnSelect.Invoke();
            EventHandler.ExecuteEvent<IInteractor>(gameObject, EventNames.c_Interactable_OnSelect_IInteractor, interactor);

            if (m_AutoInteract) { Interact(interactor); }

            return true;
        }

        /// <summary>
        /// Deselects the interactable.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        /// <returns>True if the interactable was deselected.</returns>
        public virtual bool Deselect(IInteractor interactor)
        {
            if (!CanInteract(interactor)) { return false; }

            m_OnDeselect.Invoke();
            EventHandler.ExecuteEvent<IInteractor>(gameObject, EventNames.c_Interactable_OnDeselect_IInteractor, interactor);
            return true;
        }

        /// <summary>
        /// Interact with the interactable.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        /// <returns>False if not interactable.</returns>
        public virtual bool Interact(IInteractor interactor)
        {
            if (!CanInteract(interactor)) { return false; }
            
            m_LastInteractor = interactor;

            m_OnInteract.Invoke();
            EventHandler.ExecuteEvent<IInteractor>(gameObject, EventNames.c_Interactable_OnInteract_IInteractor, interactor);
            return true;
        }

        /// <summary>
        /// Set if the interactable is interactable.
        /// </summary>
        /// <param name="isInteractable">Is it interactable.</param>
        public virtual void SetIsInteractable(bool isInteractable)
        {
            m_IsInteractable = isInteractable;
        }

        /// <summary>
        /// Remove from the interactable list in the component is disabled or destroyed.
        /// </summary>
        private void OnDisable()
        {
            if (m_LastInteractor != null) {
                m_LastInteractor.RemoveInteractable(this);
            }
        }
    }
}
