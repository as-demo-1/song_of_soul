/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.DropsAndPickups
{
    using UnityEngine;

    /// <summary>
    /// Base class for a dropper component.
    /// </summary>
    public abstract class Dropper : MonoBehaviour
    {
        [Tooltip("The pick up prefab, it must have a pick up component.")]
        [SerializeField] protected GameObject m_PickUpPrefab;
        [Tooltip("The drop spawn point.")]
        [SerializeField] protected Transform m_DropTransform;
        [Tooltip("The radius in which the drop can randomly spawn around the drop transform.")]
        [SerializeField] protected float m_DropRadius;
        [Tooltip("If true the dropper will drop copies, if not the dropper will drop the original once.")]
        [SerializeField] protected bool m_DropCopies;

        /// <summary>
        /// Drop the object.
        /// </summary>
        public abstract void Drop();

        /// <summary>
        /// Get the drop offset.
        /// </summary>
        /// <returns>The drop offset.</returns>
        protected virtual Vector3 DropOffset()
        {
            var circle = Random.insideUnitCircle * m_DropRadius;
            return new Vector3(circle.x, 1, circle.y);
        }
    }
}