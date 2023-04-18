/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.UI.WorldSpace
{
    using Opsive.UltimateInventorySystem.Utility;
    using UnityEngine;

    /// <summary>
    /// The Billboard FX allows an transform to always face the camera.
    /// </summary>
    public class BillboardFX : MonoBehaviour
    {
        [Tooltip("The camera transform.")]
        [SerializeField] protected Transform m_CamTransform;

        protected Quaternion m_OriginalRotation;

        /// <summary>
        /// Validate the component.
        /// </summary>
        private void OnValidate()
        {
            if (OnValidateUtility.IsPrefab(this)) { return; }
            if (m_CamTransform == null) {
                if (Camera.main != null) {
                    m_CamTransform = Camera.main.transform;
                }
            }
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        void Start()
        {
            if (m_CamTransform == null) {
                if (Camera.main != null) {
                    m_CamTransform = Camera.main.transform;
                }
            }

            m_OriginalRotation = transform.localRotation;
        }

        /// <summary>
        /// Update after to always face the camera.
        /// </summary>
        void LateUpdate()
        {
            transform.rotation = m_CamTransform.rotation * m_OriginalRotation;
        }
    }
}
