// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This is an abstract base typewriter class. It's the ancestor of 
    /// UnityUITypewriterEffect and TextMeshProTypewriterEffect.
    /// </summary>
    public abstract class AbstractTypewriterEffect : MonoBehaviour
    {

        /// <summary>
        /// Set `true` to type right to left.
        /// </summary>
        [Tooltip("Tick for right-to-left text such as Arabic.")]
        public bool rightToLeft = false;

        /// <summary>
        /// How fast to "type."
        /// </summary>
        [Tooltip("How fast to type. This is separate from Dialogue Manager > Subtitle Settings > Chars Per Second.")]
        public float charactersPerSecond = 50;

        /// <summary>
        /// The audio clip to play with each character.
        /// </summary>
        [Tooltip("Optional audio clip to play with each character.")]
        public AudioClip audioClip = null;

        /// <summary>
        /// If specified, randomly use these clips or the main Audio Clip.
        /// </summary>
        [Tooltip("If specified, randomly use these clips or the main Audio Clip.")]
        public AudioClip[] alternateAudioClips = new AudioClip[0];

        /// <summary>
        /// The audio source through which to play the clip. If unassigned, will look for an
        /// audio source on this GameObject.
        /// </summary>
        [Tooltip("Optional audio source through which to play the clip.")]
        public AudioSource audioSource = null;

        [Tooltip("Use AudioSource.PlayOneShot instead of Play. Slightly heavier performance but produces different effect.")]
        public bool usePlayOneShot = false;

        /// <summary>
        /// If audio clip is still playing from previous character, stop and restart it when typing next character.
        /// </summary>
        [Tooltip("If audio clip is still playing from previous character, stop and restart it when typing next character.")]
        public bool interruptAudioClip = false;

        [Tooltip("Stop audio when typing any of the Silent Characters specified below.")]
        public bool stopAudioOnSilentCharacters = false;

        [Tooltip("Stop audio when upon reaching a pause code.")]
        public bool stopAudioOnPauseCodes = false;

        /// <summary>
        /// Don't play audio on these characters.
        /// </summary>
        [Tooltip("Don't play audio on these characters.")]
        public string silentCharacters = string.Empty;

        /// <summary>
        /// Play a full pause on these characters.
        /// </summary>
        [Tooltip("Play a full pause on these characters.")]
        public string fullPauseCharacters = string.Empty;

        /// <summary>
        /// Play a quarter pause on these characters.
        /// </summary>
        [Tooltip("Play a quarter pause on these characters.")]
        public string quarterPauseCharacters = string.Empty;

        /// <summary>
        /// Duration to pause on when text contains '\\.'
        /// </summary>
        [Tooltip("Duration to pause on when text contains '\\.'")]
        public float fullPauseDuration = 1f;

        /// <summary>
        /// Duration to pause when text contains '\\,'
        /// </summary>
        [Tooltip("Duration to pause when text contains '\\,'")]
        public float quarterPauseDuration = 0.25f;

        /// <summary>
        /// Ensures this GameObject has only one typewriter effect.
        /// </summary>
        [Tooltip("Ensure this GameObject has only one typewriter effect.")]
        public bool removeDuplicateTypewriterEffects = true;

        /// <summary>
        /// Play using the current text content whenever component is enabled.
        /// </summary>
        [Tooltip("Play using the current text content whenever component is enabled.")]
        public bool playOnEnable = true;

        /// <summary>
        /// Wait one frame to allow layout elements to setup first.
        /// </summary>
        [Tooltip("Wait one frame to allow layout elements to setup first.")]
        public bool waitOneFrameBeforeStarting = false;

        /// <summary>
        /// Stop typing when the conversation ends.
        /// </summary>
        [Tooltip("Stop typing when the conversation ends.")]
        public bool stopOnConversationEnd = false;

        public abstract bool isPlaying { get; }

        protected bool paused = false;

        /// <summary>
        /// Returns the typewriter's charactersPerSecond.
        /// </summary>
        public virtual float GetSpeed()
        {
            return charactersPerSecond;
        }

        /// <summary>
        /// Sets the typewriter's charactersPerSecond. Takes effect the next time the typewriter is used.
        /// </summary>
        public virtual void SetSpeed(float charactersPerSecond)
        {
            this.charactersPerSecond = charactersPerSecond;
        }

        public abstract void Awake();

        public abstract void Start();

        public virtual void OnEnable()
        {
            if (stopOnConversationEnd && DialogueManager.hasInstance)
            {
                DialogueManager.instance.conversationEnded -= StopOnConversationEnd;
                DialogueManager.instance.conversationEnded += StopOnConversationEnd;
            }
        }

        public virtual void OnDisable()
        {
            if (stopOnConversationEnd && DialogueManager.hasInstance)
            {
                DialogueManager.instance.conversationEnded -= StopOnConversationEnd;
            }
        }

        public virtual void StopOnConversationEnd(Transform actor)
        {
            if (isPlaying) Stop();
        }

        public abstract void Stop();

        public abstract void StartTyping(string text, int fromIndex = 0);

        public abstract void StopTyping();
        
        public static string StripRPGMakerCodes(string s) // Moved to UITools, but kept for compatibility with third party code.
        {
            return UITools.StripRPGMakerCodes(s);
        }

        protected bool IsFullPauseCharacter(char c)
        {
            return IsCharacterInString(c, fullPauseCharacters);
        }

        protected bool IsQuarterPauseCharacter(char c)
        {
            return IsCharacterInString(c, quarterPauseCharacters);
        }

        protected bool IsSilentCharacter(char c)
        {
            return IsCharacterInString(c, silentCharacters);
        }

        protected bool IsCharacterInString(char c, string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == c) return true;
            }
            return false;
        }

        public virtual void StopCharacterAudio()
        {
            if (audioSource != null) audioSource.Stop();
        }

        protected virtual void PlayCharacterAudio(char c)
        {
            PlayCharacterAudio();
        }

        protected virtual void PlayCharacterAudio()
        {
            if (audioClip == null || audioSource == null) return;
            AudioClip randomClip = null;
            if (alternateAudioClips != null && alternateAudioClips.Length > 0)
            {
                var randomIndex = UnityEngine.Random.Range(0, alternateAudioClips.Length + 1);
                randomClip = (randomIndex < alternateAudioClips.Length) ? alternateAudioClips[randomIndex] : audioClip;
            }
            if (interruptAudioClip)
            {
                if (usePlayOneShot)
                {
                    if (randomClip != null) audioSource.clip = randomClip;
                    audioSource.PlayOneShot(audioSource.clip);
                }
                else
                {
                    if (audioSource.isPlaying) audioSource.Stop();
                    if (randomClip != null) audioSource.clip = randomClip;
                    audioSource.Play();
                }
            }
            else
            {
                if (!audioSource.isPlaying)
                {
                    if (randomClip != null) audioSource.clip = randomClip;
                    if (usePlayOneShot)
                    {
                        audioSource.PlayOneShot(audioSource.clip);
                    }
                    else
                    {
                        audioSource.Play();
                    }
                }
            }
        }

        protected virtual IEnumerator PauseForDuration(float duration)
        {
            paused = true;
            if (stopAudioOnPauseCodes && audioSource != null) audioSource.Stop();
            var continueTime = DialogueTime.time + duration;
            int pauseSafeguard = 0;
            while (DialogueTime.time < continueTime && pauseSafeguard < 999)
            {
                pauseSafeguard++;
                yield return null;
            }
            paused = false;
        }

    }

}
