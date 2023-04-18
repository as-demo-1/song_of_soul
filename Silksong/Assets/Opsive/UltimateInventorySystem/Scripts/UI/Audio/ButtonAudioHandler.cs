/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------


namespace Opsive.UltimateInventorySystem.UI.Audio
{
    using UnityEngine;
    using UnityEngine.UI;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;

    /// <summary>
    /// This component is used to add audio to your buttons and action buttons.
    /// </summary>
    public class ButtonAudioHandler : MonoBehaviour
    {
        [Tooltip("The button audio.")]
        [SerializeField] protected ButtonAudio m_ButtonAudio;
        [Tooltip("The button (optional), can only trigger click sounds.")]
        [SerializeField] protected Button m_Button;
        [Tooltip("The action button (optional).")]
        [SerializeField] protected ActionButton m_ActionButton;

        /// <summary>
        /// Get the references if they are missing.
        /// </summary>
        private void Awake()
        {
            if (m_Button == null) { m_Button = GetComponent<Button>(); }
            if (m_ActionButton == null) { m_ActionButton = GetComponent<ActionButton>(); }
        }

        /// <summary>
        /// Listen to the events.
        /// </summary>
        private void Start()
        {
            if (m_ButtonAudio == null) {
                Debug.LogWarning("The audio asset is missing from the button audio, no audio will be played.", gameObject);
                return;
            }

            if (m_Button != null) {

                m_Button.onClick.AddListener(PlayClickSound);
            }

            if (m_ActionButton != null) {
                m_ActionButton.OnSelectE += PlaySelectSound;
                m_ActionButton.OnSubmitE += PlayClickSound;
            }
        }

        /// <summary>
        /// Play the select sound.
        /// </summary>
        private void PlaySelectSound()
        {
            Shared.Audio.AudioManager.PlayAtPosition(m_ButtonAudio.SelectClipClip, m_ButtonAudio.SelectConfig, Vector3.zero);
        }

        /// <summary>
        /// Play the click sound.
        /// </summary>
        private void PlayClickSound()
        {
            Shared.Audio.AudioManager.PlayAtPosition(m_ButtonAudio.ClickClipClip, m_ButtonAudio.ClickConfig, Vector3.zero);
        }
    }
}