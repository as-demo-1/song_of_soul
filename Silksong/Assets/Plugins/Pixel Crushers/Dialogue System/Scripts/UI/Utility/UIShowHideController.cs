// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This is a deprecated class that has been replaced by UIAnimatorMonitor in StandardDialogueUI 
    /// and elsewhere. It's only used by the older UnityUIDialogueUI.
    /// </summary>
    public class UIShowHideController
    {

        public enum TransitionMode { State, Trigger }

        public enum State { Undefined, Showing, Shown, Hiding, Hidden }

        /// <summary>
        /// Maximum number of seconds to wait for show/hide animation.
        /// </summary>
        public static float maxWaitDuration = 5;

        public Component panel { get; set; }

        public State state { get; set; }

        public bool debug { get; set; }

        private TransitionMode m_transitionMode;
        private Animator m_animator = null;
        private bool m_lookedForAnimator = false;
        private Coroutine m_animCoroutine;

        public UIShowHideController(GameObject gameObjectToControl, Component panelToControl, TransitionMode animationMode = TransitionMode.Trigger, bool debug = false)
        {
            panel = panelToControl;
            m_animator = (gameObjectToControl != null) ? gameObjectToControl.GetComponent<Animator>() : null;
            if (m_animator == null && panelToControl != null) m_animator = panelToControl.GetComponent<Animator>();
            state = State.Undefined;
            m_transitionMode = animationMode;
            m_animCoroutine = null;
            this.debug = debug;
        }

        public void Show(string showState, bool pauseAfterAnimation, System.Action callback, bool wait = true)
        {
            CancelCurrentAnim();
            switch (m_transitionMode)
            {
                case TransitionMode.State:
                    m_animCoroutine = DialogueManager.instance.StartCoroutine(WaitForAnimationState(State.Showing, State.Shown, showState, pauseAfterAnimation, true, wait, callback));
                    break;
                case TransitionMode.Trigger:
                    m_animCoroutine = DialogueManager.instance.StartCoroutine(WaitForAnimationTrigger(State.Showing, State.Shown, showState, pauseAfterAnimation, true, wait, callback));
                    break;
            }
        }

        public void Hide(string hideState, System.Action callback)
        {
            CancelCurrentAnim();
            switch (m_transitionMode)
            {
                case TransitionMode.State:
                    m_animCoroutine = DialogueManager.instance.StartCoroutine(WaitForAnimationState(State.Hiding, State.Hidden, hideState, false, false, true, callback));
                    break;
                case TransitionMode.Trigger:
                    m_animCoroutine = DialogueManager.instance.StartCoroutine(WaitForAnimationTrigger(State.Hiding, State.Hidden, hideState, false, false, true, callback));
                    break;
            }
        }

        private IEnumerator WaitForAnimationState(State stateWhenBegin, State stateWhenEnd, string stateName, bool pauseAfterAnimation, bool panelActive, bool wait, System.Action callback)
        {
            if (state != stateWhenEnd) // Only need to do if we're not already in the state.
            {
                // If transitioning, need to wait one frame, or two Animator.Play calls may conflict on the same frame:
                if (state == State.Hiding || state == State.Showing) yield return null;

                state = stateWhenBegin;

                // Activate panel if necessary:
                if (panel != null && !panel.gameObject.activeSelf)
                {
                    panel.gameObject.SetActive(true);
                    yield return null;
                }

                // Play animation:
                if (CanTriggerAnimation(stateName))
                {
                    if (debug) Debug.Log("<color=green>" + panel.name + ".Animator.Play(" + stateName + ") time=" + Time.time + "</color>");

                    CheckAnimatorModeAndTimescale(stateName);
                    m_animator.Play(stateName);

                    // ...and wait for it to finish:
                    if (wait)
                    {
                        yield return null; // Wait to enter state.
                        var clipLength = (m_animator != null) ? m_animator.GetCurrentAnimatorStateInfo(0).length : 0;
                        if (Mathf.Approximately(0, Time.timeScale))
                        {
                            var timeout = Time.realtimeSinceStartup + clipLength;
                            while (Time.realtimeSinceStartup < timeout)
                            {
                                yield return null;
                            }
                        }
                        else
                        {
                            yield return new WaitForSeconds(clipLength);
                        }
                        if (debug) Debug.Log("<color=red>... finished " + panel.name + ".Animator.Play(" + stateName + ") time=" + Time.time + "</color>");
                    }
                }
            }

            // Finish:
            if (!panelActive) Tools.SetGameObjectActive(panel, false);
            if (pauseAfterAnimation) Time.timeScale = 0;
            m_animCoroutine = null;
            state = stateWhenEnd;
            if (callback != null) callback.Invoke();
        }

        private IEnumerator WaitForAnimationTrigger(State stateWhenBegin, State stateWhenEnd, string triggerName, bool pauseAfterAnimation, bool panelActive, bool wait, System.Action callback)
        {
            if (state != stateWhenEnd)
            {
                state = stateWhenBegin;
                if (panelActive)
                {
                    if (panel != null && !panel.gameObject.activeSelf)
                    {
                        panel.gameObject.SetActive(true);
                        yield return null;
                    }
                }
                if (CanTriggerAnimation(triggerName) && m_animator.gameObject.activeSelf)
                {
                    if (debug) Debug.Log("<color=green>" + panel.name + ".Animator.SetTrigger(" + triggerName + ") time=" + Time.time + "</color>");
                    CheckAnimatorModeAndTimescale(triggerName);
                    float timeout = Time.realtimeSinceStartup + maxWaitDuration;
#if UNITY_2019_1_OR_NEWER
                    var goalHashID = Animator.StringToHash($"{m_animator.GetLayerName(0)}.{triggerName}");
#else
                    var goalHashID = Animator.StringToHash(triggerName);
#endif
                    var oldHashId = UITools.GetAnimatorNameHash(m_animator.GetCurrentAnimatorStateInfo(0));
                    var currentHashID = oldHashId;
                    m_animator.SetTrigger(triggerName);
                    if (wait)
                    {
                        // Wait while we're not at the goal state and we're in the original state and we haven't timed out:
                        while ((currentHashID != goalHashID) && (currentHashID == oldHashId) && (Time.realtimeSinceStartup < timeout))
                        {
                            yield return null;
                            var isAnimatorValid = m_animator != null && m_animator.isActiveAndEnabled && m_animator.runtimeAnimatorController != null && m_animator.layerCount > 0;
                            currentHashID = isAnimatorValid ? UITools.GetAnimatorNameHash(m_animator.GetCurrentAnimatorStateInfo(0)) : currentHashID;
                        }
                        // If we're in the goal state and we haven't timed out, wait for the duration of the goal state:
                        if (m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 && currentHashID == goalHashID && Time.realtimeSinceStartup < timeout)
                        {
                            var clipLength = m_animator.GetCurrentAnimatorStateInfo(0).length;
                            if (Mathf.Approximately(0, Time.timeScale))
                            {
                                timeout = Time.realtimeSinceStartup + clipLength;
                                while (Time.realtimeSinceStartup < timeout)
                                {
                                    yield return null;
                                }
                            }
                            else
                            {
                                yield return new WaitForSeconds(clipLength);
                            }
                        }
                        if (debug) Debug.Log("<color=red>... finished " + panel.name + ".Animator.SetTrigger(" + triggerName + ") time=" + Time.time + "</color>");
                    }
                }
            }
            if (!panelActive) Tools.SetGameObjectActive(panel, false);
            if (pauseAfterAnimation) Time.timeScale = 0;
            m_animCoroutine = null;
            state = stateWhenEnd;
            if (callback != null) callback.Invoke();
        }

        private void CheckAnimatorModeAndTimescale(string triggerName)
        {
            if (Mathf.Approximately(0, Time.timeScale) && (m_animator.updateMode != AnimatorUpdateMode.UnscaledTime) && DialogueDebug.logWarnings)
            {
                Debug.LogWarning("Dialogue System: Time is paused but animator mode isn't set to Unscaled Time; the animation triggered by " + triggerName + " won't play.", m_animator);
            }
        }

        private void CancelCurrentAnim()
        {
            if (m_animCoroutine != null)
            {
                DialogueManager.instance.StopCoroutine(m_animCoroutine);
                m_animCoroutine = null;
            }
        }

        public void ClearTrigger(string triggerName)
        {
            if (HasAnimator() && !string.IsNullOrEmpty(triggerName) && m_animator.isActiveAndEnabled)
            {
                m_animator.ResetTrigger(triggerName);
            }
        }

        private bool CanTriggerAnimation(string stateName)
        {
            return HasAnimator() && !string.IsNullOrEmpty(stateName);
        }

        private bool HasAnimator()
        {
            if ((m_animator == null) && !m_lookedForAnimator)
            {
                m_lookedForAnimator = true;
                if (panel != null)
                {
                    m_animator = panel.GetComponent<Animator>();
                    if (m_animator == null) m_animator = panel.GetComponentInChildren<Animator>();
                    state = (m_animator != null && m_animator.gameObject.activeInHierarchy) ? State.Shown : State.Hidden;
                }
            }
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
            return (m_animator != null) && m_animator.isInitialized && m_animator.gameObject.activeSelf;
#else
            return (m_animator != null) && m_animator.gameObject.activeSelf;
#endif
        }

    }

}
