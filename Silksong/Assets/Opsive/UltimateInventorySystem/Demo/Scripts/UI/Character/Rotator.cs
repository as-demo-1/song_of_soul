/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.UI.Character
{
    using UnityEngine;

    /// <summary>
    /// Rotate a component
    /// </summary>
    public class Rotator : MonoBehaviour
    {
        [Tooltip("The rotation speed.")]
        [SerializeField] protected Vector3 m_RotateSpeed = new Vector3(0, 45, 0);
        [Tooltip("If true the transform will rotate even if the time is unscaled.")]
        [SerializeField] protected bool m_UseUnscaledTime = true;

        // Update is called once per frame
        void Update()
        {
            var deltaTime = m_UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            transform.Rotate(m_RotateSpeed * deltaTime);
        }
    }
}
