#if USE_AURORA
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Reflection;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This static utility class provides methods to help run Neverwinter Nights
    /// (Aurora Toolset) content in the Dialogue System.
    /// </summary>
    public static class NWNUtility
    {

        /// <summary>
        /// Registers a C# <c>bool NWScript(string scriptName)<c/c> method with the Lua environment.
        /// The Dialogue System doesn't implement NWScript. Instead, it calls a Lua function
        /// named NWScript, passing the name of the original NWScript script as the parameter.
        /// Your method should return a <c>bool</c> value.
        /// </summary>
        /// <param name="target">Object containing the method.</param>
        /// <param name="function">The method Lua should call when it encounters "NWScript()".</param>
        /// <example>If you have a class named MyNWScriptClass with an NWScript()
        /// implementation named MyNWScript, you can register it with this line of code:
        /// <code>NWNUtility.RegisterNWScriptFunction(this, typeof(MyNWScriptClass).GetMethod("MyNWScript"));</code>
        /// Or in WinRT (which doesn't have GetMethod()):
        /// <code>NWNUtility.RegisterNWScriptFunction(this, SymbolExtensions.GetMethodInfo(() => MyNWScript(null))););</code>
        /// </example>
        public static void RegisterNWScriptFunction(object target, MethodInfo function)
        {
            Lua.RegisterFunction("NWScript", target, function);
        }

    }

}
#endif
