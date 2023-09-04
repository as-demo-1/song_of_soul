// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Deprecated by DialogueSystemTrigger.
    /// Sets components enabled or disabled at the start and/or end of a dialogue event.
    /// The older SetEnabledOnDialogueEvent trigger was written for MonoBehaviours. On customer
    /// request, this trigger was added to handle renderers and colliders, which aren't
    /// MonoBehaviours.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class SetComponentEnabledOnDialogueEvent : ActOnDialogueEvent
    {

        [System.Serializable]
        public class SetComponentEnabledAction : Action
        {
            public Component target;
            public Toggle state;
        }

        /// <summary>
        /// Actions to take on the "start" event (e.g., OnConversationStart).
        /// </summary>
        public SetComponentEnabledAction[] onStart = new SetComponentEnabledAction[0];

        [Tooltip("When the dialogue event starts, wait one frame before processing the On Start list.")]
        public bool waitOneFrameOnStart = false;

        /// <summary>
        /// Actions to take on the "end" event (e.g., OnConversationEnd).
        /// </summary>
        public SetComponentEnabledAction[] onEnd = new SetComponentEnabledAction[0];

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

        private void TryActions(SetComponentEnabledAction[] actions, Transform actor, bool waitOneFrame)
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

        private void TryActionsNow(SetComponentEnabledAction[] actions, Transform actor)
        {
            foreach (SetComponentEnabledAction action in actions)
            {
                if (action != null && action.condition != null && action.condition.IsTrue(actor)) DoAction(action, actor);
            }
        }

        private IEnumerator TryActionsAfterOneFrameCoroutine(SetComponentEnabledAction[] actions, Transform actor)
        {
            yield return CoroutineUtility.endOfFrame;
            yield return null;
            TryActionsNow(actions, actor);
        }

        public void DoAction(SetComponentEnabledAction action, Transform actor)
        {
            if ((action != null) && (action.target != null))
            {
                Tools.SetComponentEnabled(action.target, action.state);
            }
        }

    }

}
