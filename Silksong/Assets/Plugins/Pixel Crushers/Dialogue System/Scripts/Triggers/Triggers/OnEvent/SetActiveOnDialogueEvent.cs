// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Deprecated by DialogueSystemTrigger.
    /// Sets game objects active or inactive at the start and/or end of a dialogue event. Note that
    /// components on inactive game objects (including SetActiveOnDialogueEvent) do not receive 
    /// messages. If you want to activate an inactive game object, you should put this component
    /// on a different, active game object and set the action's target to the inactive object.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class SetActiveOnDialogueEvent : ActOnDialogueEvent
    {

        [System.Serializable]
        public class SetActiveAction : Action
        {
            public Transform target;
            public Toggle state;
        }

        /// <summary>
        /// Actions to take on the "start" event (e.g., OnConversationStart).
        /// </summary>
        public SetActiveAction[] onStart = new SetActiveAction[0];

        /// <summary>
        /// Actions to take on the "end" event (e.g., OnConversationEnd).
        /// </summary>
        public SetActiveAction[] onEnd = new SetActiveAction[0];

        public override void TryStartActions(Transform actor)
        {
            TryActions(onStart, actor);
        }

        public override void TryEndActions(Transform actor)
        {
            TryActions(onEnd, actor);
        }

        private void TryActions(SetActiveAction[] actions, Transform actor)
        {
            if (actions == null) return;
            foreach (SetActiveAction action in actions)
            {
                if (action != null && action.condition != null && action.condition.IsTrue(actor)) DoAction(action, actor);
            }
        }

        public void DoAction(SetActiveAction action, Transform actor)
        {
            if (action != null)
            {
                Transform target = Tools.Select(action.target, this.transform);
                bool newValue = ToggleUtility.GetNewValue(target.gameObject.activeSelf, action.state);
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Trigger: {1}.SetActive({2})", new System.Object[] { DialogueDebug.Prefix, target.name, newValue }));
                target.gameObject.SetActive(newValue);
            }
        }

    }

}
