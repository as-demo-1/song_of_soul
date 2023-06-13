// Copyright (c) Pixel Crushers. All rights reserved.

using PixelCrushers.DialogueSystem.SequencerCommands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A sequencer plays sequences of commands such as camera cuts, animation, audio, activating
    /// game objects, etc. You can use the sequencer to play cutscenes or perform game actions.
    /// The dialogue system uses a sequencer to play a sequence for every line of dialogue. If the
    /// dialogue author hasn't specified a sequence for a line of dialogue, the dialogue system 
    /// will generate a basic, default sequence that aims the camera at the speaker.
    /// 
    /// See also: @ref sequencer
    /// 
    /// Each sequence command is implemented as a coroutine. You can add new commands by defining
    /// subclasses of SequencerCommand.
    /// </summary>
    public class Sequencer : MonoBehaviour
    {

        /// <summary>
        /// This handler is called when the sequence is done playing.
        /// </summary>
        public event Action FinishedSequenceHandler = null;

        public delegate void MessageStringDelegate(string message);

        public event MessageStringDelegate receivedMessage = null;

        /// <summary>
        /// A constant defining the name of the default camera angles prefab in case the cameraAngles property isn't set.
        /// </summary>
        private const string DefaultCameraAnglesResourceName = "Default Camera Angles";

        /// <summary>
        /// Indicates whether a sequence is currently playing. The Dialogue System can queue up any number of actions
        /// using the Play() method. This property returns true if any actions are scheduled or active.
        /// </summary>
        /// <value>
        /// <c>true</c> if is playing; otherwise, <c>false</c>.
        /// </value>
        public bool isPlaying
        {
            get { return m_isPlaying; }
        }

        public GameObject cameraAngles
        {
            get { return m_cameraAngles; }
        }

        public Camera sequencerCamera
        {
            get { return m_sequencerCamera; }
        }

        public Transform sequencerCameraTransform
        {
            get { return (m_alternateSequencerCameraObject != null) ? m_alternateSequencerCameraObject.transform : m_sequencerCamera.transform; }
        }

        public Transform speaker
        {
            get { return m_speaker; }
        }

        public Transform listener
        {
            get { return m_listener; }
        }

        /// <summary>
        /// Original camera position at start of conversation. At the end of
        /// the conversation, the camera is restored back to this position.
        /// You can change this if you want to reset the 'original' position
        /// to be elsewhere.
        /// </summary>
        public Vector3 originalCameraPosition
        {
            get { return m_originalCameraPosition; }
            set { m_originalCameraPosition = value; }
        }

        /// <summary>
        /// Original camera position at start of conversation.
        /// You can change this if you want to reset the 'original' rotation
        /// to be different.
        /// </summary>
        public Quaternion originalCameraRotation
        {
            get { return m_originalCameraRotation; }
            set { m_originalCameraRotation = value; }
        }

        /// <summary>
        /// Original 2D camera orthographic size at start of conversation.
        /// You can change this if you want to reset the the 'original' size
        /// to be different.
        /// </summary>
        public float originalOrthographicSize
        {
            get { return m_originalOrthographicSize; }
            set { m_originalOrthographicSize = value; }
        }

        /// <summary>
        /// If true, don't restore camera position to pre-sequence position when sequencer closes.
        /// </summary>
        public bool keepCameraPositionOnClose
        {
            get { return m_keepCameraPositionOnClose; }
            set { m_keepCameraPositionOnClose = value; }
        }

        /// <summary>
        /// The subtitle end time ({{end}}) if playing a dialogue entry sequence.
        /// </summary>
        public float subtitleEndTime { get; set; }

        /// <summary>
        /// The entrytag for the current dialogue entry, if playing a dialogue entry sequence.
        /// </summary>
		public string entrytag { get; set; }

        /// <summary>
        /// Currently language-localized entrytag.
        /// </summary>
        public string entrytaglocal
        {
            get { return Localization.isDefaultLanguage ? entrytag : entrytag + "_" + Localization.language; }
        }

        /// <summary>
        /// Active conversation record associated with this sequencer instance.
        /// </summary>
        public ActiveConversationRecord activeConversationRecord { get; set; }

        /// @cond FOR_V1_COMPATIBILITY
        public bool IsPlaying { get { return isPlaying; } }
        public GameObject CameraAngles { get { return cameraAngles; } }
        public Camera SequencerCamera { get { return sequencerCamera; } }
        public Transform SequencerCameraTransform { get { return sequencerCameraTransform; } }
        public Transform Speaker { get { return speaker; } }
        public Transform Listener { get { return listener; } }
        public Vector3 OriginalCameraPosition { get { return originalCameraPosition; } }
        public Quaternion OriginalCameraRotation { get { return originalCameraRotation; } }
        public float OriginalOrthographicSize { get { return originalOrthographicSize; } }
        public float SubtitleEndTime { get { return subtitleEndTime; } set { subtitleEndTime = value; } }
        /// @endcond

        /// <summary>
        /// Set <c>true</c> to disable the internal sequencer commands -- for example,
        /// if you want to replace them all with your own.
        /// </summary>
        public bool disableInternalSequencerCommands = false;

        /// <summary>
        /// <c>true</c> if the sequencer has taken control of the main camera at some point. Used to restore the 
        /// original camera position when the sequencer is closed.
        /// </summary>
        private bool m_hasCameraControl = false;

        private Camera m_originalCamera = null;

        /// <summary>
        /// The original camera position before the sequencer took control. If the sequencer doesn't take control
        /// of the camera, this property is ignored.
        /// </summary>
        private Vector3 m_originalCameraPosition = Vector3.zero;

        /// <summary>
        /// The original camera rotation before the sequencer took control. If the sequencer doesn't take control
        /// of the camera, this property is ignored.
        /// </summary>
        private Quaternion m_originalCameraRotation = Quaternion.identity;

        /// <summary>
        /// The original orthographicSize before the sequencer took control.
        /// </summary>
        private float m_originalOrthographicSize = 16;

        private bool m_keepCameraPositionOnClose = false;

        private Transform m_speaker = null;

        private Transform m_listener = null;

        private List<QueuedSequencerCommand> m_queuedCommands = new List<QueuedSequencerCommand>();

        private List<SequencerCommand> m_activeCommands = new List<SequencerCommand>();

        private List<SequencerCommand> m_commandsToDelete = new List<SequencerCommand>();

        public int numQueuedCommands { get { return m_queuedCommands.Count; } }

        public int numActiveCommands { get { return m_activeCommands.Count; } }

        private float m_delayTimeLeft = 0; // Used to track Delay(#) instead of requiring sep. sequencer command.

        private bool m_informParticipants = false;

        private bool m_closeWhenFinished = false;

        private Camera m_sequencerCameraSource = null;

        private Camera m_sequencerCamera = null;

        private GameObject m_alternateSequencerCameraObject = null;

        private GameObject m_cameraAngles = null;

        private bool m_isUsingMainCamera = false;

        private bool m_isPlaying = false;

        /// <summary>
        /// 
        /// </summary>
        public static bool reportMissingAudioFiles = false;

        private static Dictionary<string, System.Type> m_cachedComponentTypes = new Dictionary<string, Type>();

        private static Dictionary<string, string> m_shortcuts = new Dictionary<string, string>();
        private static Dictionary<string, Stack<string>> m_shortcutStack = new Dictionary<string, Stack<string>>();

        /// <summary>
        /// Registered shortcuts:
        /// </summary>
        public static Dictionary<string, string> shortcuts { get { return m_shortcuts; } }

        /// <summary>
        /// Stack of values for each shortcut. If adding a shortcut that already exists, the new
        /// value of the shortcut is added to the top of the stack. When removed, it's popped off
        /// the stack, revealing the previous value.
        /// </summary>
        public static Dictionary<string, Stack<string>> shortcutStack { get { return m_shortcutStack; } }

        private Dictionary<string, Coroutine> m_timedMessageCoroutines = new Dictionary<string, Coroutine>();

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            m_cachedComponentTypes = new Dictionary<string, Type>();
            m_shortcuts = new Dictionary<string, string>();
            m_shortcutStack = new Dictionary<string, Stack<string>>();
        }
