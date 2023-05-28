// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{

#if TMP_PRESENT

    /// <summary>
    /// This is a typewriter effect for TextMesh Pro.
    /// 
    /// Note: Handles RPGMaker codes, but not two codes next to each other.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    [DisallowMultipleComponent]
    public class TextMeshProTypewriterEffect : AbstractTypewriterEffect
    {

        [System.Serializable]
        public class AutoScrollSettings
        {
            [Tooltip("Automatically scroll to bottom of scroll rect. Useful for long text. Works best with left justification.")]
            public bool autoScrollEnabled = false;
            public UnityEngine.UI.ScrollRect scrollRect = null;
            [Tooltip("Optional. Add a UIScrollBarEnabler to main dialogue panel, assign UI elements, then assign it here to automatically enable scrollbar if content is taller than viewport.")]
            public UIScrollbarEnabler scrollbarEnabler = null;
        }

        /// <summary>
        /// Optional auto-scroll settings.
        /// </summary>
        public AutoScrollSettings autoScrollSettings = new AutoScrollSettings();

        public UnityEvent onBegin = new UnityEvent();
        public UnityEvent onCharacter = new UnityEvent();
        public UnityEvent onEnd = new UnityEvent();

        /// <summary>
        /// Indicates whether the effect is playing.
        /// </summary>
        /// <value><c>true</c> if this instance is playing; otherwise, <c>false</c>.</value>
        public override bool isPlaying { get { return typewriterCoroutine != null; } }

        /// @cond FOR_V1_COMPATIBILITY
        public bool IsPlaying { get { return isPlaying; } }
        /// @endcond

        protected const string RPGMakerCodeQuarterPause = @"\,";
        protected const string RPGMakerCodeFullPause = @"\.";
        protected const string RPGMakerCodeSkipToEnd = @"\^";
        protected const string RPGMakerCodeInstantOpen = @"\>";
        protected const string RPGMakerCodeInstantClose = @"\<";

        protected enum RPGMakerTokenType
        {
            None,
            QuarterPause,
            FullPause,
            SkipToEnd,
            InstantOpen,
            InstantClose
        }

        protected Dictionary<int, List<RPGMakerTokenType>> rpgMakerTokens = new Dictionary<int, List<RPGMakerTokenType>>();

        protected TMPro.TMP_Text m_textComponent = null;
        protected TMPro.TMP_Text textComponent
        {
            get
            {
                if (m_textComponent == null) m_textComponent = GetComponent<TMPro.TMP_Text>();
                return m_textComponent;
            }
        }

        protected LayoutElement m_layoutElement = null;
        protected LayoutElement layoutElement
        {
            get
            {
                if (m_layoutElement == null)
                {
                    m_layoutElement = GetComponent<LayoutElement>();
                    if (m_layoutElement == null) m_layoutElement = gameObject.AddComponent<LayoutElement>();
                }
                return m_layoutElement;
            }
        }

        protected AudioSource runtimeAudioSource
        {
            get
            {
                if (audioSource == null) audioSource = GetComponent<AudioSource>();
                if (audioSource == null && (audioClip != null))
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.panStereo = 0;
                }
                return audioSource;
            }
        }

        protected bool started = false;
        protected int charactersTyped = 0;
        protected Coroutine typewriterCoroutine = null;
        protected MonoBehaviour coroutineController = null;

        public override void Awake()
        {

            if (removeDuplicateTypewriterEffects) RemoveIfDuplicate();
        }

        protected void RemoveIfDuplicate()
        {
            var effects = GetComponents<TextMeshProTypewriterEffect>();
            if (effects.Length > 1)
            {
                var keep = effects[0];
                for (int i = 1; i < effects.Length; i++)
                {
                    if (effects[i].GetInstanceID() < keep.GetInstanceID())
                    {
                        keep = effects[i];
                    }
                }
                for (int i = 0; i < effects.Length; i++)
                {
                    if (effects[i] != keep)
                    {
                        Destroy(effects[i]);
                    }
                }
            }
        }

        public override void Start()
        {
            if (!IsPlaying && playOnEnable)
            {
                StopTypewriterCoroutine();
                StartTypewriterCoroutine(0);
            }
            started = true;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            if (!IsPlaying && playOnEnable && started)
            {
                StopTypewriterCoroutine();
                StartTypewriterCoroutine(0);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            Stop();
        }

        /// <summary>
        /// Pauses the effect.
        /// </summary>
        public void Pause()
        {
            paused = true;
        }

        /// <summary>
        /// Unpauses the effect. The text will resume at the point where it
        /// was paused; it won't try to catch up to make up for the pause.
        /// </summary>
        public void Unpause()
        {
            paused = false;
        }

        public void Rewind()
        {
            charactersTyped = 0;
        }

        /// <summary>
        /// Starts typing, optionally from a starting index. Characters before the 
        /// starting index will appear immediately.
        /// </summary>
        /// <param name="text">Text to type.</param>
        /// <param name="fromIndex">Character index to start typing from.</param>
        public override void StartTyping(string text, int fromIndex = 0)
        {
            StopTypewriterCoroutine();
            textComponent.text = text;
            StartTypewriterCoroutine(fromIndex);
        }

        public override void StopTyping()
        {
            Stop();
        }

        /// <summary>
        /// Play typewriter on text immediately.
        /// </summary>
        /// <param name="text"></param>
        public virtual void PlayText(string text, int fromIndex = 0)
        {
            StopTypewriterCoroutine();
            textComponent.text = text;
            StartTypewriterCoroutine(fromIndex);
        }

        protected virtual void StartTypewriterCoroutine(int fromIndex)
        {
            if (coroutineController == null || !coroutineController.gameObject.activeInHierarchy)
            {
                // This MonoBehaviour might not be enabled yet, so use one that's guaranteed to be enabled:
                MonoBehaviour controller = GetComponentInParent<AbstractDialogueUI>();
                if (controller == null) controller = DialogueManager.instance;
                coroutineController = controller;
                if (coroutineController == null) coroutineController = this;
            }
            typewriterCoroutine = coroutineController.StartCoroutine(Play(fromIndex));
        }

        /// <summary>
        /// Plays the typewriter effect.
        /// </summary>
        public virtual IEnumerator Play(int fromIndex)
        {
            if ((textComponent != null) && (charactersPerSecond > 0))
            {
                if (waitOneFrameBeforeStarting) yield return null;
                textComponent.text = textComponent.text.Replace("<br>", "\n");
                fromIndex = StripRPGMakerCodes(Tools.StripTextMeshProTags(textComponent.text)).Substring(0, fromIndex).Length;
                ProcessRPGMakerCodes();
                if (runtimeAudioSource != null) runtimeAudioSource.clip = audioClip;
                onBegin.Invoke();
                paused = false;
                float delay = 1 / charactersPerSecond;
                float lastTime = DialogueTime.time;
                float elapsed = fromIndex / charactersPerSecond;
                textComponent.maxVisibleCharacters = fromIndex;
                textComponent.ForceMeshUpdate();
                yield return null;
                textComponent.maxVisibleCharacters = fromIndex;
                textComponent.ForceMeshUpdate();
                TMPro.TMP_TextInfo textInfo = textComponent.textInfo;
                var parsedText = textComponent.GetParsedText();
                int totalVisibleCharacters = textInfo.characterCount; // Get # of Visible Character in text object
                charactersTyped = fromIndex;
                int skippedCharacters = 0;
                while (charactersTyped < totalVisibleCharacters)
                {
                    if (!paused)
                    {
                        var deltaTime = DialogueTime.time - lastTime;
                        elapsed += deltaTime;
                        var goal = (elapsed * charactersPerSecond) + skippedCharacters;
                        while (charactersTyped < goal)
                        {
                            if (rpgMakerTokens.ContainsKey(charactersTyped))
                            {
                                var tokens = rpgMakerTokens[charactersTyped];
                                for (int i = 0; i < tokens.Count; i++)
                                {
                                    var token = tokens[i];
                                    switch (token)
                                    {
                                        case RPGMakerTokenType.QuarterPause:
                                            yield return PauseForDuration(quarterPauseDuration);
                                            break;
                                        case RPGMakerTokenType.FullPause:
                                            yield return PauseForDuration(fullPauseDuration);
                                            break;
                                        case RPGMakerTokenType.SkipToEnd:
                                            charactersTyped = totalVisibleCharacters - 1;
                                            break;
                                        case RPGMakerTokenType.InstantOpen:
                                            var close = false;
                                            while (!close && charactersTyped < totalVisibleCharacters)
                                            {
                                                charactersTyped++;
                                                skippedCharacters++;
                                                if (rpgMakerTokens.ContainsKey(charactersTyped) && rpgMakerTokens[charactersTyped].Contains(RPGMakerTokenType.InstantClose))
                                                {
                                                    close = true;
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                            var typedCharacter = (0 <= charactersTyped && charactersTyped < parsedText.Length) ? parsedText[charactersTyped] : ' ';
                            if (charactersTyped < totalVisibleCharacters)
                            {
                                if (IsSilentCharacter(typedCharacter))
                                {
                                    if (stopAudioOnSilentCharacters) StopCharacterAudio();
                                }
                                else
                                {
                                    PlayCharacterAudio(typedCharacter);
                                }
                            }
                            onCharacter.Invoke();
                            charactersTyped++;
                            textComponent.maxVisibleCharacters = charactersTyped;
                            if (IsFullPauseCharacter(typedCharacter)) yield return DialogueTime.WaitForSeconds(fullPauseDuration);
                            else if (IsQuarterPauseCharacter(typedCharacter)) yield return DialogueTime.WaitForSeconds(quarterPauseDuration);
                        }
                    }
                    textComponent.maxVisibleCharacters = charactersTyped;
                    HandleAutoScroll();
                    //---Uncomment the line below to debug: 
                    //Debug.Log(textComponent.text.Substring(0, charactersTyped).Replace("<", "[").Replace(">", "]") + " (typed=" + charactersTyped + ")");
                    lastTime = DialogueTime.time;
                    var delayTime = DialogueTime.time + delay;
                    int delaySafeguard = 0;
                    while (DialogueTime.time < delayTime && delaySafeguard < 999)
                    {
                        delaySafeguard++;
                        yield return null;
                    }
                }
            }
            Stop();
        }

        protected void ProcessRPGMakerCodes()
        {
            rpgMakerTokens.Clear();
            var source = textComponent.text;
            var result = string.Empty;
            if (!source.Contains("\\")) return;
            source = Tools.StripTextMeshProTags(source);
            int safeguard = 0;
            while (!string.IsNullOrEmpty(source) && safeguard < 9999)
            {
                safeguard++;
                RPGMakerTokenType token;
                if (PeelRPGMakerTokenFromFront(ref source, out token))
                {
                    int i = result.Length;
                    if (!rpgMakerTokens.ContainsKey(i))
                    {
                        rpgMakerTokens.Add(i, new List<RPGMakerTokenType>());
                    }
                    rpgMakerTokens[i].Add(token);
                }
                else
                {
                    result += source[0];
                    source = source.Remove(0, 1);
                }
            }
            textComponent.text = Regex.Replace(textComponent.text, @"\\[\.\,\^\<\>]", string.Empty);
        }

        protected bool PeelRPGMakerTokenFromFront(ref string source, out RPGMakerTokenType token)
        {
            token = RPGMakerTokenType.None;
            if (string.IsNullOrEmpty(source) || source.Length < 2 || source[0] != '\\') return false;
            var s = source.Substring(0, 2);
            if (string.Equals(s, RPGMakerCodeQuarterPause))
            {
                token = RPGMakerTokenType.QuarterPause;
            }
            else if (string.Equals(s, RPGMakerCodeFullPause))
            {
                token = RPGMakerTokenType.FullPause;
            }
            else if (string.Equals(s, RPGMakerCodeSkipToEnd))
            {
                token = RPGMakerTokenType.SkipToEnd;
            }
            else if (string.Equals(s, RPGMakerCodeInstantOpen))
            {
                token = RPGMakerTokenType.InstantOpen;
            }
            else if (string.Equals(s, RPGMakerCodeInstantClose))
            {
                token = RPGMakerTokenType.InstantClose;
            }
            else
            {
                return false;
            }
            source = source.Remove(0, 2);
            return true;
        }

        protected void StopTypewriterCoroutine()
        {
            if (typewriterCoroutine == null) return;
            if (coroutineController == null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            else
            {
                coroutineController.StopCoroutine(typewriterCoroutine);
            }
            typewriterCoroutine = null;
            coroutineController = null;
        }

        /// <summary>
        /// Stops the effect.
        /// </summary>
        public override void Stop()
        {
            if (isPlaying)
            {
                onEnd.Invoke();
                Sequencer.Message(SequencerMessages.Typed);
            }
            StopTypewriterCoroutine();
            if (textComponent != null) 
            {
                textComponent.maxVisibleCharacters = textComponent.textInfo.characterCount;
                textComponent.ForceMeshUpdate();
            }
            HandleAutoScroll();
        }

        protected virtual void HandleAutoScroll()
        {
            if (!autoScrollSettings.autoScrollEnabled) return;

            layoutElement.preferredHeight = Mathf.Max(0, textComponent.textBounds.size.y);
            if (autoScrollSettings.scrollRect != null)
            {
                autoScrollSettings.scrollRect.normalizedPosition = new Vector2(0, 0);
            }
            if (autoScrollSettings.scrollbarEnabler != null)
            {
                autoScrollSettings.scrollbarEnabler.CheckScrollbarWithResetValue(0);
            }
        }

    }

#else

    [AddComponentMenu("")] // Use wrapper.
    public class TextMeshProTypewriterEffect : AbstractTypewriterEffect
    {
        public override bool isPlaying { get { return false; } }
        public override void Awake() { }
        public override void Start() { }
        public override void StartTyping(string text, int fromIndex = 0) { }
        public override void Stop() { }
        public override void StopTyping() { }
    }

#endif
}
