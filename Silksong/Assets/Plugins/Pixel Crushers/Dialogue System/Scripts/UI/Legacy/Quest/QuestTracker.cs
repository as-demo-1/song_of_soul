using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem.UnityGUI;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// When you attach this script to the Dialogue Manager object (or a child),
    /// it will display tracked quests. It updates when the player toggles tracking
    /// in the quest log window and at the end of conversations. If you change the
    /// state of a quest elsewhere, you must manually call UpdateTracker().
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class QuestTracker : MonoBehaviour
    {

        [Tooltip("Record the quest tracker display toggle in this PlayerPrefs key.")]
        public string playerPrefsToggleKey = "QuestTracker";

        /// <summary>
        /// The screen rect that will contain the tracker.
        /// </summary>
        public ScaledRect rect = new ScaledRect(ScaledRectAlignment.TopRight, ScaledRectAlignment.TopRight,
                                                ScaledValue.FromPixelValue(0), ScaledValue.FromPixelValue(0),
                                                ScaledValue.FromNormalizedValue(0.25f), ScaledValue.FromNormalizedValue(1f),
                                                64f, 32f);

        /// <summary>
        /// The GUI skin to use for the tracker.
        /// </summary>
        public GUISkin guiSkin;

        /// <summary>
        /// The GUI style to use for active quest titles.
        /// </summary>
        public string TitleStyle;

        /// <summary>
        /// The GUI style to use for successful quest titles.
        /// </summary>
        public string SuccessTitleStyle;

        /// <summary>
        /// The GUI style to use for failed quest titles.
        /// </summary>
        public string FailureTitleStyle;

        /// <summary>
        /// The GUI style to use for active quest entries.
        /// </summary>
        public string ActiveEntryStyle;

        /// <summary>
        /// The GUI style to use for successful quest entries.
        /// </summary>
        public string SuccessEntryStyle;

        /// <summary>
        /// The GUI style to use for failed quest entries.
        /// </summary>
        public string FailureEntryStyle;

        /// <summary>
        /// Tick to show active quests in the tracker.
        /// </summary>
        public bool showActiveQuests = true;

        /// <summary>
        /// Tick to show successful and failed quests in the tracker.
        /// </summary>
        public bool showCompletedQuests = false;

        /// <summary>
        /// Tick to look up "Entry n Success" or "Entry n Failure" if the
        /// quest entry is in the success or failure state.
        /// </summary>
        public bool showCompletedEntryText = false;

        public enum QuestDescriptionSource { Title, Description }

        public QuestDescriptionSource questDescriptionSource = QuestDescriptionSource.Title;

        private class QuestTrackerLine
        {
            public string guiStyleName;
            public GUIStyle guiStyle;
            public string text;
        }

        private Rect screenRect;

        private List<QuestTrackerLine> lines = new List<QuestTrackerLine>();

        private bool isVisible = true;

        /// <summary>
        /// Wait one frame after starting to update the tracker in case other start
        /// methods change the state of quests.
        /// </summary>
        public void Start()
        {
            isVisible = PlayerPrefs.GetInt(playerPrefsToggleKey, 1) == 1;
            StartCoroutine(UpdateTrackerAfterOneFrame());
        }

        private IEnumerator UpdateTrackerAfterOneFrame()
        {
            yield return null;
            UpdateTracker();
        }

        public void ShowTracker()
        {
            isVisible = true;
            PlayerPrefs.SetInt(playerPrefsToggleKey, 1);
        }

        public void HideTracker()
        {
            isVisible = false;
            PlayerPrefs.SetInt(playerPrefsToggleKey, 0);
        }

        public void ToggleTracker()
        {
            if (isVisible) HideTracker(); else ShowTracker();
        }

        /// <summary>
        /// The quest log window sends this message when the player toggles tracking.
        /// </summary>
        /// <param name="quest">Quest.</param>
        public void OnQuestTrackingEnabled(string quest)
        {
            UpdateTracker();
        }

        /// <summary>
        /// The quest log window sends this message when the player toggles tracking.
        /// </summary>
        /// <param name="quest">Quest.</param>
        public void OnQuestTrackingDisabled(string quest)
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

        public void UpdateTracker()
        {
            screenRect = rect.GetPixelRect();
            lines.Clear();
            QuestState flags = (showActiveQuests ? QuestState.Active : 0) |
                (showCompletedQuests ? QuestState.Success | QuestState.Failure : 0);
            foreach (string quest in QuestLog.GetAllQuests(flags))
            {
                if (QuestLog.IsQuestTrackingEnabled(quest))
                {
                    AddQuestTitle(quest);
                    AddQuestEntries(quest);
                }
            }
        }

        private void AddQuestTitle(string quest)
        {
            QuestTrackerLine line = new QuestTrackerLine();
            var questDescription = (questDescriptionSource == QuestDescriptionSource.Title)
                ? QuestLog.GetQuestTitle(quest)
                    : QuestLog.GetQuestDescription(quest);
            line.text = FormattedText.Parse(questDescription, DialogueManager.masterDatabase.emphasisSettings).text;
            line.guiStyleName = GetTitleStyleName(QuestLog.GetQuestState(quest));
            line.guiStyle = null;
            lines.Add(line);
        }

        private void AddQuestEntries(string quest)
        {
            int entryCount = QuestLog.GetQuestEntryCount(quest);
            for (int i = 1; i <= entryCount; i++)
            {
                QuestState entryState = QuestLog.GetQuestEntryState(quest, i);
                if (entryState == QuestState.Unassigned) continue;
                if ((entryState == QuestState.Success || entryState == QuestState.Failure) && !showCompletedEntryText) continue;
                QuestTrackerLine line = new QuestTrackerLine();
                var entryText = GetQuestEntryText(quest, i, entryState);
                line.text = FormattedText.Parse(entryText, DialogueManager.masterDatabase.emphasisSettings).text;
                line.guiStyleName = GetEntryStyleName(entryState);
                line.guiStyle = null;
                lines.Add(line);
            }
        }

        private string GetQuestEntryText(string quest, int entryNum, QuestState entryState)
        {
            if (entryState == QuestState.Unassigned || entryState == QuestState.Abandoned)
            {
                return string.Empty;
            }
            else if (entryState == QuestState.Success && showCompletedEntryText)
            {
                var text = DialogueLua.GetQuestField(quest, "Entry " + entryNum + " Success").asString;
                if (!string.IsNullOrEmpty(text)) return text;
            }
            else if (entryState == QuestState.Failure && showCompletedEntryText)
            {
                var text = DialogueLua.GetQuestField(quest, "Entry " + entryNum + " Failure").asString;
                if (!string.IsNullOrEmpty(text)) return text;
            }
            return QuestLog.GetQuestEntry(quest, entryNum);
        }

        private string GetTitleStyleName(QuestState state)
        {
            switch (state)
            {
                case QuestState.Active: return TitleStyle;
                case QuestState.Success: return SuccessTitleStyle;
                case QuestState.Failure: return FailureTitleStyle;
                default: return TitleStyle;
            }
        }

        private string GetEntryStyleName(QuestState entryState)
        {
            switch (entryState)
            {
                case QuestState.Active: return ActiveEntryStyle;
                case QuestState.Success: return SuccessEntryStyle;
                case QuestState.Failure: return FailureEntryStyle;
                default: return ActiveEntryStyle;
            }
        }

        void OnGUI()
        {
            if (!isVisible) return;
            if (guiSkin != null) GUI.skin = guiSkin;
            GUILayout.BeginArea(screenRect);
            foreach (var line in lines)
            {
                if (line.guiStyle == null) line.guiStyle = UnityGUITools.GetGUIStyle(line.guiStyleName, GUI.skin.label);
                GUILayout.Label(line.text, line.guiStyle);
            }
            GUILayout.EndArea();
        }

    }

}
