#if USE_NLUA
// Copyright (c) Pixel Crushers. All rights reserved.

// This version of LuaTableWrapper works with NLua.
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface = NLua;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This is a Lua table wrapper for LuaInterpreter. It isolates the
    /// underlying NLua implementation from the rest of the
    /// Dialogue System.
    /// </summary>
    public class LuaTableWrapper
    {

        /// <summary>
        /// The NLua table.
        /// </summary>
        public LuaInterface.LuaTable luaTable = null;

        public LuaTableWrapper(LuaInterface.LuaTable luaTable)
        {
            this.luaTable = luaTable;
        }

        /// <summary>
        /// Indicates whether the wrapper points to a valid Lua table.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool isValid { get { return (luaTable != null); } }

        /// <summary>
        /// Gets the number of elements in the table.
        /// </summary>
        /// <value>The count.</value>
        public int count
        {
            get
            {
                return (luaTable != null) ? luaTable.Values.Count : 0;
            }
        }

        /// <summary>
        /// Gets the keys as strings. If the table is a one-dimensional array,
        /// it returns the indices as strings.
        /// </summary>
        /// <value>The keys.</value>
        public IEnumerable<string> keys
        {
            get
            {
                if (luaTable != null)
                {
                    foreach (var key in luaTable.Keys)
                    {
                        yield return key.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the values. If a value is a table, this returns a
        /// LuaTableWrapper around it.
        /// </summary>
        /// <value>The values.</value>
        public IEnumerable<object> values
        {
            get
            {
                if (luaTable != null)
                {
                    foreach (var value in luaTable.Values)
                    {
                        yield return (value is LuaInterface.LuaTable)
                            ? new LuaTableWrapper(value as LuaInterface.LuaTable)
                            : value;
                    }
                }
            }
        }

        /// @cond FOR_V1_COMPATIBILITY
        public bool IsValid { get { return isValid; } }
        public int Count { get { return count; } }
        public IEnumerable<string> Keys
        {
            get
            {
                if (luaTable != null)
                {
                    foreach (var key in luaTable.Keys)
                    {
                        yield return key.ToString();
                    }
                }
            }
        }
        public IEnumerable<object> Values
        {
            get
            {
                if (luaTable != null)
                {
                    foreach (var value in luaTable.Values)
                    {
                        yield return (value is LuaInterface.LuaTable)
                            ? new LuaTableWrapper(value as LuaInterface.LuaTable)
                            : value;
                    }
                }
            }
        }
        /// @endcond

        /// <summary>
        /// Gets the value with the specified key. Returns a standard type such as
        /// <c>string</c>, <c>float</c>, <c>bool</c>, or <c>null</c>. If the value
        /// is a table, it returns a LuaTableWrapper around it.
        /// </summary>
        /// <param name="key">Key.</param>
        public object this[object key]
        {
            get
            {
                if (luaTable == null)
                {
                    //---Suppressed: if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: Lua table is null; lookup[{1}] failed", new string[] { DialogueDebug.Prefix, key }));
                    return null;
                }
                var value = luaTable[key];
                return (value is LuaInterface.LuaTable)
                    ? new LuaTableWrapper(value as LuaInterface.LuaTable)
                    : luaTable[key];
            }
        }

    }

}
#endif