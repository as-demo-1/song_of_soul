// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Deprecated by DialogueSystemTrigger.
    /// Runs Lua code at the start and/or end of a dialogue event.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class LuaOnDialogueEvent : ActOnDialogueEvent
    {

        /// <summary>
        /// The parameters for a Lua action.
        /// </summary>
        [System.Serializable]
        public class LuaAction : ActOnDialogueEvent.Action
        {
            [Multiline]
            public string luaCode = string.Empty;

        }
        /// <summary>
        /// Actions to take on the "start" event (e.g., OnConversationStart).
        /// </summary>
        public LuaAction[] onStart = new LuaAction[0];

        /// <summary>
        /// Actions to take on the "end" event (e.g., OnConversationEnd).
        /// </summary>
        public LuaAction[] onEnd = new LuaAction[0];

        /// <summary>
        /// If <c>true</c>, prints Lua debug information to the console.
        /// </summary>
        public bool debugLua;

        public override void TryStartActions(Transform actor)
        {
            TryActions(onStart, actor);
        }

        public override void TryEndActions(Transform actor)
        {
            TryActions(onEnd, actor);
        }

        private void TryActions(LuaAction[] actions, Transform actor)
        {
            if (actions == null) return;
            foreach (LuaAction action in actions)
            {
                if (action != null && action.condition != null && action.condition.IsTrue(actor)) DoAction(action, actor);
            }
        }

        public void DoAction(LuaAction action, Transform actor)
        {
            if (action == null) return;
            Lua.Run(action.luaCode, debugLua);
            DialogueManager.SendUpdateTracker();
        }

    }

}
