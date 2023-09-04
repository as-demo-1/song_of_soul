// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This is the abstract base class for quest log windows. You can implement a quest log
    /// window in any GUI system by creating a subclass.
    /// 
    /// When open, the window displays active and completed quests. It gets the titles, 
    /// descriptions, and states of the quests from the QuestLog class.
    /// 
    /// The window allows the player to abandon quests (if the quest's Abandonable field is
    /// true) and toggle tracking (if the quest's Trackable field is true).
    /// </summary>
    /// <remarks>
    /// If pauseWhileOpen is set to <c>true</c>, the quest log window pauses the game by setting 
    /// <c>Time.timeScale</c> to <c>0</c>. When closed, it restores the previous time scale.
    /// </remarks>
    public abstract class QuestLogWindow : MonoBehaviour
    {

        [Tooltip("Optional localized text table to use to localize no active/completed quests.")]
        public TextTable textTable = null; // v2: changed from LocalizedTextTable.

        [Tooltip("Text to show (or localize) when there are no active quests.")]
        public string noActiveQuestsText = "No Active Quests";

        [Tooltip("Text to show (or localize) when there are no completed quests.")]
        public string noCompletedQuestsText = "No Completed Quests";

        [Tooltip("Check if quest has a field named 'Visible'. If field is false, don't show quest.")]
        public bool checkVisibleField = false;

        public enum QuestHeadingSource
        {
            /// <summary>
            /// Use the name of the item for the quest heading.
            /// </summary>
            Name,

            /// <summary>
            /// Use the item's Description field for the quest heading.
            /// </summary>
            Description
        };

        /// <summary>
        /// The quest title source.
        /// </summary>
        public QuestHeadingSource questHeadingSource = QuestHeadingSource.Name;

        /// <summary>
        /// The state to assign abandoned quests.
        /// </summary>
        [Tooltip("State to assign to quests when player abandons then.")]
        [QuestState]
        public QuestState abandonQuestState = QuestState.Unassigned;

        /// <summary>
        /// If <c>true</c>, the window sets <c>Time.timeScale = 0</c> to pause the game while 
        /// displaying the quest log window.
        /// </summary>
        public bool pauseWhileOpen = true;

        /// <summary>
        /// If <c>true</c>, the cursor is unlocked while the quest log window is open.
        /// </summary>
        public bool unlockCursorWhileOpen = true;

        /// <summary>
        /// If <c>true</c>, organize the quests by group.
        /// </summary>
        [Tooltip("Organize quests by the values of their Group fields.")]
        public bool useGroups = false;

        [Tooltip("If not blank, show this text next to quest titles that haven't been viewed yet. Will be localized if text has entry in Dialogue Manager's Text Table.")]
        public string newQuestText = string.Empty;

        [Tooltip("Allow only one quest to be tracked at a time.")]
        public bool trackOneQuestAtATime = false;

        [Tooltip("Clicking again on selected quest title deselects quest.")]
        public bool deselectQuestOnSecondClick = true;

        [Serializable]
        public class QuestInfo
        {
            public string Group { get; set; }
            public string GroupDisplayName { get; set; }
            public string Title { get; set; }
            public FormattedText Heading { get; set; }
            public FormattedText Description { get; set; }
            public FormattedText[] Entries { get; set; }
            public QuestState[] EntryStates { get; set; }
            public bool Trackable { get; set; }
            public bool Track { get; set; }
            public bool Abandonable { get; set; }
            public QuestInfo(string group, string groupDisplayName, string title, FormattedText heading, FormattedText description,
                             FormattedText[] entries, QuestState[] entryStates, bool trackable,
                             bool track, bool abandonable)
            {
                this.Group = group;
                this.GroupDisplayName = groupDisplayName;
                this.Title = title;
                this.Heading = heading;
                this.Description = description;
                this.Entries = entries;
                this.EntryStates = entryStates;
                this.Trackable = trackable;
                this.Track = track;
                this.Abandonable = abandonable;
            }
            public QuestInfo(string group, string title, FormattedText heading, FormattedText description,
                             FormattedText[] entries, QuestState[] entryStates, bool trackable,
                             bool track, bool abandonable)
            {
                this.Group = group;
                this.GroupDisplayName = string.Empty;
                this.Title = title;
                this.Heading = heading;
                this.Description = description;
                this.Entries = entries;
                this.EntryStates = entryStates;
                this.Trackable = trackable;
                this.Track = track;
                this.Abandonable = abandonable;
            }
            public QuestInfo(string title, FormattedText heading, FormattedText description,
                             FormattedText[] entries, QuestState[] entryStates, bool trackable,
                             bool track, bool abandonable)
            {
                this.Group = string.Empty;
                this.GroupDisplayName = string.Empty;
                this.Title = title;
                this.Heading = heading;
                this.Description = description;
                this.Entries = entries;
                this.EntryStates = entryStates;
                this.Trackable = trackable;
                this.Track = track;
                this.Abandonable = abandonable;
            }
        }

        /// <summary>
        /// Indicates whether the quest log window is currently open.
        /// </summary>
        /// <value>
        /// <c>true</c> if open; otherwise, <c>false</c>.
        /// </value>
        public bool isOpen { get; protected set; }

        /// <summary>
        /// The current list of quests. This will change based on whether the player is
        /// viewing active or completed quests.
        /// </summary>
        /// <value>The quests.</value>
        public QuestInfo[] quests { get; protected set; }

        /// <summary>
        /// The current list of quest groups.
        /// </summary>
        /// <value>The quest group names.</value>
        public string[] groups { get; protected set; }

        /// <summary>
        /// The title of the currently-selected quest.
        /// </summary>
        /// <value>The selected quest.</value>
        public string selectedQuest { get; protected set; }

        /// <summary>
        /// The message to show if Quests[] is empty.
        /// </summary>
        /// <value>The no quests message.</value>
        public string noQuestsMessage { get; protected set; }

        /// <summary>
        /// Indicates whether the window is showing active quests or completed quests.
        /// </summary>
        /// <value><c>true</c> if showing active quests; otherwise, <c>false</c>.</value>
        public virtual bool isShowingActiveQuests { get { return currentQuestStateMask == ActiveQuestStateMask; } }

        /// @cond FOR_V1_COMPATIBILITY
        public bool IsOpen { get { return isOpen; } protected set { isOpen = value; } }
        public QuestInfo[] Quests { get { return quests; } protected set { quests = value; } }
        public string[] Groups { get { return groups; } protected set { groups = value; } }
        public string SelectedQuest { get { return selectedQuest; } protected set { selectedQuest = value; } }
        public string NoQuestsMessage { get { return noQuestsMessage; } protected set { noQuestsMessage = value; } }
        public bool IsShowingActiveQuests { get { return isShowingActiveQuests; } }
        /// @endcond

        protected const QuestState ActiveQuestStateMask = QuestState.Active | QuestState.ReturnToNPC;

        /// <summary>
        /// The current quest state mask.
        /// </summary>
        protected QuestState currentQuestStateMask = ActiveQuestStateMask;

        /// <summary>
        /// The previous time scale prior to opening the window.
        /// </summary>
        protected float previousTimeScale = 1;

        protected Coroutine refreshCoroutine = null;

        protected bool started = false;

        public virtual void Awake()
        {
            isOpen = false;
            quests = new QuestInfo[0];
            groups = new string[0];
            selectedQuest = string.Empty;
            noQuestsMessage = string.Empty;
        }

        protected virtual void Start()
        {
            started = true;
            RegisterForUpdateTrackerEvents();
        }

        protected virtual void OnEnable()
        {
            if (started) RegisterForUpdateTrackerEvents();
        }

        protected virtual void OnDisable()
        {
            refreshCoroutine = null;
            UnregisterFromUpdateTrackerEvents();
        }

        protected void RegisterForUpdateTrackerEvents()
        {
            if (!started || DialogueManager.instance == null) return;
            if (GetComponentInParent<DialogueSystemController>() != null) return; // Children of Dialogue Manager automatically receive UpdateTracker; no need to register.
            DialogueManager.instance.receivedUpdateTracker -= UpdateTracker;
            DialogueManager.instance.receivedUpdateTracker += UpdateTracker;
        }

        protected void UnregisterFromUpdateTrackerEvents()
        {
            if (!started || DialogueManager.instance == null) return;
            DialogueManager.instance.receivedUpdateTracker -= UpdateTracker;
        }

        /// <summary>
        /// Opens the window. Your implementation should override this to handle any
        /// window-opening activity, then call openedWindowHandler at the end.
        /// </summary>
        /// <param name="openedWindowHandler">Opened window handler.</param>
        public virtual void OpenWindow(Action openedWindowHandler)
        {
            openedWindowHandler();
        }

        /// <summary>
        /// Closes the window. Your implementation should override this to handle any
        /// window-closing activity, then call closedWindowHandler at the end.
        /// </summary>
        /// <param name="openedWindowHandler">Closed window handler.</param>
        public virtual void CloseWindow(Action closedWindowHandler)
        {
            closedWindowHandler();
        }

        /// <summary>
        /// Called when the quest list has been updated -- for example, when switching between
        /// active and completed quests. Your implementation may override this to do processing.
        /// </summary>
        public virtual void OnQuestListUpdated() { }

        /// <summary>
        /// Asks the player to confirm abandonment of a quest. Your implementation should override
        /// this to show a modal dialogue box or something similar. If confirmed, it should call
        /// confirmedAbandonQuestHandler.
        /// </summary>
        /// <param name="title">Title.</param>
        /// <param name="confirmedAbandonQuestHandler">Confirmed abandon quest handler.</param>
        public virtual void ConfirmAbandonQuest(string title, Action confirmedAbandonQuestHandler) { }

        /// <summary>
        /// Opens the quest window.
        /// </summary>
        public virtual void Open()
        {
            QuestLog.trackOneQuestAtATime = trackOneQuestAtATime;
            PauseGameplay();
            OpenWindow(OnOpenedWindow);
        }

        protected virtual void OnOpenedWindow()
        {
            isOpen = true;
            ShowQuests(currentQuestStateMask);
        }

        /// <summary>
        /// Closes the quest log window. While you can call this manually in your own script, this
        /// method is normally called internally when the player clicks the close button. You can 
        /// call it manually to support alternate methods of closing the window.
        /// </summary>
        /// <example>
        /// if (Input.GetKeyDown(KeyCode.L) && myQuestLogWindow.IsOpen) {
        ///     myQuestLogWindow.Close();
        /// }
        /// </example>
        public virtual void Close()
        {
            //--- No need to clear it: selectedQuest = string.Empty;
            CloseWindow(OnClosedWindow);
        }

        protected virtual void OnClosedWindow()
        {
            isOpen = false;
            ResumeGameplay();
        }

        private bool wasCursorActive = false;

        protected virtual void PauseGameplay()
        {
            if (pauseWhileOpen)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0;
            }
            if (unlockCursorWhileOpen)
            {
                wasCursorActive = Tools.IsCursorActive();
                Tools.SetCursorActive(true);
            }
        }

        protected virtual void ResumeGameplay()
        {
            if (pauseWhileOpen) Time.timeScale = previousTimeScale;
            if (unlockCursorWhileOpen && !wasCursorActive) Tools.SetCursorActive(false);
        }

        public virtual bool IsQuestVisible(string questTitle)
        {
            return !checkVisibleField || Lua.IsTrue("Quest[\"" + DialogueLua.StringToTableIndex(questTitle) + "\"].Visible ~= false");
        }

        protected virtual void ShowQuests(QuestState questStateMask)
        {
            currentQuestStateMask = questStateMask;
            noQuestsMessage = GetNoQuestsMessage(questStateMask);
            List<QuestInfo> questList = new List<QuestInfo>();
            if (useGroups)
            {
                var records = QuestLog.GetAllGroupsAndQuests(questStateMask, true);
                foreach (var record in records)
                {
                    if (!IsQuestVisible(record.questTitle)) continue;
                    questList.Add(GetQuestInfo(record.groupName, record.questTitle));
                }
            }
            else
            {
                string[] titles = QuestLog.GetAllQuests(questStateMask, true, null);
                foreach (var title in titles)
                {
                    if (!IsQuestVisible(title)) continue;
                    questList.Add(GetQuestInfo(string.Empty, title));
                }
            }
            quests = questList.ToArray();
            OnQuestListUpdated();
        }

        protected virtual QuestInfo GetQuestInfo(string group, string title)
        {            
            FormattedText description = FormattedText.Parse(QuestLog.GetQuestDescription(title), DialogueManager.masterDatabase.emphasisSettings);
            FormattedText localizedTitle = FormattedText.Parse(QuestLog.GetQuestTitle(title), DialogueManager.masterDatabase.emphasisSettings);
            FormattedText heading = (questHeadingSource == QuestHeadingSource.Description) ? description : localizedTitle;
            string localizedGroup = string.IsNullOrEmpty(group) ? string.Empty : QuestLog.GetQuestGroup(title);
            string localizedGroupDisplayName = string.IsNullOrEmpty(group) ? string.Empty : QuestLog.GetQuestGroupDisplayName(title);
            bool abandonable = QuestLog.IsQuestAbandonable(title) && isShowingActiveQuests;
            bool trackable = QuestLog.IsQuestTrackingAvailable(title) && isShowingActiveQuests;
            bool track = QuestLog.IsQuestTrackingEnabled(title);
            int entryCount = QuestLog.GetQuestEntryCount(title);
            FormattedText[] entries = new FormattedText[entryCount];
            QuestState[] entryStates = new QuestState[entryCount];
            for (int i = 0; i < entryCount; i++)
            {
                entries[i] = FormattedText.Parse(QuestLog.GetQuestEntry(title, i + 1), DialogueManager.masterDatabase.emphasisSettings);
                entryStates[i] = QuestLog.GetQuestEntryState(title, i + 1);
            }

            // Check if need to show [new]:
            if (!string.IsNullOrEmpty(newQuestText))
            {
                if (!QuestLog.WasQuestViewed(title))
                {
                    heading.text += " " + FormattedText.Parse(DialogueManager.GetLocalizedText(newQuestText)).text;
                }
            }

            return new QuestInfo(localizedGroup, localizedGroupDisplayName, title, heading, description, entries, entryStates, trackable, track, abandonable);
        }

        /// <summary>
        /// Gets the "no quests" message for a quest state (active or success|failure). This
        /// method uses the strings "No Active Quests" and "No Completed Quests" or their
        /// localized equivalents if you've set the localized text table.
        /// </summary>
        /// <returns>The "no quests" message.</returns>
        /// <param name="questStateMask">Quest state mask.</param>
        protected virtual string GetNoQuestsMessage(QuestState questStateMask)
        {
            return (questStateMask == ActiveQuestStateMask) ? GetLocalizedText(noActiveQuestsText) : GetLocalizedText(noCompletedQuestsText);
        }

        /// <summary>
        /// Gets the localized text for a field name.
        /// </summary>
        /// <returns>The localized text.</returns>
        /// <param name="fieldName">Field name.</param>
        public virtual string GetLocalizedText(string fieldName)
        {
            if ((textTable != null) && textTable.HasFieldTextForLanguage(fieldName, Localization.GetCurrentLanguageID(textTable)))
            {
                return textTable.GetFieldTextForLanguage(fieldName, Localization.GetCurrentLanguageID(textTable));
            }
            else
            {
                return DialogueManager.GetLocalizedText(fieldName);
            }
        }

        /// <summary>
        /// Determines whether the specified questInfo is for the currently-selected quest.
        /// </summary>
        /// <returns><c>true</c> if this is the selected quest; otherwise, <c>false</c>.</returns>
        /// <param name="questInfo">Quest info.</param>
        public virtual bool IsSelectedQuest(QuestInfo questInfo)
        {
            return string.Equals(questInfo.Title, selectedQuest);
        }

        /// <summary>
        /// Your GUI close button should call this.
        /// </summary>
        /// <param name="data">Ignored.</param>
        public void ClickClose(object data)
        {
            Close();
        }

        /// <summary>
        /// Your GUI "show active quests" button should call this.
        /// </summary>
        /// <param name="data">Ignored.</param>
        public virtual void ClickShowActiveQuests(object data)
        {
            ShowQuests(ActiveQuestStateMask);
        }

        /// <summary>
        /// Your GUI "show completed quests" button should call this.
        /// </summary>
        /// <param name="data">Ignored.</param>
        public virtual void ClickShowCompletedQuests(object data)
        {
            ShowQuests(QuestState.Success | QuestState.Failure);
        }

        /// <summary>
        /// Your GUI should call this when the player clicks on a quest to expand
        /// or close it.
        /// </summary>
        /// <param name="data">The quest title.</param>
        public virtual void ClickQuest(object data)
        {
            if (!IsString(data)) return;
            string clickedQuest = (string)data;
            selectedQuest = (deselectQuestOnSecondClick && string.Equals(selectedQuest, clickedQuest)) ? string.Empty : clickedQuest;

            // Mark viewed:
            if (!string.IsNullOrEmpty(newQuestText))
            {
                QuestLog.MarkQuestViewed(selectedQuest);
                foreach (var quest in quests)
                {
                    if (IsSelectedQuest(quest))
                    {
                        var newQuestInfo = GetQuestInfo(quest.Group, quest.Title);
                        quest.Heading = newQuestInfo.Heading;
                        break;
                    }
                }
            }

            OnQuestListUpdated();
        }

        /// <summary>
        /// Your GUI should call this when the player clicks to abandon a quest.
        /// </summary>
        /// <param name="data">Ignored.</param>
        public virtual void ClickAbandonQuest(object data)
        {
            if (string.IsNullOrEmpty(selectedQuest)) return;
            ConfirmAbandonQuest(selectedQuest, OnConfirmAbandonQuest);
        }

        /// <summary>
        /// Your GUI should call this when the player confirms abandonment of a quest.
        /// </summary>
        protected virtual void OnConfirmAbandonQuest()
        {
            QuestLog.SetQuestState(selectedQuest, abandonQuestState);
            selectedQuest = string.Empty;
            ShowQuests(currentQuestStateMask);
            DialogueManager.instance.BroadcastMessage(DialogueSystemMessages.OnQuestTrackingDisabled, selectedQuest, SendMessageOptions.DontRequireReceiver);
            string sequence = QuestLog.GetQuestAbandonSequence(selectedQuest);
            if (!string.IsNullOrEmpty(sequence)) DialogueManager.PlaySequence(sequence);
        }

        /// <summary>
        /// Your GUI should call this when the player clicks to toggle quest tracking.
        /// </summary>
        /// <param name="data">Ignored.</param>
        public virtual void ClickTrackQuest(object data)
        {
            if (string.IsNullOrEmpty(selectedQuest)) return;
            bool track = !QuestLog.IsQuestTrackingEnabled(selectedQuest);
            QuestLog.SetQuestTracking(selectedQuest, track);
        }

        private bool IsString(object data)
        {
            return (data != null) && (data.GetType() == typeof(string));
        }

        // Parameter-less versions of methods for GUI systems that require them for button hookups:
        public virtual void ClickShowActiveQuestsButton()
        {
            ClickShowActiveQuests(null);
        }

        public void ClickShowCompletedQuestsButton()
        {
            ClickShowCompletedQuests(null);
        }

        public void ClickCloseButton()
        {
            ClickClose(null);
        }

        public void ClickAbandonQuestButton()
        {
            ClickAbandonQuest(null);
        }

        public void ClickTrackQuestButton()
        {
            ClickTrackQuest(null);
        }

        public void UpdateTracker()
        {
            if (isOpen)
            {
                if (refreshCoroutine == null)
                {
                    refreshCoroutine = StartCoroutine(UpdateQuestDisplayAtEndOfFrame());
                }
            }
        }

        protected IEnumerator UpdateQuestDisplayAtEndOfFrame()
        {
            yield return CoroutineUtility.endOfFrame;
            refreshCoroutine = null;
            ShowQuests(currentQuestStateMask);
        }

    }

}
