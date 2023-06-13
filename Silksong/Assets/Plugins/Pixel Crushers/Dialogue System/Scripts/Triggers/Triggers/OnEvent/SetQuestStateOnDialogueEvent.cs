// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Deprecated by DialogueSystemTrigger.
    /// Sets a quest state at the start and/or end of a dialogue event.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class SetQuestStateOnDialogueEvent : ActOnDialogueEvent
    {

        [System.Serializable]
        public class SetQuestStateAction : Action
        {

            [QuestPopup]
            public string questName;

            [QuestState]
            public QuestState questState;

            public string alertMessage;
        }

        /// <summary>
        /// Actions to take on the "start" event (e.g., OnConversationStart).
        /// </summary>
        public SetQuestStateAction[] onStart = new SetQuestStateAction[0];

        /// <summary>
        /// Actions to take on the "end" event (e.g., OnConversationEnd).
        /// </summary>
        public SetQuestStateAction[] onEnd = new SetQuestStateAction[0];

        public override void TryStartActions(Transform actor)
        {
            TryActions(onStart, actor);
        }

        public override void TryEndActions(Transform actor)
        {
            TryActions(onEnd, actor);
        }

        private void TryActions(SetQuestStateAction[] actions, Transform actor)
        {
            if (actions == null) return;
            foreach (SetQuestStateAction action in actions)
            {
                if (action != null && action.condition != null && action.condition.IsTrue(actor)) DoAction(action, actor);
            }
        }

        public void DoAction(SetQuestStateAction action, Transform actor)
        {
            if ((action != null) && !string.IsNullOrEmpty(action.questName))
            {
                QuestLog.SetQuestState(action.questName, action.questState);
                if (!string.IsNullOrEmpty(action.alertMessage)) DialogueManager.ShowAlert(action.alertMessage);
                DialogueManager.SendUpdateTracker();
            }
        }

    }

}
