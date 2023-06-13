// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This is an implementation of the abstract QuestLogWindow class for the Unity UI.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class UnityUIQuestLogWindow : QuestLogWindow
    {

        /// <summary>
        /// The main quest log window panel.
        /// </summary>
        public UnityEngine.UI.Graphic mainPanel;

        /// <summary>
        /// The active quests button.
        /// </summary>
        public UnityEngine.UI.Button activeQuestsButton;

        /// <summary>
        /// The completed quests button.
        /// </summary>
        public UnityEngine.UI.Button completedQuestsButton;

        /// <summary>
        /// The quest table.
        /// </summary>
        public UnityEngine.UI.Graphic questTable;

        public UnityUIQuestGroupTemplate questGroupTemplate;

        /// <summary>
        /// The quest template.
        /// </summary>
        public UnityUIQuestTemplate questTemplate;

        /// <summary>
        /// The confirmation popup to use if the player clicks the abandon quest button.
        /// It should send ClickConfirmAbandonQuest if the player confirms, or
        /// ClickCancelAbandonQuest if the player cancels.
        /// </summary>
        public UnityEngine.UI.Graphic abandonPopup;

        /// <summary>
        /// The quest title label to set in the abandon quest dialog popup.
        /// </summary>
        public UnityEngine.UI.Text abandonQuestTitle;

        [Serializable]
        public class AnimationTransitions
        {
            public string showTrigger = "Show";
            public string hideTrigger = "Hide";

            [Tooltip("Specifies whether Show Trigger and Hide Trigger are animator states or trigger parameters.")]
            public UIShowHideController.TransitionMode transitionMode = UIShowHideController.TransitionMode.State;

            public bool debug = false;

        }

        public AnimationTransitions animationTransitions = new AnimationTransitions();

        /// <summary>
        /// Set <c>true</c> to always keep a control focused; useful for gamepads.
        /// </summary>
        [Tooltip("Always keep a control focused; useful for gamepads and keyboard.")]
        public bool autoFocus = false;

        /// <summary>
        /// If auto focusing, check on this frequency in seconds that the control is focused.
        /// </summary>
        [Tooltip("If auto focusing, check on this frequency in seconds that the control is focused.")]
        public float autoFocusCheckFrequency = 0.5f;

        public UnityEvent onOpen = new UnityEvent();
        public UnityEvent onClose = new UnityEvent();
        public UnityEvent onContentChanged = new UnityEvent();

        [Tooltip("Add an EventSystem if one isn't in the scene.")]
        public bool addEventSystemIfNeeded = true;

        /// <summary>
        /// This handler is called if the player confirms abandonment of a quest.
        /// </summary>
        protected System.Action confirmAbandonQuestHandler = null;

        private UIShowHideController m_showHideController = null;
        protected UIShowHideController showHideController
        {
            get
            {
                if (m_showHideController == null) m_showHideController = new UIShowHideController(this.gameObject, mainPanel, animationTransitions.transitionMode, animationTransitions.debug);
                return m_showHideController;
            }
        }

        protected List<string> collapsedGroups = new List<string>();

        protected List<UnityUIQuestGroupTemplate> groupTemplateInstances = new List<UnityUIQuestGroupTemplate>();

        protected List<UnityUIQuestTemplate> questTemplateInstances = new List<UnityUIQuestTemplate>();

        protected List<UnityUIQuestGroupTemplate> unusedGroupTemplateInstances = new List<UnityUIQuestGroupTemplate>();

        protected List<UnityUIQuestTemplate> unusedQuestTemplateInstances = new List<UnityUIQuestTemplate>();

        protected int siblingIndexCounter = 0;

        private float nextAutoFocusCheckTime = 0;

        public override void Awake()
        {
            base.Awake();
            Tools.DeprecationWarning(this);
        }

        /// <summary>
        /// Hide the main panel and all of the templates on start.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            if (addEventSystemIfNeeded) UITools.RequireEventSystem();
            Tools.SetGameObjectActive(mainPanel, false);
            Tools.SetGameObjectActive(abandonPopup, false);
            Tools.SetGameObjectActive(questGroupTemplate, false);
            Tools.SetGameObjectActive(questTemplate, false);
            showHideController.state = UIShowHideController.State.Hidden;
            SetStateButtonListeners();
            SetStateToggleButtons();
            if (DialogueDebug.logWarnings)
            {
                if (mainPanel == null) Debug.LogWarning(string.Format("{0}: {1} Main Panel is unassigned", new object[] { DialogueDebug.Prefix, name }));
                if (questTable == null) Debug.LogWarning(string.Format("{0}: {1} Quest Table is unassigned", new object[] { DialogueDebug.Prefix, name }));
                if (useGroups && ((questTemplate == null || !questTemplate.ArePropertiesAssigned)))
                {
                    Debug.LogWarning(string.Format("{0}: {1} Quest Group Template or one of its properties is unassigned", new object[] { DialogueDebug.Prefix, name }));
                }
                if (questTemplate == null || !questTemplate.ArePropertiesAssigned)
                {
                    Debug.LogWarning(string.Format("{0}: {1} Quest Template or one of its properties is unassigned", new object[] { DialogueDebug.Prefix, name }));
                }
            }
        }

        public virtual void Update()
        {
            if (autoFocus && isOpen &&
                UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null && 
                autoFocusCheckFrequency > 0.001f && Time.realtimeSinceStartup > nextAutoFocusCheckTime)
            {
                nextAutoFocusCheckTime = Time.realtimeSinceStartup + autoFocusCheckFrequency;
                AutoFocus();
            }
        }

        /// <summary>
        /// Open the window by showing the main panel. The bark UI may conflict with the quest
        /// log window, so temporarily disable it.
        /// </summary>
        /// <param name="openedWindowHandler">Opened window handler.</param>
        public override void OpenWindow(System.Action openedWindowHandler)
        {
            showHideController.Show(animationTransitions.showTrigger, pauseWhileOpen, openedWindowHandler, false);
            isOpen = true;
            AutoFocus();
            onOpen.Invoke();
        }

        public void AutoFocus()
        {
            GameObject go = completedQuestsButton.gameObject.activeSelf ? completedQuestsButton.gameObject : activeQuestsButton.gameObject;
            UITools.Select(go.GetComponent<UnityEngine.UI.Selectable>());
        }

        /// <summary>
        /// Close the window by hiding the main panel. Re-enable the bark UI.
        /// </summary>
        /// <param name="closedWindowHandler">Closed window handler.</param>
        public override void CloseWindow(System.Action closedWindowHandler)
        {
            ResumeGameplay();
            showHideController.Hide(animationTransitions.hideTrigger, closedWindowHandler);
            isOpen = false;
            onClose.Invoke();
        }

        /// <summary>
        /// Whenever the quest list is updated, repopulate the scroll panel.
        /// </summary>
        public override void OnQuestListUpdated()
        {
            // Move instances to the unused lists:
            unusedGroupTemplateInstances.AddRange(groupTemplateInstances);
            unusedQuestTemplateInstances.AddRange(questTemplateInstances);
            groupTemplateInstances.Clear();
            questTemplateInstances.Clear();
            siblingIndexCounter = 0;

            // Add content, drawing from unused list when possible:
            if (quests.Length == 0)
            {
                AddQuestToTable(new QuestInfo(string.Empty, new FormattedText(noQuestsMessage), FormattedText.empty, new FormattedText[0], new QuestState[0], false, false, false));
            }
            else
            {
                AddQuestsToTable();
            }

            // Destroy all unused instances:
            for (int i = 0; i < unusedGroupTemplateInstances.Count; i++)
            {
                Destroy(unusedGroupTemplateInstances[i].gameObject);
            }
            unusedGroupTemplateInstances.Clear();
            for (int i = 0; i < unusedQuestTemplateInstances.Count; i++)
            {
                Destroy(unusedQuestTemplateInstances[i].gameObject);
            }
            unusedQuestTemplateInstances.Clear();
            
            SetStateToggleButtons();

            if (mainPanel != null) UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(mainPanel.rectTransform);
        }

        protected void SetStateButtonListeners()
        {
            if (activeQuestsButton != null)
            {
                activeQuestsButton.onClick.RemoveListener(() => ClickShowActiveQuestsButton());
                activeQuestsButton.onClick.AddListener(() => ClickShowActiveQuestsButton());
            }
            if (completedQuestsButton != null)
            {
                completedQuestsButton.onClick.RemoveListener(() => ClickShowCompletedQuestsButton());
                completedQuestsButton.onClick.AddListener(() => ClickShowCompletedQuestsButton());
            }
        }

        protected void SetStateToggleButtons()
        {
            if (activeQuestsButton != null) activeQuestsButton.interactable = !isShowingActiveQuests;
            if (completedQuestsButton != null) completedQuestsButton.interactable = isShowingActiveQuests;
        }

        protected virtual void ClearQuestTable()
        {
            if (questTable == null) return;
            foreach (Transform child in questTable.transform)
            {
                if (child.gameObject.activeSelf)
                {
                    Destroy(child.gameObject);
                }
            }
            NotifyContentChanged();
        }

        protected virtual void AddQuestsToTable()
        {
            if (questTable == null) return;
            string currentGroup = null;
            var isCurrentGroupCollapsed = false;
            for (int i = 0; i < quests.Length; i++)
            {
                if (!string.Equals(quests[i].Group, currentGroup))
                {
                    currentGroup = quests[i].Group;
                    AddQuestGroupToTable(currentGroup);
                    isCurrentGroupCollapsed = collapsedGroups.Contains(currentGroup);
                }
                if (!isCurrentGroupCollapsed)
                {
                    AddQuestToTable(quests[i]);
                }
            }
            NotifyContentChanged();
        }

        /// <summary>
        /// Adds a quest group to the table using the template.
        /// </summary>
        /// <param name="questInfo">Quest group name.</param>
        protected virtual void AddQuestGroupToTable(string group)
        {
            if (string.IsNullOrEmpty(group) || questGroupTemplate == null || !questGroupTemplate.ArePropertiesAssigned) return;

            // Try to use existing instance from unused list first:
            var existingChild = unusedGroupTemplateInstances.Find(x => string.Equals(x.heading.text, group));
            if (existingChild != null)
            {
                unusedGroupTemplateInstances.Remove(existingChild);
                groupTemplateInstances.Add(existingChild);
                existingChild.transform.SetSiblingIndex(siblingIndexCounter++);
                return;
            }

            // Otherwise create a new one:
            GameObject questGroupGameObject = Instantiate(questGroupTemplate.gameObject) as GameObject;
            if (questGroupGameObject == null)
            {
                Debug.LogError(string.Format("{0}: {1} couldn't instantiate quest group template", new object[] { DialogueDebug.Prefix, name }));
                return;
            }
            questGroupGameObject.name = group;
            questGroupGameObject.transform.SetParent(questTable.transform, false);
            questGroupGameObject.SetActive(true);
            UnityUIQuestGroupTemplate template = questGroupGameObject.GetComponent<UnityUIQuestGroupTemplate>();
            if (template == null) return;
            groupTemplateInstances.Add(template);
            template.transform.SetSiblingIndex(siblingIndexCounter++);
            template.Initialize();
            template.heading.text = group;

            // Set up the collapse/expand button:
            var button = questGroupGameObject.GetComponentInChildren<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => ClickQuestGroupFoldout(group));
            }
        }

        /// <summary>
        /// Adds a quest to the table using the template.
        /// </summary>
        /// <param name="questInfo">Quest info.</param>
        protected virtual void AddQuestToTable(QuestInfo questInfo)
        {
            if ((questTable == null) || (questTemplate == null) || !questTemplate.ArePropertiesAssigned) return;

            // Try to use existing instance from unused list first:
            var existingChild = unusedQuestTemplateInstances.Find(x => x.heading.GetComponentInChildren<UnityEngine.UI.Text>() != null && string.Equals(x.heading.GetComponentInChildren<UnityEngine.UI.Text>().text, questInfo.Heading.text));
            if (existingChild != null)
            {
                unusedQuestTemplateInstances.Remove(existingChild);
                questTemplateInstances.Add(existingChild);
                existingChild.transform.SetSiblingIndex(siblingIndexCounter++);
                existingChild.description.text = questInfo.Description.text;
                existingChild.ClearQuestDetails();
                SetQuestDetails(existingChild, questInfo);
                return;
            }

            // Otherwise create a new one:
            // Instantiate a copy of the template:
            GameObject questGameObject = Instantiate(questTemplate.gameObject) as GameObject;
            if (questGameObject == null)
            {
                Debug.LogError(string.Format("{0}: {1} couldn't instantiate quest template", new object[] { DialogueDebug.Prefix, name }));
                return;
            }
            questGameObject.name = questInfo.Heading.text;
            questGameObject.transform.SetParent(questTable.transform, false);
            questGameObject.transform.SetSiblingIndex(siblingIndexCounter++);
            questGameObject.SetActive(true);
            UnityUIQuestTemplate questControl = questGameObject.GetComponent<UnityUIQuestTemplate>();
            if (questControl == null) return;
            questTemplateInstances.Add(questControl);
            questControl.Initialize();

            // Set the heading button:
            var questHeadingButton = questControl.heading;
            var heading = questHeadingButton.GetComponentInChildren<UnityEngine.UI.Text>();
            if (heading != null) heading.text = questInfo.Heading.text;
            questHeadingButton.onClick.AddListener(() => ClickQuestFoldout(questInfo.Title));

            SetQuestDetails(questControl, questInfo);
        }

        protected virtual void SetQuestDetails(UnityUIQuestTemplate questControl, QuestInfo questInfo)
        { 
            // Set details if this is the selected quest:
            if (IsSelectedQuest(questInfo))
            {

                // Set the description:
                if (questHeadingSource == QuestHeadingSource.Name)
                {
                    questControl.description.text = questInfo.Description.text;
                    Tools.SetGameObjectActive(questControl.description, true);
                }

                // Set the entry description:
                if (questInfo.EntryStates.Length > 0)
                {
                    for (int i = 0; i < questInfo.Entries.Length; i++)
                    {
                        questControl.AddEntryDescription(questInfo.Entries[i].text, questInfo.EntryStates[i]);
                    }
                }

                UnityUIQuestTitle unityUIQuestTitle = null;

                // Set the track button:
                if (questControl.trackButton != null)
                {
                    unityUIQuestTitle = questControl.trackButton.gameObject.AddComponent<UnityUIQuestTitle>();
                    unityUIQuestTitle.questTitle = questInfo.Title;
                    questControl.trackButton.onClick.RemoveAllListeners();
                    questControl.trackButton.onClick.AddListener(() => ClickTrackQuestButton());
                    Tools.SetGameObjectActive(questControl.trackButton, questInfo.Trackable);
                }

                // Set the abandon button:
                if (questControl.abandonButton != null)
                {
                    unityUIQuestTitle = questControl.abandonButton.gameObject.AddComponent<UnityUIQuestTitle>();
                    unityUIQuestTitle.questTitle = questInfo.Title;
                    questControl.abandonButton.onClick.RemoveAllListeners();
                    questControl.abandonButton.onClick.AddListener(() => ClickAbandonQuestButton());
                    Tools.SetGameObjectActive(questControl.abandonButton, questInfo.Abandonable);
                }
            }
            else
            {
                Tools.SetGameObjectActive(questControl.description, false);
                Tools.SetGameObjectActive(questControl.entryDescription, false);
                Tools.SetGameObjectActive(questControl.trackButton, false);
                Tools.SetGameObjectActive(questControl.abandonButton, false);
            }
        }

        public void NotifyContentChanged()
        {
            onContentChanged.Invoke();
        }

        public void ClickQuestFoldout(string questTitle)
        {
            ClickQuest(questTitle);
        }

        public void ClickQuestGroupFoldout(string group)
        {
            if (collapsedGroups.Contains(group))
            {
                collapsedGroups.Remove(group);
            }
            else
            {
                collapsedGroups.Add(group);
            }
            OnQuestListUpdated();
        }

        /// <summary>
        /// Track button clicked event that sets SelectedQuest first.
        /// </summary>
        /// <param name="button">Button.</param>
        protected void OnTrackButtonClicked(GameObject button)
        {
            selectedQuest = button.GetComponent<UnityUIQuestTitle>().questTitle;
            ClickTrackQuest(selectedQuest);
        }

        /// <summary>
        /// Abandon button clicked event that sets SelectedQuest first.
        /// </summary>
        /// <param name="button">Button.</param>
        protected void OnAbandonButtonClicked(GameObject button)
        {
            selectedQuest = button.GetComponent<UnityUIQuestTitle>().questTitle;
            ClickAbandonQuest(selectedQuest);
        }

        /// <summary>
        /// Opens the abandon confirmation popup.
        /// </summary>
        /// <param name="title">Quest title.</param>
        /// <param name="confirmAbandonQuestHandler">Confirm abandon quest handler.</param>
        public override void ConfirmAbandonQuest(string title, System.Action confirmAbandonQuestHandler)
        {
            this.confirmAbandonQuestHandler = confirmAbandonQuestHandler;
            OpenAbandonPopup(title);
        }

        /// <summary>
        /// Opens the abandon popup modally if assigned; otherwise immediately confirms.
        /// </summary>
        /// <param name="title">Quest title.</param>
        protected void OpenAbandonPopup(string title)
        {
            if (abandonPopup != null)
            {
                Tools.SetGameObjectActive(abandonPopup, true);
                if (abandonQuestTitle != null) abandonQuestTitle.text = QuestLog.GetQuestTitle(title);
                if (autoFocus && (UnityEngine.EventSystems.EventSystem.current != null))
                {
                    var button = abandonPopup.GetComponentInChildren<UnityEngine.UI.Button>();
                    if (button != null)
                    {
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(button.gameObject);
                    }
                }
                else
                {
                    this.confirmAbandonQuestHandler();
                }
            }
        }

        /// <summary>
        /// Closes the abandon popup.
        /// </summary>
        protected void CloseAbandonPopup()
        {
            Tools.SetGameObjectActive(abandonPopup, false);
        }

        public void ClickConfirmAbandonQuestButton()
        {
            CloseAbandonPopup();
            confirmAbandonQuestHandler();
        }

        public void ClickCancelAbandonQuestButton()
        {
            CloseAbandonPopup();
        }

    }

}
