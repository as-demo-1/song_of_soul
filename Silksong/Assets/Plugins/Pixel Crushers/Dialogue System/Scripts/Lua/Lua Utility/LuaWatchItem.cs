// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Lua expression changed delegate.
    /// </summary>
    public delegate void LuaChangedDelegate(LuaWatchItem luaWatchItem, Lua.Result newValue);

    /// <summary>
    /// Watch item for Lua observers. This allows the observer to be notified when a Lua value changes.
    /// </summary>
    public class LuaWatchItem
    {

        /// <summary>
        /// The lua expression to watch.
        /// </summary>
        public string luaExpression { get; set; }

        /// @cond FOR_V1_COMPATIBILITY
        public string LuaExpression { get { return luaExpression; } set { luaExpression = value; } }
        /// @endcond

        /// <summary>
        /// The current value of the expression.
        /// </summary>
        private string m_currentValue;

        /// <summary>
        /// Delegate to call when the expression changes.
        /// </summary>
        private event LuaChangedDelegate luaChanged;

        public static string LuaExpressionWithReturn(string luaExpression)
        {
            return luaExpression.StartsWith("return ") ? luaExpression : ("return " + luaExpression);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelCrushers.DialogueSystem.WatchItem"/> class.
        /// </summary>
        /// <param name='luaExpression'>
        /// Lua expression to watch.
        /// </param>
        /// <param name='luaChangedHandler'>
        /// Delegate to call when the expression changes.
        /// </param>
        public LuaWatchItem(string luaExpression, LuaChangedDelegate luaChangedHandler)
        {
            this.luaExpression = LuaExpressionWithReturn(luaExpression);
            this.m_currentValue = Lua.Run(this.luaExpression).asString;
            this.luaChanged = luaChangedHandler;
        }

        /// <summary>
        /// Checks if the watch item matches a specified luaExpression and luaChangedHandler.
        /// </summary>
        /// <param name='luaExpression'>
        /// The lua expression.
        /// </param>
        /// <param name='luaChangedHandler'>
        /// The notification delegate.
        /// </param>
        public bool Matches(string luaExpression, LuaChangedDelegate luaChangedHandler)
        {
            return (luaChangedHandler == luaChanged) && string.Equals(luaExpression, this.luaExpression);
        }

        /// <summary>
        /// Checks the watch item and calls the delegate if the Lua expression changed.
        /// </summary>
        public void Check()
        {
            Lua.Result result = Lua.Run(luaExpression);
            string newValue = result.asString;
            if (!string.Equals(m_currentValue, newValue))
            {
                m_currentValue = newValue;
                if (luaChanged != null) luaChanged(this, result);
            }
        }

    }

}
