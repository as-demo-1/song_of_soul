// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// When you attach this script to the Dialogue Manager object (or a child),
    /// it will display tracked quests using the new Unity UI. It updates when the player
    /// toggles tracking in the quest log window and at the end of conversations. If you 
    /// change the state of a quest elsewhere, you must manually call UpdateTracker().
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class StandardUIQuestTracker : MonoBehaviour
    {

        [Tooltip("Record the quest tracker display toggle in this PlayerPrefs key.")]
        public string playerPrefsToggleKey = "QuestTracker";

        /// <summary>
        /// The UI control that will hold quest track info (instantiated copies of the 
        /// quest track template). This is typically a Vertical Layout Group.
        /// </summary>
        [Tooltip("UI container that will hold instances of quest track template.")]
        public Transform container;

        /// <summary>
        /// Tick to show the container even if there's nothing to track.
        /// </summary>
        [Tooltip("Show Container even if there's nothing to track.")]
        public bool showContainerIfEmpty = true;

        /// <summary>
        /// The quest track template.
        /// </summary>
        [Tooltip("Template to instantiate for each tracked quest.")]
        public StandardUIQuestTrackTemplate questTrackTemplate;

        /// <summary>
        /// Tick to show active quests in the tracker.
        /// </summary>
        [Tooltip("Show active quests.")]
        public bool showActiveQuests = true;

        /// <summary>
        /// Tick to show successful and failed quests in the tracker.
        /// </summary>
        [Tooltip("Show successful and failed quests.")]
        public bool showCompletedQuests = false;

        /// <summary>
        /// Tick to look up "Entry n Success" or "Entry n Failure" if the
        /// quest entry is in the success or failure state.
        /// </summary>
        [Tooltip("Show Entry n Success or Entry n Failure text if quest entry is in success/failure state.")]
        public bool showCompletedEntryText = false;

        public enum QuestDescriptionSource { Title, Description }

        [Tooltip("Source for the quest tracker text.")]
        public QuestDescriptionSource questDescriptionSource = QuestDescriptionSource.Title;

        public bool visibleOnStart = true;

        protected List<StandardUIQuestTrackTemplate> instantiatedItems = new List<StandardUIQuestTrackTemplate>();

        protected List<StandardUIQuestTrackTemplate> unusedInstances = new List<StandardUIQuestTrackTemplate>();

        protected int siblingIndexCounter = 0;

        protected bool m_started = false;

        protected bool isVisible = true;

        protected Coroutine refreshCoroutine = null;

        /// <summary>
        /// Wait 0.5s to update the tracker in case other start
        /// methods change the state of quests.
        /// </summary>
        public virtual void Start()
        {
            m_started = true;
            RegisterForUpdateTrackerEvents();
            isVisible = PlayerPrefs.GetInt(playerPrefsToggleKey, visibleOnStart ? 1 : 0) == 1;
            if (container == null) Debug.LogWarning(string.Format("{0}: {1} Container is unassigned", new object[] { DialogueDebug.Prefix, name }));
            if (questTrackTemplate == null)
            {
                Debug.LogWarning(string.Format("{0}: {1} Quest Track Template is unassigned", new object[] { DialogueDebug.Prefix, name }));
            }
            else
            {
                questTrackTemplate.gameObject.SetActive(false);
            }
            if (isVisible)
            {
                Invoke("UpdateTracker", 0.5f);
            }
            else
            {
                HideTracker();
            }
        }

        protected virtual void OnEnable()
        {
            if (m_started) RegisterForUpdateTrackerEvents();
        }

        protected virtual void OnDisable()
        {
            refreshCoroutine = null;
            UnregisterFromUpdateTrackerEvents();
        }

        protected void RegisterForUpdateTrackerEvents()
        {
            if (!m_started || DialogueManager.instance == null) return;
            if (GetComponentInParent<DialogueSystemController>() != null) return; // Children of Dialogue Manager automatically receive UpdateTracker; no need to register.
            DialogueManager.instance.receivedUpdateTracker -= UpdateTracker;
            DialogueManager.instance.receivedUpdateTracker += UpdateTracker;
        }

        protected void UnregisterFromUpdateTrackerEvents()
        {
            if (!m_started || DialogueManager.instance == null) return;
            DialogueManager.instance.receivedUpdateTracker -= UpdateTracker;
        }

        /// <summary>
        /// Shows the quest tracker HUD.
        /// </summary>
        public virtual void ShowTracker()
        {
            isVisible = true;
            PlayerPrefs.SetInt(playerPrefsToggleKey, 1);
            if (container != null) container.gameObject.SetActive(true);
            UpdateTracker();
        }

        /// <summary>
        /// Hides the quest tracker HUD entirely.
        /// </summary>
        public virtual void HideTracker()
        {
            isVisible = false;
            PlayerPrefs.SetInt(playerPrefsToggleKey, 0);
            if (container != null) container.gameObject.SetActive(false);
        }

        /// <summary>
        /// Toggles the quest tracker HUD visibility.
        /// </summary>
        public virtual void ToggleTracker()
        {
            if (isVisible) HideTracker(); else ShowTracker();
        }

        /// <summary>
        /// The quest log window sends this message when the player toggles tracking.
        /// </summary>
        /// <param name="quest">Quest.</param>
        public virtual void OnQuestTrackingEnabled(string quest)
        {
            UpdateTracker();
        }

        /// <summary>
        /// The quest log window sends this message when the player toggles tracking.
        /// </summary>
        /// <param name="quest">Quest.</param>
        public virtual void OnQuestTrackingDisabled(string quest)
        {
            UpdateTracker();
        }

        /// <summary>
        /// Quests are often completed in conversations. This handles changes in quest states
        /// after conversations.
        /// </summary>
        /// <param name="actor">Actor.</param>
        public void OnConversationEnd(Transform actor)
        {
            UpdateTracker();
        }

        public virtual void UpdateTracker()
        {
            if (!isVisible) return;
            if (refreshCoroutine == null)
            {
                refreshCoroutine = StartCoroutine(RefreshAtEndOfFrame());
            }
        }
        
        protected virtual IEnumerator RefreshAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();

            // Move instances to the unused list:
            unusedInstances.AddRange(instantiatedItems);
            instantiatedItems.Clear();
            siblingIndexCounter = 0;

            // Add quests, drawing from unused list when possible:
            int numTracked = 0;
            QuestState flags = (showActiveQuests ? (QuestState.Active | QuestState.ReturnToNPC) : 0) |
                (showCompletedQuests ? (QuestState.Success | QuestState.Failure) : 0);
            foreach (string quest in QuestLog.GetAllQuests(flags))
            {
                if (QuestLog.IsQuestTrackingEnabled(quest))
                {
                    AddQuestTrack(quest);
                    numTracked++;
                }
            }
            if (container != null)
            {
                container.gameObject.SetActive(showContainerIfEmpty || numTracked > 0);
            }

            // Destroy remaining unused instances:
            for (int i = 0; i < unusedInstances.Count; i++)
            {
                Destroy(unusedInstances[i].gameObject);
            }
            unusedInstances.Clear();
            refreshCoroutine = null;
        }

        protected virtual void AddQuestTrack(string quest)
        {
            if (container == null || questTrackTemplate == null) return;

            var heading = GetQuestHeading(quest);

            GameObject go;
            if (unusedInstances.Count > 0)
            {
                // Try to use an unused instance:
                go = unusedInstances[0].gameObject;
                unusedInstances.RemoveAt(0);
            }
            else
            {
                // Otherwise instantiate one:
                go = Instantiate(questTrackTemplate.gameObject) as GameObject;
                if (go == null)
                {
                    Debug.LogError(string.Format("{0}: {1} couldn't instantiate quest track template", new object[] { DialogueDebug.Prefix, name }));
                    return;
                }
            }

            go.name = heading;
            go.transform.SetParent(container.transform, false);
            go.SetActive(true);
            var questTrack = go.GetComponent<StandardUIQuestTrackTemplate>();
            instantiatedItems.Add(questTrack);
            if (questTrack != null)
            {
                SetupQuestTrackInstance(questTrack, quest, heading);
                questTrack.transform.SetSiblingIndex(siblingIndexCounter++);
            }
        }

        protected virtual string GetQuestHeading(string quest)
        {
            var questDescription = (questDescriptionSource == QuestDescriptionSource.Title)
                ? QuestLog.GetQuestTitle(quest)
                : QuestLog.GetQuestDescription(quest);
            return FormattedText.Parse(questDescription, DialogueManager.masterDatabase.emphasisSettings).text;
        }

        protected virtual void SetupQuestTrackInstance(StandardUIQuestTrackTemplate questTrack, string quest, string heading)
        {
            if (questTrack == null) return;
            questTrack.Initialize();
            var questState = QuestLog.GetQuestState(quest);
            questTrack.SetDescription(heading, questState);
            int entryCount = QuestLog.GetQuestEntryCount(quest);
            for (int i = 1; i <= entryCount; i++)
            {
                var entryState = QuestLog.GetQuestEntryState(quest, i);
                var entryText = FormattedText.Parse(GetQuestEntryText(quest, i, entryState), DialogueManager.masterDatabase.emphasisSettings).text;
                if (!string.IsNullOrEmpty(entryText))
                {
                    questTrack.AddEntryDescription(entryText, entryState);
                }
            }
        }

        protected virtual string GetQuestEntryText(string quest, int entryNum, QuestState entryState)
        {
            if (entryState == QuestState.Unassigned || entryState == QuestState.Abandoned)
            {
                return string.Empty;
            }
            else if ((entryState == QuestState.Success || entryState == QuestState.Failure) && !showCompletedEntryText)
            {
                return string.Empty;
            }
            else if (entryState == QuestState.Success)
            {
                var text = DialogueLua.GetQuestField(quest, "Entry " + entryNum + " Success").asString;
                if (!string.IsNullOrEmpty(text)) return text;
            }
            else if (entryState == QuestState.Failure)
            {
                var text = DialogueLua.GetQuestField(quest, "Entry " + entryNum + " Failure").asString;
                if (!string.IsNullOrEmpty(text)) return text;
            }
            return QuestLog.GetQuestEntry(quest, entryNum);
        }

    }

}
