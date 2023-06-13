// Copyright (c) Pixel Crushers. All rights reserved.

#if !(USE_NLUA || OVERRIDE_LUA)
using UnityEngine;
using System;
using System.Reflection;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A static class that provides a global Lua virtual machine. This class provides a layer of
    /// abstraction between the low level Lua implementation (in this case LuaInterpreter) and
    /// the Dialogue System.
    /// </summary>
    public sealed class Lua
    {

        /// <summary>
        /// Stores a Lua interpreter result (LuaValue) and provides easy conversion to basic types.
        /// </summary>
        public struct Result
        {
            public Language.Lua.LuaValue luaValue;
            public LuaTableWrapper luaTableWrapper;

            public Result(Language.Lua.LuaValue luaValue)
            {
                this.luaValue = luaValue;
                this.luaTableWrapper = null;
            }

            public bool hasReturnValue { get { return luaValue != null; } }
            public string asString { get { return hasReturnValue ? luaValue.ToString() : string.Empty; } }
            public bool asBool { get { return (hasReturnValue && (luaValue is Language.Lua.LuaBoolean)) ? (luaValue as Language.Lua.LuaBoolean).BoolValue : string.Compare(asString, "True", StringComparison.OrdinalIgnoreCase) == 0; } }
            public float asFloat { get { return hasReturnValue ? Tools.StringToFloat(luaValue.ToString()) : 0; } }
            public int asInt { get { return hasReturnValue ? Tools.StringToInt(luaValue.ToString()) : 0; } }
            public LuaTableWrapper asTable { get { if (luaTableWrapper == null) luaTableWrapper = new LuaTableWrapper(luaValue as Language.Lua.LuaTable); return luaTableWrapper; } }

            public bool isString { get { return hasReturnValue && luaValue is Language.Lua.LuaString; } }
            public bool isBool { get { return hasReturnValue && luaValue is Language.Lua.LuaBoolean; } }
            public bool isNumber { get { return hasReturnValue && luaValue is Language.Lua.LuaNumber; } }
            public bool isTable { get { return hasReturnValue & (luaValue is Language.Lua.LuaTable); } }

            /// @cond FOR_V1_COMPATIBILITY
            public bool HasReturnValue { get { return hasReturnValue; } }
            public string AsString { get { return asString; } }
            public bool AsBool { get { return asBool; } }
            public float AsFloat { get { return asFloat; } }
            public int AsInt { get { return asInt; } }
            public LuaTableWrapper AsTable { get { return asTable; } }
            public bool IsString { get { return isString; } }
            public bool IsBool { get { return isBool; } }
            public bool IsNumber { get { return isNumber; } }
            public bool IsTable { get { return isTable; } }
            /// @endcond
        }

        private static Result m_noResult = new Result(null);
        public static Result noResult { get { return m_noResult; } }
        public static Result NoResult { get { return m_noResult; } }

        /// <summary>
        /// Lua.RunRaw sets this Boolean flag whenever it's invoked.
        /// </summary>
        /// <value>
        /// <c>true</c> when Lua.RunRaw is invoked</c>.
        /// </value>
        public static bool wasInvoked { get; set; }

        /// <summary>
        /// Set true to not log exceptions to the Console.
        /// </summary>
        /// <value><c>true</c> if mute exceptions; otherwise, <c>false</c>.</value>
        public static bool muteExceptions { get; set; }

        /// <summary>
        /// Set true to log warnings if trying to register a function under a name that's already registered.
        /// </summary>
        public static bool warnRegisteringExistingFunction { get; set; }

        /// <summary>
        /// Provides direct access to the Lua Interpreter environment.
        /// </summary>
        /// <value>The Interpreter environment.</value>
        public static Language.Lua.LuaTable environment { get { return m_environment; } }
        public static Language.Lua.LuaTable Environment { get { return m_environment; } }

        /// <summary>
        /// The Lua Interpreter environment.
        /// </summary>
        private static Language.Lua.LuaTable m_environment = Language.Lua.LuaInterpreter.CreateGlobalEnviroment();

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            m_environment = Language.Lua.LuaInterpreter.CreateGlobalEnviroment();
            m_noResult = new Result(null);
        }
