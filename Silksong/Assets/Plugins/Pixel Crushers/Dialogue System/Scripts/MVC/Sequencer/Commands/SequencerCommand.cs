// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Base class for Sequencer commands.
    /// </summary>
    public abstract class SequencerCommand : MonoBehaviour
    {

        /// <summary>
        /// Indicates whether the command is still playing. When your custom command is done, it
        /// should call <c>Stop()</c> to set this to <c>false</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is still playing; otherwise, <c>false</c>.
        /// </value>
        [HideInInspector] public bool isPlaying = true;

        /// <summary>
        /// Reference to the Sequencer, so you can access its properties such as SequencerCamera
        /// and CameraAngles.
        /// </summary>
        /// <value>
        /// The sequencer.
        /// </value>
        protected Sequencer sequencer
        {
            get
            {
                if (m_sequencer == null) m_sequencer = Sequencer.s_awakeSequencer;
                return m_sequencer;
            }
            private set { m_sequencer = value; }
        }
        private Sequencer m_sequencer = null;

        /// <summary>
        /// The parameters for the command.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        protected string[] parameters
        {
            get
            {
                if (m_parameters == null) m_parameters = Sequencer.s_awakeArgs;
                return m_parameters;
            }
            private set { m_parameters = value; }
        }
        private string[] m_parameters = null;

        /// <summary>
        /// Optional message to send the sequencer when the command completes. The sequencer
        /// sends this message. The command itself is not responsible for sending it.
        /// </summary>
        /// <value>The end message.</value>
        public string endMessage
        {
            get
            {
                if (m_endMessage == null) m_endMessage = Sequencer.s_awakeEndMessage;
                return m_endMessage;
            }
            private set { m_endMessage = value; }
        }
        private string m_endMessage = null;

        private Transform m_speaker = null;
        protected Transform speaker { get { return (m_speaker != null) ? m_speaker : (Sequencer != null) ? Sequencer.Speaker : null; } }

        private Transform m_listener = null;
        protected Transform listener { get { return (m_listener != null) ? m_listener : (Sequencer != null) ? Sequencer.Listener : null; } }

        /// @cond FOR_V1_COMPATIBILITY
        public bool IsPlaying { get { return isPlaying; } protected set { isPlaying = value; } }
        protected Sequencer Sequencer { get { return sequencer; } private set { sequencer = value; } }
        protected string[] Parameters { get { return parameters; } private set { parameters = value; } }
        /// @endcond

        /// <summary>
        /// Initializes the base properties. The Sequencer calls this method before handing control
        /// to the command.
        /// </summary>
        /// <param name='sequencer'>
        /// A reference to the Sequencer.
        /// </param>
        /// <param name='parameters'>
        /// The parameters for the command.
        /// </param>
        public void Initialize(Sequencer sequencer, string endMessage, Transform speaker, Transform listener, params string[] parameters)
        {
            this.sequencer = sequencer;
            this.endMessage = endMessage;
            this.parameters = parameters;
            this.m_speaker = speaker;
            this.m_listener = listener;
        }

        /// <summary>
        /// Initializes the base properties. The Sequencer calls this method before handing control
        /// to the command.
        /// </summary>
        /// <param name='sequencer'>
        /// A reference to the Sequencer.
        /// </param>
        /// <param name='parameters'>
        /// The parameters for the command.
        /// </param>
        public void Initialize(Sequencer sequencer, Transform speaker, Transform listener, params string[] parameters)
        {
            Initialize(sequencer, null, speaker, listener, parameters);
        }

        /// <summary>
        /// Call this method to indicate that the command is done playing.
        /// </summary>
        protected void Stop()
        {
            isPlaying = false;
        }

        /// <summary>
        /// Sequencer commands usually specify a subject to which the command applies (e.g., where
        /// to aim the camera). This utility function returns the specified subject.
        /// </summary>
        /// <returns>
        /// The transform of the specified subject, or null if the specifier names a game object 
        /// that isn't in the scene.
        /// </returns>
        /// <param name='specifier'>
        /// <c>"speaker"</c>, <c>"listener"</c>, or the name of a game object in the scene.
        /// </param>
        /// <param name='defaultSubject'>
        /// Default subject (overrides speaker).
        /// </param>
        protected Transform GetSubject(string specifier, Transform defaultSubject = null)
        {
            return SequencerTools.GetSubject(specifier, speaker, listener, defaultSubject);
        }

        /// <summary>
        /// Sequencer commands usually specify a subject to which the command applies (e.g., where 
        /// to aim the camera). This utility function returns the specified subject.
        /// </summary>
        /// <returns>
        /// The transform of the specified subject, or null if the specifier names a game object 
        /// that isn't in the scene.
        /// </returns>
        /// <param name='i'>
        /// The parameter index number (zero-based) that names the subject.
        /// </param>
        /// <param name='defaultSubject'>
        /// Default subject (overrides speaker).
        /// </param>
        protected Transform GetSubject(int i, Transform defaultSubject = null)
        {
            return GetSubject(GetParameter(i), defaultSubject);
        }

        /// <summary>
        /// Gets the i-th parameter (zero-based).
        /// </summary>
        /// <returns>
        /// The i-th parameter, or the specified default value if <c>i</c> is out of range.
        /// </returns>
        /// <param name='i'>
        /// The parameter index number (zero-based).
        /// </param>
        /// <param name='defaultValue'>
        /// The default value to return if <c>i</c> is out of range.
        /// </param>
        protected string GetParameter(int i, string defaultValue = null)
        {
            return SequencerTools.GetParameter(parameters, i, defaultValue);
        }

        /// <summary>
        /// Gets the i-th parameter (zero-based) as a specified type.
        /// </summary>
        /// <returns>
        /// The i-th parameter as type T, or the specified default value if <c>i</c> is out of 
        /// range or the parameter can't be converted to type T.
        /// </returns>
        /// <param name='i'>
        /// The parameter index number (zero-based).
        /// </param>
        /// <param name='defaultValue'>
        /// The default value to return if <c>i</c> is out of range or the parameter can't be 
        /// converted to type T.
        /// </param>
        /// <typeparam name='T'>
        /// The type to convert the parameter to.
        /// </typeparam>
        /// <example>
        /// // Get the second parameter as a float, defaulting to 5f:
        /// float duration = GetParameterAs<float>(1, 5f);
        /// </example>
        protected T GetParameterAs<T>(int i, T defaultValue)
        {
            return SequencerTools.GetParameterAs<T>(parameters, i, defaultValue);
        }

        /// <summary>
        /// Gets the i-th parameter as a float.
        /// </summary>
        /// <returns>
        /// The parameter as float, or defaultValue if out of range.
        /// </returns>
        protected float GetParameterAsFloat(int i, float defaultValue = 0)
        {
            return GetParameterAs<float>(i, defaultValue);
        }

        /// <summary>
        /// Gets the i-th parameter as an int.
        /// </summary>
        /// <returns>
        /// The parameter as int, or defaultValue if out of range.
        /// </returns>
        protected int GetParameterAsInt(int i, int defaultValue = 0)
        {
            return GetParameterAs<int>(i, defaultValue);
        }

        /// <summary>
        /// Gets the i-th parameter as a bool.
        /// </summary>
        /// <returns>
        /// The parameter as bool, or defaultValue if out of range.
        /// </returns>
        /// <param name='i'>
        /// The zero-based index of the parameter.
        /// </param>
        /// <param name='defaultValue'>
        /// If set to <c>true</c> default value.
        /// </param>
        protected bool GetParameterAsBool(int i, bool defaultValue = false)
        {
            return GetParameterAs<bool>(i, defaultValue);
        }

        /// <summary>
        /// Gets the parameters as a comma-separated string.
        /// </summary>
        /// <returns>
        /// The parameters.
        /// </returns>
        protected string GetParameters()
        {
            return (parameters != null) ? string.Join(",", parameters) : string.Empty;
        }

        /// <summary>
        /// Checks whether a Lua variable "Mute" is defined and <c>true</c>. If so, this
        /// indicates that audio should be muted.
        /// </summary>
        /// <returns><c>true</c> if audio is muted; otherwise, <c>false</c>.</returns>
        protected bool IsAudioMuted()
        {
            return SequencerTools.IsAudioMuted();
        }

    }

}
