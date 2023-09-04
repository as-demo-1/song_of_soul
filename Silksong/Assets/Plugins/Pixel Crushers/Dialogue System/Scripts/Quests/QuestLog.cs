// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;
using Language.Lua;

namespace PixelCrushers.DialogueSystem
{

    public delegate QuestState StringToQuestStateDelegate(string s);
    public delegate string QuestStateToStringDelegate(QuestState state);
    public delegate string CurrentQuestStateDelegate(string quest);
    public delegate void SetQuestStateDelegate(string quest, string state);
    public delegate void SetQuestEntryStateDelegate(string quest, int entryNumber, string state);
    public delegate string CurrentQuestEntryStateDelegate(string quest, int entryNumber);

    public struct QuestEntryArgs
    {
        public string questName;
        public int entryNumber;
        public QuestEntryArgs(string questName, int entryNumber)
        {
            this.questName = questName;
            this.entryNumber = entryNumber;
        }
    }

    /// <summary>
    /// A static class that manages a quest log. It uses the Lua "Item[]" table, where each item in
    /// the table whose 'Is Item' field is false represents a quest. This makes it easy to manage quests 
    /// in Chat Mapper by adding, removing, and modifying items in the built-in Item[] table. The name 
    /// of the item is the title of the quest. (Note that the Chat Mapper simulator doesn't have a 
    /// quest system, so it treats elements of the Item[] table as items.)
    /// 
    /// This class uses the following fields in the Item[] table, which is also aliased as Quest[]:
    /// 
    /// - <b>Display Name</b>: (optional) Name to use in UIs. If blank or not present, UIs use Name field.
    /// - <b>State</b> (if using Chat Mapper, add this custom field or use the Dialogue System template 
    /// project)
    /// 	- Valid values (case-sensitive): <c>unassigned</c>, <c>active</c>, <c>success</c>, 
    /// <c>failure</c>, or <c>done</c>
    /// - <b>Description</b>: The description of the quest
    /// - <b>Success Description</b> (optional): The description to be displayed when the quest has been 
    /// successfully completed
    /// - <b>Failure Description</b> (optional): The description to be displayed when the quest has ended 
    /// in failure
    /// 
    /// Note: <c>done</c> is essentially equivalent to </c>success</c>. In the remainder of the Dialogue 
    /// System's documentation,	either <c>done</c> or <c>success</c> may be used in examples, but when 
    /// using the QuestLog class, they both correspond to the same enum state, QuestState.Success.
    /// 
    /// As an example, you might define a simple quest like this:
    /// 
    /// - Item["Kill 5 Rats"]
    /// 	- State = "unassigned"
    /// 	- Description = "The baker asked me to bring him 5 rat corpses to make a pie."
    /// 	- Success Description = "I brought the baker 5 dead rats, and we ate a delicious pie!"
    /// 	- Failure Description = "I freed the Pied Piper from jail. He took all the rats. No pie for me...."
    /// 
    /// This class provides methods to add and delete quests, get and set their state, and get 
    /// their descriptions.
    /// 
    /// Note that quest states are usually updated during conversations. In most cases, you will 
    /// probably set quest states in Lua code during conversations, so you may never need to use
    /// many of the methods in this class.
    /// 
    /// The UnityQuestLogWindow provides a quest log window using Unity GUI. You can use it as-is 
    /// or use it as a template for implementing your own quest log window in another GUI system 
    /// such as NGUI.
    /// </summary>
    public static class QuestLog
    {

        /// <summary>
        /// Constant state string for unassigned quests.
        /// </summary>
        public const string UnassignedStateString = "unassigned";

        /// <summary>
        /// Constant state string for active quests.
        /// </summary>
        public const string ActiveStateString = "active";

        /// <summary>
        /// Constant state string for successfully-completed quests.
        /// </summary>
        public const string SuccessStateString = "success";

        /// <summary>
        /// Constant state string for quests ending in failure.
        /// </summary>
        public const string FailureStateString = "failure";

        /// <summary>
        /// Constant state string for quests that were abandoned.
        /// </summary>
        public const string AbandonedStateString = "abandoned";

        /// <summary>
        /// Constant state string for quests that are grantable. 
        /// This state isn't used by the Dialogue System, but it's made available for those who want to use it.
        /// </summary>
        public const string GrantableStateString = "grantable";

        /// <summary>
        /// Constant state string for quests that are waiting to return to NPC.
        /// This state isn't used by the Dialogue System, but it's made available for those who want to use it.
        /// </summary>
        public const string ReturnToNPCStateString = "returnToNPC";

        /// <summary>
        /// Constant state string for quests that are done, if you want to track done instead of success/failure.
        /// This is essentially the same as success, and corresponds to the same enum value, QuestState.Success
        /// </summary>
        public const string DoneStateString = "done";

        /// <summary>
        /// You can reassign this delegate method to override the default conversion of
        /// strings to QuestStates.
        /// </summary>
        public static StringToQuestStateDelegate StringToState = DefaultStringToState;

        public static QuestStateToStringDelegate StateToString = DefaultStateToString;

        /// <summary>
        /// You can assign a method to override the default CurrentQuestState.
        /// </summary>
        public static CurrentQuestStateDelegate CurrentQuestStateOverride = null;

        /// <summary>
        /// You can assign a method to override the default SetQuestState.
        /// </summary>
        public static SetQuestStateDelegate SetQuestStateOverride = null;

        /// <summary>
        /// You can assign a method to override the default CurrentQuestEntryState.
        /// </summary>
        public static CurrentQuestEntryStateDelegate CurrentQuestEntryStateOverride = null;

        /// <summary>
        /// You can assign a method to override the default SetQuestEntryState.
        /// </summary>
        public static SetQuestEntryStateDelegate SetQuestEntryStateOverride = null;

        /// <summary>
        /// Set true to allow only one quest to be tracked at a time.
        /// </summary>
        public static bool trackOneQuestAtATime = false;