#endif

        /// @cond FOR_V1_COMPATIBILITY
        public static bool WasInvoked { get { return wasInvoked; } set { wasInvoked = value; } }
        public static bool MuteExceptions { get { return muteExceptions; } set { muteExceptions = value; } }
        public static bool WarnRegisteringExistingFunction { get { return warnRegisteringExistingFunction; } set { warnRegisteringExistingFunction = value; } }
        /// @endcond

        /// <summary>
        /// Runs the specified luaCode.
        /// </summary>
        /// <returns>
        /// A Result struct from which you can retrieve basic data types from the first return value.
        /// </returns>
        /// <param name='luaCode'>
        /// The Lua code to run. Generally, if you want a return value, this string should start with "return".
        /// </param>
        /// <param name='debug'>
        /// If <c>true</c>, logs the Lua command to the console.
        /// </param>
        /// <param name='allowExceptions'>
        /// If <c>true</c>, exceptions are passed up to the caller. Otherwise they're caught and ignored.
        /// </param>
        /// <example>
        /// float myHeight = Lua.Run("return height").asFloat;
        /// </example>
        public static Result Run(string luaCode, bool debug, bool allowExceptions)
        {
            return new Result(RunRaw(luaCode, debug, allowExceptions));
        }

        /// <summary>
        /// Runs the specified luaCode. Exceptions are ignored.
        /// </summary>
        /// <param name="luaCode">The Lua code to run. Generally, if you want a return value, this string should start with "return".</param>
        /// <param name="debug">If set to <c>true</c>, logs the Lua command to the console.</param>
        public static Result Run(string luaCode, bool debug)
        {
            return Run(luaCode, debug, false);
        }

        /// <summary>
        /// Run the specified luaCode. The code is not logged to the console, and exceptions are ignored.
        /// </summary>
        /// <param name="luaCode">The Lua code to run. Generally, if you want a return value, this string should start with "return".</param>
        public static Result Run(string luaCode)
        {
            return Run(luaCode, false, false);
        }

        /// <summary>
        /// Evaluates a boolean condition defined in Lua code.
        /// </summary>
        /// <returns>
        /// <c>true</c> if luaCode evaluates to true; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='luaCondition'>
        /// The conditional expression to evaluate. Do not include "return" in front.
        /// </param>
        /// <param name='debug'>
        /// If <c>true</c>, logs the Lua command to the console.
        /// </param>
        /// <param name='allowExceptions'>
        /// If <c>true</c>, exceptions are passed up to the caller. Otherwise they're caught and ignored.
        /// </param>
        /// <example>
        /// if (Lua.IsTrue("height > 6")) { ... }
        /// </example>
        public static bool IsTrue(string luaCondition, bool debug, bool allowExceptions)
        {
            return Tools.IsStringNullOrEmptyOrWhitespace(luaCondition) ? true : Run("return " + luaCondition, debug, allowExceptions).asBool;
        }

        /// <summary>
        /// Evaluates a boolean condition defined in Lua code. Exceptions are ignored.
        /// </summary>
        /// <returns><c>true</c> if luaCode evaluates to true; otherwise, <c>false</returns>
        /// <param name="luaCondition">The conditional expression to evaluate. Do not include "return" in front.</param>
        /// <param name="debug">If <c>true</c>, logs the Lua command to the console.</param>
        public static bool IsTrue(string luaCondition, bool debug)
        {
            return IsTrue(luaCondition, debug, false);
        }

        /// <summary>
        /// Evaluates a boolean condition defined in Lua code. The code is not logged to the console, and exceptions are ignored.
        /// </summary>
        /// <returns><c>true</c> if luaCode evaluates to true; otherwise, <c>false</returns>
        /// <param name="luaCondition">The conditional expression to evaluate. Do not include "return" in front.</param>
        public static bool IsTrue(string luaCondition)
        {
            return IsTrue(luaCondition, false, false);
        }

        /// <summary>
        /// Runs Lua code and returns an array of return values.
        /// </summary>
        /// <returns>
        /// An array of return values, or <c>null</c> if the code generates an error.
        /// </returns>
        /// <param name='luaCode'>
        /// The Lua code to run. If you want a return value, this string should usually start with
        /// "<c>return</c>".
        /// </param>
        /// <param name='debug'>
        /// If <c>true</c>, logs the Lua command to the console.
        /// </param>
        /// <param name='allowExceptions'>
        /// If <c>true</c>, exceptions are passed up to the caller. Otherwise they're caught and logged but ignored.
        /// </param>
        public static Language.Lua.LuaValue RunRaw(string luaCode, bool debug, bool allowExceptions)
        {
            try
            {
                if (string.IsNullOrEmpty(luaCode))
                {
                    return null;
                }
                else
                {
                    if (Debug.isDebugBuild && debug) Debug.Log(string.Format("{0}: Lua({1})", new System.Object[] { DialogueDebug.Prefix, luaCode }));
                    wasInvoked = true;
                    return Language.Lua.LuaInterpreter.Interpreter(luaCode, environment);
                }
            }
            catch (Exception e)
            {
                if (Debug.isDebugBuild && !muteExceptions) Debug.LogError(string.Format("{0}: Lua code '{1}' threw exception '{2}'", new System.Object[] { DialogueDebug.Prefix, luaCode, e.Message }));
                if (allowExceptions) throw e; else return null;
            }
        }

        /// <summary>
        /// Runs Lua code and returns an array of return values. Ignores exceptions.
        /// </summary>
        /// <returns>
        /// An array of return values, or <c>null</c> if the code generates an error.
        /// </returns>
        /// <param name='luaCode'>
        /// The Lua code to run. If you want a return value, this string should usually start with
        /// "<c>return</c>".
        /// </param>
        /// <param name='debug'>
        /// If <c>true</c>, logs the Lua command to the console.
        /// </param>
        public static Language.Lua.LuaValue RunRaw(string luaCode, bool debug)
        {
            return RunRaw(luaCode, debug, false);
        }

        /// <summary>
        /// Runs Lua code and returns an array of return values. Does not log to console and ignores exceptions.
        /// </summary>
        /// <returns>
        /// An array of return values, or <c>null</c> if the code generates an error.
        /// </returns>
        /// <param name='luaCode'>
        /// The Lua code to run. If you want a return value, this string should usually start with
        /// "<c>return</c>".
        /// </param>
        public static Language.Lua.LuaValue RunRaw(string luaCode)
        {
            return RunRaw(luaCode, false, false);
        }

        /// <summary>
        /// Registers a C# function with the Lua interpreter so it can be used in Lua.
        /// </summary>
        /// <param name='path'>
        /// The name of the function in Lua.
        /// </param>
        /// <param name='target'>
        /// Target object containing the registered method. Can be <c>null</c> if a static method.
        /// </param>
        /// <param name='function'>
        /// The method that will be called from Lua.
        /// </param>
        public static void RegisterFunction(string functionName, object target, MethodInfo method)
        {
            if (environment.ContainsKey(new Language.Lua.LuaString(functionName)))
            {
                if (warnRegisteringExistingFunction && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Can't register Lua function {1}. A function with that name is already registered.", new System.Object[] { DialogueDebug.Prefix, functionName }));
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Registering Lua function {1}", new System.Object[] { DialogueDebug.Prefix, functionName }));
                environment.RegisterMethodFunction(functionName, target, method);
            }
        }

        /// <summary>
        /// Unregisters a C# function.
        /// </summary>
        /// <param name="functionName">Function name.</param>
        public static void UnregisterFunction(string functionName)
        {
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: Unregistering Lua function {1}", new System.Object[] { DialogueDebug.Prefix, functionName }));
            environment.SetNameValue(functionName, Language.Lua.LuaNil.Nil);
        }

    }

}
#endif