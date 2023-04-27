/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Audio
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// The AudioManager manages the audio to ensure to ensure no two clips are playing on the same AudioSource at the same time.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager s_Instance;
        private static AudioManager Instance
        {
            get
            {
                if (!s_Initialized) {
                    s_Instance = new GameObject("Audio Manager").AddComponent<AudioManager>();
                    s_Instance.Initialize(false);
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;

        [Tooltip("A reference to the AudioManagerModule used by the AudioManager.")]
        [SerializeField] protected AudioManagerModule m_AudioManagerModule;

        public AudioManagerModule AudioManagerModule { get { return m_AudioManagerModule; } set { m_AudioManagerModule = value; } }

        private GameObject m_GameObject;

        /// <summary>
        /// The object has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // The object may have been enabled outside of the scene unloading.
            if (s_Instance == null) {
                s_Instance = this;
                Initialize(false);
                SceneManager.sceneUnloaded -= SceneUnloaded;
            }
        }

        /// <summary>
        /// The AudioManager can also play audio clips if the target GameObject is being disabled.
        /// </summary>
        private void Start()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initialize the Audio Manager.
        /// </summary>
        protected virtual void Initialize(bool force)
        {
            if (s_Initialized && !force) { return; }

            s_Initialized = true;
            
            m_GameObject = gameObject;
            if (m_AudioManagerModule == null) {
                m_AudioManagerModule = ScriptableObject.CreateInstance<AudioManagerModule>();
            }

            if (m_AudioManagerModule.DefaultAudioConfig == null) {
                m_AudioManagerModule.DefaultAudioConfig = ScriptableObject.CreateInstance<AudioConfig>();
            }

            if (m_AudioManagerModule.DefaultAudioConfig.AudioSourcePrefab == null) {
                var audioSourceGameObject = new GameObject("AudioSource");
                audioSourceGameObject.transform.parent = m_GameObject.transform;
                var audioSource = audioSourceGameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1;
                m_AudioManagerModule.DefaultAudioConfig.AudioSourcePrefab = audioSourceGameObject;
            }
        }

        /// <summary>
        /// Get the Audio Manager Module.
        /// </summary>
        /// <returns></returns>
        public static AudioManagerModule GetAudioManagerModule()
        {
            return Instance.AudioManagerModule;
        }
        
        /// <summary>
        /// Set the Audio Manager Module.
        /// </summary>
        /// <param name="audioManagerModule">The audio Manager Module to set.</param>
        public static void SetAudioManagerModule(AudioManagerModule audioManagerModule)
        {
            Instance.AudioManagerModule  = audioManagerModule;
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult Play(GameObject gameObject, AudioClip clip)
        {
            return Instance.PlayInternal(gameObject, clip, -1, -1, false, -1);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="loop">Does the clip loop?</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult Play(GameObject gameObject, AudioClip clip, bool loop)
        {
            return Instance.PlayInternal(gameObject, clip, -1, -1, loop, -1);
        }

        /// <summary>
        /// Plays the audio clip with the specified delay.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="delay">The number of seconds to delay the clip from playing.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult PlayDelayed(GameObject gameObject, AudioClip clip, float delay)
        {
            return Instance.PlayInternal(gameObject, clip, -1, -1, false, delay);
        }

        /// <summary>
        /// Plays the audio clip with the specified delay.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="loop">Does the clip loop?</param>
        /// <param name="delay">The number of seconds to delay the clip from playing.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult PlayDelayed(GameObject gameObject, AudioClip clip, float delay, bool loop)
        {
            return Instance.PlayInternal(gameObject, clip, -1, -1, loop, delay);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult Play(GameObject gameObject, AudioClip clip, float pitch)
        {
            return Instance.PlayInternal(gameObject, clip, -1, pitch, false, -1);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <param name="loop">Does the clip loop?</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult Play(GameObject gameObject, AudioClip clip, float pitch, bool loop)
        {
            return Instance.PlayInternal(gameObject, clip, -1, pitch, loop, -1);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="volume">The volume to play the clip at.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult Play(GameObject gameObject, AudioClip clip, float volume, float pitch)
        {
            return Instance.PlayInternal(gameObject, clip, volume, pitch, false, -1);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="volume">The volume to play the clip at.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <param name="delay">The number of seconds to delay the clip from playing.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult Play(GameObject gameObject, AudioClip clip, float volume, float pitch, float delay)
        {
            return Instance.PlayInternal(gameObject, clip, volume, pitch, false, delay);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="audioConfig">The Audio Config.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult Play(GameObject gameObject, AudioClip clip, AudioConfig audioConfig)
        {
            return Instance.PlayInternal(gameObject, new AudioClipInfo(clip, audioConfig));
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="audioConfig">The AudioConfig that should be played.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult Play(GameObject gameObject, AudioConfig audioConfig)
        {
            return Instance.PlayInternal(gameObject, new AudioClipInfo(null, audioConfig));
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that shoud play the AudioClip.</param>
        /// <param name="clipInfo">The AudioClip and AudioConfig used to play the audio.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult Play(GameObject gameObject, AudioClipInfo clipInfo)
        {
            return Instance.PlayInternal(gameObject, clipInfo);
        }

        /// <summary>
        /// Internal method which plays the audio clip.
        /// </summary>
        /// <param name="playGameObject">The GameObject that should play the AudioClip.</param>
        /// <param name="clipInfo">The AudioClip and AudioConfig used to play the audio.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        protected virtual PlayResult PlayInternal(GameObject playGameObject, AudioClipInfo clipInfo)
        {
            if (playGameObject == null) {
                playGameObject = m_GameObject;
            }
            return m_AudioManagerModule.PlayAudio(playGameObject, clipInfo);
        }

        /// <summary>
        /// Internal method which plays the audio clip.
        /// </summary>
        /// <param name="playGameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="volume">The volume to play the clip at.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <param name="loop">Does the clip loop?</param>
        /// <param name="delay">The number of seconds to delay the clip from playing.</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        protected virtual PlayResult PlayInternal(GameObject playGameObject, AudioClip clip, float volume, float pitch, bool loop, float delay)
        {
            var audioModifier = new AudioModifier();
            audioModifier.LoopOverride = new BoolOverride(BoolOverride.Override.Constant, loop);
            if (volume > -1) { audioModifier.VolumeOverride = new FloatOverride(FloatOverride.Override.Constant, volume); }
            if (pitch > -1) { audioModifier.PitchOverride = new FloatOverride(FloatOverride.Override.Constant, pitch); }
            if (delay > -1) { audioModifier.DelayOverride = new FloatOverride(FloatOverride.Override.Constant, delay); }
            return PlayInternal(playGameObject, new AudioClipInfo(clip, null, audioModifier));
        }

        /// <summary>
        /// Plays the audio clip at the specified position.
        /// </summary>
        /// <param name="clip">The clip that should be played.</param>
        /// <param name="position">The position that the clip should be played at.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult PlayAtPosition(AudioClip clip, Vector3 position)
        {
            return Instance.PlayAtPositionInternal(clip, position, -1, -1);
        }

        /// <summary>
        /// Plays the audio clip at the specified position.
        /// </summary>
        /// <param name="clip">The clip that should be played.</param>
        /// <param name="position">The position that the clip should be played at.</param>
        /// <param name="volume">The volume to play the clip at.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult PlayAtPosition(AudioClip clip, Vector3 position, float volume, float pitch)
        {
            return Instance.PlayAtPositionInternal(clip, position, volume, pitch);
        }

        /// <summary>
        /// Plays the audio clip at the specified position.
        /// </summary>
        /// <param name="clip">The clip that should be played.</param>
        /// <param name="audioConfig">Defines how the AudioClip should be played.</param>
        /// <param name="position">The position that the clip should be played at.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult PlayAtPosition(AudioClip clip, AudioConfig audioConfig, Vector3 position)
        {
            return Instance.PlayAtPositionInternal(Instance.m_GameObject, new AudioClipInfo(clip, audioConfig), position);
        }

        /// <summary>
        /// Plays the audio clip at the specified position.
        /// </summary>
        /// <param name="audioConfig">Defines how the AudioClip should be played.</param>
        /// <param name="position">The position that the clip should be played at.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult PlayAtPosition(AudioConfig audioConfig, Vector3 position)
        {
            return Instance.PlayAtPositionInternal(Instance.m_GameObject, new AudioClipInfo(null, audioConfig), position);
        }

        /// <summary>
        /// Plays the audio clip at the specified position on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject that shoud play the clip.</param>
        /// <param name="clip">The clip that should be played.</param>
        /// <param name="audioConfig">Defines how the AudioClip should be played.</param>
        /// <param name="position">The position that the clip should be played at.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        public static PlayResult PlayAtPosition(GameObject gameObject, AudioClip clip, AudioConfig audioConfig, Vector3 position)
        {
            return Instance.PlayAtPositionInternal(gameObject, new AudioClipInfo(clip, audioConfig), position);
        }

        /// <summary>
        /// Internal method which plays the audio clip at the specified position on the specified GameObject.
        /// </summary>
        /// <param name="playGameObject">The GameObject that shoud play the clip.</param>
        /// <param name="clipInfo">The AudioClip and config that should be played.</param>
        /// <param name="position">The position that the clip should be played at.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        protected virtual PlayResult PlayAtPositionInternal(GameObject playGameObject, AudioClipInfo clipInfo, Vector3 position)
        {
            if (playGameObject == null) {
                playGameObject = m_GameObject;
            }
            return m_AudioManagerModule.PlayAtPosition(playGameObject, clipInfo, position);
        }

        /// <summary>
        /// Internal method which plays the audio clip at the specified position.
        /// </summary>
        /// <param name="clip">The clip that should be played.</param>
        /// <param name="position">The position that the clip should be played at.</param>
        /// <param name="volume">The volume to play the clip at.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <returns>The result of playing the AudioClip.</returns>
        protected virtual PlayResult PlayAtPositionInternal(AudioClip clip, Vector3 position, float volume, float pitch)
        {
            var audioModifier = new AudioModifier();
            if (volume > -1) { audioModifier.VolumeOverride = new FloatOverride(FloatOverride.Override.Constant, volume); }
            if (pitch > -1) { audioModifier.PitchOverride = new FloatOverride(FloatOverride.Override.Constant, pitch); }
            return m_AudioManagerModule.PlayAtPosition(m_GameObject, new AudioClipInfo(clip, null, audioModifier), position);
        }

        /// <summary>
        /// Stops any playing audio on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to stop the audio on.</param>
        public static AudioSource Stop(GameObject gameObject)
        {
            return Instance.StopInternal(gameObject, null);
        }

        /// <summary>
        /// Stops any playing audio on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to stop the audio on.</param>
        /// <param name="audioConfig">The AudioConfig that should be stopped.</param>
        /// <returns>The AudioSource that was stopped.</returns>
        public static AudioSource Stop(GameObject gameObject, AudioConfig audioConfig)
        {
            return Instance.StopInternal(gameObject, audioConfig);
        }

        /// <summary>
        /// Internal method which stops any playing audio on the specified GameObject and AudioConfig.
        /// </summary>
        /// <param name="playGameObject">The GameObject to stop the audio on.</param>
        /// <param name="audioConfig">The AudioConfig that should be stopped.</param>
        private AudioSource StopInternal(GameObject playGameObject, AudioConfig audioConfig)
        {
            return m_AudioManagerModule.Stop(playGameObject, audioConfig);
        }

        /// <summary>
        /// Internal method which stops any playing audio on the specified GameObject and AudioConfig.
        /// </summary>
        /// <param name="gameObject">The GameObject to stop the audio on.</param>
        /// <param name="playResult">The PlayResult from when the audio was played.</param>
        public static AudioSource Stop(GameObject gameObject, PlayResult playResult)
        {
            return Instance.StopInternal(gameObject, playResult);
        }

        /// <summary>
        /// Internal method which stops any playing audio on the specified GameObject and AudioConfig.
        /// </summary>
        /// <param name="playGameObject">The GameObject to stop the audio on.</param>
        /// <param name="playResult">The PlayResult from when the audio was played.</param>
        private AudioSource StopInternal(GameObject playGameObject, PlayResult playResult)
        {
            return m_AudioManagerModule.Stop(playGameObject, playResult);
        }

        /// <summary>
        /// Copies the AudioSource properties from the original AudioSource to the new AudioSource.
        /// </summary>
        /// <param name="originalAudioSource">The original AudioSource to copy from.</param>
        /// <param name="newAudioSource">The AudioSource to copy to.</param>
        public static void CopyAudioProperties(AudioSource originalAudioSource, AudioSource newAudioSource)
        {
            if (originalAudioSource == null || newAudioSource == null) {
                return;
            }
            newAudioSource.bypassEffects = originalAudioSource.bypassEffects;
            newAudioSource.bypassListenerEffects = originalAudioSource.bypassListenerEffects;
            newAudioSource.bypassReverbZones = originalAudioSource.bypassReverbZones;
            newAudioSource.dopplerLevel = originalAudioSource.dopplerLevel;
            newAudioSource.ignoreListenerPause = originalAudioSource.ignoreListenerPause;
            newAudioSource.ignoreListenerVolume = originalAudioSource.ignoreListenerVolume;
            newAudioSource.loop = originalAudioSource.loop;
            newAudioSource.maxDistance = originalAudioSource.maxDistance;
            newAudioSource.minDistance = originalAudioSource.minDistance;
            newAudioSource.mute = originalAudioSource.mute;
            newAudioSource.outputAudioMixerGroup = originalAudioSource.outputAudioMixerGroup;
            newAudioSource.panStereo = originalAudioSource.panStereo;
            newAudioSource.playOnAwake = originalAudioSource.playOnAwake;
            newAudioSource.pitch = originalAudioSource.pitch;
            newAudioSource.priority = originalAudioSource.priority;
            newAudioSource.reverbZoneMix = originalAudioSource.reverbZoneMix;
            newAudioSource.rolloffMode = originalAudioSource.rolloffMode;
            newAudioSource.spatialBlend = originalAudioSource.spatialBlend;
            newAudioSource.spatialize = originalAudioSource.spatialize;
            newAudioSource.spatializePostEffects = originalAudioSource.spatializePostEffects;
            newAudioSource.spread = originalAudioSource.spread;
            newAudioSource.velocityUpdateMode = originalAudioSource.velocityUpdateMode;
            newAudioSource.volume = originalAudioSource.volume;
        }

        /// <summary>
        /// Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        private void SceneUnloaded(Scene scene)
        {
            s_Initialized = false;
            s_Instance = null;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
        }

        /// <summary>
        /// Reset the static variables for domain reloading.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReset()
        {
            if (s_Initialized && s_Instance.m_AudioManagerModule != null) {
                s_Instance.m_AudioManagerModule.DomainReset();
            }
            s_Initialized = false;
            s_Instance = null;
        }
    }
}