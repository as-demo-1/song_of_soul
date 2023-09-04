// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This is the Standard UI implementation of the abstract QuestLogWindow class.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class StandardUIQuestLogWindow : QuestLogWindow
    {

        #region Serialized Fields

        [Header("Main Panel")]

        public UIPanel mainPanel;
        public UITextField showingActiveQuestsHeading;
        public UITextField showingCompletedQuestHeading;
        [Tooltip("Button to switch display to active quests.")]
        public UnityEngine.UI.Button activeQuestsButton;
        [Tooltip("Button to switch display to completed quests.")]
        public UnityEngine.UI.Button completedQuestsButton;

        [Header("Selection Panel")]

        public RectTransform questSelectionContentContainer;
        public StandardUIFoldoutTemplate questGroupTemplate;
        [Tooltip("Use this template for active quests.")]
        public StandardUIQuestTitleButtonTemplate activeQuestHeadingTemplate;
        [Tooltip("Use this template for the currently-selected active quest.")]
        public StandardUIQuestTitleButtonTemplate selectedActiveQuestHeadingTemplate;
        [Tooltip("Use this template for completed quests.")]
        public StandardUIQuestTitleButtonTemplate completedQuestHeadingTemplate;
        [Tooltip("Use this template for the currently-selected completed quest.")]
        public StandardUIQuestTitleButtonTemplate selectedCompletedQuestHeadingTemplate;
        [Tooltip("If there are no quests to show, show the No Active/Completed Quests Text above.")]
        public bool showNoQuestsText = true;
        [Tooltip("Select first quest in list when open. If unticked and Always Auto Focus is ticked, selects button assigned to main panel's First Selected field (Close button).")]
        public bool selectFirstQuestOnOpen = false;
        [Tooltip("Show details when quest button is selected (highlighted/hovered), not when clicked.")]
        public bool showDetailsOnSelect = false;
        [Tooltip("Keep all groups expanded.")]
        public bool keepGroupsExpanded = false;

        [Header("Details Panel")]

        public RectTransform questDetailsContentContainer;
        public StandardUITextTemplate questHeadingTextTemplate;
        public StandardUITextTemplate questDescriptionTextTemplate;
        public StandardUITextTemplate questEntryActiveTextTemplate;
        public StandardUITextTemplate questEntrySuccessTextTemplate;
        public StandardUITextTemplate questEntryFailureTextTemplate;
        public StandardUIButtonTemplate abandonButtonTemplate;

        [Header("Abandon Quest Panel")]

        public UIPanel abandonQuestPanel;
        public UITextField abandonQuestTitleText;

        [Header("Events")]
        public UnityEvent onOpen = new UnityEvent();
        public UnityEvent onClose = new UnityEvent();

        [Tooltip("Add an EventSystem if one isn't in the scene.")]
        public bool addEventSystemIfNeeded = true;

        #endregion

        #region Runtime Properties

        private StandardUIInstancedContentManager m_selectionPanelContentManager = new StandardUIInstancedContentManager();
        protected StandardUIInstancedContentManager selectionPanelContentManager
        {
            get { return m_selectionPanelContentManager; }
            set { m_selectionPanelContentManager = value; }
        }

        private StandardUIInstancedContentManager m_detailsPanelContentManager = new StandardUIInstancedContentManager();
        protected StandardUIInstancedContentManager detailsPanelContentManager
        {
            get { return m_detailsPanelContentManager; }
            set { m_detailsPanelContentManager = value; }
        }

        protected List<string> expandedGroupNames = new List<string>();
        protected System.Action confirmAbandonQuestHandler = null;
        private Coroutine m_refreshCoroutine = null;
        private bool m_isAwake = false;

        #endregion

        #region Initialization

        public override void Awake()
        {
            m_isAwake = true;
            base.Awake();
            if (addEventSystemIfNeeded) UITools.RequireEventSystem();
            InitializeTemplates();
        }

        protected virtual void InitializeTemplates()
        {
            if (DialogueDebug.logWarnings)
            {
                if (mainPanel == null) Debug.LogWarning("Dialogue System: Main Panel is unassigned.", this);
                if (questSelectionContentContainer == null) Debug.LogWarning("Dialogue System: Quest Selection Content Container is unassigned.", this);
                if (questGroupTemplate == null) Debug.LogWarning("Dialogue System: Quest Group Template is unassigned.", this);
                if (activeQuestHeadingTemplate == null) Debug.LogWarning("Dialogue System: Active Quest Title Template is unassigned.", this);
                if (completedQuestHeadingTemplate == null) Debug.LogWarning("Dialogue System: Completed Quest Title Template is unassigned.", this);
                if (questDetailsContentContainer == null) Debug.LogWarning("Dialogue System: Quest Details Content Container is unassigned.", this);
                if (questHeadingTextTemplate == null) Debug.LogWarning("Dialogue System: Quest Heading Text Template is unassigned.", this);
                if (questDescriptionTextTemplate == null) Debug.LogWarning("Dialogue System: Quest Body Text Template is unassigned.", this);
                if (abandonQuestPanel == null) Debug.LogWarning("Dialogue System: Abandon Quest Panel is unassigned.", this);
                if (abandonQuestTitleText == null) Debug.LogWarning("Dialogue System: Abandon Quest Title Text is unassigned.", this);
            }
            Tools.SetGameObjectActive(questGroupTemplate, false);
            Tools.SetGameObjectActive(activeQuestHeadingTemplate, false);
            Tools.SetGameObjectActive(completedQuestHeadingTemplate, false);
            Tools.SetGameObjectActive(selectedActiveQuestHeadingTemplate, false);
            Tools.SetGameObjectActive(selectedCompletedQuestHeadingTemplate, false);
            Tools.SetGameObjectActive(questHeadingTextTemplate, false);
            Tools.SetGameObjectActive(questDescriptionTextTemplate, false);
            Tools.SetGameObjectActive(questEntryActiveTextTemplate, false);
            Tools.SetGameObjectActive(questEntrySuccessTextTemplate, false);
            Tools.SetGameObjectActive(questEntryFailureTextTemplate, false);
            Tools.SetGameObjectActive(abandonButtonTemplate, false);
        }

        #endregion

        #region Show & Hide

        /// <summary>
        /// Open the window by showing the main panel.
        /// </summary>
        /// <param name="openedWindowHandler">Opened window handler.</param>
        public override void OpenWindow(System.Action openedWindowHandler)
        {
            mainPanel.Open();
            openedWindowHandler();
            onOpen.Invoke();
            if (selectFirstQuestOnOpen && quests.Length > 0)
            {
                RepaintSelectedQuest(quests[0]);
            }
        }

        /// <summary>
        /// Close the window by hiding the main panel. Re-enable the bark UI.
        /// </summary>
        /// <param name="closedWindowHandler">Closed window handler.</param>
        public override void CloseWindow(System.Action closedWindowHandler)
        {
            closedWindowHandler();
            mainPanel.Close();
            onClose.Invoke();
        }

        public virtual void Toggle()
        {
            if (isOpen) Close(); else Open();
        }

        /// <summary>
        /// True if the group is expanded in the UI.
        /// </summary>
        public virtual bool IsGroupExpanded(string groupName)
        {
            return keepGroupsExpanded || expandedGroupNames.Contains(groupName);
        }

        /// <summary>
        /// Toggles whether a group is expanded or not.
        /// </summary>
        /// <param name="groupName">Group to toggle.</param>
        public virtual void ToggleGroup(string groupName)
        {
            if (IsGroupExpanded(groupName))
            {
                expandedGroupNames.Remove(groupName);
            }
            else
            {
                expandedGroupNames.Add(groupName);
            }
        }

        protected void SetStateToggleButtons()
        {
            if (activeQuestsButton != null) activeQuestsButton.interactable = !isShowingActiveQuests;
            if (completedQuestsButton != null) completedQuestsButton.interactable = isShowingActiveQuests;
        }

        public virtual void Repaint()
        {
            if (!isOpen) return;
            if (m_refreshCoroutine == null) m_refreshCoroutine = StartCoroutine(RefreshAtEndOfFrame());
        }

        private IEnumerator RefreshAtEndOfFrame()
        {
            // Wait until end of frame so we only refresh once in case we receive multiple
            // requests to refresh during the same frame.
            yield return CoroutineUtility.endOfFrame;
            m_refreshCoroutine = null;
            OnQuestListUpdated();
        }

        public string foldoutToSelect = null;
        public string questTitleToSelect = null;

        public override void OnQuestListUpdated()
        {
            if (!m_isAwake) return;
            UnityEngine.UI.Selectable elementToSelect = null;
            showingActiveQuestsHeading.SetActive(isShowingActiveQuests);
            showingCompletedQuestHeading.SetActive(!isShowingActiveQuests);
            selectionPanelContentManager.Clear();

            // Get group names, and draw selected quest in its panel while we're at it:
            var groupNames = new List<string>();
            var groupDisplayNames = new Dictionary<string, string>();
            int numGroupless = 0;
            var repaintedQuestDetails = false;
            if (quests.Length > 0)
            {
                foreach (var quest in quests)
                {
                    if (IsSelectedQuest(quest))
                    {
                        RepaintSelectedQuest(quest);
                        repaintedQuestDetails = true;
                    }
                    var groupName = quest.Group;
                    var groupDisplayName = string.IsNullOrEmpty(quest.GroupDisplayName) ? quest.Group : quest.GroupDisplayName;
                    if (string.IsNullOrEmpty(groupName)) numGroupless++;
                    if (string.IsNullOrEmpty(groupName) || groupNames.Contains(groupName)) continue;
                    groupNames.Add(groupName);
                    groupDisplayNames[groupName] = groupDisplayName;
                }
            }
            if (!repaintedQuestDetails) RepaintSelectedQuest(null);

            // Add quests by group:
            foreach (var groupName in groupNames)
            {
                var groupFoldout = selectionPanelContentManager.Instantiate<StandardUIFoldoutTemplate>(questGroupTemplate);
                selectionPanelContentManager.Add(groupFoldout, questSelectionContentContainer);
                groupFoldout.Assign(groupDisplayNames[groupName], IsGroupExpanded(groupName));
                var targetGroupName = groupName;
                var targetGroupFoldout = groupFoldout;
                if (!keepGroupsExpanded)
                {
                    groupFoldout.foldoutButton.onClick.AddListener(() => { OnClickGroup(targetGroupName, targetGroupFoldout); });
                }
                if (string.Equals(foldoutToSelect, groupName))
                {
                    elementToSelect = groupFoldout.foldoutButton;
                    foldoutToSelect = null;
                }
                foreach (var quest in quests)
                {
                    if (string.Equals(quest.Group, groupName))
                    {
                        var template = IsSelectedQuest(quest)
                            ? GetSelectedQuestTitleTemplate(quest)
                            : GetQuestTitleTemplate(quest);
                        var questTitle = selectionPanelContentManager.Instantiate<StandardUIQuestTitleButtonTemplate>(template);
                        questTitle.Assign(quest.Title, quest.Heading.text, OnToggleTracking);
                        selectionPanelContentManager.Add(questTitle, groupFoldout.interiorPanel);
                        var target = quest.Title;
                        questTitle.button.onClick.AddListener(() => { OnClickQuest(target); });
                        if (showDetailsOnSelect) AddShowDetailsOnSelect(questTitle.button, target);
                        if (string.Equals(quest.Title, questTitleToSelect))
                        {
                            elementToSelect = questTitle.button;
                            questTitleToSelect = null;
                        }
                    }
                }
            }

            // Add groupless quests:
            foreach (var quest in quests)
            {
                if (!string.IsNullOrEmpty(quest.Group)) continue;
                var template = IsSelectedQuest(quest)
                    ? GetSelectedQuestTitleTemplate(quest)
                    : GetQuestTitleTemplate(quest);
                var questTitle = selectionPanelContentManager.Instantiate<StandardUIQuestTitleButtonTemplate>(template);
                questTitle.Assign(quest.Title, quest.Heading.text, OnToggleTracking);
                selectionPanelContentManager.Add(questTitle, questSelectionContentContainer);
                var target = quest.Title;
                questTitle.button.onClick.AddListener(() => { OnClickQuest(target); });
                if (showDetailsOnSelect) AddShowDetailsOnSelect(questTitle.button, target);
                if (string.Equals(quest.Title, questTitleToSelect))
                {
                    elementToSelect = questTitle.button;
                    questTitleToSelect = null;
                }
            }

            // If no quests, add no quests text:
            if (quests.Length == 0 && showNoQuestsText)
            {
                var questTitle = selectionPanelContentManager.Instantiate<StandardUIQuestTitleButtonTemplate>(completedQuestHeadingTemplate);
                var dummyText = noQuestsMessage;
                questTitle.Assign(dummyText, dummyText, null);
                selectionPanelContentManager.Add(questTitle, questSelectionContentContainer);
            }

            SetStateToggleButtons();
            mainPanel.RefreshSelectablesList();
            if (mainPanel != null) UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(mainPanel.GetComponent<RectTransform>());
            if (elementToSelect != null)
            {
                StartCoroutine(SelectElement(elementToSelect));
            }
            else if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null && mainPanel != null && mainPanel.firstSelected != null && InputDeviceManager.autoFocus)
            {
                UITools.Select(mainPanel.firstSelected.GetComponent<UnityEngine.UI.Selectable>());
            }
        }

        protected virtual StandardUIQuestTitleButtonTemplate GetQuestTitleTemplate(QuestInfo quest)
        {
            return isShowingActiveQuests
                ? activeQuestHeadingTemplate
                : completedQuestHeadingTemplate;
        }

        protected virtual StandardUIQuestTitleButtonTemplate GetSelectedQuestTitleTemplate(QuestInfo quest)
        {
            return isShowingActiveQuests
                ? (selectedActiveQuestHeadingTemplate ?? activeQuestHeadingTemplate)
                : (selectedCompletedQuestHeadingTemplate ?? completedQuestHeadingTemplate);
        }

        protected IEnumerator SelectElement(UnityEngine.UI.Selectable elementToSelect)
        {
            yield return null;
            UITools.Select(elementToSelect);
        }

        protected virtual void AddShowDetailsOnSelect(UnityEngine.UI.Button button, string target)
        {
            var eventTrigger = button.GetComponent<UnityEngine.EventSystems.EventTrigger>() ?? button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            // On joystick navigation:
            var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
            entry.eventID = UnityEngine.EventSystems.EventTriggerType.Select;
            entry.callback.AddListener((eventData) => { ShowDetailsOnSelect(target); });
            eventTrigger.triggers.Add(entry);

            // On cursor hover:
            entry = new UnityEngine.EventSystems.EventTrigger.Entry();
            entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            entry.callback.AddListener((eventData) => { ShowDetailsOnSelect(target); });
            eventTrigger.triggers.Add(entry);
        }

        protected virtual void OnClickGroup(string groupName, StandardUIFoldoutTemplate groupFoldout)
        {
            ToggleGroup(groupName);
            groupFoldout.ToggleInterior();
        }

        protected virtual void ShowDetailsOnSelect(string questTitle)
        {
            if (!string.Equals(selectedQuest, questTitle)) SelectQuest(questTitle);
        }

        protected virtual void OnClickQuest(string questTitle)
        {
            SelectQuest(questTitle);
        }

        public virtual void SelectQuest(string questTitle)
        {
            questTitleToSelect = questTitle;
            ClickQuest(questTitle);
        }

        protected virtual void RepaintSelectedQuest(QuestInfo quest)
        {
            detailsPanelContentManager.Clear();
            if (quest != null)
            {
                // Title:
                var titleInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(questHeadingTextTemplate);
                titleInstance.Assign(quest.Heading.text);
                detailsPanelContentManager.Add(titleInstance, questDetailsContentContainer);

                // Description:
                var descriptionInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(questDescriptionTextTemplate);
                descriptionInstance.Assign(quest.Description.text);
                detailsPanelContentManager.Add(descriptionInstance, questDetailsContentContainer);

                // Entries:
                for (int i = 0; i < quest.Entries.Length; i++)
                {
                    var entryTemplate = GetEntryTemplate(quest.EntryStates[i]);
                    if (entryTemplate != null)
                    {
                        var entryInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(entryTemplate);
                        entryInstance.Assign(quest.Entries[i].text);
                        detailsPanelContentManager.Add(entryInstance, questDetailsContentContainer);
                    }
                }

                // Abandon button:
                if (currentQuestStateMask == QuestState.Active && QuestLog.IsQuestAbandonable(quest.Title))
                {
                    var abandonButtonInstance = detailsPanelContentManager.Instantiate<StandardUIButtonTemplate>(abandonButtonTemplate);
                    detailsPanelContentManager.Add(abandonButtonInstance, questDetailsContentContainer);
                    abandonButtonInstance.button.onClick.AddListener(ClickAbandonQuestButton);
                }
            }
        }

        protected virtual StandardUITextTemplate GetEntryTemplate(QuestState state)
        {
            switch (state)
            {
                case QuestState.Active:
                    return questEntryActiveTextTemplate;
                case QuestState.Success:
                    return (questEntrySuccessTextTemplate != null) ? questEntrySuccessTextTemplate : questEntryActiveTextTemplate;
                case QuestState.Failure:
                    return (questEntryFailureTextTemplate != null) ? questEntryFailureTextTemplate : questEntryActiveTextTemplate;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Toggles quest tracking.
        /// </summary>
        /// <param name="value">Tracking on or off.</param>
        /// <param name="data">Quest name (string).</param>
        public virtual void OnToggleTracking(bool value, object data)
        {
            var quest = (string)data;
            if (string.IsNullOrEmpty(quest)) return;
            var previousSelected = selectedQuest;
            selectedQuest = quest;
            ClickTrackQuest(quest);
            selectedQuest = previousSelected;
        }

        /// <summary>
        /// Opens the abandon confirmation popup.
        /// </summary>
        /// <param name="title">Quest title.</param>
        /// <param name="confirmAbandonQuestHandler">Confirm abandon quest handler.</param>
        public override void ConfirmAbandonQuest(string title, System.Action confirmAbandonQuestHandler)
        {
            if (abandonQuestPanel == null || selectedQuest == null) return;
            this.confirmAbandonQuestHandler = confirmAbandonQuestHandler;
            abandonQuestTitleText.text = QuestLog.GetQuestTitle(selectedQuest);
            abandonQuestPanel.Open();
        }

        public virtual void AbandonQuestConfirmed()
        {
            OnConfirmAbandonQuest();
            detailsPanelContentManager.Clear();
        }

        protected override void ShowQuests(QuestState questStateMask)
        {
            if (questStateMask != currentQuestStateMask) detailsPanelContentManager.Clear();
            base.ShowQuests(questStateMask);
        }

        #endregion

    }

}
