// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Lua watch frequencies.
    /// </summary>
    public enum LuaWatchFrequency
    {
        /// <summary>
        /// Check every frame that Lua code was run.
        /// </summary>
        EveryUpdate,

        /// <summary>
        /// Check every time an actor speaks a DialogueEntry in a conversation.
        /// </summary>
        EveryDialogueEntry,

        /// <summary>
        /// Check at the end of conversations.
        /// </summary>
        EndOfConversation
    };

    /// <summary>
    /// This class maintains a list of watchers by watch frequency.
    /// </summary>
    public class LuaWatchers
    {

        private LuaWatchList m_everyUpdateList = new LuaWatchList();
        private LuaWatchList m_everyDialogueEntryList = new LuaWatchList();
        private LuaWatchList m_endOfConversationList = new LuaWatchList();

        /// <summary>
        /// Adds a Lua observer.
        /// </summary>
        /// <param name='luaExpression'>
        /// Lua expression to observe.
        /// </param>
        /// <param name='frequency'>
        /// Frequency to check.
        /// </param>
        /// <param name='luaChangedHandler'>
        /// Delegate to call when the expression changes.
        /// </param>
        public void AddObserver(string luaExpression, LuaWatchFrequency frequency, LuaChangedDelegate luaChangedHandler)
        {
            switch (frequency)
            {
                case LuaWatchFrequency.EveryUpdate:
                    m_everyUpdateList.AddObserver(luaExpression, luaChangedHandler);
                    break;
                case LuaWatchFrequency.EveryDialogueEntry:
                    m_everyDialogueEntryList.AddObserver(luaExpression, luaChangedHandler);
                    break;
                case LuaWatchFrequency.EndOfConversation:
                    m_endOfConversationList.AddObserver(luaExpression, luaChangedHandler);
                    break;
                default:
                    Debug.LogError("Dialogue System: Internal error - unexpected Lua watch frequency " + frequency);
                    break;
            }
        }

        /// <summary>
        /// Removes a Lua observer.
        /// </summary>
        /// <param name='luaExpression'>
        /// Lua expression.
        /// </param>
        /// <param name='frequency'>
        /// Frequency.
        /// </param>
        /// <param name='luaChangedHandler'>
        /// Lua changed handler.
        /// </param>
        public void RemoveObserver(string luaExpression, LuaWatchFrequency frequency, LuaChangedDelegate luaChangedHandler)
        {
            switch (frequency)
            {
                case LuaWatchFrequency.EveryUpdate:
                    m_everyUpdateList.RemoveObserver(luaExpression, luaChangedHandler);
                    break;
                case LuaWatchFrequency.EveryDialogueEntry:
                    m_everyDialogueEntryList.RemoveObserver(luaExpression, luaChangedHandler);
                    break;
                case LuaWatchFrequency.EndOfConversation:
                    m_endOfConversationList.RemoveObserver(luaExpression, luaChangedHandler);
                    break;
                default:
                    Debug.LogError("Dialogue System: Internal error - unexpected Lua watch frequency " + frequency);
                    break;
            }
        }

        /// <summary>
        /// Removes all Lua observers for a specified frequency.
        /// </summary>
        /// <param name='frequency'>
        /// Frequency.
        /// </param>
        public void RemoveAllObservers(LuaWatchFrequency frequency)
        {
            switch (frequency)
            {
                case LuaWatchFrequency.EveryUpdate:
                    m_everyUpdateList.RemoveAllObservers();
                    break;
                case LuaWatchFrequency.EveryDialogueEntry:
                    m_everyDialogueEntryList.RemoveAllObservers();
                    break;
                case LuaWatchFrequency.EndOfConversation:
                    m_endOfConversationList.RemoveAllObservers();
                    break;
                default:
                    Debug.LogError("Dialogue System: Internal error - unexpected Lua watch frequency " + frequency);
                    break;
            }
        }

        /// <summary>
        /// Removes all Lua observers.
        /// </summary>
        public void RemoveAllObservers()
        {
            m_everyUpdateList.RemoveAllObservers();
            m_everyDialogueEntryList.RemoveAllObservers();
            m_endOfConversationList.RemoveAllObservers();
        }

        /// <summary>
        /// Checks the observers for a specified frequency.
        /// </summary>
        /// <param name='frequency'>
        /// Frequency.
        /// </param>
        public void NotifyObservers(LuaWatchFrequency frequency)
        {
            switch (frequency)
            {
                case LuaWatchFrequency.EveryUpdate:
                    m_everyUpdateList.NotifyObservers();
                    break;
                case LuaWatchFrequency.EveryDialogueEntry:
                    m_everyDialogueEntryList.NotifyObservers();
                    break;
                case LuaWatchFrequency.EndOfConversation:
                    m_endOfConversationList.NotifyObservers();
                    break;
                default:
                    Debug.LogError("Dialogue System: Internal error - unexpected Lua watch frequency " + frequency);
                    break;
            }
        }

    }

}
