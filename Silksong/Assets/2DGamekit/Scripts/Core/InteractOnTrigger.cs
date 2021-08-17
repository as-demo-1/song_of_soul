using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gamekit2D
{
    [RequireComponent(typeof(SphereCollider))]
    public class InteractOnTrigger : MonoBehaviour
    {
        public LayerMask layers;
        public UnityEvent OnEnter, OnExit;
        public InventoryController.InventoryChecker[] inventoryChecks;

        SphereCollider m_Collider;

        void Reset()
        {
            layers = LayerMask.NameToLayer("Everything");
            m_Collider = GetComponent<SphereCollider>();
            m_Collider.radius = 5;
            m_Collider.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (layers.Contains (other.gameObject))
            {
                ExecuteOnEnter(other);
            }
        }

        protected virtual void ExecuteOnEnter(Collider other)
        {
            OnEnter.Invoke();
            for (var i = 0; i < inventoryChecks.Length; i++)
            {
                inventoryChecks[i].CheckInventory(other.GetComponentInChildren<InventoryController>());
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (layers.Contains (other.gameObject))
            {
                ExecuteOnExit(other);
            }
        }

        protected virtual void ExecuteOnExit(Collider other)
        {
            OnExit.Invoke();
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "InteractionTrigger", false);
        }
    }
}