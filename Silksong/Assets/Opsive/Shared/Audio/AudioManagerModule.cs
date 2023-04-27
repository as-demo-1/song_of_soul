/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Audio
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Audio;

    /// <summary>
    /// Container class for a list of AudioSources.
    /// </summary>
    public class AudioSourceGroup
    {
        [Tooltip("The GameObject that the group belongs to.")]
        protected GameObject m_GameObject;
        [Tooltip("The AudioConfig used if no AudioConfig is specified.")]
        protected AudioConfig m_DefaultAudioConfig;
        [Tooltip("The AudioMixerGroup that the AudioSourceGroup belongs to.")]
        protected AudioMixerGroup m_AudioMixerGroup;
        [Tooltip("All of the AudioSources within the group.")]
        protected List<AudioSource> m_AllAudioSources;
        [Tooltip("All of the shared AudioSources within the group.")]
        protected List<AudioSource> m_SharedAudioSources;
        [Tooltip("A mapping between AudioConfig and list of AudioSources.")]
        protected Dictionary<AudioConfig, List<AudioSource>> m_AudioSourceListByAudioConfig;

        public GameObject GameObject => m_GameObject;
        public AudioConfig DefaultAudioConfig => m_DefaultAudioConfig;
        public AudioMixerGroup AudioMixerGroup => m_AudioMixerGroup;
        public List<AudioSource> AllAudioSources => m_AllAudioSources;
        public List<AudioSource> SharedAudioSources => m_SharedAudioSources;
        public Dictionary<AudioConfig, List<AudioSource>> AudioSourceListByAudioConfig => m_AudioSourceListByAudioConfig;

        /// <summary>
        /// Three parameter constructor.
        /// </summary>
        public AudioSourceGroup(GameObject gameObject, AudioConfig defaultAudioConfig, AudioMixerGroup defaultMixerGroup)
        {
            m_GameObject = gameObject;
            m_DefaultAudioConfig = defaultAudioConfig;
            m_AudioMixerGroup = defaultMixerGroup;

            m_AllAudioSources = new List<AudioSource>();
            m_SharedAudioSources = new List<AudioSource>();
            m_AudioSourceListByAudioConfig = new Dictionary<AudioConfig, List<AudioSource>>();

            var existingAudioSources = gameObject.GetComponents<AudioSource>();

            m_AllAudioSources.AddRange(existingAudioSources);
            m_SharedAudioSources.AddRange(existingAudioSources);
            m_AudioSourceListByAudioConfig.Add(defaultAudioConfig, new List<AudioSource>(existingAudioSources));
        }
    }

    /// <summary>
    /// Organizes a group of AudioSources by the AudioConfig and GameObject.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioManagerModule", menuName = "Opsive/Audio/Audio Manager Module", order = 1)]
    public class AudioManagerModule : ScriptableObject
    {
        [Tooltip("A reference to the default AudioConfig used if no AudioConfig can be found.")]
        [SerializeField] protected AudioConfig m_DefaultAudioConfig;
        [Tooltip("The AudioMixerGroup used by the AudioManager.")]
        [SerializeField] protected AudioMixerGroup m_AudioMixerGroup;

        public AudioConfig DefaultAudioConfig { get => m_DefaultAudioConfig; set => m_DefaultAudioConfig = value; }
        public Dictionary<GameObject, AudioSourceGroup> AudioSourceGroups { get => m_AudioSourceGroups; set => m_AudioSourceGroups = value; }

        private Dictionary<GameObject, AudioSourceGroup> m_AudioSourceGroups = new Dictionary<GameObject, AudioSourceGroup>();

        /// <summary>
        /// Returns the AudioSourceGroup that belongs to the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The AudioSourceGroup that belongs to the specified GameObject.</param>
        /// <returns>The AudioSourceGroup that belongs to the specified GameObject.</returns>
        public AudioSourceGroup GetAudioGroup(GameObject gameObject)
        {
            if (m_AudioSourceGroups.TryGetValue(gameObject, out var audioGroup)) {
                return audioGroup;
            }

            audioGroup = new AudioSourceGroup(gameObject, m_DefaultAudioConfig, m_AudioMixerGroup);
            m_AudioSourceGroups.Add(gameObject, audioGroup);

            return audioGroup;
        }

        /// <summary>
        /// Returns the playing AudioSource on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject that the AudioSource should be retrieved from.</param>
        /// <param name="audioConfig">The AudioConfig that contains the AudioSource.</param>
        /// <returns>The playing AudioSource on the specified GameObject.</returns>
        public virtual AudioSource GetActiveAudioSource(GameObject gameObject, AudioConfig audioConfig = null)
        {
            var audioGroup = GetAudioGroup(gameObject);
            if (audioConfig == null) { audioConfig = m_DefaultAudioConfig; }
            return audioConfig.GetActiveAudioSource(audioGroup);
        }

        /// <summary>
        /// Returns the next AudioSource that is not playing on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject that the AudioSource should be retrieved from.</param>
        /// <param name="audioConfig">The AudioConfig that contains the AudioSource.</param>
        /// <returns>The playing AudioSource on the specified GameObject.</returns>
        public virtual AudioSource GetAvailableAudioSource(GameObject gameObject, AudioConfig audioConfig = null)
        {
            var audioGroup = GetAudioGroup(gameObject);
            if (audioConfig == null) { audioConfig = m_DefaultAudioConfig; }

            return audioConfig.GetAvailableAudioSource(audioGroup);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that the AudioClip should be played on.</param>
        /// <param name="audioClipInfo">The AudioClipInfo that should be played.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public virtual PlayResult PlayAudio(GameObject gameObject, AudioClipInfo audioClipInfo)
        {
            var audioGroup = GetAudioGroup(gameObject);
            if (audioClipInfo.AudioConfig == null) {
                audioClipInfo = new AudioClipInfo(audioClipInfo, m_DefaultAudioConfig);
            }

            return audioClipInfo.AudioConfig.Play(audioGroup, audioClipInfo);
        }

        /// <summary>
        /// Plays the audio clip at the specified position.
        /// </summary>
        /// <param name="gameObject">The GameObject that the AudioClip should be played on.</param>
        /// <param name="audioClipInfo">The AudioClipInfo that should be played.</param>
        /// <param name="position">The position that the AudioClip should play at.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public virtual PlayResult PlayAtPosition(GameObject gameObject, AudioClipInfo audioClipInfo, Vector3 position)
        {
            var playResult = PlayAudio(gameObject, audioClipInfo);
            playResult.AudioSource.transform.position = position;
            return playResult;
        }

        /// <summary>
        /// Stops playing the audio on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject that contains the playing AudioSource.</param>
        /// <param name="audioConfig">The AudioConfig that should be stopped.</param>
        /// <returns>The AudioSource that was stopped playing (can be null).</returns>
        public virtual AudioSource Stop(GameObject gameObject, AudioConfig audioConfig)
        {
            var activeAudioSource = GetActiveAudioSource(gameObject, audioConfig);
            if (activeAudioSource == null) {
                return null;
            }

            activeAudioSource.Stop();
            return activeAudioSource;
        }

        /// <summary>
        /// Stops playing the audio on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject that contains the playing AudioSource.</param>
        /// <param name="playResult">The PlayResult from when the audio was played.</param>
        /// <returns>The AudioSource that was stopped playing (can be null).</returns>
        public virtual AudioSource Stop(GameObject gameObject, PlayResult playResult)
        {
            var activeAudioSource = playResult.AudioSource;
            if (activeAudioSource == null) {
                return null;
            }

            activeAudioSource.Stop();
            return activeAudioSource;
        }

        /// <summary>
        /// Reset the static variables for domain reloading.
        /// </summary>
        public void DomainReset()
        {
            m_AudioSourceGroups = new Dictionary<GameObject, AudioSourceGroup>();
        }
    }
}