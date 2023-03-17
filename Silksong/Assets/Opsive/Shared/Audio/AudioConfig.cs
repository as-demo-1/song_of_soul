/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Audio
{
    using System;
    using System.Collections.Generic;
    using Opsive.Shared.Game;
    using UnityEngine;
    using UnityEngine.Audio;
    using Random = UnityEngine.Random;

    /// <summary>
    /// Specifies if the float value should be overridden with another value.
    /// </summary>
    [Serializable]
    public struct FloatOverride
    {
        /// <summary>
        /// The options for overriding the value.
        /// </summary>
        public enum Override
        {
            NoOverride, // The value is not overridden.
            Constant,   // The value is overridden with a constant.
            Random      // The value is overridden with a random value between two constants.
        }

        [Tooltip("The value override option.")]
        [SerializeField] private Override m_ValueOverride;
        [Tooltip("The first constant.")]
        [SerializeField] private float m_Constant1;
        [Tooltip("The second constant.")]
        [SerializeField] private float m_Constant2;

        public Override ValueOverride => m_ValueOverride;
        public float Constant1 => m_Constant1;
        public float Constant2 => m_Constant2;

        public float Value
        {
            get
            {
                switch (m_ValueOverride) {
                    case Override.NoOverride:
                        return -1;
                    case Override.Constant:
                        return m_Constant1;
                    case Override.Random:
                        return Random.Range(m_Constant1, m_Constant2);
                    default:
                        return -1;
                }
            }
        }

        /// <summary>
        /// Three parameter FloatOverride constructor.
        /// </summary>
        /// <param name="valueOverride">Specifies if the value is overridden.</param>
        /// <param name="constant1">The first constant.</param>
        /// <param name="constant2">The second constant.</param>
        public FloatOverride(Override valueOverride, float constant1 = 0, float constant2 = 0)
        {
            m_ValueOverride = valueOverride;
            m_Constant1 = constant1;
            m_Constant2 = constant2;
        }
    }
    [Serializable]
    public struct BoolOverride
    {
        /// <summary>
        /// The options for overriding the value.
        /// </summary>
        public enum Override
        {
            NoOverride, // The value is not overridden.
            Constant,   // The value is overridden with a constant.
            Random      // The value is overridden with a random value.
        }

        [Tooltip("The value override option.")]
        [SerializeField] private Override m_ValueOverride;
        [Tooltip("The value of the override.")]
        [SerializeField] private bool m_Value;

        public Override ValueOverride => m_ValueOverride;
        public bool Value
        {
            get
            {
                switch (m_ValueOverride) {
                    case Override.NoOverride:
                        return false;
                    case Override.Constant:
                        return m_Value;
                    case Override.Random:
                        return Random.value > 0.5f;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Two parameter BoolOverride constructor.
        /// </summary>
        /// <param name="valueOverride">Specifies if the value is overridden.</param>
        /// <param name="value">The overridden value.</param>
        public BoolOverride(Override valueOverride, bool value = false)
        {
            m_ValueOverride = valueOverride;
            m_Value = value;
        }
    }

    /// <summary>
    /// Specifies any overrides for the AudioConfig.
    /// </summary>
    [Serializable]
    public struct AudioModifier
    {
        [Tooltip("Allows the audio output to be overridden.")]
        [SerializeField] private AudioMixerGroup m_OutputOverride;
        [Tooltip("Allows the audio loop to be overridden.")]
        [SerializeField] private BoolOverride m_LoopOverride;
        [Tooltip("Allows the volume to be overridden.")]
        [SerializeField] private FloatOverride m_VolumeOverride;
        [Tooltip("Allows the pitch to be overridden.")]
        [SerializeField] private FloatOverride m_PitchOverride;
        [Tooltip("Allows the stereo pan to be overridden.")]
        [SerializeField] private FloatOverride m_StereoPanOverride;
        [Tooltip("Allows the spatial blend to be overridden.")]
        [SerializeField] private FloatOverride m_SpatialBlendOverride;
        [Tooltip("Allows the reverb zone to be overridden.")]
        [SerializeField] private FloatOverride m_ReverbZoneOverride;
        [Tooltip("Allows the audio delay to be overridden.")]
        [SerializeField] private FloatOverride m_DelayOverride;


        public FloatOverride VolumeOverride { get => m_VolumeOverride; set => m_VolumeOverride = value; }
        public FloatOverride PitchOverride { get => m_PitchOverride; set => m_PitchOverride = value; }
        public FloatOverride DelayOverride { get => m_DelayOverride; set => m_DelayOverride = value; }
        public AudioMixerGroup OutputOverride { get => m_OutputOverride; set => m_OutputOverride = value; }
        public BoolOverride LoopOverride { get => m_LoopOverride; set => m_LoopOverride = value; }
        public FloatOverride StereoPanOverride { get => m_StereoPanOverride; set => m_StereoPanOverride = value; }
        public FloatOverride SpatialBlendOverride { get => m_SpatialBlendOverride; set => m_SpatialBlendOverride = value; }
        public FloatOverride ReverbZoneOverride { get => m_ReverbZoneOverride; set => m_ReverbZoneOverride = value; }

        /// <summary>
        /// Five parameter constructor.
        /// </summary>
        public AudioModifier(FloatOverride volumeOverride, FloatOverride pitchOverride, FloatOverride delayOverride, BoolOverride loopOverride, AudioMixerGroup outputOverride)
        {
            m_VolumeOverride = volumeOverride;
            m_PitchOverride = pitchOverride;
            m_DelayOverride = delayOverride;
            m_LoopOverride = loopOverride;
            m_OutputOverride = outputOverride;
            m_StereoPanOverride = default;
            m_SpatialBlendOverride = default;
            m_ReverbZoneOverride = default;
        }

        /// <summary>
        /// Overrides the audio parameter if necessary.
        /// </summary>
        /// <param name="audioSource">The AudioSource that may be overridden.</param>
        public void ModifyAudioSource(AudioSource audioSource)
        {
            if (m_OutputOverride != null) {
                audioSource.outputAudioMixerGroup = m_OutputOverride;
            }
            
            if (m_LoopOverride.ValueOverride != BoolOverride.Override.NoOverride) {
                audioSource.loop = m_LoopOverride.Value;
            }
            
            if (m_VolumeOverride.ValueOverride != FloatOverride.Override.NoOverride) {
                audioSource.volume = m_VolumeOverride.Value;
            }

            if (m_PitchOverride.ValueOverride != FloatOverride.Override.NoOverride) {
                audioSource.pitch = m_PitchOverride.Value;
            }

            if (m_StereoPanOverride.ValueOverride != FloatOverride.Override.NoOverride) {
                audioSource.panStereo = m_StereoPanOverride.Value;
            }
            
            if (m_SpatialBlendOverride.ValueOverride != FloatOverride.Override.NoOverride) {
                audioSource.spatialBlend = m_SpatialBlendOverride.Value;
            }
            
            if (m_ReverbZoneOverride.ValueOverride != FloatOverride.Override.NoOverride) {
                audioSource.reverbZoneMix = m_ReverbZoneOverride.Value;
            }
        }
    }

    /// <summary>
    /// Defines how an AudioClip should be played.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Opsive/Audio/Audio Config", order = 1)]
    public class AudioConfig : ScriptableObject
    {
        /// <summary>
        /// Specifies the element that should be selected within the AudioClips array.
        /// </summary>
        [Serializable]
        public enum ClipSelection
        {
            Random,     // A random location within the array.
            Sequence,   // Loops from the first to the last array element.
            Index,      // Specify the index of the clip to play, defaults to 0.
        }

        [Tooltip("An array of AudioClips that can be played.")]
        [SerializeField] protected AudioClip[] m_AudioClips;
        [Tooltip("Specifies the element that should be selected within the AudioClips array.")]
        [SerializeField] protected ClipSelection m_ClipSelection;
        [Tooltip("Prefab which contains the AudioSource that should play the audio.")]
        [SerializeField] protected GameObject m_AudioSourcePrefab;
        [Tooltip("Can the AudioSource be shared between multiple AudioClips?")]
        [SerializeField] protected bool m_ShareAudioSource = true;
        [Tooltip("Should the AudioSource be replaced by a new AudioSource?")]
        [SerializeField] protected bool m_ReplacePreviousAudioSource = false;
        [Tooltip("Should the properties be copied if an AudioSource exists on the originating GameObject?")]
        [SerializeField] protected bool m_CopyExistingAudioSourceProperties = true;
        [Tooltip("Optionally specify the AudioModifier for the AudioConfig.")]
        [SerializeField] protected AudioModifier m_AudioModifier;

        public AudioClip[] AudioClips { get => m_AudioClips; set => m_AudioClips = value; }
        public ClipSelection Selection { get => m_ClipSelection; set => m_ClipSelection = value; }
        public GameObject AudioSourcePrefab { get => m_AudioSourcePrefab; set => m_AudioSourcePrefab = value; }
        public bool ShareAudioSource { get => m_ShareAudioSource; set => m_ShareAudioSource = value; }
        public bool ReplacePreviousAudioSource { get => m_ReplacePreviousAudioSource; set => m_ReplacePreviousAudioSource = value; }
        public AudioModifier AudioModifier { get => m_AudioModifier; set => m_AudioModifier = value; }

        [NonSerialized] protected AudioSource m_AudioSource;
        [NonSerialized] protected int m_AudioClipIndex = 0;

        public AudioSource OriginalAudioSource
        {
            get
            {
                if (m_AudioSourcePrefab == null) {
                    return null;
                }
                if (m_AudioSource == null) {
                    m_AudioSource = m_AudioSourcePrefab.GetCachedComponent<AudioSource>();
                }
                return m_AudioSource;
            }
        }
        public int AudioClipIndex { get => m_AudioClipIndex; set => m_AudioClipIndex = value; }

        /// <summary>
        /// Copies the AudioSource properties from the original AudioSource to the new AudioSource.
        /// </summary>
        /// <param name="originalAudioSource">The original AudioSource to copy from.</param>
        /// <param name="newAudioSource">The AudioSource to copy to.</param>
        public virtual void CopyAudioProperties(AudioSource originalAudioSource, AudioSource newAudioSource)
        {
            AudioManager.CopyAudioProperties(originalAudioSource, newAudioSource);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public PlayResult Play(GameObject gameObject)
        {
            return AudioManager.Play(gameObject, this);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that shoud play the AudioClip.</param>
        /// <param name="audioClip">The AudioClip that should be played.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public PlayResult Play(GameObject gameObject, AudioClip audioClip)
        {
            return AudioManager.Play(gameObject, new AudioClipInfo(audioClip, this));
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that shoud play the AudioClip.</param>
        /// <param name="clipIndex">The index of the AudioClip that should be played.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public PlayResult Play(GameObject gameObject, int clipIndex)
        {
            return AudioManager.Play(gameObject, new AudioClipInfo(clipIndex, this));
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that shoud play the AudioClip.</param>
        /// <param name="audioClip">The AudioClip that should be played.</param>
        /// <param name="audioModifier">The AudioModifier that should override the AudioSource settings.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public PlayResult Play(GameObject gameObject, AudioClip audioClip, AudioModifier audioModifier)
        {
            return AudioManager.Play(gameObject, new AudioClipInfo(audioClip, this, audioModifier));
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that shoud play the AudioClip.</param>
        /// <param name="clipIndex">The index of the AudioClip that should be played.</param>
        /// <param name="audioModifier">The AudioModifier that should override the AudioSource settings.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public PlayResult Play(GameObject gameObject, int clipIndex, AudioModifier audioModifier)
        {
            return AudioManager.Play(gameObject, new AudioClipInfo(clipIndex, this, audioModifier));
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="audioSourceGroup">The AudioSourceGroup that the AudioClip should be played from.</param>
        /// <param name="audioClip">The AudioClip that should be played.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public PlayResult Play(AudioSourceGroup audioSourceGroup, AudioClip audioClip)
        {
            var audioSource = GetNextAudioSource(audioSourceGroup);

            return Play(audioSource, new AudioClipInfo(audioClip, this));
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="audioSourceGroup">The AudioSourceGroup that the AudioClip should be played from.</param>
        /// <param name="audioClipInfo">The AudioClipInfo that should be played.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public PlayResult Play(AudioSourceGroup audioSourceGroup, AudioClipInfo audioClipInfo)
        {
            var audioSource = GetNextAudioSource(audioSourceGroup);

            return Play(audioSource, audioClipInfo);
        }

        /// <summary>
        /// Returns the next AudioClip within the AudioClips array.
        /// </summary>
        /// <param name="clipIndex">The clip index within the AudioClip Array.</param>
        /// <returns>The next AudioClip within the AudioClips array (can be null).</returns>
        private AudioClip GetNextAudioClip(int clipIndex)
        {
            if (m_AudioClips == null || m_AudioClips.Length == 0) {
                return null;
            }

            if (clipIndex >= 0 && clipIndex < m_AudioClips.Length) {
                return m_AudioClips[clipIndex];
            }

            switch (m_ClipSelection) {
                case ClipSelection.Random:
                    return m_AudioClips[Random.Range(0, m_AudioClips.Length)];
                case ClipSelection.Sequence:
                    if (m_AudioClipIndex < 0 || m_AudioClipIndex >= m_AudioClips.Length) {
                        m_AudioClipIndex = 0;
                    }

                    var index = m_AudioClipIndex;
                    m_AudioClipIndex++;
                    return m_AudioClips[index];
                case ClipSelection.Index:
                    return m_AudioClips[m_AudioClipIndex];
            }

            return null;
        }

        /// <summary>
        /// Returns the AudioClip for the index provided within the AudioClips array.
        /// </summary>
        /// <returns>The AudioClip for the index provided within the AudioClips array (can be null).</returns>
        public AudioClip GetAudioClip(int clipIndex)
        {
            if (m_AudioClips == null || m_AudioClips.Length == 0) {
                return null;
            }

            if (clipIndex < 0 || clipIndex >= m_AudioClips.Length) {
                return null;
            }

            return m_AudioClips[clipIndex];
        }

        /// <summary>
        /// Plays the audio clip at the specified position.
        /// </summary>
        /// <param name="position">The position that the clip should be played at.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public virtual PlayResult PlayAtPosition(Vector3 position)
        {
            return AudioManager.PlayAtPosition(GetNextAudioClip(-1), this, position);
        }

        /// <summary>
        /// Plays the audio clip at the specified position.
        /// </summary>
        /// <param name="audioClip">The clip that should be played.</param>
        /// <param name="position">The position that the clip should be played at.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public virtual PlayResult PlayAtPosition(AudioClip audioClip, Vector3 position)
        {
            return AudioManager.PlayAtPosition(audioClip, this, position);
        }

        /// <summary>
        /// Plays the AudioClipInfo on the specified AudioSource.
        /// </summary>
        /// <param name="audioSource">The AudioSource that should play the AudioClip.</param>
        /// <param name="audioClipInfo">The AudioClipInfo that should be played.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public virtual PlayResult Play(AudioSource audioSource, AudioClipInfo audioClipInfo)
        {
            if (audioClipInfo.AudioClip == null) {
                audioClipInfo = new AudioClipInfo(audioClipInfo, GetNextAudioClip(audioClipInfo.ClipIndex));
            }

            audioSource.clip = audioClipInfo.AudioClip;
            if (!m_CopyExistingAudioSourceProperties) {
                CopyAudioProperties(OriginalAudioSource, audioSource);
            }

            m_AudioModifier.ModifyAudioSource(audioSource);
            audioClipInfo.AudioModifier.ModifyAudioSource(audioSource);

            if (audioClipInfo.AudioModifier.DelayOverride.ValueOverride != FloatOverride.Override.NoOverride) {
                audioSource.PlayDelayed(audioClipInfo.AudioModifier.DelayOverride.Value);
            } else if (m_AudioModifier.DelayOverride.ValueOverride != FloatOverride.Override.NoOverride) {
                audioSource.PlayDelayed(m_AudioModifier.DelayOverride.Value);
            } else {
                audioSource.Play();
            }

            return new PlayResult(audioSource, audioClipInfo);
        }

        /// <summary>
        /// Returns the next AudioSource within the AudioSourceGroup.
        /// </summary>
        /// <param name="audioSourceGroup">The AudioSourceGroup that should retrieve the AudioSource.</param>
        /// <returns>The found AudioSource.</returns>
        protected virtual AudioSource GetNextAudioSource(AudioSourceGroup audioSourceGroup)
        {
            AudioSource audioSource = null;
            if (m_ReplacePreviousAudioSource) {
                audioSource = GetActiveAudioSource(audioSourceGroup);
            }
            if (audioSource == null) {
                audioSource = GetAvailableAudioSource(audioSourceGroup);
            }

            return audioSource;
        }

        /// <summary>
        /// Returns the AudioSource that is currently playing within the AudioSourceGroup.
        /// </summary>
        /// <param name="audioSourceGroup">The AudioSourceGroup that contains the AudioSource.</param>
        /// <returns>The found AudioSource.</returns>
        public AudioSource GetActiveAudioSource(AudioSourceGroup audioSourceGroup)
        {
            if (m_ShareAudioSource) {
                for (int i = 0; i < audioSourceGroup.SharedAudioSources.Count; i++) {
                    var sharedAudioSource = audioSourceGroup.SharedAudioSources[i];
                    if (sharedAudioSource.isPlaying) {
                        return sharedAudioSource;
                    }
                }
            } else if (audioSourceGroup.AudioSourceListByAudioConfig.TryGetValue(this, out var linkedAudioSources)) {
                for (int i = 0; i < linkedAudioSources.Count; i++) {
                    if (linkedAudioSources[i].isPlaying) {
                        return linkedAudioSources[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the next AudioSource that is not playing.
        /// </summary>
        /// <param name="audioSourceGroup">The AudioSourceGroup that the AudioClip should be played from.</param>
        /// <returns>The next AudioSource that is not playing.</returns>
        public AudioSource GetAvailableAudioSource(AudioSourceGroup audioSourceGroup)
        {
            if (m_ShareAudioSource) {
                for (int i = 0; i < audioSourceGroup.SharedAudioSources.Count; i++) {
                    var sharedAudioSource = audioSourceGroup.SharedAudioSources[i];
                    if (sharedAudioSource.isPlaying == false) {
                        return sharedAudioSource;
                    }
                }
            } else if (audioSourceGroup.AudioSourceListByAudioConfig.TryGetValue(this, out var linkedAudioSources)) {
                for (int i = 0; i < linkedAudioSources.Count; i++) {
                    if (linkedAudioSources[i].isPlaying == false) {
                        return linkedAudioSources[i];
                    }
                }
            } else {
                linkedAudioSources = new List<AudioSource>();
                audioSourceGroup.AudioSourceListByAudioConfig.Add(this, linkedAudioSources);
            }

            // No existing AudioSource is available. Create a new one.
            var audioSource = AddAudioSource(audioSourceGroup);
            // If an AudioSource exists on the GameObject then the properties should be copied.
            if (m_CopyExistingAudioSourceProperties) {
                var existingAudioSource = audioSourceGroup.GameObject.GetCachedComponent<AudioSource>();
                if (existingAudioSource != null) {
                    AudioManager.CopyAudioProperties(existingAudioSource, audioSource);
                }
            }
            return audioSource;
        }

        /// <summary>
        /// Adds a new AudioSource to the AudioSourceGroup.
        /// </summary>
        /// <param name="audioSourceGroup">The AudioSourceGroup that the AudioSource should be added to.</param>
        /// <returns>The new AudioSource.</returns>
        protected virtual AudioSource AddAudioSource(AudioSourceGroup audioSourceGroup)
        {
            GameObject audioGameObject;
            var audioPrefab = m_AudioSourcePrefab;
            if (audioPrefab == null) {
                audioPrefab = AudioManager.GetAudioManagerModule().DefaultAudioConfig.AudioSourcePrefab;
                if (audioPrefab == null) {
                    Debug.LogWarning("The AudioManager Module is missing a Default Audio Source Prefab in the Default Audio Config.");
                    audioPrefab = new GameObject("AudioSource").AddComponent<AudioSource>().gameObject;
                    audioPrefab.GetComponent<AudioSource>().playOnAwake = false;
                }
            }

            audioGameObject = ObjectPoolBase.Instantiate(audioPrefab, audioSourceGroup.GameObject.transform);
            audioGameObject.name = m_ShareAudioSource ? "Shared Audio Source" : "Reserved Audio Source: " + name;
            var newAudioSource = audioGameObject.GetComponent<AudioSource>();
            if (newAudioSource == null) {
                Debug.LogWarning("The Audio Source Prefab is missing an Audio Source.", this);
                newAudioSource = audioGameObject.AddComponent<AudioSource>();
                newAudioSource.playOnAwake = false;
            }

            // The new AudioSource may be active.
            newAudioSource.Stop();

            audioSourceGroup.AllAudioSources.Add(newAudioSource);
            if (m_ShareAudioSource) {
                audioSourceGroup.SharedAudioSources.Add(newAudioSource);
            }
            if (audioSourceGroup.AudioSourceListByAudioConfig.TryGetValue(this, out var linkedAudioSources)) {
                linkedAudioSources.Add(newAudioSource);
            } else {
                linkedAudioSources = new List<AudioSource>();
                audioSourceGroup.AudioSourceListByAudioConfig.Add(this, linkedAudioSources);
                linkedAudioSources.Add(newAudioSource);
            }

            // Return the new AudioSource.
            return newAudioSource;
        }
    }
}