        public static void RegisterQuestLogFunctions()
        {
            // Unity 2017.3 bug IL2CPP can't do lambdas:
            //Lua.RegisterFunction("CurrentQuestState", null, SymbolExtensions.GetMethodInfo(() => CurrentQuestState(string.Empty)));
            //Lua.RegisterFunction("CurrentQuestEntryState", null, SymbolExtensions.GetMethodInfo(() => CurrentQuestEntryState(string.Empty, (double)0)));
            //Lua.RegisterFunction("SetQuestState", null, SymbolExtensions.GetMethodInfo(() => SetQuestState(string.Empty, string.Empty)));
            //Lua.RegisterFunction("SetQuestEntryState", null, SymbolExtensions.GetMethodInfo(() => SetQuestEntryState(string.Empty, (double)0, string.Empty)));

            Lua.RegisterFunction("CurrentQuestState", null, typeof(QuestLog).GetMethod("CurrentQuestState"));
            Lua.RegisterFunction("CurrentQuestEntryState", null, typeof(QuestLog).GetMethod("CurrentQuestEntryState"));
            Lua.RegisterFunction("SetQuestState", null, typeof(QuestLog).GetMethod("SetQuestState", new[] { typeof(string), typeof(string) }));
            Lua.RegisterFunction("SetQuestEntryState", null, typeof(QuestLog).GetMethod("SetQuestEntryState", new[] { typeof(string), typeof(double), typeof(string) }));
            Lua.RegisterFunction("UpdateQuestIndicators", null, typeof(QuestLog).GetMethod("UpdateQuestIndicators", new[] { typeof(string) }));
        }

        /// <summary>
        /// Adds a quest to the Lua Item[] table.
        /// </summary>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <param name='description'>
        /// Description of the quest when active.
        /// </param>
        /// <param name='successDescription'>
        /// Description of the quest when successfully completed.
        /// </param>
        /// <param name='failureDescription'>
        /// Description of the quest when completed in failure.
        /// </param>
        /// <param name='state'>
        /// Quest state.
        /// </param>
        /// <example>
        /// QuestLog.AddQuest("Kill 5 Rats", "The baker asked me to bring 5 rat corpses.", QuestState.Unassigned);
        /// </example>
        public static void AddQuest(string questName, string description, string successDescription, string failureDescription, QuestState state)
        {
            if (!string.IsNullOrEmpty(questName))
            {
                Lua.Run(string.Format("Item[\"{0}\"] = {{ Name = \"{1}\", Is_Item = false, Description = \"{2}\", Success_Description = \"{3}\", Failure_Description = \"{4}\", State = \"{5}\" }}",
                                      new System.Object[] { DialogueLua.StringToTableIndex(questName),
                                      DialogueLua.DoubleQuotesToSingle(questName),
                                      DialogueLua.DoubleQuotesToSingle(description),
                                      DialogueLua.DoubleQuotesToSingle(successDescription),
                                      DialogueLua.DoubleQuotesToSingle(failureDescription),
                                       StateToString(state) }),
                        DialogueDebug.logInfo);
            }
        }

        /// <summary>
        /// Adds a quest to the Lua Item[] table.
        /// </summary>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <param name='description'>
        /// Description of the quest.
        /// </param>
        /// <param name='state'>
        /// Quest state.
        /// </param>
        /// <example>
        /// QuestLog.AddQuest("Kill 5 Rats", "The baker asked me to bring 5 rat corpses.", QuestState.Unassigned);
        /// </example>
        public static void AddQuest(string questName, string description, QuestState state)
        {
            if (!string.IsNullOrEmpty(questName))
            {
                Lua.Run(string.Format("Item[\"{0}\"] = {{ Name = \"{1}\", Is_Item = false, Description = \"{2}\", State = \"{3}\" }}",
                                      new System.Object[] { DialogueLua.StringToTableIndex(questName),
                                      DialogueLua.DoubleQuotesToSingle(questName),
                                      DialogueLua.DoubleQuotesToSingle(description),
                                      StateToString(state) }),
                        DialogueDebug.logInfo);
            }
        }

        /// <summary>
        /// Adds a quest to the Lua Item[] table, and sets the state to Unassigned.
        /// </summary>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <param name='description'>
        /// Description of the quest.
        /// </param>
        /// <example>
        /// QuestLog.AddQuest("Kill 5 Rats", "The baker asked me to bring 5 rat corpses.");
        /// </example>
        public static void AddQuest(string questName, string description)
        {
            AddQuest(questName, description, QuestState.Unassigned);
        }

        /// <summary>
        /// Deletes a quest from the Lua Item[] table. Use this method if you want to remove a quest entirely.
        /// If you just want to set the state of a quest, use SetQuestState.
        /// </summary>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <example>
        /// QuestLog.RemoveQuest("Kill 5 Rats");
        /// </example>
        public static void DeleteQuest(string questName)
        {
            if (!string.IsNullOrEmpty(questName))
            {
                Lua.Run(string.Format("Item[\"{0}\"] = nil", new System.Object[] { DialogueLua.StringToTableIndex(questName) }), DialogueDebug.logInfo);
            }
        }

        /// <summary>
        /// Gets the quest state.
        /// </summary>
        /// <returns>
        /// The quest state.
        /// </returns>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <example>
        /// if (QuestLog.QuestState("Kill 5 Rats") == QuestState.Active) {
        ///     Smith.Say("Killing rats, eh? Here, take this hammer.");
        /// }
        /// </example>
        public static QuestState GetQuestState(string questName)
        {
            return StringToState(CurrentQuestState(questName)); //---Was: DialogueLua.GetQuestField(questName, "State").AsString);
        }

        /// <summary>
        /// Gets the quest state.
        /// </summary>
        /// <param name="questName">Name of the quest.</param>
        /// <returns>The quest state string ("unassigned", "success", etc.).</returns>
        public static string CurrentQuestState(string questName)
        {
            if (CurrentQuestStateOverride != null)
            {
                return CurrentQuestStateOverride(questName);
            }
            else
            {
                return DefaultCurrentQuestState(questName);
            }
        }

