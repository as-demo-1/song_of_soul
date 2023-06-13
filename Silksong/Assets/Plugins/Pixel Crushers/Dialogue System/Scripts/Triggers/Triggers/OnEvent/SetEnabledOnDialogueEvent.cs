// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Deprecated by DialogueSystemTrigger.
    /// Sets MonoBehaviours enabled or disabled at the start and/or end of a dialogue event.
    /// Deprecated by SetComponentEnabledOnDialogueEvent.
    /// </summary>
    [AddComponentMenu("")] // Hide. Deprecated by SetComponentEnabledOnDialogueEvent.
    public class SetEnabledOnDialogueEvent : ActOnDialogueEvent
    {

        [System.Serializable]
        public class SetEnabledAction : Action
        {
            public MonoBehaviour target;
            public Toggle state;
        }

        /// <summary>
        /// Actions to take on the "start" event (e.g., OnConversationStart).
        /// </summary>
        public SetEnabledAction[] onStart = new SetEnabledAction[0];

        [Tooltip("When the dialogue event starts, wait one frame before processing the On Start list.")]
        public bool waitOneFrameOnStart = false;

        /// <summary>
        /// Actions to take on the "end" event (e.g., OnConversationEnd).
        /// </summary>
        public SetEnabledAction[] onEnd = new SetEnabledAction[0];

        [Tooltip("When the dialogue event starts, wait one frame before processing the On End list.")]
        public bool waitOneFrameOnEnd = false;

        public override void TryStartActions(Transform actor)
        {
            TryActions(onStart, actor, waitOneFrameOnStart);
        }

        public override void TryEndActions(Transform actor)
        {
            TryActions(onEnd, actor, waitOneFrameOnEnd);
        }

        private void TryActions(SetEnabledAction[] actions, Transform actor, bool waitOneFrame)
        {
            if (actions == null) return;
            if (waitOneFrame)
            {
                StartCoroutine(TryActionsAfterOneFrameCoroutine(actions, actor));
            }
            else
            {
                TryActionsNow(actions, actor);
            }
        }

        private void TryActionsNow(SetEnabledAction[] actions, Transform actor)
        { 
            foreach (SetEnabledAction action in actions)
            {
                if (action != null && action.condition != null && action.condition.IsTrue(actor)) DoAction(action, actor);
            }
        }

        private IEnumerator TryActionsAfterOneFrameCoroutine(SetEnabledAction[] actions, Transform actor)
        {
            Debug.Log("Waiting 1 frame");
            yield return new WaitForEndOfFrame();
            yield return null;
            yield return new WaitForSeconds(2);
            TryActionsNow(actions, actor);
        }

        public void DoAction(SetEnabledAction action, Transform actor)
        {
            if (action != null)
            {
                MonoBehaviour target = Tools.Select(action.target, this);
                if (target == null) return;
                bool newValue = ToggleUtility.GetNewValue(target.enabled, action.state);
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Trigger: {1}.{2}.enabled = {3}", new System.Object[] { DialogueDebug.Prefix, target.name, target.GetType().Name, newValue }));
                target.enabled = newValue;
            }
        }

    }

}
