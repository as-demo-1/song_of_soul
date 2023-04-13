/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Audio
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The object returned after playing an AudioClip.
    /// </summary>
    [Serializable]
    public struct PlayResult
    {
        [Tooltip("The AudioSource that played the AudioClip.")]
        [SerializeField] private AudioSource m_AudioSource;
        [Tooltip("THe config used to play the AudioClip.")]
        [SerializeField] private AudioClipInfo m_AudioClipInfo;
        
        public AudioSource AudioSource => m_AudioSource;
        public AudioClipInfo AudioClipInfo => m_AudioClipInfo;

        /// <summary>
        /// Two parameter constructor.
        /// </summary>
        /// <param name="audioSource">The AudioSource that played the AudioClip.</param>
        /// <param name="audioClipInfo">The config used to play the AudioClip.</param>
        public PlayResult(AudioSource audioSource, AudioClipInfo audioClipInfo)
        {
            m_AudioSource = audioSource;
            m_AudioClipInfo = audioClipInfo;
        }
    }
}