        /// <summary>
        /// Default built-in version of CurrentQuestState.
        /// </summary>
        public static string DefaultCurrentQuestState(string questName)
        {
            return DialogueLua.GetQuestField(questName, "State").asString;
        }

        /// <summary>
        /// Sets the quest state.
        /// </summary>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <param name='state'>
        /// New state.
        /// </param>
        /// <example>
        /// if (PiedPiperIsFree) {
        ///     QuestLog.SetQuestState("Kill 5 Rats", QuestState.Failure);
        /// }
        /// </example>
        public static void SetQuestState(string questName, QuestState state)
        {
            SetQuestState(questName, StateToString(state));
        }

        /// <summary>
        /// Sets the quest state, using the override delegate if assigned; otherwise
        /// using the default method DefaultSetQuestState.
        /// </summary>
        /// <param name="questName">Name of the quest.</param>
        /// <param name="state">New state.</param>
        public static void SetQuestState(string questName, string state)
        {
            if (SetQuestStateOverride != null)
            {
                SetQuestStateOverride(questName, state);
            }
            else
            {
                DefaultSetQuestState(questName, state);
            }
        }

        /// <summary>
        /// Default built-in method to set quest state.
        /// </summary>
        public static void DefaultSetQuestState(string questName, string state)
        {
            if (DialogueLua.DoesTableElementExist("Quest", questName))
            {
                DialogueLua.SetQuestField(questName, "State", state);
                SendUpdateTracker();
                InformQuestStateChange(questName);
            }
            else
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Quest '" + questName + "' doesn't exist. Can't set state to " + state);
            }
        }

        private static void SendUpdateTracker()
        {
            DialogueManager.SendUpdateTracker();
        }

        public static void InformQuestStateChange(string questName)
        {
            DialogueManager.instance.BroadcastMessage(DialogueSystemMessages.OnQuestStateChange, questName, SendMessageOptions.DontRequireReceiver);
        }