#endif
        private SequenceParser m_parser = new SequenceParser();

        private const float InstantThreshold = 0.001f;

        /// <summary>
        /// Sends OnSequencerMessage(message) to the Dialogue Manager. Since sequencers are usually on 
        /// the Dialogue Manager object, this is a convenient way to send a message to all active sequencers.
        /// You can use this method if your sequence is waiting for a message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        public static void Message(string message)
        {
            if (DialogueManager.instance == null) return;
            DialogueManager.instance.SendMessage(DialogueSystemMessages.OnSequencerMessage, message, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Registers a sequencer shortcut with the Dialogue System. When playing sequences, shortcuts wrapped in
        /// double braces are replaced by their values.
        /// </summary>
        /// <param name="shortcut">Shortcut ID</param>
        /// <param name="value">Sequence that replaces the shortcut ID.</param>
        public static void RegisterShortcut(string shortcut, string value)
        {
            if (string.IsNullOrEmpty(shortcut) || shortcut.Equals("end") || shortcut.Equals("default")) return;
            var key = "{{" + shortcut + "}}";
            if (m_shortcuts.ContainsKey(key))
            {
                m_shortcuts[key] = value;
            }
            else
            {
                m_shortcuts.Add(key, value);
            }

            // Also add to a stack so we can restore the previous value of the shortcut once unregistered:
            if (!m_shortcutStack.ContainsKey(key))
            {
                m_shortcutStack.Add(key, new Stack<string>());
            }
            m_shortcutStack[key].Push(value);
        }

        /// <summary>
        /// Unregisters a shortcut from the Dialogue System.
        /// </summary>
        /// <param name="shortcut">Shortcut to remove.</param>
        public static void UnregisterShortcut(string shortcut)
        {
            var key = "{{" + shortcut + "}}";
            if (m_shortcuts.ContainsKey(key))
            {
                m_shortcuts.Remove(key);
            }

            // Remove from stack. If stack has a previous value, set it, too.
            if (m_shortcutStack.ContainsKey(key))
            {
                if (m_shortcutStack[key].Count > 0)
                {
                    m_shortcutStack[key].Pop();
                    if (m_shortcutStack[key].Count > 0)
                    {
                        var previousValue = m_shortcutStack[key].Pop();
                        m_shortcuts.Add(key, previousValue);
                    }
                }
                if (m_shortcutStack[key].Count == 0)
                {
                    m_shortcutStack.Remove(key);
                }
            }
        }

        public static string ReplaceShortcuts(string sequence)
        {
            if (!sequence.Contains("{{")) return sequence;
            foreach (var kvp in m_shortcuts)
            {
                sequence = sequence.Replace(kvp.Key, kvp.Value);
            }
            return sequence;
        }

        private static Regex ShortcutRegex = null;

        private static void ReportUnrecognizedShortcuts(string sequence)
        {
            if (ShortcutRegex == null) ShortcutRegex = new Regex(@"{{.+}}");
            foreach (Match match in ShortcutRegex.Matches(sequence))
            {
                if (string.Equals("{{default}}", match.Value)) continue;
                Debug.LogWarning("Dialogue System: Unrecognized shortcut " + match.Value);
            }
        }

        public void UseCamera(Camera sequencerCamera, GameObject cameraAngles)
        {
            UseCamera(sequencerCamera, null, cameraAngles);
        }

        public void UseCamera(Camera sequencerCamera, GameObject alternateSequencerCameraObject, GameObject cameraAngles)
        {
            this.m_originalCamera = Camera.main;
            this.m_sequencerCameraSource = sequencerCamera;
            this.m_alternateSequencerCameraObject = alternateSequencerCameraObject;
            this.m_cameraAngles = cameraAngles;
            //--- Delay until/ needed: GetCamera();
            GetCameraAngles();
        }

        private void GetCameraAngles()
        {
            if (m_cameraAngles == null)
            {
                DialogueManager.LoadAsset(DefaultCameraAnglesResourceName, typeof(GameObject),
                    (asset) => { m_cameraAngles = asset as GameObject; });
            }
        }

        private void GetCamera()
        {
            if (m_sequencerCamera == null)
            {
                if (m_alternateSequencerCameraObject != null)
                {
                    m_isUsingMainCamera = true;
                    m_sequencerCamera = m_alternateSequencerCameraObject.GetComponent<Camera>();
                }
                else if (m_sequencerCameraSource != null)
                {
                    GameObject source = m_sequencerCameraSource.gameObject;
                    GameObject sequencerCameraObject = Instantiate(source, source.transform.position, source.transform.rotation) as GameObject;
                    m_sequencerCamera = sequencerCameraObject.GetComponent<Camera>();
                    if (m_sequencerCamera != null)
                    {
                        m_sequencerCamera.transform.parent = this.transform;
                        m_sequencerCamera.gameObject.SetActive(false);
                        m_isUsingMainCamera = false;
                    }
                    else
                    {
                        Destroy(sequencerCameraObject);
                    }
                }
                if (m_sequencerCamera == null)
                {
                    m_sequencerCamera = UnityEngine.Camera.main;
                    m_isUsingMainCamera = true;
                }
                // Make sure a sequencerCamera exists:
                if (m_sequencerCamera == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(DialogueDebug.Prefix + ": No MainCamera found in scene. Creating one for the Sequencer Camera.", this);
                    GameObject go = new GameObject("Sequencer Camera", typeof(Camera), typeof(AudioListener));
#if !UNITY_2017_1_OR_NEWER
                    go.AddComponent<GUILayer>();
#endif
                    m_sequencerCamera = go.GetComponent<Camera>();
                    m_isUsingMainCamera = true;
                }
            }
            // Make sure a camera is tagged MainCamera; use sequencerCamera if no other:
            if (UnityEngine.Camera.main == null && m_sequencerCamera != null)
            {
                m_sequencerCamera.tag = "MainCamera";
                m_isUsingMainCamera = true;
            }
        }

        private void DestroyCamera()
        {
            if ((m_sequencerCamera != null) && !m_isUsingMainCamera)
            {
                m_sequencerCamera.gameObject.SetActive(false);
                Destroy(m_sequencerCamera.gameObject, 1);
                m_sequencerCamera = null;
            }
        }

        /// <summary>
        /// Restores the original camera position. Waits two frames first, to allow any
        /// active, required actions to finish.
        /// </summary>
        private IEnumerator RestoreCamera()
        {
            yield return null;
            yield return null;
            ReleaseCameraControl();
        }

        /// <summary>
        /// Switches the sequencer camera to a different camera object immediately.
        /// Restores the previous camera first.
        /// </summary>
        /// <param name="newCamera">New camera.</param>
        public void SwitchCamera(Camera newCamera)
        {
            if ((m_sequencerCamera != null) && !m_isUsingMainCamera)
            {
                Destroy(m_sequencerCamera.gameObject, 1);
            }
            ReleaseCameraControl();
            m_hasCameraControl = false;
            m_originalCamera = null;
            m_originalCameraPosition = Vector3.zero;
            m_originalCameraRotation = Quaternion.identity;
            m_originalOrthographicSize = 16;
            m_sequencerCameraSource = null;
            m_sequencerCamera = null;
            m_alternateSequencerCameraObject = null;
            m_isUsingMainCamera = false;
            UseCamera(newCamera, m_cameraAngles);
            TakeCameraControl();
        }

        /// <summary>
        /// Takes control of the camera.
        /// </summary>
        public void TakeCameraControl()
        {
            GetCamera();
            if (m_hasCameraControl) return;
            m_hasCameraControl = true;
            if (m_alternateSequencerCameraObject != null)
            {
                m_originalCamera = m_sequencerCamera;
                m_originalCameraPosition = m_alternateSequencerCameraObject.transform.position;
                m_originalCameraRotation = m_alternateSequencerCameraObject.transform.rotation;
            }
            else
            {
                m_originalCamera = UnityEngine.Camera.main;
                if (UnityEngine.Camera.main != null)
                {
                    m_originalCameraPosition = UnityEngine.Camera.main.transform.position;
                    m_originalCameraRotation = UnityEngine.Camera.main.transform.rotation;
                    m_originalCamera.gameObject.SetActive(false);
                }
                m_originalOrthographicSize = m_sequencerCamera.orthographicSize;
                m_sequencerCamera.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Releases control of the camera.
        /// </summary>
        private void ReleaseCameraControl()
        {
            if (!m_hasCameraControl) return;
            m_hasCameraControl = false;
            if (m_alternateSequencerCameraObject != null && !keepCameraPositionOnClose)
            {
                m_alternateSequencerCameraObject.transform.position = m_originalCameraPosition;
                m_alternateSequencerCameraObject.transform.rotation = m_originalCameraRotation;
            }
            else
            {
                if (m_sequencerCamera != null) // May have disappeared if changed scene during conversation.
                {
                    if (!keepCameraPositionOnClose)
                    {
                        m_sequencerCamera.transform.position = m_originalCameraPosition;
                        m_sequencerCamera.transform.rotation = m_originalCameraRotation;
                        m_sequencerCamera.orthographicSize = m_originalOrthographicSize;
                    }
                    m_sequencerCamera.gameObject.SetActive(false);
                }
                if (m_originalCamera != null)
                {
                    m_originalCamera.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Opens this instance. Simply resets hasCameraControl.
        /// </summary>
        public void Open()
        {
            entrytag = string.Empty;
            //--- Delay until/ needed: GetCamera();
            m_hasCameraControl = false;
            GetCameraAngles();
        }

        /// <summary>
        /// Closes and destroy this sequencer. Stops all actions and restores the original camera 
        /// position.
        /// </summary>
        public void Close()
        {
            if (FinishedSequenceHandler != null) FinishedSequenceHandler();
            FinishedSequenceHandler = null;
            Stop();
            StartCoroutine(RestoreCamera());
            Destroy(this, 1);
        }

        public void OnDestroy()
        {
            DestroyCamera();
        }

        public void Update()
        {
            if (m_isPlaying)
            {
                CheckQueuedCommands();
                CheckActiveCommands();
                if (m_delayTimeLeft > 0)
                {
                    switch (DialogueTime.mode)
                    {
                        case DialogueTime.TimeMode.Realtime:
                            m_delayTimeLeft -= Time.unscaledDeltaTime;
                            break;
                        case DialogueTime.TimeMode.Gameplay:
                            m_delayTimeLeft -= Time.deltaTime;
                            break;
                    }
                }
            }
#if LATEUPDATE_MESSAGES
        }

        public void LateUpdate()
        {
#endif
            if (m_isPlaying)
            {
                foreach (string message in queuedMessages)
                {
                    OnSequencerMessage(message);
                }
                queuedMessages.Clear();
                if ((m_queuedCommands.Count == 0) && (m_activeCommands.Count == 0) && m_delayTimeLeft <= 0)
                {
                    FinishSequence();
                }
            }
        }

        private void FinishSequence()
        {
            m_isPlaying = false;
            if (FinishedSequenceHandler != null) FinishedSequenceHandler();
            if (m_informParticipants) InformParticipants(DialogueSystemMessages.OnSequenceEnd);
            if (m_closeWhenFinished)
            {
                FinishedSequenceHandler = null;
                Close();
            }
            s_awakeSequencer = null;
        }

        public void SetParticipants(Transform speaker, Transform listener)
        {
            this.m_speaker = speaker;
            this.m_listener = listener;
        }

        private void InformParticipants(string message)
        {
            if (m_speaker != null)
            {
                m_speaker.BroadcastMessage(message, m_speaker, SendMessageOptions.DontRequireReceiver);
                if ((m_listener != null) && (m_listener != m_speaker)) m_listener.BroadcastMessage(message, m_speaker, SendMessageOptions.DontRequireReceiver);
            }
            if (DialogueManager.instance.transform != m_speaker && DialogueManager.instance.transform != m_listener)
            {
                var actor = (m_speaker != null) ? m_speaker : ((m_listener != null) ? m_listener : DialogueManager.instance.transform);
                DialogueManager.instance.BroadcastMessage(message, actor, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <summary>
        /// Parses a sequence string and plays the individual commands.
        /// </summary>
        /// <param name='sequence'>
        /// The sequence to play, in the form:
        /// 
        /// <code>
        /// \<sequence\> ::= \<statement\> ; \<statement\> ; ...
        /// </code>
        /// 
        /// <code>
        /// \<statement\> ::= [required] \<command\>( \<arg\>, \<arg\>, ... ) [@\<time\>] [->Message(Y)]
        /// </code>
        /// 
        /// For example, the sequence below shows a wide angle shot of the speaker reloading and
        /// firing, and then cuts to a closeup of the listener.
        /// 
        /// <code>
        /// Camera(Wide); Animation(Reload); Animation(Fire)@2; required Camera(Closeup, listener)@3.5
        /// </code>
        /// </param>
        public void PlaySequence(string sequence)
        {
            m_isPlaying = true;
            if (string.IsNullOrEmpty(sequence)) return;

            // Replace [var=varName] and [lua()] tags:
            sequence = FormattedText.ParseCode(sequence);

            // Replace shortcuts:
            if (sequence.Contains("{{"))
            {
                sequence = ReplaceShortcuts(sequence);
                sequence = FormattedText.ParseCode(sequence); // Replace any [var] or [lua] in shortcuts.
                if (DialogueDebug.logWarnings && sequence.Contains("{{")) ReportUnrecognizedShortcuts(sequence);
            }

            // Substitute entrytaglocal and entrytag:
            if (!string.IsNullOrEmpty(entrytag) && sequence.Contains(SequencerKeywords.Entrytag))
            {
                sequence = sequence.Replace(SequencerKeywords.EntrytagLocal, entrytaglocal).Replace(SequencerKeywords.Entrytag, entrytag);
            }

            var commands = m_parser.Parse(sequence);
            if (commands != null)
            {
                for (int i = 0; i < commands.Count; i++)
                {
                    PlayCommand(commands[i]);
                }
            }
        }

        public void PlaySequence(string sequence, bool informParticipants, bool destroyWhenDone)
        {
            this.m_closeWhenFinished = destroyWhenDone;
            this.m_informParticipants = informParticipants;
            if (informParticipants) InformParticipants("OnSequenceStart");
            PlaySequence(sequence);
        }

        public void PlaySequence(string sequence, Transform speaker, Transform listener, bool informParticipants, bool destroyWhenDone)
        {
            SetParticipants(speaker, listener);
            PlaySequence(sequence, informParticipants, destroyWhenDone);
        }

        public void PlaySequence(string sequence, Transform speaker, Transform listener, bool informParticipants, bool destroyWhenDone, bool delayOneFrame)
        {
            if (delayOneFrame)
            {
                StartCoroutine(PlaySequenceAfterOneFrame(sequence, speaker, listener, informParticipants, destroyWhenDone));
            }
            else
            {
                PlaySequence(sequence, speaker, listener, informParticipants, destroyWhenDone);
            }
        }

        public IEnumerator PlaySequenceAfterOneFrame(string sequence, Transform speaker, Transform listener, bool informParticipants, bool destroyWhenDone)
        {
            yield return null;
            PlaySequence(sequence, speaker, listener, informParticipants, destroyWhenDone);
        }

        /// <summary>
        /// Schedules a command to be played.
        /// </summary>
        /// <param name='commandName'>
        /// The command to play. See @ref sequencerCommands for the list of valid commands.
        /// </param>
        /// <param name='required'>
        /// If <c>true</c>, the command will play even if Stop() is called. If this command absolutely must run (for example, 
        /// setting up the final camera angle at the end of the sequence), set required to true.
        /// </param>
        /// <param name='time'>
        /// The time delay in seconds at which to start the command. If time is <c>0</c>, the command starts immediately.
        /// </param>
        /// <param name='args'>
        /// An array of arguments for the command. Pass <c>null</c> if no arguments are required.
        /// </param>
        /// <example>
        /// // At the 2 second mark, cut the camera to a closeup of the listener.
        /// string[] args = new string[] { "Closeup", "listener" };
        /// Play("Camera", true, 2, args);
        /// </example>
        public void PlayCommand(string commandName, bool required, float time, string message, string endMessage, params string[] args)
        {
            PlayCommand(null, commandName, required, time, message, endMessage, args);
        }

        public void PlayCommand(QueuedSequencerCommand commandRecord)
        {
            if (commandRecord == null) return;
            PlayCommand(commandRecord, commandRecord.command, commandRecord.required, commandRecord.startTime, commandRecord.messageToWaitFor,
                commandRecord.endMessage, commandRecord.parameters);
        }

        public void PlayCommand(QueuedSequencerCommand commandRecord, string commandName, bool required, float time, string message, string endMessage, params string[] args)
        {
            if (DialogueDebug.logInfo)
            {
                if (args != null)
                {
                    if (string.IsNullOrEmpty(message) && string.IsNullOrEmpty(endMessage))
                    {
                        Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: Sequencer.Play( {1}{2}({3})@{4} )", new System.Object[] { DialogueDebug.Prefix, (required ? "required " : string.Empty), commandName, string.Join(", ", args), time }));
                    }
                    else if (string.IsNullOrEmpty(endMessage))
                    {
                        Debug.Log(string.Format("{0}: Sequencer.Play( {1}{2}({3})@Message({4}) )", new System.Object[] { DialogueDebug.Prefix, (required ? "required " : string.Empty), commandName, string.Join(", ", args), message }));
                    }
                    else if (string.IsNullOrEmpty(message))
                    {
                        Debug.Log(string.Format("{0}: Sequencer.Play( {1}{2}({3})->Message({4}) )", new System.Object[] { DialogueDebug.Prefix, (required ? "required " : string.Empty), commandName, string.Join(", ", args), endMessage }));
                    }
                    else
                    {
                        Debug.Log(string.Format("{0}: Sequencer.Play( {1}{2}({3})@Message({4})->Message({5}) )", new System.Object[] { DialogueDebug.Prefix, (required ? "required " : string.Empty), commandName, string.Join(", ", args), message, endMessage }));
                    }
                }
            }
            m_isPlaying = true;
            if ((time <= InstantThreshold) && !IsTimePaused() && string.IsNullOrEmpty(message))
            {
                ActivateCommand(commandName, endMessage, speaker, listener, args);
            }
            else
            {
                if (commandRecord != null)
                {
                    commandRecord.startTime += DialogueTime.time;
                    commandRecord.speaker = speaker;
                    commandRecord.listener = listener;
                    m_queuedCommands.Add(commandRecord);
                }
                else
                {
                    m_queuedCommands.Add(new QueuedSequencerCommand(commandName, args, DialogueTime.time + time, message, endMessage, required, speaker, listener));
                }
            }
        }

        private bool IsTimePaused()
        {
            return DialogueTime.isPaused;
        }

        /// <summary>
        /// SequencerCommand can refer to these if they run in Awake.
        /// </summary>
        public static Sequencer s_awakeSequencer;
        public static string s_awakeEndMessage;
        public static Transform s_awakeSpeaker;
        public static Transform s_awakeListener;
        public static string[] s_awakeArgs;

        private void ActivateCommand(string commandName, string endMessage, Transform speaker, Transform listener, string[] args)
        {
            float duration = 0;
            if (string.IsNullOrEmpty(commandName))
            {
                //--- Removed; just a nuisance: if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer received a blank string as a command name", new System.Object[] { DialogueDebug.Prefix }));
            }
            else if (HandleCommandInternally(commandName, args, out duration))
            {
                if (!string.IsNullOrEmpty(endMessage))
                {
                    var guid = Guid.NewGuid().ToString();
                    var coroutine = StartCoroutine(SendTimedSequencerMessage(endMessage, duration, guid));
                    m_timedMessageCoroutines.Add(guid, coroutine);
                }
            }
            else
            {
                System.Type componentType = FindSequencerCommandType(commandName);
                s_awakeSequencer = this;
                s_awakeEndMessage = endMessage;
                s_awakeSpeaker = speaker;
                s_awakeListener = listener;
                s_awakeArgs = args;
                SequencerCommand command = (componentType == null) ? null : gameObject.AddComponent(componentType) as SequencerCommand;
                if (command != null)
                {
                    command.Initialize(this, endMessage, speaker, listener, args);
                    m_activeCommands.Add(command);
                }
                else
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Can't find any built-in sequencer command named {1}() or a sequencer command component named SequencerCommand{1}()", new System.Object[] { DialogueDebug.Prefix, commandName }));
                }
            }
        }

        private System.Type FindSequencerCommandType(string commandName)
        {
            if (m_cachedComponentTypes.ContainsKey(commandName))
            {
                return m_cachedComponentTypes[commandName];
            }
            else
            {
                var component = GetTypeFromName("SequencerCommand" + commandName);
                m_cachedComponentTypes[commandName] = component;
                return component;
            }
        }

        public static void Preload()
        {
            // Cache sequencer commands:
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.Name.StartsWith("SequencerCommand"))
                        {
                            var commandName = type.Name.Substring("SequencerCommand".Length);
                            m_cachedComponentTypes[commandName] = type;
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore exceptions.
                }
            }

            // Call parser to get JIT compilation out of the way for editor and .NET builds:
            var parser = new SequenceParser();
            parser.Parse("None();");

            // Preload default camera angles prefab:
            Resources.Load<GameObject>(DefaultCameraAnglesResourceName);
        }

        public System.Type GetTypeFromName(string typeName)
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                try
                {
                    var types = assembly.GetTypes();
                    for (int j = 0; j < types.Length; j++)
                    {
                        var type = types[j];
                        if (string.Equals(type.Name, typeName)) return type;
                    }
                }
                catch (Exception)
                {
                    // Ignore exceptions.
                }
            }
            return null;
        }

        // Previous method that only looked in specific assemblies:

        //private System.Type FindSequencerCommandType(string commandName, string assemblyName)
        //{
        //    System.Type componentType = FindSequencerCommandType("PixelCrushers.DialogueSystem.SequencerCommands.", commandName, assemblyName);
        //    if (componentType != null) return componentType;
        //    componentType = FindSequencerCommandType("PixelCrushers.DialogueSystem.", commandName, assemblyName);
        //    if (componentType != null) return componentType;
        //    componentType = FindSequencerCommandType(string.Empty, commandName, assemblyName);
        //    return componentType;
        //}

        //private System.Type FindSequencerCommandType(string namespacePrefix, string commandName, string assemblyName)
        //{
        //    string fullPath = string.Format("{0}SequencerCommand{1},{2}", new System.Object[] { namespacePrefix, commandName, assemblyName });
        //    return Type.GetType(fullPath, false);
        //}

        private IEnumerator SendTimedSequencerMessage(string endMessage, float delay, string guid)
        {
            yield return StartCoroutine(DialogueTime.WaitForSeconds(delay));
            if (m_timedMessageCoroutines.ContainsKey(guid)) m_timedMessageCoroutines.Remove(guid);
            OnSequencerMessage(endMessage);
        }

        private void ActivateCommand(QueuedSequencerCommand queuedCommand)
        {
            ActivateCommand(queuedCommand.command, queuedCommand.endMessage, queuedCommand.speaker, queuedCommand.listener, queuedCommand.parameters);
        }

        private void CheckQueuedCommands()
        {
            if ((m_queuedCommands.Count > 0) && !IsTimePaused())
            {
                float now = DialogueTime.time;
                try
                {
                    foreach (var queuedCommand in m_queuedCommands)
                    {
                        if (now >= queuedCommand.startTime) ActivateCommand(queuedCommand.command, queuedCommand.endMessage, queuedCommand.speaker, queuedCommand.listener, queuedCommand.parameters);
                    }
                }
                catch (InvalidOperationException) { } // Allow unusual commands to kill the conversation.
                m_queuedCommands.RemoveAll(queuedCommand => (now >= queuedCommand.startTime));
            }
        }

        public void OnSequencerMessage(string message)
        {
            try
            {
                if ((m_queuedCommands.Count > 0) && !string.IsNullOrEmpty(message))
                {
                    // Activate any queued commands that are waiting for the message:
                    var m_queuedCommandsWaitingForMessage = m_queuedCommands.FindAll(x => string.Equals(message, x.messageToWaitFor));
                    for (int i = 0; i < m_queuedCommandsWaitingForMessage.Count; i++)
                    {
                        var queuedCommand = m_queuedCommandsWaitingForMessage[i];
                        ActivateCommand(queuedCommand.command, queuedCommand.endMessage, queuedCommand.speaker, queuedCommand.listener, queuedCommand.parameters);
                    }
                    // Then delete them from the queue:
                    for (int i = 0; i < m_queuedCommandsWaitingForMessage.Count; i++)
                    {
                        m_queuedCommands.Remove(m_queuedCommandsWaitingForMessage[i]);
                    }
                }
            }
            catch (Exception e)
            {
                // We don't care if the collection is modified:
                bool ignore = (e is InvalidOperationException || e is ArgumentOutOfRangeException);
                if (!ignore) throw;
            }
            finally
            {
                if (receivedMessage != null) receivedMessage(message);
            }
        }

        // Processed in LateUpdate():
        private List<string> queuedMessages = new List<string>();

        private void CheckActiveCommands()
        {
            m_commandsToDelete.Clear();
            if (m_activeCommands.Count > 0)
            {
                var count = m_activeCommands.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    var command = m_activeCommands[i];
                    if (command != null && !command.isPlaying)
                    {
                        if (!string.IsNullOrEmpty(command.endMessage))
                        {
                            // Queue for LateUpdate:
                            queuedMessages.Add(command.endMessage);
                        }
                        m_commandsToDelete.Add(command);
                    }
                }
            }
            for (int i = 0; i < m_commandsToDelete.Count; i++)
            {
                m_activeCommands.Remove(m_commandsToDelete[i]);
                Destroy(m_commandsToDelete[i]);
            }
            m_commandsToDelete.Clear();
        }

        /// <summary>
        /// Stops all scheduled and active commands.
        /// </summary>
        public void Stop()
        {
            StopTimedSequencerMessageCoroutines();
            StopQueued();
            StopActive();
        }

        private void StopTimedSequencerMessageCoroutines()
        {
            foreach (var coroutine in m_timedMessageCoroutines.Values)
            {
                StopCoroutine(coroutine);
            }
            m_timedMessageCoroutines.Clear();
        }

        public void StopQueued()
        {
            if (m_queuedCommands.Count == 0) return;
            // Put the remaining commands in a new list so we can clear m_queuedCommands
            // in case one of the required commands causes another invocation of StopQueued(),
            // such as "required Continue()@Message(X)".
            var commandsToProcess = new List<QueuedSequencerCommand>(m_queuedCommands);
            m_queuedCommands.Clear();
            foreach (var queuedCommand in commandsToProcess)
            {
                if (queuedCommand.required) ActivateCommand(queuedCommand.command, string.Empty, queuedCommand.speaker, queuedCommand.listener, queuedCommand.parameters);
            }
        }

        public void StopActive()
        {
            foreach (var command in m_activeCommands)
            {
                if (command != null)
                {
                    if (!string.IsNullOrEmpty(command.endMessage)) OnSequencerMessage(command.endMessage);
                    StartCoroutine(DestroyAfterOneFrame(command));
                }
            }
            m_activeCommands.Clear();
            m_delayTimeLeft = 0;
        }

        IEnumerator DestroyAfterOneFrame(SequencerCommand command)
        {
            yield return null;
            Destroy(command);
        }

        /// <summary>
        /// Attempts to handles the command internally so the sequencer doesn't have to farm out 
        /// the work to a SequencerCommand component.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this method could handle the command internally; otherwise <c>false</c>.
        /// </returns>
        /// <param name='commandName'>
        /// The command to try to play.
        /// </param>
        /// <param name='args'>
        /// The arguments to the command.
        /// </param>
        private bool HandleCommandInternally(string commandName, string[] args, out float duration)
        {
            duration = 0;
            if (disableInternalSequencerCommands) return false;
            if (string.Equals(commandName, "None") || string.IsNullOrEmpty(commandName))
            {
                return true;
            }
            else if (string.Equals(commandName, "Delay"))
            {
                return HandleDelayInternally(commandName, args, out duration);
            }
            else if (string.Equals(commandName, "Camera"))
            {
                return TryHandleCameraInternally(commandName, args);
            }
            else if (string.Equals(commandName, "Animation"))
            {
                return HandleAnimationInternally(commandName, args, out duration);
            }
            else if (string.Equals(commandName, "AnimatorController"))
            {
                return HandleAnimatorControllerInternally(commandName, args);
            }
            else if (string.Equals(commandName, "AnimatorLayer"))
            {
                return TryHandleAnimatorLayerInternally(commandName, args);
            }
            else if (string.Equals(commandName, "AnimatorBool"))
            {
                return HandleAnimatorBoolInternally(commandName, args);
            }
            else if (string.Equals(commandName, "AnimatorInt"))
            {
                return HandleAnimatorIntInternally(commandName, args);
            }
            else if (string.Equals(commandName, "AnimatorFloat"))
            {
                return TryHandleAnimatorFloatInternally(commandName, args);
            }
            else if (string.Equals(commandName, "AnimatorTrigger"))
            {
                return HandleAnimatorTriggerInternally(commandName, args);
            }
            else if (string.Equals(commandName, "AnimatorPlay"))
            {
                return HandleAnimatorPlayInternally(commandName, args);
            }
            else if (string.Equals(commandName, "Audio"))
            {
                return HandleAudioInternally(commandName, args);
            }
            else if (string.Equals(commandName, "AudioStop"))
            {
                return HandleAudioStopInternally(commandName, args);
            }
            else if (string.Equals(commandName, "ClearSubtitleText"))
            {
                return HandleClearSubtitleText(commandName, args);
            }
            else if (string.Equals(commandName, "MoveTo"))
            {
                return TryHandleMoveToInternally(commandName, args);
            }
            else if (string.Equals(commandName, "LookAt"))
            {
                return TryHandleLookAtInternally(commandName, args);
            }
            else if (string.Equals(commandName, "NavMeshAgent"))
            {
                return HandleNavMeshAgentInternally(commandName, args);
            }
            else if (string.Equals(commandName, "OpenPanel"))
            {
                return HandleOpenPanelInternally(commandName, args);
            }
            else if (string.Equals(commandName, "SendMessage"))
            {
                return HandleSendMessageInternally(commandName, false, args);
            }
            else if (string.Equals(commandName, "SendMessageUpwards"))
            {
                return HandleSendMessageInternally(commandName, true, args);
            }
            else if (string.Equals(commandName, "SetActive"))
            {
                return HandleSetActiveInternally(commandName, args);
            }
            else if (string.Equals(commandName, "SetEnabled"))
            {
                return HandleSetEnabledInternally(commandName, args);
            }
            else if (string.Equals(commandName, "HidePanel"))
            {
                return HandleHidePanelInternally(commandName, args);
            }
            else if (string.Equals(commandName, "SetPanel"))
            {
                return HandleSetPanelInternally(commandName, args);
            }
            else if (string.Equals(commandName, "SetMenuPanel"))
            {
                return HandleSetMenuPanelInternally(commandName, args);
            }
            else if (string.Equals(commandName, "SetDialoguePanel"))
            {
                return HandleSetDialoguePanelInternally(commandName, args);
            }
            else if (string.Equals(commandName, "SetPortrait"))
            {
                return HandleSetPortraitInternally(commandName, args);
            }
            else if (string.Equals(commandName, "SetTimeout"))
            {
                return HandleSetTimeoutInternally(commandName, args);
            }
            else if (string.Equals(commandName, "SetContinueMode"))
            {
                return HandleSetContinueModeInternally(commandName, args);
            }
            else if (string.Equals(commandName, "Continue"))
            {
                return HandleContinueInternally();
            }
            else if (string.Equals(commandName, "SetVariable"))
            {
                return HandleSetVariableInternally(commandName, args);
            }
            else if (string.Equals(commandName, "ShowAlert"))
            {
                return HandleShowAlertInternally(commandName, args);
            }
            else if (string.Equals(commandName, "UpdateTracker"))
            {
                return HandleUpdateTrackerInternally();
            }
            else if (string.Equals(commandName, "RandomizeNextEntry"))
            {
                return HandleRandomizeNextEntryInternally();
            }
            else if (string.Equals(commandName, "StopConversation"))
            {
                return HandleStopConversationInternally();
            }
            else if (string.Equals(commandName, "SequencerMessage"))
            {
                return HandleSequencerMessageInternally(commandName, args);
            }
            return false;
        }

        private bool HandleDelayInternally(string commandName, string[] args, out float duration)
        {
            duration = SequencerTools.GetParameterAsFloat(args, 0);
            m_delayTimeLeft = Mathf.Max(m_delayTimeLeft, duration);
            if (DialogueDebug.logInfo) Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: Sequencer: Delay({1})", new System.Object[] { DialogueDebug.Prefix, duration }));
            return true;
        }

        private bool TryHandleCameraInternally(string commandName, string[] args)
        {
            float duration = SequencerTools.GetParameterAsFloat(args, 2, 0);
            if (duration < InstantThreshold)
            {

                // Handle right now:
                string angle = SequencerTools.GetParameter(args, 0, "default");
                Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), m_speaker, m_listener);

                // Get the angle:
                bool isDefault = string.Equals(angle, "default");
                if (isDefault) angle = SequencerTools.GetDefaultCameraAngle(subject);
                bool isOriginal = string.Equals(angle, "original");
                Transform angleTransform = isOriginal
                    ? m_originalCamera.transform
                    : ((m_cameraAngles != null) ? m_cameraAngles.transform.Find(angle) : null);
                bool isLocalTransform = true;
                if (angleTransform == null)
                {
                    isLocalTransform = false;
                    GameObject go = GameObject.Find(angle);
                    if (go != null) angleTransform = go.transform;
                }

                // Log:
                if (DialogueDebug.logInfo) Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: Sequencer: Camera({1}, {2}, {3}s)", new System.Object[] { DialogueDebug.Prefix, angle, Tools.GetObjectName(subject), duration }));
                if ((angleTransform == null) && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Camera angle '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, angle }));
                if ((subject == null) && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Camera subject '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, SequencerTools.GetParameter(args, 1) }));

                // If we have a camera angle and subject, move the camera to it:
                TakeCameraControl();
                if (isOriginal)
                {
                    sequencerCameraTransform.rotation = originalCameraRotation;
                    sequencerCameraTransform.position = originalCameraPosition;
                }
                else if (angleTransform != null && subject != null)
                {
                    Transform cameraTransform = sequencerCameraTransform;
                    if (isLocalTransform)
                    {
                        cameraTransform.rotation = subject.rotation * angleTransform.localRotation;
                        cameraTransform.position = subject.position + subject.rotation * angleTransform.localPosition;
                    }
                    else
                    {
                        cameraTransform.rotation = angleTransform.rotation;
                        cameraTransform.position = angleTransform.position;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Handles the "Animation(animation[, gameobject|speaker|listener[, finalAnimation]])" action.
        /// 
        /// Arguments:
        /// -# Name of a legacy animation in the Animation component.
        /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
        /// </summary>
        private bool HandleAnimationInternally(string commandName, string[] args, out float duration)
        {
            duration = 0;

            // If the command has >2 args (last is finalAnimation), need to handle in the coroutine version:
            if ((args != null) && (args.Length > 2)) return false;

            string animation = SequencerTools.GetParameter(args, 0);
            Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), m_speaker, m_listener);
            Animation anim = (subject == null) ? null : subject.GetComponent<Animation>();
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: Animation({1}, {2})", new System.Object[] { DialogueDebug.Prefix, animation, Tools.GetObjectName(subject) }));
            if (subject == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Animation() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
            }
            else if (anim == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Animation() command: no Animation component found on {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
            }
            else if (string.IsNullOrEmpty(animation))
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Animation() command: Animation name is blank.", new System.Object[] { DialogueDebug.Prefix }));
            }
            else
            {
                anim.CrossFade(animation);
                duration = (anim[animation] != null) ? anim[animation].length : 0;
            }
            return true;
        }

        /// <summary>
        /// Handles the "AnimatorController(controllerName[, gameobject|speaker|listener])" action.
        /// 
        /// Arguments:
        /// -# Path to an animator controller inside a Resources folder.
        /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
        /// </summary>
        private bool HandleAnimatorControllerInternally(string commandName, string[] args)
        {
            string controllerName = SequencerTools.GetParameter(args, 0);
            Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), m_speaker, m_listener);
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorController({1}, {2})", new System.Object[] { DialogueDebug.Prefix, controllerName, Tools.GetObjectName(subject) }));

            // Load animator controller:
            try
            {
                DialogueManager.LoadAsset(controllerName, typeof(RuntimeAnimatorController),
                    (asset) =>
                    {
                        var animatorControllerAsset = asset as RuntimeAnimatorController;
                        RuntimeAnimatorController animatorController = null;
                        if (animatorControllerAsset != null) animatorController = Instantiate<RuntimeAnimatorController>(animatorControllerAsset);
                        if (subject == null)
                        {
                            if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorController() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
                        }
                        else if (animatorController == null)
                        {
                            if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorController() command: failed to load animator controller '{1}'.", new System.Object[] { DialogueDebug.Prefix, controllerName }));
                        }
                        else
                        {
                            Animator animator = subject.GetComponentInChildren<Animator>();
                            if (animator == null)
                            {
                                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorController() command: No Animator component found on {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
                            }
                            else
                            {
                                animator.runtimeAnimatorController = animatorController;
                            }
                        }
                    });
            }
            catch (Exception)
            {
            }
            return true;
        }

        /// <summary>
        /// Handles the "AnimatorLayer(layerIndex[, weight[, gameobject|speaker|listener[, duration]]])" 
        /// action if duration is zero.
        /// 
        /// Arguments:
        /// -# Index number of a layer on the subject's animator controller. Default: 1.
        /// -# (Optional) New weight. Default: <c>1f</c>.
        /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
        /// -# (Optional) Duration in seconds to smooth to the new weight.
        /// </summary>
        private bool TryHandleAnimatorLayerInternally(string commandName, string[] args)
        {
            float duration = SequencerTools.GetParameterAsFloat(args, 3, 0);
            if (duration < InstantThreshold)
            {

                int layerIndex = SequencerTools.GetParameterAsInt(args, 0, 1);
                float weight = SequencerTools.GetParameterAsFloat(args, 1, 1f);
                Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 2), m_speaker, m_listener);
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorLayer({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, layerIndex, weight, Tools.GetObjectName(subject) }));
                if (subject == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorLayer() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
                }
                else
                {
                    Animator animator = subject.GetComponentInChildren<Animator>();
                    if (animator == null)
                    {
                        if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorLayer(): No Animator component found on {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
                    }
                    else
                    {
                        animator.SetLayerWeight(layerIndex, weight);
                    }
                }
                return true;

            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Handles the "AnimatorBool(animatorParameter[, true|false[, gameobject|speaker|listener]])" action.
        /// 
        /// Arguments:
        /// -# Name of a Mecanim animator parameter.
        /// -# (Optional) True or false. Default: <c>true</c>.
        /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
        /// </summary>
        private bool HandleAnimatorBoolInternally(string commandName, string[] args)
        {
            string animatorParameter = SequencerTools.GetParameter(args, 0);
            bool parameterValue = SequencerTools.GetParameterAsBool(args, 1, true);
            Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 2), m_speaker, m_listener);
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorBool({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, animatorParameter, parameterValue, Tools.GetObjectName(subject) }));
            if (subject == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorBool() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
            }
            else if (string.IsNullOrEmpty(animatorParameter))
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorBool() command: animator parameter name is blank.", new System.Object[] { DialogueDebug.Prefix }));
            }
            else
            {
                Animator animator = subject.GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: No Animator component found on {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
                }
                else
                {
                    animator.SetBool(animatorParameter, parameterValue);
                }
            }
            return true;
        }

        /// <summary>
        /// Handles the "AnimatorInt(animatorParameter[, value[, gameobject|speaker|listener]])" action.
        /// 
        /// Arguments:
        /// -# Name of a Mecanim animator parameter.
        /// -# (Optional) Integer value. Default: <c>1</c>.
        /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
        /// </summary>
        private bool HandleAnimatorIntInternally(string commandName, string[] args)
        {
            string animatorParameter = SequencerTools.GetParameter(args, 0);
            int parameterValue = SequencerTools.GetParameterAsInt(args, 1, 1);
            Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 2), m_speaker, m_listener);
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorInt({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, animatorParameter, parameterValue, Tools.GetObjectName(subject) }));
            if (subject == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorInt() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
            }
            else if (string.IsNullOrEmpty(animatorParameter))
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorInt() command: animator parameter name is blank.", new System.Object[] { DialogueDebug.Prefix }));
            }
            else
            {
                Animator animator = subject.GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: No Animator component found on {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
                }
                else
                {
                    animator.SetInteger(animatorParameter, parameterValue);
                }
            }
            return true;
        }

        /// <summary>
        /// Handles the "AnimatorFloat(animatorParameter[, value[, gameobject|speaker|listener[, duration]]])" 
        /// action if duration is zero.
        /// 
        /// Arguments:
        /// -# Name of a Mecanim animator parameter.
        /// -# (Optional) Float value. Default: <c>1f</c>.
        /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
        /// -# (Optional) Duration in seconds to smooth to the value.
        /// </summary>
        private bool TryHandleAnimatorFloatInternally(string commandName, string[] args)
        {
            float duration = SequencerTools.GetParameterAsFloat(args, 3, 0);
            if (duration < InstantThreshold)
            {

                string animatorParameter = SequencerTools.GetParameter(args, 0);
                float parameterValue = SequencerTools.GetParameterAsFloat(args, 1, 1f);
                Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 2), m_speaker, m_listener);
                if (DialogueDebug.logInfo) Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: Sequencer: AnimatorFloat({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, animatorParameter, parameterValue, Tools.GetObjectName(subject) }));
                if (subject == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorFloat() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
                }
                else if (string.IsNullOrEmpty(animatorParameter))
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorFloat() command: animator parameter name is blank.", new System.Object[] { DialogueDebug.Prefix }));
                }
                else
                {
                    Animator animator = subject.GetComponentInChildren<Animator>();
                    if (animator == null)
                    {
                        if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: No Animator component found on {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
                    }
                    else
                    {
                        animator.SetFloat(animatorParameter, parameterValue);
                    }
                }
                return true;

            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Handles the "AnimatorTrigger(animatorParameter[, gameobject|speaker|listener])" action,
        /// which sets a trigger parameter on a subject's Animator.
        /// 
        /// Arguments:
        /// -# Name of a Mecanim animator state.
        /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
        /// </summary>
        private bool HandleAnimatorTriggerInternally(string commandName, string[] args)
        {
            string animatorParameter = SequencerTools.GetParameter(args, 0);
            Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), m_speaker, m_listener);
            Animator animator = (subject != null) ? subject.GetComponentInChildren<Animator>() : null;
            if (animator == null)
            {
                if (DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: Sequencer: AnimatorTrigger({1}, {2}): No Animator found on {2}", new System.Object[] { DialogueDebug.Prefix, animatorParameter, (subject != null) ? subject.name : SequencerTools.GetParameter(args, 1) }));
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorTrigger({1}, {2})", new System.Object[] { DialogueDebug.Prefix, animatorParameter, subject }));
            }
            if (animator != null) animator.SetTrigger(animatorParameter);
            return true;
        }

        /// <summary>
        /// Handles the "AnimatorPlay(stateName[, gameobject|speaker|listener[, [crossfadeDuration[, layer]]])" action.
        /// 
        /// Arguments:
        /// -# Name of a Mecanim animator state.
        /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
        /// -# (Optional) Crossfade duration. Default: 0 (play immediately).
        /// -# (Optional) Layer. Default: -1 (any layer).
        /// </summary>
        private bool HandleAnimatorPlayInternally(string commandName, string[] args)
        {
            string stateName = SequencerTools.GetParameter(args, 0);
            Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), m_speaker, m_listener);
            float crossfadeDuration = SequencerTools.GetParameterAsFloat(args, 2);
            int layer = SequencerTools.GetParameterAsInt(args, 3, -1);
            bool noactivate = false;
            for (int i = 1; i < args.Length; i++)
            {
                if (string.Equals("noactivate", args[i], StringComparison.OrdinalIgnoreCase))
                {
                    noactivate = true;
                    break;
                }
            }
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorPlay({1}, {2}, fade={3}, layer={4})", new System.Object[] { DialogueDebug.Prefix, stateName, Tools.GetObjectName(subject), crossfadeDuration, layer }));
            if (subject == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorPlay() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
            }
            else if (string.IsNullOrEmpty(stateName))
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorPlay() command: state name is blank.", new System.Object[] { DialogueDebug.Prefix }));
            }
            else
            {
                Animator animator = subject.GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: No Animator component found on {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
                }
                else
                {
                    if (!animator.gameObject.activeSelf && !noactivate) animator.gameObject.SetActive(true);
                    if (animator.gameObject.activeInHierarchy)
                    {
                        if (Tools.ApproximatelyZero(crossfadeDuration))
                        {
                            animator.Play(stateName, layer);
                        }
                        else
                        {
                            animator.CrossFadeInFixedTime(stateName, crossfadeDuration, layer);
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Handles the "Audio(clip[, gameobject|speaker|listener[, oneshot]])" action. This action loads the 
        /// specified clip from Resources into the subject's audio source component and plays it.
        /// 
        /// Arguments:
        /// -# Path to the clip (inside a Resources folder).
        /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. 
        /// Default: speaker.
        /// </summary>
        private bool HandleAudioInternally(string commandName, string[] args)
        {
            string clipName = SequencerTools.GetParameter(args, 0);
            Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), m_speaker, m_listener);
            bool oneshot = SequencerTools.GetParameterAsBool(args, 2, false) ||
                string.Equals("oneshot", SequencerTools.GetParameter(args, 2), StringComparison.OrdinalIgnoreCase);

            // Skip if muted:
            if (SequencerTools.IsAudioMuted())
            {
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: Audio({1}, {2}): skipping; audio is muted", new System.Object[] { DialogueDebug.Prefix, clipName, subject }));
                return true;
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: Audio({1}, {2})", new System.Object[] { DialogueDebug.Prefix, clipName, subject }));
            }

            // Load clip:
            DialogueManager.LoadAsset(clipName, typeof(AudioClip),
                (asset) =>
                {
                    var clip = asset as AudioClip;
                    if ((clip == null) && DialogueDebug.logWarnings && reportMissingAudioFiles) Debug.LogWarning(string.Format("{0}: Sequencer: Audio() command: clip '{1}' could not be found or loaded.", new System.Object[] { DialogueDebug.Prefix, clipName }));

                    // Play clip:
                    if (clip != null)
                    {
                        AudioSource audioSource = SequencerTools.GetAudioSource(subject);
                        if (audioSource == null)
                        {
                            if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Audio() command: can't find or add AudioSource to {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
                        }
                        else if (oneshot)
                        {
                            audioSource.PlayOneShot(clip);
                        }
                        else
                        {
                            audioSource.clip = clip;
                            audioSource.Play();
                        }
                    }
                });
            return true;
        }

        /// <summary>
        /// Handles the "AudioStop([gameobject|speaker|listener])" action. This action stops the
        /// subject's Audio Source.
        /// 
        /// Arguments:
        /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. 
        /// Default: speaker.
        /// </summary>
        private bool HandleAudioStopInternally(string commandName, string[] args)
        {
            Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 0), m_speaker, m_listener);
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: AudioStop({1})", new System.Object[] { DialogueDebug.Prefix, subject }));
            var audioSource = SequencerTools.GetAudioSource(subject);
            if (audioSource != null) audioSource.Stop();
            return true;
        }

        /// <summary>
        /// Tries to handle the "MoveTo(target, [, subject[, duration]])" action. This action matches the 
        /// subject to the target's position and rotation.
        /// 
        /// Arguments:
        /// -# The target. 
        /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. 
        /// -# (Optional) Duration in seconds.
        /// Default: speaker.
        /// </summary>
        private bool TryHandleMoveToInternally(string commandName, string[] args)
        {
            float duration = SequencerTools.GetParameterAsFloat(args, 2, 0);
            if (duration < InstantThreshold)
            {

                // Handle now:
                Transform target = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 0), m_speaker, m_listener);
                Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), m_speaker, m_listener);
                if (DialogueDebug.logInfo) Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: Sequencer: MoveTo({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, target, subject, duration }));
                if ((subject == null) && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: MoveTo() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
                if ((target == null) && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: MoveTo() command: target is null.", new System.Object[] { DialogueDebug.Prefix }));
                if (subject != null && target != null)
                {
                    var subjectRigidbody = subject.GetComponent<Rigidbody>();
                    var navMeshAgent = subject.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (navMeshAgent != null)
                    {
                        navMeshAgent.Warp(target.position);
                        if (subjectRigidbody != null)
                        {
                            subjectRigidbody.MoveRotation(target.rotation);
                        }
                        else
                        {
                            subject.rotation = target.rotation;
                        }
                    }
                    if (subjectRigidbody != null && !subjectRigidbody.isKinematic)
                    {
                        subjectRigidbody.MoveRotation(target.rotation);
                        subjectRigidbody.MovePosition(target.position);
                    }
                    else
                    {
                        subject.position = target.position;
                        subject.rotation = target.rotation;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to handle the "LookAt([target[, subject[, duration[, allAxes]]]])" action. This action
        /// rotates the subject to look at a target. If target is omitted, this action rotates
        /// the speaker and listener to look at each other.
        /// 
        /// Arguments:
        /// -# Target to look at. Can be speaker, listener, or the name of a game object. Default: listener.
        /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
        /// -# (Optional) Duration in seconds.
        /// -# (Optional) allAxes to rotate on all axes (otherwise stays upright).
        /// </summary>
        private bool TryHandleLookAtInternally(string commandName, string[] args)
        {
            float duration = SequencerTools.GetParameterAsFloat(args, 2, 0);
            bool yAxisOnly = (string.Compare(SequencerTools.GetParameter(args, 3), "allAxes", System.StringComparison.OrdinalIgnoreCase) != 0);

            if (duration < InstantThreshold)
            {

                // Handle now:
                if ((args == null) || (args.Length == 0) || ((args.Length == 1) && string.IsNullOrEmpty(args[0])))
                {
                    // Handle empty args (speaker and listener look at each other):
                    if ((m_speaker != null) && (m_listener != null))
                    {
                        if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: LookAt() [speaker<->listener]", new System.Object[] { DialogueDebug.Prefix }));
                        DoLookAt(m_speaker, m_listener.position, yAxisOnly);
                        DoLookAt(m_listener, m_speaker.position, yAxisOnly);
                    }
                }
                else
                {
                    // Otherwise handle subject and target:
                    Transform target = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 0), m_speaker, m_listener);
                    Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), m_speaker, m_listener);
                    if (DialogueDebug.logInfo) Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: Sequencer: LookAt({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, target, subject, duration }));
                    if ((subject == null) && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: LookAt() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
                    if ((target == null) && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: LookAt() command: target is null.", new System.Object[] { DialogueDebug.Prefix }));
                    if ((subject != target) && (subject != null) && (target != null))
                    {
                        DoLookAt(subject, target.position, yAxisOnly);
                    }
                }
                return true;

            }
            else
            {
                return false;
            }
        }

        private void DoLookAt(Transform subject, Vector3 position, bool yAxisOnly)
        {
            if (yAxisOnly)
            {
                subject.LookAt(new Vector3(position.x, subject.position.y, position.z));
            }
            else
            {
                subject.LookAt(position);
            }
        }

        /// <summary>
        /// Handles NavMeshAgent(stop|destination, [agent])
        /// 
        /// - stop|destination: 'stop' stops the agent. Otherwise specify a destination GameObject.
        /// - agent: NavMeshAgent GameObject. Default: speaker.
        /// </summary>
        private bool HandleNavMeshAgentInternally(string commandName, string[] args)
        {
            var stop = string.Equals(SequencerTools.GetParameter(args, 0), "stop", System.StringComparison.OrdinalIgnoreCase);
            var destination = stop ? null : SequencerTools.GetSubject(SequencerTools.GetParameter(args, 0), m_speaker, m_listener);
            var subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), m_speaker, m_listener);
