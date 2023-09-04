// Copyright (c) Pixel Crushers. All rights reserved.

#if !(USE_NLUA || OVERRIDE_LUA)
using UnityEngine;
using System.Collections.Generic;
using Language.Lua;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This is a Lua table wrapper for LuaInterpreter. It isolates the
    /// underlying LuaInterpreter implementation from the rest of the
    /// Dialogue System.
    /// </summary>
    public class LuaTableWrapper
    {

        /// <summary>
        /// The LuaInterpreter Lua table.
        /// </summary>
        public Language.Lua.LuaTable luaTable = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelCrushers.DialogueSystem.LuaTableWrapper"/> class.
        /// </summary>
        /// <param name="luaTable">Lua table.</param>
        public LuaTableWrapper(LuaTable luaTable)
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
                return (luaTable != null) ? Mathf.Max(luaTable.Length, luaTable.Count) : 0;
            }
        }

        /// <summary>
        /// Gets the keys as strings.If the table is a one-dimensional
        /// array, this returns the indices as strings.
        /// </summary>
        /// <value>The keys.</value>
        public IEnumerable<string> keys
        {
            get
            {
                if ((luaTable != null) && (count > 0))
                {
                    foreach (LuaValue key in luaTable.Keys)
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
                    if (luaTable.Length > 0)
                    {
                        // Get list values:
                        foreach (LuaValue value in luaTable.ListValues)
                        {
                            yield return (value is LuaTable)
                                ? new LuaTableWrapper(value as LuaTable)
                                : LuaInterpreterExtensions.LuaValueToObject(value);
                        }
                    }
                    else if (luaTable.Count > 0)
                    {
                        // Get dictionary values:
                        foreach (KeyValuePair<LuaValue, LuaValue> kvp in luaTable.KeyValuePairs)
                        {
                            yield return (kvp.Value is LuaTable)
                                ? new LuaTableWrapper(kvp.Value as LuaTable)
                                : LuaInterpreterExtensions.LuaValueToObject(kvp.Value);
                        }
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
                if ((luaTable != null) && (count > 0))
                {
                    foreach (LuaValue key in luaTable.Keys)
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
                    if (luaTable.Length > 0)
                    {
                        // Get list values:
                        foreach (LuaValue value in luaTable.ListValues)
                        {
                            yield return (value is LuaTable)
                                ? new LuaTableWrapper(value as LuaTable)
                                : LuaInterpreterExtensions.LuaValueToObject(value);
                        }
                    }
                    else if (luaTable.Count > 0)
                    {
                        // Get dictionary values:
                        foreach (KeyValuePair<LuaValue, LuaValue> kvp in luaTable.KeyValuePairs)
                        {
                            yield return (kvp.Value is LuaTable)
                                ? new LuaTableWrapper(kvp.Value as LuaTable)
                                : LuaInterpreterExtensions.LuaValueToObject(kvp.Value);
                        }
                    }
                }
            }
        }
        /// @endcond

        /// <summary>
        /// Gets the value with the specified string key. Returns a standard type such as
        /// <c>string</c>, <c>float</c>, <c>bool</c>, or <c>null</c>. If the value
        /// is a table, it returns a LuaTableWrapper around it.
        /// </summary>
        /// <param name="key">Key.</param>
        public object this[string key]
        {
            get
            {
                if (luaTable == null)
                {
                    if (DialogueDebug.logErrors) Debug.LogError(string.Format("{0}: Lua table is null; lookup[{1}] failed", new object[] { DialogueDebug.Prefix, key }));
                    return null;
                }
                LuaValue luaValue = LuaNil.Nil;
                if (luaTable.Length > 0)
                {

                    // Get list value:
                    luaValue = luaTable.GetValue(Tools.StringToInt(key));

                }
                else
                {
                    // Get dictionary value:
                    LuaValue luaValueKey = luaTable.GetKey(key);
                    if (luaValueKey == LuaNil.Nil)
                    {
                        //--- Suppressed: if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: Lua table does not contain key [{1}]", new string[] { DialogueDebug.Prefix, key }));
                        return null;
                    }
                    luaValue = luaTable.GetValue(key);
                }
                if (luaValue is LuaTable)
                {
                    return new LuaTableWrapper(luaValue as LuaTable);
                }
                else
                {
                    return LuaInterpreterExtensions.LuaValueToObject(luaValue);
                }
            }
        }

        /// <summary>
        /// Gets the value with the specified int key. Returns a standard type such as
        /// <c>string</c>, <c>float</c>, <c>bool</c>, or <c>null</c>. If the value
        /// is a table, it returns a LuaTableWrapper around it.
        /// </summary>
        /// <param name="key">Key.</param>
        public object this[int key]
        {
            get
            {
                if (luaTable == null)
                {
                    if (DialogueDebug.logErrors) Debug.LogError(string.Format("{0}: Lua table is null; lookup[{1}] failed", new object[] { DialogueDebug.Prefix, key }));
                    return null;
                }
                LuaValue luaValue = LuaNil.Nil;
                if (luaTable.Length > 0)
                {

                    // Get list value:
                    luaValue = luaTable.GetValue(key);

                }
                else
                {

                    // Get dictionary value:
                    LuaValue luaValueKey = luaTable.GetKey(key.ToString());
                    if (luaValueKey == LuaNil.Nil)
                    {
                        //--- Suppressed: if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: Lua table does not contain key [{1}]", new string[] { DialogueDebug.Prefix, key }));
                        return null;
                    }
                    luaValue = luaTable.GetValue(key);

                }
                if (luaValue is LuaTable)
                {
                    return new LuaTableWrapper(luaValue as LuaTable);
                }
                else
                {
                    return LuaInterpreterExtensions.LuaValueToObject(luaValue);
                }
            }
        }

    }

}
#endif