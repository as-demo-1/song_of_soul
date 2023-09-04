// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A static class that provides a simplified interface to the Dialogue System's core
    /// functions. This class manages a singleton instance of DialogueSystemController.
    /// Starting in version 2.0, it will NOT auto-instantiate an instance if none exists.
    /// </summary>
    public static class DialogueManager
    {

        private static DialogueSystemController m_instance = null;

        /// <summary>
        /// Gets the instance of DialogueSystemController.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static DialogueSystemController instance
        {
            get
            {
                if (m_instance == null) m_instance = GameObjectUtility.FindFirstObjectByType<DialogueSystemController>();
                return m_instance;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the singleton has found or created an instance.
        /// </summary>
        /// <value><c>true</c> if has instance; otherwise, <c>false</c>.</value>
        public static bool hasInstance { get { return instance != null; } }

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            m_instance = null;
        }
#endif



        /// <summary>
        /// Gets the database manager.
        /// </summary>
        /// <value>
        /// The database manager.
        /// </value>
        public static DatabaseManager databaseManager { get { return hasInstance ? instance.databaseManager : null; } }

        /// <summary>
        /// Gets the master database.
        /// </summary>
        /// <value>
        /// The master database.
        /// </value>
        public static DialogueDatabase masterDatabase { get { return hasInstance ? instance.masterDatabase : null; } }

        /// <summary>
        /// Gets the dialogue UI.
        /// </summary>
        /// <value>
        /// The dialogue UI.
        /// </value>
        public static IDialogueUI dialogueUI
        {
            get { return (instance != null) ? instance.dialogueUI : null; }
            set { instance.dialogueUI = value; }
        }

        /// <summary>
        /// Convenience property that casts the dialogueUI property as a StandardDialogueUI.
        /// If the dialogueUI is not a StandardDialogueUI, returns null.
        /// </summary>
        public static StandardDialogueUI standardDialogueUI
        {
            get { return (instance != null) ? instance.standardDialogueUI : null; }
            set { instance.standardDialogueUI = value; }
        }

        /// <summary>
        /// Gets the display settings.
        /// </summary>
        /// <value>
        /// The display settings.
        /// </value>
        public static DisplaySettings displaySettings { get { return hasInstance ? instance.displaySettings : null; } }

        /// <summary>
        /// Indicates whether a conversation is active.
        /// </summary>
        /// <value>
        /// <c>true</c> if a conversation is active; otherwise, <c>false</c>.
        /// </value>
        public static bool isConversationActive { get { return hasInstance ? instance.isConversationActive : false; } }

        /// <summary>
        /// Indicates whether more than one conversation can play at the same time.
        /// </summary>
        /// <value><c>true</c> if simultaneous conversations are allowed; otherwise, <c>false</c>.</value>
        public static bool allowSimultaneousConversations { get { return hasInstance ? instance.allowSimultaneousConversations : false; } }

        /// <summary>
        /// If not allowing simultaneous conversations and a conversation is active, stop it if another conversation wants to start.
        /// </summary>
        /// <value><c>true</c> to interrupt active conversation if another wants to start; otherwise, <c>false</c>.</value>
        public static bool interruptActiveConversations { get { return hasInstance ? instance.interruptActiveConversations : false; } }

        /// <summary>
        /// The IsDialogueEntryValid delegate (if one is assigned). This is an optional delegate that you
        /// can add to check if a dialogue entry is valid before allowing a conversation to use it.
        /// It should return <c>true</c> if the entry is valid.
        /// </summary>
        public static IsDialogueEntryValidDelegate isDialogueEntryValid
        {
            get { return hasInstance ? instance.isDialogueEntryValid : null; }
            set { instance.isDialogueEntryValid = value; }
        }

        /// <summary>
        /// If response timeout action is set to Custom and menu times out, call this method.
        /// </summary>
        public static System.Action customResponseTimeoutHandler
        {
            get { return hasInstance ? instance.customResponseTimeoutHandler : null; }
            set { instance.customResponseTimeoutHandler = value; }
        }

        /// <summary>
        /// The GetInputButtonDown delegate. Overrides calls to the standard Unity 
        /// Input.GetButtonDown function.
        /// </summary>
        public static GetInputButtonDownDelegate getInputButtonDown
        {
            get { return hasInstance ? instance.getInputButtonDown : null; }
            set { instance.getInputButtonDown = value; }
        }

        /// <summary>
        /// Gets the current actor in the last conversation started (or <c>null</c> if no 
        /// conversation is active or the last conversation started has ended).
        /// See https://www.pixelcrushers.com/dialogue_system/manual2x/html/triggers_and_interaction.html#interactionDialogueSystemTriggerGameObjectAssignments
        /// for an explanation of how a conversation's actor and conversant are assigned
        /// at runtime.
        /// </summary>
        /// <value>
        /// The actor in the current conversation.
        /// </value>
        public static Transform currentActor { get { return hasInstance ? instance.currentActor : null; } }

        /// <summary>
        /// Gets the current conversant in the last conversation started (or <c>null</c> if no 
        /// conversation is active or the last conversation started has ended).
        /// See https://www.pixelcrushers.com/dialogue_system/manual2x/html/triggers_and_interaction.html#interactionDialogueSystemTriggerGameObjectAssignments
        /// for an explanation of how a conversation's actor and conversant are assigned
        /// at runtime.
        /// </summary>
        /// <value>
        /// The conversant in the current conversation.
        /// </value>
        public static Transform currentConversant { get { return hasInstance ? instance.currentConversant : null; } }

        /// <summary>
        /// Gets the current conversation state of the last conversation started (or <c>null</c> if no
        /// conversation is active or the last conversation started has ended).
        /// </summary>
        /// <value>The current conversation state.</value>
        public static ConversationState currentConversationState { get { return hasInstance ? instance.currentConversationState : null; } }

        /// <summary>
        /// Gets the title of the last conversation started.
        /// </summary>
        /// <value>The title of the last conversation started.</value>
        public static string lastConversationStarted { get { return hasInstance ? instance.lastConversationStarted : string.Empty; } }

        /// <summary>
        /// Gets the title of the last conversation that ended.
        /// </summary>
        /// <value>The title of the last conversation ended.</value>
        public static string lastConversationEnded { get { return hasInstance ? instance.lastConversationEnded : string.Empty; } }

        /// <summary>
        /// Gets the ID of the last conversation started.
        /// </summary>
        public static int lastConversationID { get { return hasInstance ? instance.lastConversationID : -1; } }

        /// <summary>
        /// Gets the active conversation's ConversationController.
        /// </summary>
        public static ConversationController conversationController { get { return hasInstance ? instance.conversationController : null; } }

        /// <summary>
        /// Gets the active conversation's ConversationModel.
        /// </summary>
        public static ConversationModel conversationModel { get { return hasInstance ? instance.conversationModel : null; } }

        /// <summary>
        /// Gets the active conversation's ConversationView.
        /// </summary>
        public static ConversationView conversationView { get { return hasInstance ? instance.conversationView : null; } }

        /// <summary>
        /// If <c>true</c>, Dialogue System Triggers set to OnStart should wait until save data has been applied or variables initialized.
        /// </summary>
        public static bool onStartTriggerWaitForSaveDataApplied
        {
            get { return hasInstance ? instance.onStartTriggerWaitForSaveDataApplied : false; }
            set { if (hasInstance) instance.onStartTriggerWaitForSaveDataApplied = value; }
        }

        /// <summary>
        /// Gets or sets the debug level.
        /// </summary>
        /// <value>
        /// The debug level.
        /// </value>
        public static DialogueDebug.DebugLevel debugLevel
        {
            get { return hasInstance ? instance.debugLevel : DialogueDebug.level; }
            set { if (hasInstance) instance.debugLevel = value; DialogueDebug.level = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether exceptions in Lua code will be caught by
        /// the calling method or allowed to rise up to Unity. Defaults to <c>false</c>.
        /// </summary>
        /// <value><c>true</c> if Lua exceptions are allowed; otherwise, <c>false</c>.</value>
        public static bool allowLuaExceptions
        {
            get { return hasInstance ? instance.allowLuaExceptions : false; }
            set { instance.allowLuaExceptions = value; }
        }

        /// @cond FOR_V1_COMPATIBILITY
        public static DialogueSystemController Instance { get { return instance; } }
        public static bool HasInstance { get { return hasInstance; } }
        public static DatabaseManager DatabaseManager { get { return databaseManager; } }
        public static DialogueDatabase MasterDatabase { get { return masterDatabase; } }
        public static IDialogueUI DialogueUI { get { return dialogueUI; } set { dialogueUI = value; } }
        public static DisplaySettings DisplaySettings { get { return displaySettings; } }
        public static bool IsConversationActive { get { return isConversationActive; } }
        public static bool AllowSimultaneousConversations { get { return allowSimultaneousConversations; } }
        public static IsDialogueEntryValidDelegate IsDialogueEntryValid { get { return isDialogueEntryValid; } set { isDialogueEntryValid = value; } }
        public static GetInputButtonDownDelegate GetInputButtonDown { get { return getInputButtonDown; } set { getInputButtonDown = value; } }
        public static Transform CurrentActor { get { return currentActor; } }
        public static Transform CurrentConversant { get { return currentConversant; } }
        public static ConversationState CurrentConversationState { get { return currentConversationState; } }
        public static string LastConversationStarted { get { return lastConversationStarted; } }
        public static int LastConversationID { get { return lastConversationID; } }
        public static ConversationController ConversationController { get { return conversationController; } }
        public static ConversationModel ConversationModel { get { return conversationModel; } }
        public static ConversationView ConversationView { get { return conversationView; } }
        public static DialogueDebug.DebugLevel DebugLevel { get { return debugLevel; } set { debugLevel = value; } }
        public static bool AllowLuaExceptions { get { return allowLuaExceptions; } set { allowLuaExceptions = value; } }
        /// @endcond

        /// <summary>
        /// Sets the language to use for localized text.
        /// </summary>
        /// <param name='language'>
        /// Language to use. Specify <c>null</c> or an emtpy string to use the default language.
        /// </param>
        public static void SetLanguage(string language)
        {
            if (!hasInstance) return;
            instance.SetLanguage(language);
        }

        /// <summary>
        /// Enables or disables Dialogue System input.
        /// </summary>
        /// <param name="value">True to enable, false to disable.</param>
        public static void SetDialogueSystemInput(bool value)
        {
            if (!hasInstance) return;
            instance.SetDialogueSystemInput(value);
        }

        /// <summary>
        /// Returns true if Dialogue System input is disabled.
        /// </summary>
        /// <returns></returns>
        public static bool IsDialogueSystemInputDisabled()
        {
            return hasInstance ? instance.IsDialogueSystemInputDisabled() : true;
        }

        /// <summary>
        /// Adds a dialogue database to memory. To save memory or reduce load time, you may want to 
        /// break up your dialogue data into multiple smaller databases. You can add or remove 
        /// these databases as needed.
        /// </summary>
        /// <param name='database'>
        /// The database to add.
        /// </param>
        public static void AddDatabase(DialogueDatabase database)
        {
            if (!hasInstance) return;
            instance.AddDatabase(database);
        }

        /// <summary>
        /// Removes a dialogue database from memory. To save memory or reduce load time, you may 
        /// want to break up your dialogue data into multiple smaller databases. You can add or 
        /// remove these databases as needed.
        /// </summary>
        /// <param name='database'>
        /// The database to remove.
        /// </param>
        public static void RemoveDatabase(DialogueDatabase database)
        {
            if (!hasInstance) return;
            instance.RemoveDatabase(database);
        }

        /// <summary>
        /// Resets the database to a default state.
        /// </summary>
        /// <param name='databaseResetOptions'>
        /// Accepts the following values:
        /// - RevertToDefault: Restores the default database, removing any other databases that 
        /// were added after startup.
        /// - KeepAllLoaded: Keeps all loaded databases in memory, but reverts them to their 
        /// starting values.
        /// </param>
        public static void ResetDatabase(DatabaseResetOptions databaseResetOptions)
        {
            if (!hasInstance) return;
            instance.ResetDatabase(databaseResetOptions);
        }

        /// <summary>
        /// Resets the database to a default state, keeping all loaded databases.
        /// </summary>
        public static void ResetDatabase()
        {
            if (!hasInstance) return;
            instance.ResetDatabase();
        }

        /// <summary>
        /// Preloads the master database. The Dialogue System delays loading of the dialogue database 
        /// until the data is needed. This avoids potentially long delays during Start(). If you want
        /// to load the database manually (for example to run Lua commands on its contents), call
        /// this method.
        /// </summary>
        public static void PreloadMasterDatabase()
        {
            if (!hasInstance) return;
            instance.PreloadMasterDatabase();
        }

        /// <summary>
        /// Preloads the dialogue UI. The Dialogue System delays loading of the dialogue UI until the
        /// first time it's needed for a conversation or alert. Since dialogue UIs often contain
        /// textures and other art assets, loading can cause a slight pause. You may want to preload
        /// the UI at a time of your design by using this method.
        /// </summary>
        public static void PreloadDialogueUI()
        {
            if (!hasInstance) return;
            instance.PreloadDialogueUI();
        }

        /// <summary>
        /// Checks whether a conversation has any valid entries linked from the start entry, since it's possible that
        /// the conditions of all entries could be false.
        /// </summary>
        /// <returns>
        /// <c>true</c>, if the conversation has a valid entry, <c>false</c> otherwise.
        /// </returns>
        /// <param name="title">
        /// The title of the conversation to look up in the master database.
        /// </param>
        /// <param name='actor'>
        /// The transform of the actor (primary participant). The sequencer uses this to direct 
        /// camera angles and perform other actions. In PC-NPC conversations, the actor is usually
        /// the PC.
        /// </param>
        /// <param name='conversant'>
        /// The transform of the conversant (the other participant). The sequencer uses this to 
        /// direct camera angles and perform other actions. In PC-NPC conversations, the conversant
        /// is usually the NPC.
        /// </param>
        /// <param name="initialDialogueEntryID">Optional starting entry ID; omit to start at beginning.</param>
        public static bool ConversationHasValidEntry(string title, Transform actor, Transform conversant, int initialDialogueEntryID = -1)
        {
            return hasInstance ? instance.ConversationHasValidEntry(title, actor, conversant, initialDialogueEntryID) : false;
        }

        /// <summary>
        /// Checks whether a conversation has any valid entries linked from the start entry, since it's possible that
        /// the conditions of all entries could be false.
        /// </summary>
        /// <returns>
        /// <c>true</c>, if the conversation has a valid entry, <c>false</c> otherwise.
        /// </returns>
        /// <param name="title">
        /// The title of the conversation to look up in the master database.
        /// </param>
        /// <param name='actor'>
        /// The transform of the actor (primary participant). The sequencer uses this to direct 
        /// camera angles and perform other actions. In PC-NPC conversations, the actor is usually
        /// the PC.
        /// </param>
        public static bool ConversationHasValidEntry(string title, Transform actor)
        {
            return ConversationHasValidEntry(title, actor, null);
        }

        /// <summary>
        /// Checks whether a conversation has any valid entries linked from the start entry, since it's possible that
        /// the conditions of all entries could be false.
        /// </summary>
        /// <returns>
        /// <c>true</c>, if the conversation has a valid entry, <c>false</c> otherwise.
        /// </returns>
        /// <param name="title">
        /// The title of the conversation to look up in the master database.
        /// </param>
        public static bool ConversationHasValidEntry(string title)
        {
            return ConversationHasValidEntry(title, null, null);
        }

        /// <summary>
        /// Starts a conversation, which also broadcasts an OnConversationStart message to the 
        /// actor and conversant. Your scripts can listen for OnConversationStart to do anything
        /// necessary at the beginning of a conversation, such as pausing other gameplay or 
        /// temporarily disabling player control. See the Feature Demo scene, which uses the
        /// SetEnabledOnDialogueEvent component to disable player control during conversations.
        /// </summary>
        /// <param name='title'>
        /// The title of the conversation to look up in the master database.
        /// </param>
        /// <param name='actor'>
        /// The transform of the actor (primary participant). The sequencer uses this to direct 
        /// camera angles and perform other actions. In PC-NPC conversations, the actor is usually
        /// the PC.
        /// </param>
        /// <param name='conversant'>
        /// The transform of the conversant (the other participant). The sequencer uses this to 
        /// direct camera angles and perform other actions. In PC-NPC conversations, the conversant
        /// is usually the NPC.
        /// </param>
        /// <param name='initialDialogueEntryID'> 
        /// The initial dialogue entry ID, or -1 to start from the beginning.
        /// </param>
        /// <example>
        /// StartConversation("Shopkeeper Conversation", player, shopkeeper);
        /// </example>
        public static void StartConversation(string title, Transform actor, Transform conversant, int initialDialogueEntryID)
        {
            if (!hasInstance) return;
            instance.StartConversation(title, actor, conversant, initialDialogueEntryID);
        }

        /// <summary>
        /// Starts a conversation, which also broadcasts an OnConversationStart message to the 
        /// actor and conversant. Your scripts can listen for OnConversationStart to do anything
        /// necessary at the beginning of a conversation, such as pausing other gameplay or 
        /// temporarily disabling player control. See the Feature Demo scene, which uses the
        /// SetEnabledOnDialogueEvent component to disable player control during conversations.
        /// </summary>
        /// <param name='title'>
        /// The title of the conversation to look up in the master database.
        /// </param>
        /// <param name='actor'>
        /// The transform of the actor (primary participant). The sequencer uses this to direct 
        /// camera angles and perform other actions. In PC-NPC conversations, the actor is usually
        /// the PC.
        /// </param>
        /// <param name='conversant'>
        /// The transform of the conversant (the other participant). The sequencer uses this to 
        /// direct camera angles and perform other actions. In PC-NPC conversations, the conversant
        /// is usually the NPC.
        /// </param>
        /// <example>
        /// StartConversation("Shopkeeper Conversation", player, shopkeeper);
        /// </example>
        public static void StartConversation(string title, Transform actor, Transform conversant)
        {
            if (!hasInstance) return;
            instance.StartConversation(title, actor, conversant);
        }

        /// <summary>
        /// Starts a conversation, which also broadcasts an OnConversationStart message to the 
        /// actor. Your scripts can listen for OnConversationStart to do anything
        /// necessary at the beginning of a conversation, such as pausing other gameplay or 
        /// temporarily disabling player control. See the Feature Demo scene, which uses the
        /// SetEnabledOnDialogueEvent component to disable player control during conversations.
        /// </summary>
        /// <param name='title'>
        /// The title of the conversation to look up in the master database.
        /// </param>
        /// <param name='actor'>
        /// The transform of the actor (primary participant). The sequencer uses this to direct 
        /// camera angles and perform other actions. In PC-NPC conversations, the actor is usually
        /// the PC.
        /// </param>
        public static void StartConversation(string title, Transform actor)
        {
            if (!hasInstance) return;
            instance.StartConversation(title, actor);
        }

        /// <summary>
        /// Starts the conversation with no transforms specified for the actor or conversant.
        /// </summary>
        /// <param name='title'>
        /// The title of the conversation to look up in the master database.
        /// </param>
        public static void StartConversation(string title)
        {
            if (!hasInstance) return;
            instance.StartConversation(title, null, null);
        }

        /// <summary>
        /// Stop the current conversation immediately, and sends an OnConversationEnd message to 
        /// the actor and conversant. Your scripts can listen for OnConversationEnd to do anything
        /// necessary at the end of a conversation, such as resuming other gameplay or re-enabling
        /// player control.
        /// </summary>
        public static void StopConversation()
        {
            if (!hasInstance) return;
            instance.StopConversation();
        }

        /// <summary>
        /// Stops all current conversations immediately.
        /// </summary>
        public static void StopAllConversations()
        {
            if (!hasInstance) return;
            instance.StopAllConversations();
        }

        /// <summary>
        /// Updates the responses for the current state of the current conversation.
        /// If the response menu entries' conditions have changed while the response menu is
        /// being shown, you can call this method to update the response menu.
        /// </summary>
        public static void UpdateResponses()
        {
            if (!hasInstance) return;
            instance.UpdateResponses();
        }

        /// <summary>
        /// Changes an actor's Display Name.
        /// </summary>
        /// <param name="actorName">Actor's Name field.</param>
        /// <param name="newDisplayName">New Display Name value.</param>
        public static void ChangeActorName(string actorName, string newDisplayName)
        {
            DialogueSystemController.ChangeActorName(actorName, newDisplayName);
        }

        /// <summary>
        /// Causes a character to bark a line at another character. A bark is a line spoken outside
        /// of a full conversation. It uses a simple gameplay bark UI instead of the dialogue UI.
        /// </summary>
        /// <param name='conversationTitle'>
        /// Title of the conversation that contains the bark lines. In this conversation, all 
        /// dialogue entries linked from the first entry are considered bark lines.
        /// </param>
        /// <param name='speaker'>
        /// The character barking the line.
        /// </param>
        /// <param name='listener'>
        /// The character being barked at.
        /// </param>
        /// <param name='barkHistory'>
        /// Bark history used to track the most recent bark, so the bark controller can go through
        /// the bark lines in a specified order.
        /// </param>
        public static void Bark(string conversationTitle, Transform speaker, Transform listener, BarkHistory barkHistory)
        {
            if (!hasInstance) return;
            instance.Bark(conversationTitle, speaker, listener, barkHistory);
        }

        /// <summary>
        /// Causes a character to bark a line at another character. A bark is a line spoken outside
        /// of a full conversation. It uses a simple gameplay bark UI instead of the dialogue UI.
        /// Since this form of the Bark() method does not include a BarkHistory, a random bark is
        /// selected from the bark lines.
        /// </summary>
        /// <param name='conversationTitle'>
        /// Title of the conversation that contains the bark lines. In this conversation, all 
        /// dialogue entries linked from the first entry are considered bark lines.
        /// </param>
        /// <param name='speaker'>
        /// The character barking the line.
        /// </param>
        /// <param name='listener'>
        /// The character being barked at.
        /// </param>
        public static void Bark(string conversationTitle, Transform speaker, Transform listener)
        {
            if (!hasInstance) return;
            instance.Bark(conversationTitle, speaker, listener);
        }

        /// <summary>
        /// Causes a character to bark a line. A bark is a line spoken outside
        /// of a full conversation. It uses a simple gameplay bark UI instead of the dialogue UI.
        /// Since this form of the Bark() method does not include a BarkHistory, a random bark is
        /// selected from the bark lines.
        /// </summary>
        /// <param name='conversationTitle'>
        /// Title of the conversation that contains the bark lines. In this conversation, all 
        /// dialogue entries linked from the first entry are considered bark lines.
        /// </param>
        /// <param name='speaker'>
        /// The character barking the line.
        /// </param>
        public static void Bark(string conversationTitle, Transform speaker)
        {
            if (!hasInstance) return;
            instance.Bark(conversationTitle, speaker);
        }

        /// <summary>
        /// Causes a character to bark a line. A bark is a line spoken outside
        /// of a full conversation. It uses a simple gameplay bark UI instead of the dialogue UI.
        /// </summary>
        /// <param name='conversationTitle'>
        /// Title of the conversation that contains the bark lines. In this conversation, all 
        /// dialogue entries linked from the first entry are considered bark lines.
        /// </param>
        /// <param name='speaker'>
        /// The character barking the line.
        /// </param>
        /// <param name='barkHistory'>
        /// Bark history used to track the most recent bark, so the bark controller can go through
        /// the bark lines in a specified order.
        /// </param>
        public static void Bark(string conversationTitle, Transform speaker, BarkHistory barkHistory)
        {
            if (!hasInstance) return;
            instance.Bark(conversationTitle, speaker, barkHistory);
        }

        /// <summary>
        /// Causes a character to bark a literal string instead of looking up its line from
        /// a conversation.
        /// </summary>
        /// <param name="barkText">The string to bark.</param>
        /// <param name="speaker">The barker.</param>
        /// <param name="listener">The target of the bark. (May be null)</param>
        /// <param name="sequence">The optional sequence to play. (May be null or empty)</param>
        public static void BarkString(string barkText, Transform speaker, Transform listener = null, string sequence = null)
        {
            if (!hasInstance) return;
            instance.BarkString(barkText, speaker, listener, sequence);
        }

        /// <summary>
        /// Returns the default duration that a string of bark text will be shown.
        /// </summary>
        /// <param name="barkText">The text that will be barked (to determine the duration).</param>
        /// <returns>The default duration in seconds for the specified bark text.</returns>
        public static float GetBarkDuration(string barkText)
        {
            return hasInstance ? instance.GetBarkDuration(barkText) : 0;
        }

        /// <summary>
        /// Shows an alert message using the dialogue UI.
        /// </summary>
        /// <param name='message'>
        /// The message to show.
        /// </param>
        /// <param name='duration'>
        /// The duration in seconds to show the message.
        /// </param>
        public static void ShowAlert(string message, float duration)
        {
            if (!hasInstance) return;
            instance.ShowAlert(message, duration);
        }

        /// <summary>
        /// Shows an alert message using the dialogue UI for the UI's default duration.
        /// </summary>
        /// <param name='message'>
        /// The message to show.
        /// </param>
        public static void ShowAlert(string message)
        {
            if (!hasInstance) return;
            instance.ShowAlert(message);
        }

        /// <summary>
        /// Checks Lua Variable['Alert'] to see if we need to show an alert.
        /// </summary>
        public static void CheckAlerts()
        {
            if (!hasInstance) return;
            instance.CheckAlerts();
        }

        /// <summary>
        /// Hides the currently-displaying alert message.
        /// </summary>
        public static void HideAlert()
        {
            if (!hasInstance) return;
            instance.HideAlert();
        }

        /// <summary>
        /// Gets localized text.
        /// </summary>
        /// <returns>If the specified field exists in the text tables, returns the field's 
        /// localized text for the current language. Otherwise returns the field itself.</returns>
        /// <param name="s">The field to look up.</param>
        public static string GetLocalizedText(string s)
        {
            return hasInstance ? instance.GetLocalizedText(s) : s;
        }

        /// <summary>
        /// Starts a sequence. See @ref sequencer.
        /// </summary>
        /// <param name="sequence">The sequence to play.</param>
        /// <param name="speaker">The speaker, for sequence commands that reference the speaker.</param>
        /// <param name="listener">The listener, for sequence commands that reference the listener.</param>
        /// <param name="informParticipants">Specifies whether to send OnSequenceStart and OnSequenceEnd messages to the speaker and listener. Default is <c>true</c>.</param>
        /// <param name="destroyWhenDone">Specifies whether destroy the sequencer when done playing the sequence. Default is </param>
        /// <param name="entrytag">The entrytag to associate with the sequence.</param>
        /// <returns>The sequencer that is playing the sequence.</returns>
        public static Sequencer PlaySequence(string sequence, Transform speaker, Transform listener, bool informParticipants, bool destroyWhenDone, string entrytag)
        {
            return hasInstance ? instance.PlaySequence(sequence, speaker, listener, informParticipants, destroyWhenDone, entrytag) : null;
        }

        /// <summary>
        /// Starts a sequence. See @ref sequencer.
        /// </summary>
        /// <returns>
        /// The sequencer that is playing the sequence.
        /// </returns>
        /// <param name='sequence'>
        /// The sequence to play.
        /// </param>
        /// <param name='speaker'>
        /// The speaker, for sequence commands that reference the speaker.
        /// </param>
        /// <param name='listener'>
        /// The listener, for sequence commands that reference the listener.
        /// </param>
        /// <param name='informParticipants'>
        /// Specifies whether to send OnSequenceStart and OnSequenceEnd messages to the speaker and
        /// listener. Default is <c>true</c>.
        /// </param>
        /// <param name='destroyWhenDone'>
        /// Specifies whether destroy the sequencer when done playing the sequence. Default is 
        /// <c>true</c>.
        /// </param>
        public static Sequencer PlaySequence(string sequence, Transform speaker, Transform listener, bool informParticipants, bool destroyWhenDone)
        {
            return hasInstance ? instance.PlaySequence(sequence, speaker, listener, informParticipants, destroyWhenDone) : null;
        }

        /// <summary>
        /// Starts a sequence. See @ref sequencer.
        /// </summary>
        /// <returns>
        /// The sequencer that is playing the sequence.
        /// </returns>
        /// <param name='sequence'>
        /// The sequence to play.
        /// </param>
        /// <param name='speaker'>
        /// The speaker, for sequence commands that reference the speaker.
        /// </param>
        /// <param name='listener'>
        /// The listener, for sequence commands that reference the listener.
        /// </param>
        /// <param name='informParticipants'>
        /// Specifies whether to send OnSequenceStart and OnSequenceEnd messages to the speaker and
        /// listener. Default is <c>true</c>.
        /// </param>
        public static Sequencer PlaySequence(string sequence, Transform speaker, Transform listener, bool informParticipants)
        {
            return hasInstance ? instance.PlaySequence(sequence, speaker, listener, informParticipants) : null;
        }

        /// <summary>
        /// Starts a sequence, and sends OnSequenceStart/OnSequenceEnd messages to the 
        /// participants. See @ref sequencer.
        /// </summary>
        /// <returns>
        /// The sequencer that is playing the sequence.
        /// </returns>
        /// <param name='sequence'>
        /// The sequence to play.
        /// </param>
        /// <param name='speaker'>
        /// The speaker, for sequence commands that reference the speaker.
        /// </param>
        /// <param name='listener'>
        /// The listener, for sequence commands that reference the listener.
        /// </param>
        public static Sequencer PlaySequence(string sequence, Transform speaker, Transform listener)
        {
            return hasInstance ? instance.PlaySequence(sequence, speaker, listener) : null;
        }

        /// <summary>
        /// Starts a sequence. See @ref sequencer.
        /// </summary>
        /// <returns>
        /// The sequencer that is playing the sequence.
        /// </returns>
        /// <param name='sequence'>
        /// The sequence to play.
        /// </param>
        public static Sequencer PlaySequence(string sequence)
        {
            return hasInstance ? instance.PlaySequence(sequence) : null;
        }

        /// <summary>
        /// Stops a sequence.
        /// </summary>
        /// <param name='sequencer'>
        /// The sequencer playing the sequence.
        /// </param>
        public static void StopSequence(Sequencer sequencer)
        {
            instance.StopSequence(sequencer);
        }

        /// <summary>
        /// Pauses the Dialogue System. Also broadcasts OnDialogueSystemPause to 
        /// the Dialogue Manager and conversation participants. Conversations,
        /// timers, typewriter and fade effects, and the AudioWait() and Voice()
        /// sequencer commands will be paused. Other than this, AudioSource,
        /// Animation, and Animator components will not be paused; it's up to
        /// you to handle them as appropriate for your project.
        /// </summary>
        public static void Pause()
        {
            if (!hasInstance) return;
            instance.Pause();
        }

        /// <summary>
        /// Unpauses the Dialogue System. Also broadcasts OnDialogueSystemUnpause to 
        /// the Dialogue Manager and conversation participants.
        /// </summary>
        public static void Unpause()
        {
            if (!hasInstance) return;
            instance.Unpause();
        }

        /// <summary>
        /// Sets the dialogue UI.
        /// </summary>
        /// <param name='gameObject'>
        /// Game object containing an implementation of IDialogueUI.
        /// </param>
        public static void UseDialogueUI(GameObject gameObject)
        {
            instance.UseDialogueUI(gameObject);
        }

        /// <summary>
        /// Sets the dialogue UI's main panel visible or invisible.
        /// </summary>
        /// <param name="show">If true, show (or re-show) the panel; if false, hide it.</param>
        /// <param name="immediate">If true, skip animation and change immediately.</param>
        public static void SetDialoguePanel(bool show, bool immediate = false)
        {
            instance.SetDialoguePanel(show, immediate);
        }

        /// <summary>
        /// Sets an actor's portrait. If can be:
        /// - 'default' or <c>null</c> to use the primary portrait defined in the database,
        /// - 'pic=#' to use an alternate portrait defined in the database (numbered from 2), or
        /// - the name of a texture in a Resources folder.
        /// </summary>
        /// <param name="actorName">Actor name.</param>
        /// <param name="portraitName">Portrait name.</param>
        public static void SetPortrait(string actorName, string portraitName)
        {
            if (!hasInstance) return;
            instance.SetPortrait(actorName, portraitName);
        }

        /// <summary>
        /// Adds a Lua expression observer.
        /// </summary>
        /// <param name='luaExpression'>
        /// Lua expression to watch.
        /// </param>
        /// <param name='frequency'>
        /// Frequency to check the expression.
        /// </param>
        /// <param name='luaChangedHandler'>
        /// Delegate to call when the expression changes. This should be in the form:
        /// <code>void MyDelegate(LuaWatchItem luaWatchItem, Lua.Result newValue) {...}</code>
        /// </param>
        public static void AddLuaObserver(string luaExpression, LuaWatchFrequency frequency, LuaChangedDelegate luaChangedHandler)
        {
            if (!hasInstance) return;
            instance.AddLuaObserver(luaExpression, frequency, luaChangedHandler);
        }

        /// <summary>
        /// Removes a Lua expression observer. To be removed, the expression, frequency, and
        /// notification delegate must all match.
        /// </summary>
        /// <param name='luaExpression'>
        /// Lua expression being watched.
        /// </param>
        /// <param name='frequency'>
        /// Frequency that the expression is being watched.
        /// </param>
        /// <param name='luaChangedHandler'>
        /// Delegate that's called when the expression changes.
        /// </param>
        public static void RemoveLuaObserver(string luaExpression, LuaWatchFrequency frequency, LuaChangedDelegate luaChangedHandler)
        {
            if (!hasInstance) return;
            instance.RemoveLuaObserver(luaExpression, frequency, luaChangedHandler);
        }

        /// <summary>
        /// Removes all Lua expression observers for a specified frequency.
        /// </summary>
        /// <param name='frequency'>
        /// Frequency.
        /// </param>
        public static void RemoveAllObservers(LuaWatchFrequency frequency)
        {
            if (!hasInstance) return;
            instance.RemoveAllObservers(frequency);
        }

        /// <summary>
        /// Removes all Lua expression observers.
        /// </summary>
        public static void RemoveAllObservers()
        {
            if (!hasInstance) return;
            instance.RemoveAllObservers();
        }

        /// <summary>
        /// Registers an asset bundle with the Dialogue System. This allows sequencer
        /// commands to load assets inside it.
        /// </summary>
        /// <param name="bundle">Asset bundle.</param>
        public static void RegisterAssetBundle(AssetBundle bundle)
        {
            if (!hasInstance) return;
            instance.RegisterAssetBundle(bundle);
        }

        /// <summary>
        /// Unregisters an asset bundle from the Dialogue System. Always unregister
        /// asset bundles before freeing them.
        /// </summary>
        /// <param name="bundle">Asset bundle.</param>
        public static void UnregisterAssetBundle(AssetBundle bundle)
        {
            if (!hasInstance) return;
            instance.UnregisterAssetBundle(bundle);
        }

        /// <summary>
        /// Loads a named asset from the registered asset bundles or from Resources.
        /// Note: This version of LoadAsset does not load from Addressables.
        /// </summary>
        /// <returns>The asset, or <c>null</c> if not found.</returns>
        /// <param name="name">Name of the asset.</param>
        public static UnityEngine.Object LoadAsset(string name)
        {
            return hasInstance ? instance.LoadAsset(name) : null;
        }

        /// <summary>
        /// Loads a named asset from the registered asset bundles or from Resources.
        /// Note: This version of LoadAsset does not load from Addressables.
        /// </summary>
        /// <returns>The asset, or <c>null</c> if not found.</returns>
        /// <param name="name">Name of the asset.</param>
        /// <param name="type">Type of the asset.</param>
        public static UnityEngine.Object LoadAsset(string name, System.Type type)
        {
            return hasInstance ? instance.LoadAsset(name, type) : null;
        }

        /// <summary>
        /// Loads a named asset from the registered asset bundles, Resources, or
        /// Addressables. Returns the asset in a callback delegate. Addressables
        /// will be unloaded when the scene is unloaded. To unload them earlier,
        /// use DialogueManager.UnloadAsset(). 
        /// 
        /// By default, scene changes unload all addressables loaded via 
        /// DialogueManager.LoadAsset(). To prevent the unload, set
        /// DialogueManager.instance.unloadAddressablesOnSceneChange to false.
        /// </summary>
        /// <param name="name">Name of the asset.</param>
        /// <param name="type">Type of the asset</param>
        /// <param name="assetLoaded">Delegate method to call when returning loaded asset, or <c>null</c> if not found.</param>
        public static void LoadAsset(string name, System.Type type, AssetLoadedDelegate assetLoaded)
        {
            if (hasInstance)
            {
                instance.LoadAsset(name, type, assetLoaded);
            }
            else if (assetLoaded != null)
            {
                assetLoaded(null);
            }
        }

        /// <summary>
        /// Unloads an object previously loaded by LoadAsset. Only unloads
        /// if using addressables.
        /// </summary>
        public static void UnloadAsset(object obj)
        {
            if (hasInstance) instance.UnloadAsset(obj);
        }

        /// <summary>
        /// Sends an "UpdateTracker" message to the Dialogue Manager, which may have a quest tracker component
        /// that listens for this message, 
        /// </summary>
        public static void SendUpdateTracker()
        {
            if (!hasInstance) return;
            instance.BroadcastMessage(DialogueSystemMessages.UpdateTracker, SendMessageOptions.DontRequireReceiver);
        }

    }

}
