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
    /// Container struct which allows the default functionality to be overridden for a specific clip.
    /// </summary>
    [Serializable]
    public struct AudioClipInfo
    {
        [Tooltip("The index of the AudioClip inside the AudioConfig.")]
        [SerializeField] private int m_ClipIndex;
        [Tooltip("A reference to the clip that should be played.")]
        [SerializeField] private AudioClip m_AudioClip;
        [Tooltip("The overridden AudioConfig.")]
        [SerializeField] private AudioConfig m_AudioConfig;
        [Tooltip("The overridden AudioModifier.")]
        [SerializeField] private AudioModifier m_AudioModifier;

        public int ClipIndex => m_ClipIndex;
        public AudioClip AudioClip => m_AudioClip;
        public AudioConfig AudioConfig => m_AudioConfig;
        public AudioModifier AudioModifier => m_AudioModifier;

        /// <summary>
        /// One parameter constructor.
        /// </summary>
        /// <param name="audioConfig">A reference to the overridden AudioConfig.</param>
        public AudioClipInfo(AudioConfig audioConfig)
        {
            m_ClipIndex = -1;
            m_AudioClip = null;
            m_AudioConfig = audioConfig;
            m_AudioModifier = new AudioModifier();
        }

        /// <summary>
        /// Two parameter constructor.
        /// </summary>
        /// <param name="audioClip">The clip that should be played.</param>
        /// <param name="audioConfig">A reference to the overridden AudioConfig.</param>
        public AudioClipInfo(AudioClip audioClip, AudioConfig audioConfig)
        {
            m_ClipIndex = -1;
            m_AudioClip = audioClip;
            m_AudioConfig = audioConfig;
            m_AudioModifier = new AudioModifier();
        }

        /// <summary>
        /// Two parameter constructor.
        /// </summary>
        /// <param name="clipIndex">The index of the AudioClip inside the AudioConfig.</param>
        /// <param name="audioConfig">A reference to the overridden AudioConfig.</param>
        public AudioClipInfo(int clipIndex, AudioConfig audioConfig)
        {
            m_ClipIndex = clipIndex;
            m_AudioClip = audioConfig != null ? audioConfig.GetAudioClip(clipIndex) : null;
            m_AudioConfig = audioConfig;
            m_AudioModifier = new AudioModifier();
        }

        /// <summary>
        /// Three parameter constructor.
        /// </summary>
        /// <param name="audioClip">The clip that should be played.</param>
        /// <param name="audioConfig">A reference to the overridden AudioConfig.</param>
        /// <param name="audioModifier">A reference to the overridden AudioModifier.</param>
        public AudioClipInfo(AudioClip audioClip, AudioConfig audioConfig, AudioModifier audioModifier)
        {
            m_ClipIndex = -1;
            m_AudioClip = audioClip;
            m_AudioConfig = audioConfig;
            m_AudioModifier = audioModifier;
        }

        /// <summary>
        /// Three parameter constructor.
        /// </summary>
        /// <param name="clipIndex">The index of the AudioClip inside the AudioConfig.</param>
        /// <param name="audioConfig">A reference to the overridden AudioConfig.</param>
        /// <param name="audioModifier">A reference to the overridden AudioModifier.</param>
        public AudioClipInfo(int clipIndex, AudioConfig audioConfig, AudioModifier audioModifier)
        {
            m_ClipIndex = clipIndex;
            m_AudioClip = null;
            m_AudioConfig = audioConfig;
            m_AudioModifier = audioModifier;
        }

        /// <summary>
        /// Three parameter constructor.
        /// </summary>
        /// <param name="clipIndex">The index of the AudioClip inside the AudioConfig.</param>
        /// <param name="audioClip">The clip that should be played.</param>
        /// <param name="audioConfig">A reference to the overridden AudioConfig.</param>
        /// <param name="audioModifier">A reference to the overridden AudioModifier.</param>
        public AudioClipInfo(int clipIndex, AudioClip audioClip, AudioConfig audioConfig, AudioModifier audioModifier)
        {
            m_ClipIndex = clipIndex;
            m_AudioClip = audioClip;
            m_AudioConfig = audioConfig;
            m_AudioModifier = audioModifier;
        }

        /// <summary>
        /// Two parameter constructor which copies another AudioClipInfo.
        /// </summary>
        /// <param name="audioClipInfo">The AudioClipInfo that should be copied.</param>
        /// <param name="clipIndex">The index of the Audio Clip inside the Audio Config.</param>
        public AudioClipInfo(AudioClipInfo audioClipInfo, int clipIndex)
        {
            m_ClipIndex = clipIndex;
            m_AudioClip = audioClipInfo.m_AudioClip;
            m_AudioConfig = audioClipInfo.m_AudioConfig;
            m_AudioModifier = audioClipInfo.m_AudioModifier;
        }

        /// <summary>
        /// Two parameter constructor which copies another AudioClipInfo.
        /// </summary>
        /// <param name="audioClipInfo">The AudioClipInfo that should be copied.</param>
        /// <param name="audioModifier">A reference to the overridden AudioModifier.</param>
        public AudioClipInfo(AudioClipInfo audioClipInfo, AudioModifier audioModifier)
        {
            m_ClipIndex = audioClipInfo.m_ClipIndex;
            m_AudioClip = audioClipInfo.m_AudioClip;
            m_AudioConfig = audioClipInfo.m_AudioConfig;
            m_AudioModifier = audioModifier;
        }

        /// <summary>
        /// Two parameter constructor which copies another AudioClipInfo.
        /// </summary>
        /// <param name="audioClipInfo">The AudioClipInfo that should be copied.</param>
        /// <param name="audioConfig">A reference to the overridden AudioConfig.</param>
        public AudioClipInfo(AudioClipInfo audioClipInfo, AudioConfig audioConfig)
        {
            m_ClipIndex = audioClipInfo.m_ClipIndex;
            m_AudioClip = audioClipInfo.m_AudioClip;
            m_AudioConfig = audioConfig;
            m_AudioModifier = audioClipInfo.m_AudioModifier;
        }

        /// <summary>
        /// Two parameter constructor which copies another AudioClipInfo.
        /// </summary>
        /// <param name="audioClipInfo">The AudioClipInfo that should be copied.</param>
        /// <param name="audioClip">The clip that should be played.</param>
        public AudioClipInfo(AudioClipInfo audioClipInfo, AudioClip audioClip)
        {
            m_ClipIndex = audioClipInfo.m_ClipIndex;
            m_AudioClip = audioClip;
            m_AudioConfig = audioClipInfo.m_AudioConfig;
            m_AudioModifier = audioClipInfo.m_AudioModifier;
        }

        /// <summary>
        /// One parameter constructor which copies another AudioClipInfo.
        /// </summary>
        /// <param name="audioClipInfo">The AudioClipInfo that should be copied.</param>
        public AudioClipInfo(AudioClipInfo audioClipInfo)
        {
            m_ClipIndex = audioClipInfo.m_ClipIndex;
            m_AudioClip = audioClipInfo.m_AudioClip;
            m_AudioConfig = audioClipInfo.m_AudioConfig;
            m_AudioModifier = audioClipInfo.m_AudioModifier;
        }
    }
}