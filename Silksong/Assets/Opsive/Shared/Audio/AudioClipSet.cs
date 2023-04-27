/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Audio
{
    using UnityEngine;
    using Random = UnityEngine.Random;

    /// <summary>
    /// The AudioClipSet contains an array of AudioClips.
    /// </summary>
    [System.Serializable]
    public class AudioClipSet
    {
        [Tooltip("The AudioConfig for the set.")]
        [SerializeField] protected AudioConfig m_AudioConfig;
        [Tooltip("An array of AudioClips which belong to the set.")]
        [SerializeField] protected AudioClip[] m_AudioClips;

        public AudioConfig AudioConfig { get { return m_AudioConfig; } set { m_AudioConfig = value; } }
        public AudioClip[] AudioClips { get { return m_AudioClips; } set { m_AudioClips = value; } }

        PlayResult m_LastPlayResult;

        /// <summary>
        /// Plays the audio clip with a random set index.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public PlayResult PlayAudioClip(GameObject gameObject)
        {
            return PlayAudioClip(gameObject, -1);
        }

        /// <summary>
        /// Plays the audio clip with a random set index.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clipIndex">The index of the AudioClip that should be played.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public PlayResult PlayAudioClip(GameObject gameObject, int clipIndex)
        {
            return PlayAudioClip(gameObject, new AudioModifier(), clipIndex);
        }

        /// <summary>
        /// Plays the audio clip with a random set index.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="loop">Does the clip loop?</param>
        /// <param name="clipIndex">The index of the AudioClip that should be played.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public PlayResult PlayAudioClip(GameObject gameObject, bool loop, int clipIndex = -1)
        {
            return PlayAudioClip(gameObject, new AudioModifier()
            {
                LoopOverride = new BoolOverride(BoolOverride.Override.Constant, loop)
            }, clipIndex);
        }

        /// <summary>
        /// Plays the audio clip with a random set index.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="audioModifier">The AudioModifier that should override the AudioSource settings.</param>
        /// <param name="clipIndex">The index of the AudioClip that should be played.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public PlayResult PlayAudioClip(GameObject gameObject, AudioModifier audioModifier, int clipIndex = -1)
        {
            var audioClipInfo = GetAudioClipInfo(clipIndex);
            audioClipInfo = new AudioClipInfo(audioClipInfo, audioModifier);
            m_LastPlayResult = AudioManager.Play(gameObject, audioClipInfo);
            return m_LastPlayResult;
        }

        /// <summary>
        /// Plays the audio clip at the specified position.
        /// </summary>
        /// <param name="position">The position that the audio clip should be played at.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public PlayResult PlayAtPosition(Vector3 position)
        {
            return PlayAtPosition(position, -1);
        }

        /// <summary>
        /// Plays the audio clip at the specified position.
        /// </summary>
        /// <param name="position">The position that the audio clip should be played at.</param>
        /// <param name="clipIndex">The index of the AudioClip that should be played.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public PlayResult PlayAtPosition(Vector3 position, int clipIndex)
        {
            var audioClipConfig = GetAudioClipInfo(clipIndex);
            m_LastPlayResult = AudioManager.PlayAtPosition(audioClipConfig.AudioClip, audioClipConfig.AudioConfig, position);
            return m_LastPlayResult;
        }

        /// <summary>
        /// Returns the AudioClipInfo that should be played.
        /// </summary>
        /// <param name="index">The index of the AudioClipInfo.</param>
        /// <returns>An AudioClipInfo with the specified index.</returns>
        private AudioClipInfo GetAudioClipInfo(int index)
        {
            if (m_AudioConfig != null) {
                return new AudioClipInfo(index, m_AudioConfig);
            }

            if (m_AudioClips == null || m_AudioClips.Length == 0) {
                return new AudioClipInfo();
            }

            if (index < 0 || index >= m_AudioClips.Length) {
                index = Random.Range(0, m_AudioClips.Length);
            }

            var audioClip = m_AudioClips[index];
            return new AudioClipInfo(audioClip, m_AudioConfig);
        }

        /// <summary>
        /// Stops playing the audio on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to stop the audio on.</param>
        public void Stop(GameObject gameObject)
        {
            Stop(gameObject, m_LastPlayResult);
        }

        /// <summary>
        /// Stops playing the audio on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to stop the audio on.</param>
        /// <param name="audioConfig">The audioConfig used to match the audio source to stop.</param>
        public void Stop(GameObject gameObject, AudioConfig audioConfig)
        {
            AudioManager.Stop(gameObject, audioConfig);
        }

        /// <summary>
        /// Stops playing the audio on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to stop the audio on.</param>
        /// <param name="playResult">The result AudioClip and AudioConfig that was played.</param>
        public void Stop(GameObject gameObject, PlayResult playResult)
        {
            AudioManager.Stop(gameObject, playResult);
        }

        /// <summary>
        /// Stops playing the audio on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to stop the audio on.</param>
        /// <param name="audioClipInfo">The AudioClipInfo that should be stopped.</param>
        public void Stop(GameObject gameObject, AudioClipInfo audioClipInfo)
        {
            AudioManager.Stop(gameObject, audioClipInfo.AudioConfig);
        }
    }
}