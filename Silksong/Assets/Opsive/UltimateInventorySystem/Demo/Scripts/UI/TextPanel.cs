/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.UI
{
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using System.Threading.Tasks;
    using UnityEngine;
    using Text = Opsive.Shared.UI.Text;

    public class TextPanel : MonoBehaviour
    {
        [SerializeField] protected GameObject m_Panel;
        [SerializeField] protected Text m_Text;

        protected float m_Timer;

        /// <summary>
        /// Display the text.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="displayTime">The time in seconds to display for.</param>
        public virtual void DisplayText(string text, float displayTime)
        {
            m_Text.text = text;
            m_Panel.SetActive(true);

#pragma warning disable 4014
            DisableAfterDelay(displayTime);
#pragma warning restore 4014
        }

        protected virtual async Task DisableAfterDelay(float delay)
        {
            m_Timer = Time.unscaledTime + delay;
            while (Time.unscaledTime < m_Timer) {
                await Task.Yield();
            }
            m_Panel.SetActive(false);
        }
    }
}
