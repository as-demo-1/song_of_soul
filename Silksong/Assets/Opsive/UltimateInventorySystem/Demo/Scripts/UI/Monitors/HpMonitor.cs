/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.UI.Monitors
{
    using Opsive.UltimateInventorySystem.Demo.Damageable;
    using Opsive.UltimateInventorySystem.Demo.Events;
    using UnityEngine;
    using UnityEngine.UI;
    using EventHandler = Opsive.Shared.Events.EventHandler;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// A component that lets you monitor the Hp of a damageable.
    /// </summary>
    public class HpMonitor : MonoBehaviour
    {
        [Tooltip("The damageable to monitor.")]
        [SerializeField] protected Damageable m_Damageable;
        [Tooltip("The health slider.")]
        [SerializeField] protected Slider m_Slider;
        [Tooltip("The text displaying the damageables Hp.")]
        [SerializeField] protected Text m_HpTextText;

        /// <summary>
        /// Initialize the listener.
        /// </summary>
        private void Awake()
        {
            if (m_Damageable == null) {
                Debug.LogWarning("Missing damageable on HP monitor.", gameObject);
                return;
            }

            EventHandler.RegisterEvent<Damageable>(m_Damageable, DemoEventNames.c_Damageable_OnHpChange_Damageable, UpdateUI);
        }

        /// <summary>
        /// Update the UI.
        /// </summary>
        private void Start()
        {
            if (m_Damageable == null) { return; }

            UpdateUI(m_Damageable);
        }

        /// <summary>
        /// Update the UI.
        /// </summary>
        private void UpdateUI(Damageable damageable)
        {
            m_Slider.value = 100 * m_Damageable.CurrentHp / (float)m_Damageable.MaxHp;
            m_HpTextText.text = m_Damageable.CurrentHp.ToString().PadLeft(3) + " / " + m_Damageable.MaxHp.ToString();
        }

        /// <summary>
        /// Stop the listener.
        /// </summary>
        private void OnDestroy()
        {
            if (m_Damageable == null) { return; }

            EventHandler.UnregisterEvent<Damageable>(m_Damageable, DemoEventNames.c_Damageable_OnHpChange_Damageable, UpdateUI);
        }
    }
}