        public static void InformQuestEntryStateChange(string questName, int entryNumber)
        {
            DialogueManager.instance.BroadcastMessage(DialogueSystemMessages.OnQuestEntryStateChange, new QuestEntryArgs(questName, entryNumber), SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Reports whether a quest is unassigned.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the quest is unassigned; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        public static bool IsQuestUnassigned(string questName)
        {
            return GetQuestState(questName) == QuestState.Unassigned;
        }

        /// <summary>
        /// Reports whether a quest is active.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the quest is active; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        public static bool IsQuestActive(string questName)
        {
            return GetQuestState(questName) == QuestState.Active;
        }

        /// <summary>
        /// Reports whether a quest was successfully completed.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the quest was successfully completed; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        public static bool IsQuestSuccessful(string questName)
        {
            return GetQuestState(questName) == QuestState.Success;
        }

        /// <summary>
        /// Reports whether a quest ended in failure.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the quest ended in failure; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        public static bool IsQuestFailed(string questName)
        {
            return GetQuestState(questName) == QuestState.Failure;
        }

        /// <summary>
        /// Reports whether a quest was abandoned (i.e., in the Abandoned state).
        /// </summary>
        /// <returns><c>true</c> if the quest was abandoned; otherwise, <c>false</c>.</returns>
        /// <param name="questName">Name of the quest.</param>
        public static bool IsQuestAbandoned(string questName)
        {
            return GetQuestState(questName) == QuestState.Abandoned;
        }

        /// <summary>
        /// Reports whether a quest is done, either successful or failed.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the quest is done; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        public static bool IsQuestDone(string questName)
        {
            QuestState state = GetQuestState(questName);
            return ((state == QuestState.Success) || (state == QuestState.Failure));
        }

        /// <summary>
        /// Reports whether a quest's current state is one of the states marked in a state bit mask.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the quest's current state is in the state bit mask.
        /// </returns>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <param name='stateMask'>
        /// A QuestState bit mask (e.g., <c>QuestState.Success | QuestState.Failure</c>).
        /// </param>
        public static bool IsQuestInStateMask(string questName, QuestState stateMask)
        {
            QuestState state = GetQuestState(questName);
            return ((stateMask & state) == state);
        }

        /// <summary>
        /// Reports whether a quest entry's current state is one of the states marked in a state bit mask.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the quest entry's current state is in the state bit mask.
        /// </returns>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <param name="entryNumber">
        /// Quest entry number.
        /// </param>
        /// <param name='stateMask'>
        /// A QuestState bit mask (e.g., <c>QuestState.Success | QuestState.Failure</c>).
        /// </param>
        public static bool IsQuestEntryInStateMask(string questName, int entryNumber, QuestState stateMask)
        {
            QuestState state = GetQuestEntryState(questName, entryNumber);
            return ((stateMask & state) == state);
        }

        /// <summary>
        /// Starts a quest by setting its state to active.
        /// </summary>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <example>
        /// StartQuest("Kill 5 Rats");
        /// </example>
        public static void StartQuest(string questName)
        {
            SetQuestState(questName, QuestState.Active);
        }

        /// <summary>
        /// Marks a quest successful.
        /// </summary>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        public static void CompleteQuest(string questName)
        {
            SetQuestState(questName, QuestState.Success);
        }

        /// <summary>
        /// Marks a quest as failed.
        /// </summary>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        public static void FailQuest(string questName)
        {
            SetQuestState(questName, QuestState.Failure);
        }

        /// <summary>
        /// Marks a quest as abandoned (i.e., in the Abandoned state).
        /// </summary>
        /// <param name="questName">Name of the quest.</param>
        public static void AbandonQuest(string questName)
        {
            SetQuestState(questName, QuestState.Abandoned);
        }
        /// <summary>
        /// Converts a string representation into a state enum value.
        /// </summary>
        /// <returns>
        /// The state (e.g., <c>QuestState.Active</c>).
        /// </returns>
        /// <param name='s'>
        /// The string representation (e.g., "active").
        /// </param>
        public static QuestState DefaultStringToState(string s)
        {
            if (string.Equals(s, ActiveStateString)) return QuestState.Active;
            if (string.Equals(s, SuccessStateString) || string.Equals(s, DoneStateString)) return QuestState.Success;
            if (string.Equals(s, FailureStateString)) return QuestState.Failure;
            if (string.Equals(s, AbandonedStateString)) return QuestState.Abandoned;
            if (string.Equals(s, GrantableStateString)) return QuestState.Grantable;
            if (string.Equals(s, ReturnToNPCStateString)) return QuestState.ReturnToNPC;
            return QuestState.Unassigned;
        }

        public static string DefaultStateToString(QuestState state)
        {
            switch (state)
            {
                default:
                case QuestState.Unassigned: return UnassignedStateString;
                case QuestState.Active: return ActiveStateString;
                case QuestState.Success: return SuccessStateString;
                case QuestState.Failure: return FailureStateString;
                case QuestState.Abandoned: return AbandonedStateString;
                case QuestState.Grantable: return GrantableStateString;
                case QuestState.ReturnToNPC: return ReturnToNPCStateString;
            }
        }

        ///// <summary>
        ///// Converts a state to its string representation.
        ///// </summary>
        ///// <returns>
        ///// The string representation (e.g., "active").
        ///// </returns>
        ///// <param name='state'>
        ///// The state (e.g., <c>QuestState.Active</c>).
        ///// </param>
        //public static string StateToString(QuestState state)
        //{
        //    switch (state)
        //    {
        //        case QuestState.Unassigned: return UnassignedStateString;
        //        case QuestState.Active: return ActiveStateString;
        //        case QuestState.Success: return SuccessStateString;
        //        case QuestState.Failure: return FailureStateString;
        //        case QuestState.Abandoned: return AbandonedStateString;
        //        case QuestState.Grantable: return GrantableStateString;
        //        case QuestState.ReturnToNPC: return ReturnToNPCStateString;
        //        default: return UnassignedStateString;
        //    }
        //}

        /// <summary>
        /// Gets the localized quest display name.
        /// </summary>
        /// <returns>
        /// The quest title (display name) in the current language.
        /// </returns>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        public static string GetQuestTitle(string questName)
        {
            var title = DialogueLua.GetLocalizedQuestField(questName, "Display Name").asString;
            if (string.IsNullOrEmpty(title)) title = DialogueLua.GetLocalizedQuestField(questName, "Name").asString;
            return title;
        }

        /// <summary>
        /// Gets a quest description, based on the current state of the quest (i.e., SuccessDescription, FailureDescription, or just Description).
        /// </summary>
        /// <returns>
        /// The quest description.
        /// </returns>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <example>
        /// GUILayout.Label("Objective: " + QuestLog.GetQuestDescription("Kill 5 Rats"));
        /// </example>
        public static string GetQuestDescription(string questName)
        {
            switch (GetQuestState(questName))
            {
                case QuestState.Success:
                    return GetQuestDescription(questName, QuestState.Success) ?? GetQuestDescription(questName, QuestState.Active);
                case QuestState.Failure:
                    return GetQuestDescription(questName, QuestState.Failure) ?? GetQuestDescription(questName, QuestState.Active);
                default:
                    return GetQuestDescription(questName, QuestState.Active);
            }
        }

        /// <summary>
        /// Gets the localized quest description for a specific state.
        /// </summary>
        /// <returns>
        /// The quest description.
        /// </returns>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <param name='state'>
        /// State to check.
        /// </param>
        public static string GetQuestDescription(string questName, QuestState state)
        {
            string descriptionFieldName = GetDefaultDescriptionFieldForState(state);
            string result = DialogueLua.GetLocalizedQuestField(questName, descriptionFieldName).asString;
            return (string.Equals(result, "nil") || string.IsNullOrEmpty(result)) ? null : result;
        }

        private static string GetDefaultDescriptionFieldForState(QuestState state)
        {
            switch (state)
            {
                case QuestState.Success:
                    return "Success_Description";
                case QuestState.Failure:
                    return "Failure_Description";
                default:
                    return "Description";
            }
        }

        /// <summary>
        /// Sets the quest description for a specified state.
        /// </summary>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <param name='state'>
        /// Set the description for this state (i.e., regular, success, or failure).
        /// </param>
        /// <param name='description'>
        /// The description.
        /// </param>
        public static void SetQuestDescription(string questName, QuestState state, string description)
        {
            DialogueLua.SetQuestField(questName, GetDefaultDescriptionFieldForState(state), description);
        }

        /// <summary>
        /// Gets the quest abandon sequence. The QuestLogWindow plays this sequence when the player
        /// abandons a quest.
        /// </summary>
        /// <returns>The quest abandon sequence.</returns>
        /// <param name="questName">Quest name.</param>
        public static string GetQuestAbandonSequence(string questName)
        {
            return DialogueLua.GetLocalizedQuestField(questName, "Abandon Sequence").asString;
        }

        /// <summary>
        /// Sets the quest abandon sequence. The QuestLogWindow plays this sequence when the 
        /// player abandons a quest.
        /// </summary>
        /// <param name="questName">Quest name.</param>
        /// <param name="sequence">Sequence to play when the quest is abandoned.</param>
        public static void SetQuestAbandonSequence(string questName, string sequence)
        {
            DialogueLua.SetLocalizedQuestField(questName, "Abandon Sequence", sequence);
        }

        /// <summary>
        /// Gets the quest entry count.
        /// </summary>
        /// <returns>The quest entry count.</returns>
        /// <param name="questName">Name of the quest.</param>
        public static int GetQuestEntryCount(string questName)
        {
            return DialogueLua.GetQuestField(questName, "Entry_Count").asInt;
        }

        /// <summary>
        /// Adds a quest entry to a quest.
        /// </summary>
        /// <param name="questName">Name of the quest.</param>
        /// <param name="entryNumber">Entry number.</param>
        /// <param name="description">The quest entry description.</param>
        public static void AddQuestEntry(string questName, string description)
        {
            int entryCount = GetQuestEntryCount(questName);
            entryCount++;
            DialogueLua.SetQuestField(questName, "Entry_Count", entryCount);
            string entryFieldName = GetEntryFieldName(entryCount);
            DialogueLua.SetQuestField(questName, entryFieldName, DialogueLua.DoubleQuotesToSingle(description));
            string entryStateFieldName = GetEntryStateFieldName(entryCount);
            DialogueLua.SetQuestField(questName, entryStateFieldName, "unassigned");
        }

        /// <summary>
        /// Gets the localized quest entry description.
        /// </summary>
        /// <returns>The quest entry description.</returns>
        /// <param name="questName">Name of the quest.</param>
        /// <param name="entryNumber">Entry number.</param>
        public static string GetQuestEntry(string questName, int entryNumber)
        {
            string entryFieldName = GetEntryFieldName(entryNumber);
            return DialogueLua.GetLocalizedQuestField(questName, entryFieldName).asString;
        }

        /// <summary>
        /// Sets the localized quest entry description.
        /// </summary>
        /// <param name="questName">Name of the quest.</param>
        /// <param name="entryNumber">Entry number.</param>
        /// <param name="description">The quest entry description.</param>
        public static void SetQuestEntry(string questName, int entryNumber, string description)
        {
            string entryFieldName = GetEntryFieldName(entryNumber);
            DialogueLua.SetLocalizedQuestField(questName, entryFieldName, DialogueLua.DoubleQuotesToSingle(description));
        }

        /// <summary>
        /// Gets the state of the quest entry.
        /// </summary>
        /// <returns>The quest entry state.</returns>
        /// <param name="questName">Name of the quest.</param>
        /// <param name="entryNumber">Entry number.</param>
        public static QuestState GetQuestEntryState(string questName, int entryNumber)
        {
            //---Was: string s = DialogueLua.GetQuestField(questName, GetEntryStateFieldName(entryNumber)).AsString;
            //---     return StringToState(s);
            return StringToState(CurrentQuestEntryState(questName, entryNumber));
        }

        /// <summary>
        /// Gets the state of the quest entry.
        /// </summary>
        /// <returns>The quest entry state.</returns>
        /// <param name="questName">Name of the quest.</param>
        /// <param name="entryNumber">Entry number.</param>
        public static string CurrentQuestEntryState(string questName, double entryNumber)
        {
            if (CurrentQuestEntryStateOverride != null)
            {
                return CurrentQuestEntryStateOverride(questName, (int)entryNumber);
            }
            else
            {
                return DefaultCurrentQuestEntryState(questName, (int)entryNumber);
            }
        }

        /// <summary>
        /// Default built-in implementation of CurrentQuestEntryState.
        /// </summary>
        public static string DefaultCurrentQuestEntryState(string questName, int entryNumber)
        {
            return DialogueLua.GetQuestField(questName, GetEntryStateFieldName((int)entryNumber)).asString;
        }

        /// <summary>
        /// Sets the state of the quest entry.
        /// </summary>
        /// <param name="questName">Name of the quest.</param>
        /// <param name="entryNumber">Entry number.</param>
        /// <param name="state">State.</param>
        public static void SetQuestEntryState(string questName, int entryNumber, QuestState state)
        {
            SetQuestEntryState(questName, entryNumber, StateToString(state));
        }

        /// <summary>
        /// Sets the state of a quest entry.
        /// </summary>
        /// <param name="questName">Name of the quest.</param>
        /// <param name="entryNumber">Entry number.</param>
        /// <param name="state">State.</param>
        public static void SetQuestEntryState(string questName, double entryNumber, string state)
        {
            if (SetQuestEntryStateOverride != null)
            {
                SetQuestEntryStateOverride(questName, (int)entryNumber, state);
            }
            else
            {
                DefaultSetQuestEntryState(questName, (int)entryNumber, state);
            }
        }

        /// <summary>
        /// Default built-in method to set quest entry state.
        /// </summary>
        public static void DefaultSetQuestEntryState(string questName, int entryNumber, string state)
        {
            if (DialogueLua.DoesTableElementExist("Quest", questName))
            {
                DialogueLua.SetQuestField(questName, GetEntryStateFieldName((int)entryNumber), state);
                InformQuestStateChange(questName);
                InformQuestEntryStateChange(questName, (int)entryNumber);
                SendUpdateTracker();
            }
            else
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Quest '" + questName + "' doesn't exist. Can't set entry " + (int)entryNumber + " state to " + state);
            }
        }

        public static string GetEntryFieldName(int entryNumber)
        {
            return string.Format("Entry_{0}", new System.Object[] { entryNumber });
        }

        public static string GetEntryStateFieldName(int entryNumber)
        {
            return string.Format("Entry_{0}_State", new System.Object[] { entryNumber });
        }

        /// <summary>
        /// Determines if quest tracking is available (that is, if the quest has a "Trackable" field).
        /// </summary>
        /// <returns><c>true</c> if quest tracking is available; otherwise, <c>false</c>.</returns>
        /// <param name="questName">Quest name.</param>
        public static bool IsQuestTrackingAvailable(string questName)
        {
            return DialogueLua.GetQuestField(questName, "Trackable").asBool;
        }

        /// <summary>
        /// Specifies whether quest tracking is available (that is, if the quest has a "Trackable" field).
        /// </summary>
        /// <param name="questName">Quest name.</param>
        /// <param name="value">Trackable or not.</param>
        public static void SetQuestTrackingAvailable(string questName, bool value)
        {
            DialogueLua.SetQuestField(questName, "Trackable", value);
            SendUpdateTracker();
        }

        /// <summary>
        /// Determines if tracking is enabled for a quest.
        /// </summary>
        /// <returns><c>true</c> if tracking enabled on the specified quest; otherwise, <c>false</c>.</returns>
        /// <param name="questName">Quest name.</param>
        public static bool IsQuestTrackingEnabled(string questName)
        {
            return IsQuestTrackingAvailable(questName)
                ? DialogueLua.GetQuestField(questName, "Track").asBool
                : false;
        }

        /// <summary>
        /// Sets quest tracking on or off (by setting the Track field). If turning on tracking
        /// and tracking is currently not available (Trackable field is false), this also sets
        /// the Trackable field true.
        /// </summary>
        /// <param name="questName">Quest name.</param>
        /// <param name="value">If set to <c>true</c>, tracking is enabled.</param>
        public static void SetQuestTracking(string questName, bool value)
        {
            if (value == true)
            {
                // If only one quest can be tracked at time, untrack all others:
                if (trackOneQuestAtATime)
                {
                    var quests = GetAllQuests();
                    foreach (var otherQuestName in quests)
                    {
                        if (string.Equals(otherQuestName, questName)) continue;
                        if (IsQuestTrackingEnabled(otherQuestName))
                        {
                            DialogueLua.SetQuestField(otherQuestName, "Track", false);
                            DialogueManager.instance.BroadcastMessage(DialogueSystemMessages.OnQuestTrackingDisabled, otherQuestName, SendMessageOptions.DontRequireReceiver);
                        }
                    }
                }
                // Make sure tracking is set to be available for this quest:
                if (!IsQuestTrackingAvailable(questName))
                {
                    SetQuestTrackingAvailable(questName, true);
                }
            }
            // Track this quest:
            DialogueLua.SetQuestField(questName, "Track", value);
            SendUpdateTracker();
            DialogueManager.instance.BroadcastMessage(value ? DialogueSystemMessages.OnQuestTrackingEnabled : DialogueSystemMessages.OnQuestTrackingDisabled, questName, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Determines if a quest is abandonable (that is, is has a field named "Abandonable" that's true.)
        /// </summary>
        /// <returns><c>true</c> if the quest is abandonable; otherwise, <c>false</c>.</returns>
        /// <param name="questName">Quest name.</param>
        public static bool IsQuestAbandonable(string questName)
        {
            return DialogueLua.GetQuestField(questName, "Abandonable").asBool;
        }

        /// <summary>
        /// Returns true if quest has a field named "Visible" that is currently true or doesn't have the field.
        /// </summary>
        public static bool IsQuestVisible(string questName)
        {
            var result = Lua.Run($"return Quest[{DialogueLua.StringToTableIndex(questName)}].Visible").asString;
            if (string.IsNullOrEmpty(result) || string.Equals(result, "nil")) return true;
            return string.Compare(result, "false", true) == 0;
        }

        /// <summary>
        /// Sets a quest's Visible field true or false.
        /// </summary>
        public static void SetQuestVisibility(string questName)
        {
            DialogueLua.SetQuestField(questName, "Visible", true);
        }

        /// <summary>
        /// Returns true if quest has a field named "Viewed" that is currently true.
        /// Used if QuestLogWindow.newQuestText is not blank.
        /// </summary>
        public static bool WasQuestViewed(string questName)
        {
            return DialogueLua.GetQuestField(questName, "Viewed").asBool;
        }

        /// <summary>
        /// Marks a quest as viewed (i.e., in the quest log window).
        /// Generally only set/used when QuestLogWindow.newQuestText is not blank.
        /// </summary>
        /// <param name="questName"></param>
        public static void MarkQuestViewed(string questName)
        {
            DialogueLua.SetQuestField(questName, "Viewed", true);
        }

        /// <summary>
        /// Gets the group that a quest belongs to.
        /// </summary>
        /// <returns>The quest group name, or empty string if no group.</returns>
        /// <param name="questName">Quest name.</param>
        public static string GetQuestGroup(string questName)
        {
            return DialogueLua.GetLocalizedQuestField(questName, "Group").asString;
        }

        public static string GetQuestGroupDisplayName(string questName)
        {
            var result = DialogueLua.GetLocalizedQuestField(questName, "Group Display Name").asString;
            if (string.IsNullOrEmpty(result) || result == "nil") result = GetQuestGroup(questName);
            return result;
        }

        /// <summary>
        /// Gets all quest group names.
        /// </summary>
        /// <returns>The group names for active quests, sorted by name.</returns>
        public static string[] GetAllGroups()
        {
            return GetAllGroups(QuestState.Active, true);
        }

        /// <summary>
        /// Gets all quest group names.
        /// </summary>
        /// <returns>The group names, sorted by name.</returns>
        /// <param name="flags">Flags for the quest states to filter.</param>
        public static string[] GetAllGroups(QuestState flags)
        {
            return GetAllGroups(flags, true);
        }

        /// <summary>
        /// Gets all quest group names.
        /// </summary>
        /// <returns>The group names.</returns>
        /// <param name="flags">Flags for the quest states to filter.</param>
        /// <param name="sortByGroupName">If set to <c>true</c> sort by group name.</param>
        public static string[] GetAllGroups(QuestState flags, bool sortByGroupName)
        {
            List<string> groups = new List<string>();
            LuaTableWrapper itemTable = Lua.Run("return Item").asTable;
            if (!itemTable.isValid)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Quest Log couldn't access Lua Item[] table. Has the Dialogue Manager loaded a database yet?", new System.Object[] { DialogueDebug.Prefix }));
                return groups.ToArray();
            }
            foreach (var itemTableValue in itemTable.values)
            {
                LuaTableWrapper fields = itemTableValue as LuaTableWrapper;
                if (fields == null) continue;
                string questName = null;
                string group = null;
                bool isItem = false;
                try
                {
                    object questNameObject = fields["Name"];
                    questName = (questNameObject != null) ? questNameObject.ToString() : string.Empty;
                    object groupObject = fields["Group"];
                    group = (groupObject != null) ? groupObject.ToString() : string.Empty;
                    isItem = false;
                    object isItemObject = fields["Is_Item"];
                    if (isItemObject != null)
                    {
                        if (isItemObject.GetType() == typeof(bool))
                        {
                            isItem = (bool)isItemObject;
                        }
                        else
                        {
                            isItem = Tools.StringToBool(isItemObject.ToString());
                        }
                    }
                }
                catch { }
                if (!isItem)
                {
                    if (!groups.Contains(group) && IsQuestInStateMask(questName, flags))
                    {
                        groups.Add(group);
                    }
                }
            }
            if (sortByGroupName) groups.Sort();
            return groups.ToArray();
        }

        /// <summary>
        /// Gets an array of all active quests.
        /// </summary>
        /// <returns>
        /// The names of all active quests, sorted by Name.
        /// </returns>
        /// <example>
        /// string[] activeQuests = QuestLog.GetAllQuests();
        /// </example>
        public static string[] GetAllQuests()
        {
            return GetAllQuests(QuestState.Active, true, null);
        }

        /// <summary>
        /// Gets an array of all quests matching the specified state bitmask.
        /// </summary>
        /// <returns>The names of all quests matching the specified state bitmask, sorted by Name.</returns>
        /// <param name="flags">A bitmask of QuestState values.</param>
        /// <example>
        /// string[] completedQuests = QuestLog.GetAllQuests( QuestState.Success | QuestState.Failure );
        /// </example>
        public static string[] GetAllQuests(QuestState flags)
        {
            return GetAllQuests(flags, true, null);
        }

        /// <summary>
        /// Gets an array of all quests matching the specified state bitmask.
        /// </summary>
        /// <returns>The names of all quests matching the specified state bitmask.</returns>
        /// <param name='flags'>A bitmask of QuestState values.</param>
        /// <param name='sortByName'>If `true`, sorts the names by name.</param>
        /// <example>
        /// string[] completedQuests = QuestLog.GetAllQuests( QuestState.Success | QuestState.Failure, true );
        /// </example>
        public static string[] GetAllQuests(QuestState flags, bool sortByName)
        {
            return GetAllQuests(flags, sortByName, null);
        }

        /// <summary>
        /// Gets an array of all quests matching the specified state bitmask and in the specified group.
        /// </summary>
        /// <returns>The names of all quests matching the specified state bitmask.</returns>
        /// <param name='flags'>A bitmask of QuestState values.</param>
        /// <param name='sortByName'>If `true`, sorts the names by name.</param>
        /// <param name='group'>If not null, return only quests in the specified group.</param>
        /// <example>
        /// string[] completedQuests = QuestLog.GetAllQuests( QuestState.Success | QuestState.Failure, true );
        /// </example>
        public static string[] GetAllQuests(QuestState flags, bool sortByName, string group)
        {
            List<string> questNames = new List<string>();
            LuaTableWrapper itemTable = Lua.Run("return Item").asTable;
            if (!itemTable.isValid)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Quest Log couldn't access Lua Item[] table. Has the Dialogue Manager loaded a database yet?", new System.Object[] { DialogueDebug.Prefix }));
                return questNames.ToArray();
            }
            var filterGroup = (group != null);
            foreach (var itemTableValue in itemTable.values)
            {
                LuaTableWrapper fields = itemTableValue as LuaTableWrapper;
                if (fields == null) continue;
                string questName = null;
                string thisGroup = null;
                bool isItem = false;
                try
                {
                    object questNameObject = fields["Name"];
                    questName = (questNameObject != null) ? questNameObject.ToString() : string.Empty;
                    if (filterGroup)
                    {
                        object groupObject = fields["Group"];
                        thisGroup = (groupObject != null) ? groupObject.ToString() : string.Empty;
                    }
                    isItem = false;
                    object isItemObject = fields["Is_Item"];
                    if (isItemObject != null)
                    {
                        if (isItemObject.GetType() == typeof(bool))
                        {
                            isItem = (bool)isItemObject;
                        }
                        else
                        {
                            isItem = Tools.StringToBool(isItemObject.ToString());
                        }
                    }
                }
                catch { }
                if (!isItem)
                {
                    if (string.IsNullOrEmpty(questName))
                    {
                        if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: A quest name (item name in Item[] table) is null or empty", new System.Object[] { DialogueDebug.Prefix }));
                    }
                    else if (!filterGroup || string.Equals(group, thisGroup))
                    {
                        if (IsQuestInStateMask(questName, flags))
                        {
                            questNames.Add(questName);
                        }
                    }
                }
            }
            if (sortByName) questNames.Sort();
            return questNames.ToArray();
        }

        /// <summary>
        /// Gets all quests (including their group names) in a specified state.
        /// </summary>
        /// <returns>An array of QuestGroupRecord elements.</returns>
        /// <param name="flags">A bitmask of QuestState values.</param>
        /// <param name="flags">Sort by group and name.</param>
        public static QuestGroupRecord[] GetAllGroupsAndQuests(QuestState flags, bool sort = true)
        {
            List<QuestGroupRecord> list = new List<QuestGroupRecord>();
            LuaTableWrapper itemTable = Lua.Run("return Item").asTable;
            if (!itemTable.isValid)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Quest Log couldn't access Lua Item[] table. Has the Dialogue Manager loaded a database yet?", new System.Object[] { DialogueDebug.Prefix }));
                return list.ToArray();
            }
            foreach (var itemTableValue in itemTable.values)
            {
                LuaTableWrapper fields = itemTableValue as LuaTableWrapper;
                if (fields == null) continue;
                string questName = null;
                string group = null;
                bool isItem = false;
                try
                {
                    object questNameObject = fields["Name"];
                    questName = (questNameObject != null) ? questNameObject.ToString() : string.Empty;
                    object groupObject = fields["Group"];
                    group = (groupObject != null) ? groupObject.ToString() : string.Empty;
                    isItem = false;
                    object isItemObject = fields["Is_Item"];
                    if (isItemObject != null)
                    {
                        if (isItemObject.GetType() == typeof(bool))
                        {
                            isItem = (bool)isItemObject;
                        }
                        else
                        {
                            isItem = Tools.StringToBool(isItemObject.ToString());
                        }
                    }
                }
                catch { }
                if (!isItem)
                {
                    if (string.IsNullOrEmpty(questName))
                    {
                        if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: A quest name (item name in Item[] table) is null or empty", new System.Object[] { DialogueDebug.Prefix }));
                    }
                    else if (IsQuestInStateMask(questName, flags))
                    {
                        list.Add(new QuestGroupRecord(group, questName));
                    }
                }
            }
            if (sort) list.Sort();
            return list.ToArray();
        }

        /// <summary>
        /// Quest changed delegate.
        /// </summary>
        public delegate void QuestChangedDelegate(string questName, QuestState newState);

        /// <summary>
        /// The quest watch item class is used internally by the QuestLog class to manage
        /// Lua observers on quest states.
        /// </summary>
        public class QuestWatchItem
        {

            private string questName;
            private int entryNumber;
            private LuaWatchFrequency frequency;
            private string luaExpression;
            private QuestChangedDelegate questChangedHandler;

            public QuestWatchItem(string questName, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler)
            {
                this.questName = questName;
                this.entryNumber = 0;
                this.frequency = frequency;
                this.luaExpression = string.Format("return Item[\"{0}\"].State", new System.Object[] { DialogueLua.StringToTableIndex(questName) });
                this.questChangedHandler = questChangedHandler;
                DialogueManager.AddLuaObserver(luaExpression, frequency, OnLuaChanged);
            }

            public QuestWatchItem(string questName, int entryNumber, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler)
            {
                this.questName = questName;
                this.entryNumber = entryNumber;
                this.frequency = frequency;
                this.luaExpression = string.Format("return Item[\"{0}\"].Entry_{1}_State", new System.Object[] { DialogueLua.StringToTableIndex(questName), entryNumber });
                this.questChangedHandler = questChangedHandler;
                DialogueManager.AddLuaObserver(luaExpression, frequency, OnLuaChanged);
            }

            public bool Matches(string questName, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler)
            {
                return string.Equals(questName, this.questName) && (frequency == this.frequency) && (questChangedHandler == this.questChangedHandler);
            }

            public bool Matches(string questName, int entryNumber, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler)
            {
                return string.Equals(questName, this.questName) && (entryNumber == this.entryNumber) && (frequency == this.frequency) && (questChangedHandler == this.questChangedHandler);
            }

            public void StopObserving()
            {
                DialogueManager.RemoveLuaObserver(luaExpression, frequency, OnLuaChanged);
            }

            private void OnLuaChanged(LuaWatchItem luaWatchItem, Lua.Result newResult)
            {
                if (string.Equals(luaWatchItem.luaExpression, this.luaExpression) && (questChangedHandler != null))
                {
                    questChangedHandler(questName, StringToState(newResult.asString));
                }
            }
        }

        /// <summary>
        /// The quest watch list.
        /// </summary>
        private static readonly List<QuestWatchItem> questWatchList = new List<QuestWatchItem>();

        /// <summary>
        /// Adds a quest state observer.
        /// </summary>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <param name='frequency'>
        /// Frequency to check the quest state.
        /// </param>
        /// <param name='questChangedHandler'>
        /// Delegate to call when the quest state changes. This should be in the form:
        /// <code>void MyDelegate(string questName, QuestState newState) {...}</code>
        /// </param>
        public static void AddQuestStateObserver(string questName, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler)
        {
            questWatchList.Add(new QuestWatchItem(questName, frequency, questChangedHandler));
        }

        /// <summary>
        /// Adds a quest state observer.
        /// </summary>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <param name='entryNumber'>
        /// The entry number (1...Entry Count) in the quest.
        /// </param>
        /// <param name='frequency'>
        /// Frequency to check the quest state.
        /// </param>
        /// <param name='questChangedHandler'>
        /// Delegate to call when the quest state changes. This should be in the form:
        /// <code>void MyDelegate(string questName, QuestState newState) {...}</code>
        /// </param>
        public static void AddQuestStateObserver(string questName, int entryNumber, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler)
        {
            questWatchList.Add(new QuestWatchItem(questName, entryNumber, frequency, questChangedHandler));
        }

        /// <summary>
        /// Removes a quest state observer. To be removed, the questName, frequency, and delegate must
        /// all match.
        /// </summary>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <param name='frequency'>
        /// Frequency that the quest state is being checked.
        /// </param>
        /// <param name='questChangedHandler'>
        /// Quest changed handler delegate.
        /// </param>
        public static void RemoveQuestStateObserver(string questName, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler)
        {
            foreach (var questWatchItem in questWatchList)
            {
                if (questWatchItem.Matches(questName, frequency, questChangedHandler)) questWatchItem.StopObserving();
            }
            questWatchList.RemoveAll(questWatchItem => questWatchItem.Matches(questName, frequency, questChangedHandler));
        }

        /// <summary>
        /// Removes a quest state observer. To be removed, the questName, frequency, and delegate must
        /// all match.
        /// </summary>
        /// <param name='questName'>
        /// Name of the quest.
        /// </param>
        /// <param name='entryNumber'>
        /// The entry number (1...Entry Count) in the quest.
        /// </param>
        /// <param name='frequency'>
        /// Frequency that the quest state is being checked.
        /// </param>
        /// <param name='questChangedHandler'>
        /// Quest changed handler delegate.
        /// </param>
        public static void RemoveQuestStateObserver(string questName, int entryNumber, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler)
        {
            foreach (var questWatchItem in questWatchList)
            {
                if (questWatchItem.Matches(questName, entryNumber, frequency, questChangedHandler)) questWatchItem.StopObserving();
            }
            questWatchList.RemoveAll(questWatchItem => questWatchItem.Matches(questName, entryNumber, frequency, questChangedHandler));
        }

        /// <summary>
        /// Removes all quest state observers.
        /// </summary>
        public static void RemoveAllQuestStateObservers()
        {
            foreach (var questWatchItem in questWatchList)
            {
                questWatchItem.StopObserving();
            }
            questWatchList.Clear();
        }

        /// <summary>
        /// Updates all quest state listeners who are listening for questName.
        /// </summary>
        public static void UpdateQuestIndicators(string questName)
        {
            var dispatcher = GameObjectUtility.FindFirstObjectByType<QuestStateDispatcher>();
            if (dispatcher != null) dispatcher.OnQuestStateChange(questName);
        }

    }

}
