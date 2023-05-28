// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Deprecated by DialogueSystemTrigger.
    /// Sets an animation clip on dialogue events. You can use this to set a character
    /// to idle when a conversation starts, and back to gameplay state when it ends.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class SetAnimationOnDialogueEvent : ActOnDialogueEvent
    {

        [System.Serializable]
        public class SetAnimationAction : Action
        {
            public Transform target;
            public AnimationClip animationClip;
        }

        /// <summary>
        /// Actions to take on the "start" event (e.g., OnConversationStart).
        /// </summary>
        public SetAnimationAction[] onStart = new SetAnimationAction[0];

        /// <summary>
        /// Actions to take on the "end" event (e.g., OnConversationEnd).
        /// </summary>
        public SetAnimationAction[] onEnd = new SetAnimationAction[0];

        public override void TryStartActions(Transform actor)
        {
            TryActions(onStart, actor);
        }

        public override void TryEndActions(Transform actor)
        {
            TryActions(onEnd, actor);
        }

        private void TryActions(SetAnimationAction[] actions, Transform actor)
        {
            if (actions == null) return;
            foreach (SetAnimationAction action in actions)
            {
                if (action != null && action.condition != null && action.condition.IsTrue(actor)) DoAction(action, actor);
            }
        }

        public void DoAction(SetAnimationAction action, Transform actor)
        {
            if (action != null)
            {
                Transform target = Tools.Select(action.target, this.transform);
                Animation animation = target.GetComponentInChildren<Animation>();
                if (animation == null)
                {
                    if (DialogueDebug.logWarnings) Debug.Log(string.Format("{0}: Trigger: {1}.SetAnimation() can't find Animation component", new System.Object[] { DialogueDebug.Prefix, target.name }));
                }
                else
                {
                    if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Trigger: {1}.SetAnimation({2})", new System.Object[] { DialogueDebug.Prefix, target.name, action.animationClip }));
                    animation.CrossFade(action.animationClip.name);
                }
            }
        }

    }

}