#if UNITY_5_3 || UNITY_5_4
            var navMeshAgent = (subject != null) ? subject.GetComponent<NavMeshAgent>() : null;
#else
            var navMeshAgent = (subject != null) ? subject.GetComponent<UnityEngine.AI.NavMeshAgent>() : null;
#endif
            if (!stop && destination == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Sequencer: NavMeshAgent(" + SequencerTools.GetParameter(args, 0) + "," + SequencerTools.GetParameter(args, 1) + "): Destination not found.");
            }
            else if (navMeshAgent == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Sequencer: NavMeshAgent(" + SequencerTools.GetParameter(args, 0) + "," + SequencerTools.GetParameter(args, 1) + "): NavMeshAgent subject not found.");
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Sequencer: NavMeshAgent(" + SequencerTools.GetParameter(args, 0) + "," + SequencerTools.GetParameter(args, 1) + ")");
                if (!stop) navMeshAgent.SetDestination(destination.position);
#if UNITY_5_3 || UNITY_5_4
                if (stop) navMeshAgent.Stop();
#else
                navMeshAgent.isStopped = stop;
#endif
            }
            return true;
        }

        /// <summary>
        /// Handles the "SendMessage(methodName[, arg[, gameobject|speaker|listener|everyone[, broadcast]]])" action.
        /// This action calls GameObject.SendMessage(methodName, arg) on the subject. Doesn't 
        /// require receiver.
        /// 
        /// Arguments:
        /// -# A methodName to run on the subject.
        /// -# (Optional) A string argument to pass to the method.
        /// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
        /// </summary>
        private bool HandleSendMessageInternally(string commandName, bool upwards, string[] args)
        {
            string methodName = SequencerTools.GetParameter(args, 0);
            string arg = SequencerTools.GetParameter(args, 1);
            string subjectArg = SequencerTools.GetParameter(args, 2);
            bool everyone = string.Equals(subjectArg, "everyone", StringComparison.OrdinalIgnoreCase);
            Transform subject = everyone ? DialogueManager.instance.transform
                : SequencerTools.GetSubject(SequencerTools.GetParameter(args, 2), m_speaker, m_listener);
            bool broadcast = string.Equals(SequencerTools.GetParameter(args, 3), "broadcast", StringComparison.OrdinalIgnoreCase);
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: {1}({2}, {3}, {4}, {5})", new System.Object[] { DialogueDebug.Prefix, commandName, methodName, arg, subject, SequencerTools.GetParameter(args, 3) }));
            if ((subject == null) && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: {1}() command: subject is null.", new System.Object[] { DialogueDebug.Prefix, commandName }));
            if (string.IsNullOrEmpty(methodName) && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: {1}() command: message is blank.", new System.Object[] { DialogueDebug.Prefix, commandName }));
            if (upwards && broadcast && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: {1}() command: 'broadcast' is ignored by SendCommandUpwards.", new System.Object[] { DialogueDebug.Prefix, commandName }));
            if (upwards && everyone && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: {1}() command: 'everyone' is ignored by SendCommandUpwards.", new System.Object[] { DialogueDebug.Prefix, commandName }));
            if (subject != null && !string.IsNullOrEmpty(methodName))
            {
                if (upwards)
                {
                    subject.SendMessageUpwards(methodName, arg, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    if (everyone)
                    {
                        Tools.SendMessageToEveryone(methodName, arg);
                    }
                    else if (broadcast)
                    {
                        subject.BroadcastMessage(methodName, arg, SendMessageOptions.DontRequireReceiver);
                    }
                    else
                    {
                        subject.SendMessage(methodName, arg, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Handles the "SetActive(gameobject[, true|false|flip])" action.
        /// 
        /// Arguments:
        /// -# The name of a game object. Can't be speaker or listener, since they're involved in the conversation.
        /// -# (Optional) true, false, or flip (negate the current value).
        /// </summary>
        private bool HandleSetActiveInternally(string commandName, string[] args)
        {
            var specifier = SequencerTools.GetParameter(args, 0);
            string arg = SequencerTools.GetParameter(args, 1);

            // Special handling for 'tag=':
            if (SequencerTools.SpecifierSpecifiesTag(specifier))
            {
                var tag = SequencerTools.GetSpecifiedTag(specifier);
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SetActive({1}, {2}): (all GameObjects matching tag)", new System.Object[] { DialogueDebug.Prefix, specifier, arg }));
                if (string.Equals(arg, "false", StringComparison.OrdinalIgnoreCase))
                {
                    // Deactivating is easy; just use FindGameObjectsWithTag:
                    var gameObjects = GameObject.FindGameObjectsWithTag(tag);
                    for (int i = 0; i < gameObjects.Length; i++)
                    {
                        gameObjects[i].SetActive(false);
                    }
                }
                else
                {
                    // Activating or flipping requires finding objects even if they're inactive:
                    var gameObjects = Tools.FindGameObjectsWithTagHard(tag);
                    for (int i = 0; i < gameObjects.Length; i++)
                    {
                        var go = gameObjects[i];
                        bool newValue = string.Equals(arg, "flip", StringComparison.OrdinalIgnoreCase) ? !go.activeSelf : true;
                        go.SetActive(newValue);
                    }
                }
                return true;
            }

            var subject = SequencerTools.GetSubject(specifier, speaker, listener); //---Was: SequencerTools.FindSpecifier(specifier);
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SetActive({1}, {2})", new System.Object[] { DialogueDebug.Prefix, subject, arg }));
            if (subject == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetActive() command: subject '{1}' is null.", new System.Object[] { DialogueDebug.Prefix, ((args.Length > 0) ? args[0] : string.Empty) }));
            }
            else if ((subject == m_speaker) || (subject == m_listener))
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetActive() command: subject '{1}' cannot be speaker or listener.", new System.Object[] { DialogueDebug.Prefix, ((args.Length > 0) ? args[0] : string.Empty) }));
            }
            else
            {
                bool newValue = true;
                if (!string.IsNullOrEmpty(arg))
                {
                    if (string.Equals(arg.ToLower(), "false")) newValue = false;
                    else if (string.Equals(arg.ToLower(), "flip")) newValue = !subject.gameObject.activeSelf;
                }
                subject.gameObject.SetActive(newValue);
            }
            return true;
        }

        /// <summary>
        /// Handles the "SetEnabled(component[, true|false|flip[, subject]])" action.
        /// 
        /// Arguments:
        /// -# The name of a component on the subject.
        /// -# (Optional) true, false, or flip (negate the current value).
        /// -# (Optional) The subject (speaker, listener, or the name of a game object); defaults to speaker.
        /// </summary>
        private bool HandleSetEnabledInternally(string commandName, string[] args)
        {
            string componentName = SequencerTools.GetParameter(args, 0);
            string arg = SequencerTools.GetParameter(args, 1);
            string specifier = SequencerTools.GetParameter(args, 2);

            // Special handling for 'tag=':
            if (SequencerTools.SpecifierSpecifiesTag(specifier))
            {
                var tag = SequencerTools.GetSpecifiedTag(specifier);
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SetEnabled({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, componentName, arg, specifier }));
                var gameObjects = GameObject.FindGameObjectsWithTag(tag);
                for (int i = 0; i < gameObjects.Length; i++)
                {
                    var go = gameObjects[i];
                    var comp = (go != null) ? go.GetComponent(componentName) as Component : null;
                    if (comp != null)
                    {
                        Toggle state = Toggle.True;
                        if (!string.IsNullOrEmpty(arg))
                        {
                            if (string.Equals(arg.ToLower(), "false")) state = Toggle.False;
                            else if (string.Equals(arg.ToLower(), "flip")) state = Toggle.Flip;
                        }
                        Tools.SetComponentEnabled(comp, state);
                    }
                }
                return true;
            }

            Transform subject = SequencerTools.GetSubject(specifier, m_speaker, m_listener);
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SetEnabled({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, componentName, arg, subject }));
            if (subject == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetEnabled() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
            }
            else
            {
                Component component = subject.GetComponent(componentName) as Component;
                if (component == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetEnabled() command: component '{1}' not found on {2}.", new System.Object[] { DialogueDebug.Prefix, componentName, subject.name }));
                }
                else
                {
                    Toggle state = Toggle.True;
                    if (!string.IsNullOrEmpty(arg))
                    {
                        if (string.Equals(arg.ToLower(), "false")) state = Toggle.False;
                        else if (string.Equals(arg.ToLower(), "flip")) state = Toggle.Flip;
                    }
                    Tools.SetComponentEnabled(component, state);
                }
            }
            return true;
        }

        /// <summary>
        /// Handles the ClearSubtitleText(panel# | all) sequencer command.
        /// </summary>
        private bool HandleClearSubtitleText(string commandName, string[] args)
        {
            string panelID = SequencerTools.GetParameter(args, 0);
            var all = string.Equals(panelID, "all", StringComparison.OrdinalIgnoreCase);
            var panelNumber = all ? 0 : Tools.StringToInt(panelID);
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: ClearSubtitleText({1})", new System.Object[] { DialogueDebug.Prefix, panelID }));
            var standardDialogueUI = DialogueManager.dialogueUI as StandardDialogueUI;
            if (standardDialogueUI != null)
            {
                if (all)
                {
                    standardDialogueUI.conversationUIElements.ClearAllSubtitleText();
                    //for (int i = 0; i < standardDialogueUI.conversationUIElements.subtitlePanels.Length; i++)
                    //{
                    //    if (standardDialogueUI.conversationUIElements.subtitlePanels[i] == null) continue;
                    //    standardDialogueUI.conversationUIElements.subtitlePanels[i].ClearText();
                    //}
                }
                else if (0 <= panelNumber && panelNumber < standardDialogueUI.conversationUIElements.subtitlePanels.Length &&
                    standardDialogueUI.conversationUIElements.subtitlePanels[panelNumber] != null)
                {
                    standardDialogueUI.conversationUIElements.subtitlePanels[panelNumber].ClearText();
                }
            }
            return true;
        }

        private List<int> m_setDialoguePanelPreviouslyOpenSubtitlePanels = null;
        private List<int> m_setDialoguePanelPreviouslyFocusedSubtitlePanels = null;
        private List<int> m_setDialoguePanelPreviouslyOpenMenuPanels = null;
        private List<bool> m_setDialoguePanelPreviousClearText = null;
        private List<bool> m_setDialoguePanelPreviousContinueButtonStates = null;

        public void SetDialoguePanel(bool show, bool immediate)
        {
            var dialogueUI = DialogueManager.dialogueUI as AbstractDialogueUI;
            if (dialogueUI != null)
            {
                var standardDialogueUI = dialogueUI as StandardDialogueUI;
                if (show)
                {
                    // Show dialogue panel:
                    dialogueUI.dialogueControls.Show();

                    // Also re-open any recorded previously-open panels:
                    if (standardDialogueUI != null)
                    {
                        if (m_setDialoguePanelPreviouslyOpenSubtitlePanels != null)
                        {
                            for (int i = 0; i < m_setDialoguePanelPreviouslyOpenSubtitlePanels.Count; i++)
                            {
                                var subtitlePanelNumber = m_setDialoguePanelPreviouslyOpenSubtitlePanels[i];
                                standardDialogueUI.conversationUIElements.subtitlePanels[subtitlePanelNumber].panelState = UIPanel.PanelState.Closed;
                                standardDialogueUI.conversationUIElements.subtitlePanels[subtitlePanelNumber].Open();
                                standardDialogueUI.conversationUIElements.subtitlePanels[subtitlePanelNumber].ActivateUIElements();
                                if (m_setDialoguePanelPreviouslyFocusedSubtitlePanels != null &&
                                    m_setDialoguePanelPreviouslyFocusedSubtitlePanels.Contains(subtitlePanelNumber))
                                {
                                    standardDialogueUI.conversationUIElements.subtitlePanels[subtitlePanelNumber].Focus();
                                }
                                standardDialogueUI.conversationUIElements.subtitlePanels[subtitlePanelNumber].clearTextOnClose = m_setDialoguePanelPreviousClearText[subtitlePanelNumber];
                                if (m_setDialoguePanelPreviousContinueButtonStates[subtitlePanelNumber] == true) standardDialogueUI.conversationUIElements.subtitlePanels[subtitlePanelNumber].ShowContinueButton();
                            }
                        }
                        if (m_setDialoguePanelPreviouslyOpenMenuPanels != null)
                        {
                            for (int i = 0; i < m_setDialoguePanelPreviouslyOpenMenuPanels.Count; i++)
                            {
                                var menuPanelNumber = m_setDialoguePanelPreviouslyOpenMenuPanels[i];
                                standardDialogueUI.conversationUIElements.menuPanels[menuPanelNumber].panelState = UIPanel.PanelState.Closed;
                                standardDialogueUI.conversationUIElements.menuPanels[menuPanelNumber].Open();
                            }
                        }
                    }
                }
                else
                {
                    // Record currently open panels:
                    if (standardDialogueUI != null)
                    {
                        if (m_setDialoguePanelPreviouslyOpenMenuPanels == null) m_setDialoguePanelPreviouslyOpenMenuPanels = new List<int>();
                        if (m_setDialoguePanelPreviouslyOpenSubtitlePanels == null) m_setDialoguePanelPreviouslyOpenSubtitlePanels = new List<int>();
                        if (m_setDialoguePanelPreviouslyFocusedSubtitlePanels == null) m_setDialoguePanelPreviouslyFocusedSubtitlePanels = new List<int>();
                        if (m_setDialoguePanelPreviousClearText == null) m_setDialoguePanelPreviousClearText = new List<bool>();
                        if (m_setDialoguePanelPreviousContinueButtonStates == null) m_setDialoguePanelPreviousContinueButtonStates = new List<bool>();
                        m_setDialoguePanelPreviouslyOpenMenuPanels.Clear();
                        m_setDialoguePanelPreviouslyOpenSubtitlePanels.Clear();
                        m_setDialoguePanelPreviouslyFocusedSubtitlePanels.Clear();
                        m_setDialoguePanelPreviousClearText.Clear();
                        m_setDialoguePanelPreviousContinueButtonStates.Clear();
                        for (int i = 0; i < standardDialogueUI.conversationUIElements.subtitlePanels.Length; i++)
                        {
                            if (standardDialogueUI.conversationUIElements.subtitlePanels[i] == null) continue;
                            m_setDialoguePanelPreviousClearText.Add(standardDialogueUI.conversationUIElements.subtitlePanels[i].clearTextOnClose);
                            m_setDialoguePanelPreviousContinueButtonStates.Add(standardDialogueUI.conversationUIElements.subtitlePanels[i].continueButton != null && standardDialogueUI.conversationUIElements.subtitlePanels[i].continueButton.gameObject.activeInHierarchy);
                            standardDialogueUI.conversationUIElements.subtitlePanels[i].clearTextOnClose = false;
                            if (standardDialogueUI.conversationUIElements.subtitlePanels[i].isOpen &&
                                standardDialogueUI.conversationUIElements.subtitlePanels[i].panelState != UIPanel.PanelState.Closing)
                            {
                                if (standardDialogueUI.conversationUIElements.subtitlePanels[i].hasFocus)
                                {
                                    m_setDialoguePanelPreviouslyFocusedSubtitlePanels.Add(i);
                                }
                                if (immediate) standardDialogueUI.conversationUIElements.subtitlePanels[i].HideImmediate();
                                else standardDialogueUI.conversationUIElements.subtitlePanels[i].Close();
                                m_setDialoguePanelPreviouslyOpenSubtitlePanels.Add(i);
                            }
                        }
                        for (int i = 0; i < standardDialogueUI.conversationUIElements.menuPanels.Length; i++)
                        {
                            if (standardDialogueUI.conversationUIElements.menuPanels[i] == null) continue;
                            if (standardDialogueUI.conversationUIElements.menuPanels[i].isOpen)
                            {
                                m_setDialoguePanelPreviouslyOpenMenuPanels.Add(i);
                                if (immediate) standardDialogueUI.conversationUIElements.menuPanels[i].HideImmediate();
                                else standardDialogueUI.conversationUIElements.menuPanels[i].Close();
                            }
                        }
                        if (immediate) standardDialogueUI.conversationUIElements.HideImmediate();
                    }

                    // Then hide dialogue panel:
                    dialogueUI.dialogueControls.Hide();
                }
            }
        }

        /// <summary>
        /// Handles the "SetDialoguePanel(true|false)" action.
        /// 
        /// Arguments:
        /// -# true|false: Show or hide the main dialogue panel.
        /// </summary>
        private bool HandleSetDialoguePanelInternally(string commandName, string[] args)
        {
            var arg = SequencerTools.GetParameter(args, 0);
            if (!(string.Equals(arg, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(arg, "false", StringComparison.OrdinalIgnoreCase)))
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetDialoguePanel({1}): Parameter must be true or false", new System.Object[] { DialogueDebug.Prefix, arg }));
                return true;
            }
            var show = string.Equals(arg, "true", StringComparison.OrdinalIgnoreCase);
            var immediate = string.Equals(SequencerTools.GetParameter(args, 1), "immediate", StringComparison.OrdinalIgnoreCase);
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SetDialoguePanel({1})", new System.Object[] { DialogueDebug.Prefix, show }));
            SetDialoguePanel(show, immediate);
            //var dialogueUI = DialogueManager.dialogueUI as AbstractDialogueUI;
            //if (dialogueUI != null)
            //{
            //    var standardDialogueUI = dialogueUI as StandardDialogueUI;
            //    if (show)
            //    {
            //        // Show dialogue panel:
            //        dialogueUI.dialogueControls.Show();

            //        // Also re-open any recorded previously-open panels:
            //        if (standardDialogueUI != null)
            //        {
            //            if (m_setDialoguePanelPreviouslyOpenSubtitlePanels != null)
            //            {
            //                for (int i = 0; i < m_setDialoguePanelPreviouslyOpenSubtitlePanels.Count; i++)
            //                {
            //                    var subtitlePanelNumber = m_setDialoguePanelPreviouslyOpenSubtitlePanels[i];
            //                    standardDialogueUI.conversationUIElements.subtitlePanels[subtitlePanelNumber].panelState = UIPanel.PanelState.Closed;
            //                    standardDialogueUI.conversationUIElements.subtitlePanels[subtitlePanelNumber].Open();
            //                }
            //            }
            //            if (m_setDialoguePanelPreviouslyOpenMenuPanels != null)
            //            {
            //                for (int i = 0; i < m_setDialoguePanelPreviouslyOpenMenuPanels.Count; i++)
            //                {
            //                    var menuPanelNumber = m_setDialoguePanelPreviouslyOpenMenuPanels[i];
            //                    standardDialogueUI.conversationUIElements.menuPanels[menuPanelNumber].panelState = UIPanel.PanelState.Closed;
            //                    standardDialogueUI.conversationUIElements.menuPanels[menuPanelNumber].Open();
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        // Record currently open panels:
            //        if (standardDialogueUI != null)
            //        {
            //            if (m_setDialoguePanelPreviouslyOpenMenuPanels == null) m_setDialoguePanelPreviouslyOpenMenuPanels = new List<int>();
            //            if (m_setDialoguePanelPreviouslyOpenSubtitlePanels == null) m_setDialoguePanelPreviouslyOpenSubtitlePanels = new List<int>();
            //            m_setDialoguePanelPreviouslyOpenMenuPanels.Clear();
            //            m_setDialoguePanelPreviouslyOpenSubtitlePanels.Clear();
            //            for (int i = 0; i < standardDialogueUI.conversationUIElements.subtitlePanels.Length; i++)
            //            {
            //                if (standardDialogueUI.conversationUIElements.subtitlePanels[i] == null) continue;
            //                if (standardDialogueUI.conversationUIElements.subtitlePanels[i].isOpen)
            //                {
            //                    if (immediate) standardDialogueUI.conversationUIElements.subtitlePanels[i].HideImmediate();
            //                    else standardDialogueUI.conversationUIElements.subtitlePanels[i].Close();
            //                    m_setDialoguePanelPreviouslyOpenSubtitlePanels.Add(i);
            //                }
            //            }
            //            for (int i = 0; i < standardDialogueUI.conversationUIElements.menuPanels.Length; i++)
            //            {
            //                if (standardDialogueUI.conversationUIElements.menuPanels[i] == null) continue;
            //                if (standardDialogueUI.conversationUIElements.menuPanels[i].isOpen)
            //                {
            //                    m_setDialoguePanelPreviouslyOpenMenuPanels.Add(i);
            //                    if (immediate) standardDialogueUI.conversationUIElements.menuPanels[i].HideImmediate();
            //                    else standardDialogueUI.conversationUIElements.menuPanels[i].Close();
            //                }
            //            }
            //            if (immediate) standardDialogueUI.conversationUIElements.HideImmediate();
            //        }

            //        // Then hide dialogue panel:
            //        dialogueUI.dialogueControls.Hide();
            //    }
            //}
            return true;
        }

        /// <summary>
        /// Handles the "OpenPanel(panelNum, [open|close|focus|unfocus])" action.
        /// 
        /// Arguments:
        /// -# The panel number or 'default' or 'bark'.
        /// -# The state to put the panel in. Default: open.
        /// </summary>
        private bool HandleOpenPanelInternally(string commandName, string[] args)
        {
            string panelID = SequencerTools.GetParameter(args, 0);
            var subtitlePanelNumber = string.Equals(panelID, "default", StringComparison.OrdinalIgnoreCase) ? SubtitlePanelNumber.Default
                            : string.Equals(panelID, "bark", StringComparison.OrdinalIgnoreCase) ? SubtitlePanelNumber.UseBarkUI
                            : PanelNumberUtility.IntToSubtitlePanelNumber(Tools.StringToInt(panelID));
            var mode = SequencerTools.GetParameter(args, 1);
            if (string.IsNullOrEmpty(mode)) mode = "open";
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: OpenPanel({1}, {2})", new System.Object[] { DialogueDebug.Prefix, subtitlePanelNumber, mode }));
            var standardDialogueUI = DialogueManager.dialogueUI as StandardDialogueUI;
            if (standardDialogueUI != null)
            {
                var panels = standardDialogueUI.conversationUIElements.subtitlePanels;
                var i = PanelNumberUtility.GetSubtitlePanelIndex(subtitlePanelNumber);
                if (0 <= i && i < panels.Length)
                {
                    var panel = panels[i];
                    if (string.Equals("open", mode, StringComparison.OrdinalIgnoreCase))
                    {
                        standardDialogueUI.conversationUIElements.standardSubtitleControls.OpenSubtitlePanelLikeStart(subtitlePanelNumber);
                    }
                    else if (string.Equals("close", mode, StringComparison.OrdinalIgnoreCase))
                    {
                        panel.Close();
                    }
                    else if (string.Equals("focus", mode, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!panel.isOpen) panel.Open();
                        panel.Focus();
                    }
                    else if (string.Equals("unfocus", mode, StringComparison.OrdinalIgnoreCase))
                    {
                        panel.Unfocus();
                    }
                    else
                    {
                        if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: OpenPanel({1}, {2}): Unrecognized mode.", new System.Object[] { DialogueDebug.Prefix, subtitlePanelNumber, mode }));
                    }
                }
                else
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: OpenPanel({1}, {2}): Invalid panel number.", new System.Object[] { DialogueDebug.Prefix, subtitlePanelNumber, mode }));
                }
            }
            else
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: OpenPanel({1}, {2}): Current dialogue UI is not a Standard Dialogue UI.", new System.Object[] { DialogueDebug.Prefix, subtitlePanelNumber, mode }));
            }
            return true;
        }

        private bool HandleHidePanelInternally(string commandName, string[] args)
        {
            var panelNumber = SequencerTools.GetParameterAsInt(args, 0);
            var portraitOnly = string.Equals("portrait", SequencerTools.GetParameter(args, 1), StringComparison.OrdinalIgnoreCase);
            var portraitImageOnly = string.Equals("portraitimage", SequencerTools.GetParameter(args, 1), StringComparison.OrdinalIgnoreCase);
            var dialogueUI = DialogueManager.dialogueUI as StandardDialogueUI;
            var commandSummary = "HidePanel(" + panelNumber + (portraitOnly ? ", portrait" : string.Empty) + ")";
            if (dialogueUI == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Sequencer: " + commandSummary + " can't run. Not using a Standard Dialogue UI.");
            }
            else if (!(0 <= panelNumber && panelNumber < dialogueUI.conversationUIElements.subtitlePanels.Length))
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Sequencer: " + commandSummary + "dialogue UI doesn't have panel #" + panelNumber + ".");
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Sequencer: " + commandSummary);
                var panel = dialogueUI.conversationUIElements.subtitlePanels[panelNumber];
                if (panel == null) return true;
                if (portraitOnly)
                {
                    Tools.SetGameObjectActive(panel.portraitImage, false);
                    Tools.SetGameObjectActive(panel.portraitName.gameObject, false);
                }
                else if (portraitImageOnly)
                {
                    Tools.SetGameObjectActive(panel.portraitImage, false);
                }
                else
                {
                    panel.Close();
                }
            }
            return true;
        }

        /// <summary>
        /// Handles the "SetPanel(actorName, panelNum)" action.
        /// 
        /// Arguments:
        /// -# The name of a GameObject or actor in the dialogue database. Default: speaker.
        /// -# The panel number or 'default' or 'bark'.
        /// </summary>
        private bool HandleSetPanelInternally(string commandName, string[] args)
        {
            string actorName = SequencerTools.GetParameter(args, 0);
            var actorTransform = CharacterInfo.GetRegisteredActorTransform(actorName) ?? SequencerTools.GetSubject(actorName, speaker, listener, speaker);
            string panelID = SequencerTools.GetParameter(args, 1);
            var subtitlePanelNumber = string.Equals(panelID, "default", StringComparison.OrdinalIgnoreCase) ? SubtitlePanelNumber.Default
                            : string.Equals(panelID, "bark", StringComparison.OrdinalIgnoreCase) ? SubtitlePanelNumber.UseBarkUI
                            : PanelNumberUtility.IntToSubtitlePanelNumber(Tools.StringToInt(panelID));
            var dialogueActor = (actorTransform != null) ? actorTransform.GetComponent<DialogueActor>() : null;
            if (dialogueActor != null)
            {
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SetPanel({1}, {2})", new System.Object[] { DialogueDebug.Prefix, actorTransform, subtitlePanelNumber }), actorTransform);
                dialogueActor.SetSubtitlePanelNumber(subtitlePanelNumber);
            }
            var actor = DialogueManager.masterDatabase.GetActor((dialogueActor != null && !string.IsNullOrEmpty(dialogueActor.actor)) ? dialogueActor.actor : actorName);
            if (actor == null)
            {
                if (dialogueActor == null && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetPanel({1}, {2}): No actor named {1}", new System.Object[] { DialogueDebug.Prefix, actorName, subtitlePanelNumber }));
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SetPanel({1}, {2})", new System.Object[] { DialogueDebug.Prefix, actorName, subtitlePanelNumber }));
                var standardDialogueUI = DialogueManager.dialogueUI as IStandardDialogueUI;
                if (standardDialogueUI != null)
                {
                    standardDialogueUI.OverrideActorPanel(actor, subtitlePanelNumber);
                }
            }
            return true;
        }

        /// <summary>
        /// Handles the "SetMenuPanel(actorName, panelNum)" action.
        /// 
        /// Arguments:
        /// -# The name of a GameObject or actor in the dialogue database. Default: speaker.
        /// -# The panel number or 'default'.
        /// </summary>
        private bool HandleSetMenuPanelInternally(string commandName, string[] args)
        {
            string actorName = SequencerTools.GetParameter(args, 0);
            Transform actorTransform = null;
            if (string.IsNullOrEmpty(actorName) || string.Equals(SequencerKeywords.Speaker, actorName, StringComparison.OrdinalIgnoreCase))
            {
                actorTransform = speaker;
            }
            else if (string.Equals(SequencerKeywords.Listener, actorName, StringComparison.OrdinalIgnoreCase))
            {
                actorTransform = listener;
            }
            else
            {
                actorTransform = CharacterInfo.GetRegisteredActorTransform(actorName);
                if (actorTransform == null)
                {
                    var actorGO = GameObject.Find(actorName);
                    if (actorGO != null) actorTransform = actorGO.transform;
                }
            }
            string panelID = SequencerTools.GetParameter(args, 1);
            var menuPanelNumber = string.Equals(panelID, "default", StringComparison.OrdinalIgnoreCase) ? MenuPanelNumber.Default
                            : PanelNumberUtility.IntToMenuPanelNumber(Tools.StringToInt(panelID));
            var dialogueActor = (actorTransform != null) ? actorTransform.GetComponent<DialogueActor>() : null;

            if (actorTransform != null)
            {
                // Prefer to override menu panel by transform / DialogueActor:
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SetMenuPanel({1}, {2})", new System.Object[] { DialogueDebug.Prefix, actorTransform, menuPanelNumber }), actorTransform);
                if (dialogueActor != null)
                {
                    dialogueActor.SetMenuPanelNumber(menuPanelNumber); // Also sets dialogue UI.
                }
                else
                {
                    var standardDialogueUI = DialogueManager.dialogueUI as IStandardDialogueUI;
                    if (standardDialogueUI != null)
                    {
                        standardDialogueUI.OverrideActorMenuPanel(actorTransform, menuPanelNumber, null);
                    }
                }
                return true;
            }
            else
            {
                // If no transform, override menu panel by actor ID:
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SetMenuPanel({1}, {2})", new System.Object[] { DialogueDebug.Prefix, actorName, menuPanelNumber }));
                if (string.Equals(actorName, SequencerKeywords.Speaker, StringComparison.OrdinalIgnoreCase))
                {
                    var conversation = DialogueManager.masterDatabase.GetConversation(DialogueManager.lastConversationID);
                    if (conversation != null) actorName = DialogueManager.masterDatabase.GetActor(conversation.ActorID).Name;
                }
                var actor = DialogueManager.masterDatabase.GetActor(actorName);
                var standardDialogueUI = DialogueManager.dialogueUI as StandardDialogueUI;
                if (actor != null && standardDialogueUI != null)
                {
                    standardDialogueUI.OverrideActorMenuPanel(actor, menuPanelNumber, null);
                    return true;
                }
            }
            if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetMenuPanel({1}, {2}): Requires a DialogueActor or GameObject named {1}", new System.Object[] { DialogueDebug.Prefix, actorName, menuPanelNumber }));
            return false;
        }

        /// <summary>
        /// Handles the "SetPortrait(actorName, textureName)" action.
        /// 
        /// Arguments:
        /// -# The name of an actor in the dialogue database.
        /// -# The name of a texture that can be loaded from Resources, or 'default', 
        /// or 'pic=#' or 'pic=varName'.
        /// </summary>
        private bool HandleSetPortraitInternally(string commandName, string[] args)
        {
            string actorName = SequencerTools.GetParameter(args, 0);
            string textureName = SequencerTools.GetParameter(args, 1);
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SetPortrait({1}, {2})", new System.Object[] { DialogueDebug.Prefix, actorName, textureName }));
            Actor actor = DialogueManager.masterDatabase.GetActor(actorName);
            if (actor == null)
            {
                var actorGameObject = SequencerTools.GetSubject(actorName, speaker, listener, speaker);
                actor = DialogueManager.masterDatabase.GetActor(DialogueActor.GetActorName(actorGameObject));
                if (actor != null) actorName = actor.Name;
            }
            bool isDefault = string.Equals(textureName, "default");
            bool isPicTag = (textureName != null) && textureName.StartsWith("pic=");
            Sprite sprite = null;
            if (isDefault)
            {
                sprite = null;
            }
            else if (isPicTag)
            {
                string picValue = textureName.Substring("pic=".Length);
                int picNumber;
                if (!int.TryParse(picValue, out picNumber))
                {
                    if (DialogueLua.DoesVariableExist(picValue))
                    {
                        picNumber = DialogueLua.GetVariable(picValue).asInt;
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("{0}: Sequencer: SetPortrait() command: pic variable '{1}' not found.", new System.Object[] { DialogueDebug.Prefix, picValue }));
                    }
                }
                sprite = (actor != null) ? actor.GetPortraitSprite(picNumber) : null;
            }
            else
            {
                if (actor == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetPortrait() command: actor '{1}' not found.", new System.Object[] { DialogueDebug.Prefix, actorName }));
                }
                else if (isDefault)
                {
                    DialogueLua.SetActorField(actorName, DialogueSystemFields.CurrentPortrait, string.Empty);
                }
                else
                {
                    DialogueManager.LoadAsset(textureName, typeof(Sprite),
                    (asset) =>
                    {
                        var spriteAsset = asset as Sprite;
                        if (spriteAsset != null)
                        {
                            DialogueLua.SetActorField(actorName, DialogueSystemFields.CurrentPortrait, textureName);
                            DialogueManager.instance.SetActorPortraitSprite(actorName, spriteAsset);
                        }
                        else
                        {
                            DialogueManager.LoadAsset(textureName, typeof(Texture2D),
                            (textureAsset) =>
                            {
                                spriteAsset = UITools.CreateSprite(textureAsset as Texture2D);
                                if (spriteAsset == null)
                                {
                                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetPortrait() command: sprite/texture '{1}' not found.", new System.Object[] { DialogueDebug.Prefix, textureName }));
                                }
                                DialogueLua.SetActorField(actorName, DialogueSystemFields.CurrentPortrait, textureName);
                                DialogueManager.instance.SetActorPortraitSprite(actorName, spriteAsset);
                            });
                        }
                    });
                    return true;
                }
            }

            if (DialogueDebug.logWarnings)
            {
                if (actor == null) Debug.LogWarning(string.Format("{0}: Sequencer: SetPortrait() command: actor '{1}' not found.", new System.Object[] { DialogueDebug.Prefix, actorName }));
                if ((sprite == null) && !isDefault) Debug.LogWarning(string.Format("{0}: Sequencer: SetPortrait() command: texture '{1}' not found.", new System.Object[] { DialogueDebug.Prefix, textureName }));
            }
            if (actor != null)
            {
                if (isDefault)
                {
                    DialogueLua.SetActorField(actorName, DialogueSystemFields.CurrentPortrait, string.Empty);
                }
                else
                {
                    if (sprite != null) DialogueLua.SetActorField(actorName, DialogueSystemFields.CurrentPortrait, textureName);
                }
                DialogueManager.instance.SetActorPortraitSprite(actorName, sprite);
            }
            return true;
        }

        /// <summary>
        /// Handles the "SetMenuPanel(actorName, panelNum)" action.
        /// 
        /// Arguments:
        /// -# The name of a GameObject or actor in the dialogue database. Default: speaker.
        /// -# The panel number or 'default'.
        /// </summary>
        private bool HandleSetTimeoutInternally(string commandName, string[] args)
        {
            float duration = SequencerTools.GetParameterAsFloat(args, 0);
            if (DialogueDebug.logInfo) Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: Sequencer: SetTimeout({1})", DialogueDebug.Prefix, duration));
            if (DialogueManager.displaySettings != null && DialogueManager.displaySettings.inputSettings != null)
            {
                DialogueManager.displaySettings.inputSettings.responseTimeout = duration;
            }
            return true;
        }

        private static DisplaySettings.SubtitleSettings.ContinueButtonMode savedContinueButtonMode = DisplaySettings.SubtitleSettings.ContinueButtonMode.Always;

        /// <summary>
        /// Handles "SetContinueMode(true|false)".
        /// </summary>
        private bool HandleSetContinueModeInternally(string commandName, string[] args)
        {
            if (args == null || args.Length < 1)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetContinueMode(true|false|original) requires a true/false/original parameter", new System.Object[] { DialogueDebug.Prefix }));
                return true;
            }
            else
            {
                var arg = SequencerTools.GetParameter(args, 0);
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SetContinueMode({1})", new System.Object[] { DialogueDebug.Prefix, arg }));
                if (DialogueManager.instance == null || DialogueManager.displaySettings == null || DialogueManager.displaySettings.subtitleSettings == null) return true;
                if (string.Equals(arg, "original", StringComparison.OrdinalIgnoreCase))
                {
                    // Restore original mode:
                    if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SetContinueMode({1}): Restoring original mode {2}", new System.Object[] { DialogueDebug.Prefix, arg, savedContinueButtonMode }));
                    DialogueManager.displaySettings.subtitleSettings.continueButton = savedContinueButtonMode;
                }
                else
                {
                    // Set requested mode:
                    DisplaySettings.SubtitleSettings.ContinueButtonMode mode;
                    if (TryGetContinueMode(arg, out mode))
                    {
                        savedContinueButtonMode = DialogueManager.displaySettings.subtitleSettings.continueButton;
                        DialogueManager.displaySettings.subtitleSettings.continueButton = mode;
                    }
                    else
                    {
                        if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetContinueMode(true|false|original|...) requires a valid mode. See online manual for options.", new System.Object[] { DialogueDebug.Prefix }));
                        return true;
                    }
                }
                // If a conversation is open, update its continue button mode immediately:
                if (DialogueManager.conversationView != null)
                {
                    if (DialogueManager.conversationView.displaySettings.conversationOverrideSettings != null)
                    {
                        DialogueManager.conversationView.displaySettings.conversationOverrideSettings.continueButton = DialogueManager.displaySettings.subtitleSettings.continueButton;
                    }
                    DialogueManager.conversationView.SetupContinueButton();
                }
                return true;
            }
        }

        private bool TryGetContinueMode(string arg, out DisplaySettings.SubtitleSettings.ContinueButtonMode mode)
        {
            if (string.Equals(arg, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(arg, "always", StringComparison.OrdinalIgnoreCase))
            {
                mode = DisplaySettings.SubtitleSettings.ContinueButtonMode.Always;
            }
            else if (string.Equals(arg, "false", StringComparison.OrdinalIgnoreCase) || string.Equals(arg, "never", StringComparison.OrdinalIgnoreCase))
            {
                mode = DisplaySettings.SubtitleSettings.ContinueButtonMode.Never;
            }
            else if (string.Equals(arg, "optional", StringComparison.OrdinalIgnoreCase))
            {
                mode = DisplaySettings.SubtitleSettings.ContinueButtonMode.Optional;
            }
            else
            {
                mode = DisplaySettings.SubtitleSettings.ContinueButtonMode.Never;
                var found = false;
                var enumValues = System.Enum.GetValues(typeof(DisplaySettings.SubtitleSettings.ContinueButtonMode));
                for (int i = 0; i < enumValues.Length; i++)
                {
                    var enumMode = (DisplaySettings.SubtitleSettings.ContinueButtonMode)i;
                    if (string.Equals(arg, enumMode.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        mode = enumMode;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Handles "Continue()", which simulates a continue button click.
        /// </summary>
        /// <returns></returns>
        private bool HandleContinueInternally()
        {
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: Continue()", new System.Object[] { DialogueDebug.Prefix }));
            DialogueManager.instance.BroadcastMessage("OnConversationContinueAll", SendMessageOptions.DontRequireReceiver);
            return true;
        }

        /// <summary>
        /// Handles "SetVariable(variableName, value)".
        /// Thanks to Darkkingdom for this sequencer command!
        /// </summary>
        private bool HandleSetVariableInternally(string commandName, string[] args)
        {
            if (args == null || args.Length < 2)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetVariable(variableName, value) requires two parameters", new System.Object[] { DialogueDebug.Prefix }));
            }
            else
            {
                var variableName = SequencerTools.GetParameter(args, 0);
                var variableValue = SequencerTools.GetParameter(args, 1);
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SetVariable({1}, {2})", new System.Object[] { DialogueDebug.Prefix, variableName, variableValue }));
                bool boolValue;
                float floatValue;
                if (bool.TryParse(variableValue, out boolValue))
                {
                    DialogueLua.SetVariable(variableName, boolValue);
                }
                else if (float.TryParse(variableValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out floatValue))
                {
                    DialogueLua.SetVariable(variableName, floatValue);
                }
                else
                {
                    DialogueLua.SetVariable(variableName, variableValue);
                }
            }
            return true;
        }

        /// <summary>
        /// Handles the "ShowAlert([duration])" action.
        /// 
        /// Arguments:
        /// -# (Optional) Duration.
        /// </summary>
        private bool HandleShowAlertInternally(string commandName, string[] args)
        {
            bool hasDuration = ((args.Length > 0) && !string.IsNullOrEmpty(args[0]));
            float duration = hasDuration ? SequencerTools.GetParameterAsFloat(args, 0) : 0;
            if (DialogueDebug.logInfo)
            {
                if (hasDuration)
                {
                    Debug.Log(string.Format("{0}: Sequencer: ShowAlert({1})", new System.Object[] { DialogueDebug.Prefix, duration }));
                }
                else
                {
                    Debug.Log(string.Format("{0}: Sequencer: ShowAlert()", new System.Object[] { DialogueDebug.Prefix }));
                }
            }
            try
            {
                string message = Lua.Run("return Variable['Alert']").asString;
                if (!string.IsNullOrEmpty(message))
                {
                    Lua.Run("Variable['Alert'] = ''");
                    if (hasDuration)
                    {
                        DialogueManager.ShowAlert(message, duration);
                    }
                    else
                    {
                        DialogueManager.ShowAlert(message);
                    }
                }
            }
            catch (Exception)
            {
            }
            return true;
        }

        /// <summary>
        /// Handles the "UpdateTracker()" command.
        /// </summary>
        private bool HandleUpdateTrackerInternally()
        {
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: UpdateTracker()", new System.Object[] { DialogueDebug.Prefix }));
            DialogueManager.SendUpdateTracker();
            return true;
        }

        private bool HandleRandomizeNextEntryInternally()
        {
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: RandomizeNextEntry()", new System.Object[] { DialogueDebug.Prefix }));
            if (DialogueManager.conversationController != null) DialogueManager.conversationController.randomizeNextEntry = true;
            return true;
        }

        private bool HandleStopConversationInternally()
        {
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: StopConversation()", new System.Object[] { DialogueDebug.Prefix }));
            DialogueManager.StopConversation();
            return true;
        }

        // Note: Sends globally.
        private bool HandleSequencerMessageInternally(string commandName, string[] args)
        {
            var message = SequencerTools.GetParameter(args, 0);
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Sequencer: SequencerMessage({1})", new System.Object[] { DialogueDebug.Prefix, message }));
            if (!string.IsNullOrEmpty(message))
            {
                Sequencer.Message(message);
            }
            return true;
        }

    }

}
