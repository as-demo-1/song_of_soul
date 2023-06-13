#if USE_UNET
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Networking;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This class for Unity Networking's High Level API (UNET HLAPI) adds Lua functions to 
    /// set variables and quest states on all network clients. Add it to the player prefab.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class LuaNetworkCommands : NetworkBehaviour
    {

        public override void OnStartLocalPlayer()
        {
            RegisterLuaFunctions();
        }

        public void RegisterLuaFunctions()
        {
            Lua.RegisterFunction("NetSetBool", this, SymbolExtensions.GetMethodInfo(() => NetSetBool(string.Empty, false)));
            Lua.RegisterFunction("NetSetNumber", this, SymbolExtensions.GetMethodInfo(() => NetSetNumber(string.Empty, (double)0)));
            Lua.RegisterFunction("NetSetString", this, SymbolExtensions.GetMethodInfo(() => NetSetString(string.Empty, string.Empty)));
            Lua.RegisterFunction("NetSetQuestState", this, SymbolExtensions.GetMethodInfo(() => NetSetQuestState(string.Empty, string.Empty)));
            Lua.RegisterFunction("NetSetQuestEntryState", this, SymbolExtensions.GetMethodInfo(() => NetSetQuestEntryState(string.Empty, (double)0, string.Empty)));
        }

        public void UnregisterLuaFunctions()
        {
            Lua.UnregisterFunction("NetSetBool");
            Lua.UnregisterFunction("NetSetNumber");
            Lua.UnregisterFunction("NetSetString");
            Lua.UnregisterFunction("NetSetQuestState");
            Lua.UnregisterFunction("NetSetQuestEntryState");
        }

        public void NetSetBool(string variableName, bool value)
        {
            CmdSetBool(variableName, value);
        }

        public void NetSetNumber(string variableName, double value)
        {
            CmdSetFloat(variableName, (float)value);
        }

        public void NetSetString(string variableName, string value)
        {
            CmdSetString(variableName, value);
        }

        public void NetSetQuestState(string questName, string state)
        {
            CmdSetQuestState(questName, state);
        }

        public void NetSetQuestEntryState(string questName, double entryNumber, string state)
        {
            CmdSetQuestEntryState(questName, (int)entryNumber, state);
        }

        [Command]
        void CmdSetBool(string variableName, bool value)
        {
            RpcSetBool(variableName, value);
        }

        [Command]
        void CmdSetFloat(string variableName, float value)
        {
            RpcSetFloat(variableName, value);
        }

        [Command]
        void CmdSetString(string variableName, string value)
        {
            RpcSetString(variableName, value);
        }

        [Command]
        void CmdSetQuestState(string questName, string state)
        {
            RpcSetQuestState(questName, state);
        }

        [Command]
        void CmdSetQuestEntryState(string questName, int entryNumber, string state)
        {
            RpcSetQuestEntryState(questName, entryNumber, state);
        }

        [ClientRpc]
        void RpcSetBool(string variableName, bool value)
        {
            DialogueLua.SetVariable(variableName, value);
        }

        [ClientRpc]
        void RpcSetFloat(string variableName, float value)
        {
            DialogueLua.SetVariable(variableName, value);
        }

        [ClientRpc]
        void RpcSetString(string variableName, string value)
        {
            DialogueLua.SetVariable(variableName, value);
        }

        [ClientRpc]
        void RpcSetQuestState(string questName, string state)
        {
            QuestLog.SetQuestState(questName, state);
        }

        [ClientRpc]
        void RpcSetQuestEntryState(string questName, int entryNumber, string state)
        {
            QuestLog.SetQuestEntryState(questName, entryNumber, state);
        }

    }
}
#endif