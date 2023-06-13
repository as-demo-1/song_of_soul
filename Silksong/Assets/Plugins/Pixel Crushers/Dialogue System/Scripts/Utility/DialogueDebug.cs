// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A simple static class to keep track of a global debug level setting for Dialogue System
    /// log messages. The DialogueManager / DialogueSystemController sets level. You can also
    /// set it manually. This class doesn't provide any wrappers for Debug.Log() because they
    /// would intercept the reference point that the editor goes to when you double-click the
    /// log message in the console window.
    /// </summary>
    public static class DialogueDebug
    {

        /// <summary>
        /// Dialogue system log messages are prefixed with this string.
        /// </summary>
        public const string Prefix = "Dialogue System";

        /// <summary>
        /// The debug levels.
        /// 
        /// - None: Don't log anything.
        /// - Error: Only log critical errors.
        /// - Warning: Log warnings and errors.
        /// - Info: Log trace information, warnings, and errors.
        /// </summary>
        public enum DebugLevel
        {
            None = 0,
            Error = 1,
            Warning = 2,
            Info = 3
        }

        /// <summary>
        /// The current global debug level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        public static DebugLevel level { get; set; }

        /// <summary>
        /// Should the dialogue system log trace information?
        /// </summary>
        /// <value>
        /// <c>true</c> if it should log trace info, warnings, and errors; otherwise, <c>false</c>.
        /// </value>
        public static bool logInfo
        {
            get { return (level >= DebugLevel.Info) && Debug.isDebugBuild; }
        }

        /// <summary>
        /// Should the dialogue system log warnings and trace info?
        /// </summary>
        /// <value>
        /// <c>true</c> to log warnings and errors; otherwise, <c>false</c>.
        /// </value>
        public static bool logWarnings
        {
            get { return (level >= DebugLevel.Warning) && Debug.isDebugBuild; }
        }

        /// <summary>
        /// Should the dialogue system log critical errors?
        /// </summary>
        /// <value>
        /// <c>true</c> to log errors; otherwise, <c>false</c>.
        /// </value>
        public static bool logErrors
        {
            get { return (level >= DebugLevel.Error) && Debug.isDebugBuild; }
        }

        /// @cond FOR_V1_COMPATIBILITY
        public static DebugLevel Level { get { return level; } set { level = value; } }
        public static bool LogInfo { get { return logInfo; } }
        public static bool LogWarnings { get { return logWarnings; } }
        public static bool LogErrors { get { return logErrors; } }
        /// @endcond

        static DialogueDebug()
        {
            level = DebugLevel.Warning;
        }

    }

}
