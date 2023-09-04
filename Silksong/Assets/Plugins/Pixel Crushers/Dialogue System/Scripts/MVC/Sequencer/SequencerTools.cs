// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A static utility class for Sequencer.
    /// </summary>
    public static class SequencerTools
    {

        private static Dictionary<string, Transform> registeredSubjects = new Dictionary<string, Transform>();
        private static bool hasHookedIntoSceneLoaded = false;

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            registeredSubjects = new Dictionary<string, Transform>();
            hasHookedIntoSceneLoaded = false;
        }
#endif

        public static void HookIntoSceneLoaded()
        {
            if (!hasHookedIntoSceneLoaded)
            {
                hasHookedIntoSceneLoaded = true;
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        private static void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            CleanNullSubjects();
        }

        /// <summary>
        /// Registers a GameObject by name for faster lookup.
        /// </summary>
        public static void RegisterSubject(Transform subject)
        {
            if (subject == null) return;
            registeredSubjects[subject.name] = subject;
            HookIntoSceneLoaded();
        }

        /// <summary>
        /// Unregisters a registered GameObject.
        /// </summary>
        public static void UnregisterSubject(Transform subject)
        {
            if (subject == null || !registeredSubjects.ContainsKey(subject.name)) return;
            registeredSubjects.Remove(subject.name);
        }

        /// <summary>
        /// Clears null entries from registeredSubjects.
        /// </summary>
        public static void CleanNullSubjects()
        {
            registeredSubjects.RemoveAll(x => x == null);
        }

        /// <summary>
        /// Sequencer commands usually specify a subject to which the command applies (e.g., where to
        /// aim the camera). This utility function that returns the specified subject.
        /// </summary>
        /// <returns>
        /// The transform of the specified subject, or null if the specifier names a game
        /// object that isn't in the scene.
        /// </returns>
        /// <param name='specifier'>
        /// <c>"speaker"</c>, <c>"listener"</c>, or the name of a game object in the scene.
        /// </param>
        /// <param name='speaker'>
        /// Speaker.
        /// </param>
        /// <param name='listener'>
        /// Listener.
        /// </param>
        /// <param name='defaultSubject'>
        /// Default subject.
        /// </param>
        public static Transform GetSubject(string specifier, Transform speaker, Transform listener, Transform defaultSubject = null)
        {
            if (string.IsNullOrEmpty(specifier))
            {
                return defaultSubject ?? speaker;
            }
            else if (string.Compare(specifier, SequencerKeywords.Speaker, System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                return speaker;
            }
            else if (string.Compare(specifier, SequencerKeywords.Listener, System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                return listener;
            }
            else if (string.Compare(specifier, SequencerKeywords.SpeakerPortrait, System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                return GetPortraitImage(speaker);
            }
            else if (string.Compare(specifier, SequencerKeywords.ListenerPortrait, System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                return GetPortraitImage(listener);
            }
            else if (specifier.StartsWith(SequencerKeywords.ActorPrefix))
            {
                return CharacterInfo.GetRegisteredActorTransform(specifier.Substring(SequencerKeywords.ActorPrefix.Length));
            }
            else
            {
                GameObject go = FindSpecifier(specifier);
                return (go != null) ? go.transform : defaultSubject;
            }
        }

        /// <summary>
        /// Returns true if specifier specifies a tag ('tag=foo').
        /// </summary>
        public static bool SpecifierSpecifiesTag(string specifier)
        {
            return !string.IsNullOrEmpty(specifier) && specifier.StartsWith("tag=", System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Assumes specifier specifies a tag ('tag=foo'). Returns the tag.
        /// </summary>
        public static string GetSpecifiedTag(string specifier)
        {
            return specifier.Substring("tag=".Length);
        }

        /// <summary>
        /// Finds a game object with the specified name, returning any match in scene before
        /// trying to return any non-scene matches in the project. Checks GameObjects registered
        /// with the specifier as its actor name first.
        /// </summary>
        /// <returns>
        /// The specified game object.
        /// </returns>
        /// <param name='specifier'>
        /// The name to search for.
        /// </param>
        /// <param name='onlyActiveInScene'>Only search active objects in the scene.</param>
        public static GameObject FindSpecifier(string specifier, bool onlyActiveInScene = false)
        {
            if (string.IsNullOrEmpty(specifier)) return null;

            // Check for 'tag=' keyword:
            if (SpecifierSpecifiesTag(specifier))
            {
                var tag = GetSpecifiedTag(specifier);
                var taggedGO = GameObject.FindGameObjectWithTag(tag);
                if (taggedGO != null) return taggedGO;
                var results = Tools.FindGameObjectsWithTagHard(tag);
                return (results.Length > 0) ? results[0] : null;
            }

            // Search registered actors:
            var t = CharacterInfo.GetRegisteredActorTransform(specifier);
            if (t != null) return t.gameObject;

            // Check registered subjects:
            if (registeredSubjects.TryGetValue(specifier, out t) && t != null) return t.gameObject;

            // Search for active objects in scene:
            var match = GameObject.Find(specifier);
            if (match != null) return match;

            if (onlyActiveInScene) return null;

            // Search for all objects in scene, including inactive as long as it's a child of an active object:
            match = Tools.GameObjectHardFind(specifier);
            if (match != null) return match;

            // Search for all objects, including loaded-but-not-instantiated (i.e., prefabs):
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (string.Compare(specifier, go.name, System.StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return go;
                }
            }
            return null;
        }

        /// <returns>
        /// The transform of the Portrait Image GameObject, or null if not found.
        /// Must be using Standard Dialogue UI. Not for use with simultaneous conversations.
        /// </returns>
        public static Transform GetPortraitImage(Transform subject)
        {
            if (DialogueManager.standardDialogueUI == null) return null;
            if (subject == null) return null;

            var subtitleControls = DialogueManager.standardDialogueUI.conversationUIElements.standardSubtitleControls;
            DialogueActor dialogueActor;
            StandardUISubtitlePanel panel = null;

            if (DialogueManager.isConversationActive && DialogueManager.currentConversationState != null)
            {
                var subtitle = DialogueManager.currentConversationState.subtitle;
                if (subtitle.speakerInfo != null && subtitle.speakerInfo.transform == subject)
                {
                    panel = subtitleControls.GetPanel(subtitle, out dialogueActor);
                }
            }

            if (panel == null)
            {
                StandardUISubtitlePanel defaultPanel = subtitleControls.defaultNPCPanel;
                dialogueActor = DialogueActor.GetDialogueActorComponent(subject);
                if (dialogueActor != null)
                {
                    var actor = DialogueManager.masterDatabase.GetActor(dialogueActor.actor);
                    if (actor != null) defaultPanel = actor.IsPlayer ? subtitleControls.defaultPCPanel : subtitleControls.defaultNPCPanel;
                }
                panel = subtitleControls.GetActorTransformPanel(subject, defaultPanel, out dialogueActor);
            }

            return (panel != null && panel.portraitImage != null) ? panel.portraitImage.transform : null;
        }

        /// <summary>
        /// Gets the default camera angle for a subject.
        /// </summary>
        /// <returns>The default camera angle. If the subject doesn't have a DefaultCameraAngle
        /// component, returns "Closeup".</returns>
        /// <param name="subject">Subject.</param>
        public static string GetDefaultCameraAngle(Transform subject)
        {
            var defaultCameraAngle = (subject != null) ? subject.GetComponentInChildren<DefaultCameraAngle>() : null;
            return (defaultCameraAngle != null) ? defaultCameraAngle.cameraAngle : "Closeup";
        }

        /// <summary>
        /// Gets <c>parameters[i]</c>.
        /// </summary>
        /// <returns>
        /// <c>parameters[i]</c>, or the specified default value if <c>i</c> is out of range.
        /// </returns>
        /// <param name='parameters'>
        /// An array of parameters.
        /// </param>
        /// <param name='i'>
        /// The index into <c>parameters[]</c>
        /// </param>
        /// <param name='defaultValue'>
        /// The default value to return if <c>i</c> is out of range.
        /// </param>
        public static string GetParameter(string[] parameters, int i, string defaultValue = null)
        {
            return ((parameters != null) && (i < parameters.Length)) ? parameters[i] : defaultValue;
        }

        /// <summary>
        /// Gets <c>parameters[i]</c> as the specified type. Culture invariant (i.e., floats use '.' for
        /// decimal point).
        /// </summary>
        /// <returns>
        /// <c>parameters[i]</c> as type T, or the specified default value if <c>i</c> is out of range
        /// or <c>parameters[i]</c> can't be converted to type T.
        /// </returns>
        /// <param name='parameters'>
        /// An array of parameters.
        /// </param>
        /// <param name='i'>
        /// The index into <c>parameters[]</c>
        /// </param>
        /// <param name='defaultValue'>
        /// The default value to return if <c>i</c> is out of range or the parameter can't be converted
        /// to type T.
        /// </param>
        /// <typeparam name='T'>
        /// The type to convert the parameter to.
        /// </typeparam>
        /// <example>
        /// // Get parameters[1] as a float, defaulting to 5f:
        /// float duration = GetParameterAs<float>(parameters, 1, 5f);
        /// </example>
        public static T GetParameterAs<T>(string[] parameters, int i, T defaultValue)
        {
            try
            {
                return ((parameters != null) && (i < parameters.Length))
                    ? (T)System.Convert.ChangeType(parameters[i], typeof(T), System.Globalization.CultureInfo.InvariantCulture)
                    : defaultValue;
            }
            catch (System.Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the i-th parameter as a float.
        /// </summary>
        /// <returns>
        /// The parameter as float, or defaultValue if out of range.
        /// </returns>
        /// <param name='parameters'>
        /// The array of parameters.
        /// </param>
        /// <param name='i'>
        /// The zero-based index of the parameter.
        /// </param>
        /// <param name='defaultValue'>
        /// The default value to use if the parameter doesn't exist or isn't valid for the type.
        /// </param>
        public static float GetParameterAsFloat(string[] parameters, int i, float defaultValue = 0)
        {
            return GetParameterAs<float>(parameters, i, defaultValue);
        }

        /// <summary>
        /// Gets the i-th parameter as an int.
        /// </summary>
        /// <returns>
        /// The parameter as an int, or defaultValue if out of range.
        /// </returns>
        /// <param name='parameters'>
        /// The array of parameters.
        /// </param>
        /// <param name='i'>
        /// The zero-based index of the parameter.
        /// </param>
        /// <param name='defaultValue'>
        /// The default value to use if the parameter doesn't exist or isn't valid for the type.
        /// </param>
        public static int GetParameterAsInt(string[] parameters, int i, int defaultValue = 0)
        {
            return GetParameterAs<int>(parameters, i, defaultValue);
        }

        /// <summary>
        /// Gets the i-th parameter as a bool.
        /// </summary>
        /// <returns>
        /// The parameter as bool, or defaultValue if out of range.
        /// </returns>
        /// <param name='parameters'>
        /// The array of parameters.
        /// </param>
        /// <param name='i'>
        /// The zero-based index of the parameter.
        /// </param>
        /// <param name='defaultValue'>
        /// The default value to use if the parameter doesn't exist or isn't valid for the type.
        /// </param>
        public static bool GetParameterAsBool(string[] parameters, int i, bool defaultValue = false)
        {
            return GetParameterAs<bool>(parameters, i, defaultValue);
        }

        /// <summary>
        /// Gets the audio source on a subject, using the Dialogue Manager as the subject if the
        /// specified subject is <c>null</c>. If no audio source exists on the subject, this
        /// method adds one.
        /// </summary>
        /// <returns>The audio source.</returns>
        /// <param name="subject">Subject.</param>
        public static AudioSource GetAudioSource(Transform subject)
        {
            GameObject go = (subject != null) ? subject.gameObject : DialogueManager.instance.gameObject;
            AudioSource audio = go.GetComponentInChildren<AudioSource>();
            if (audio == null)
            { 
                audio = go.AddComponent<AudioSource>();
                audio.playOnAwake = false;
                audio.loop = false;
            }
            return audio;
        }

        /// <summary>
        /// Checks if a Lua variable "Mute" is <c>true</c>.
        /// </summary>
        /// <returns><c>true</c> if audio is muted; otherwise, <c>false</c>.</returns>
        public static bool IsAudioMuted()
        {
            return DialogueLua.GetVariable("Mute").asBool;
        }

    }

}
