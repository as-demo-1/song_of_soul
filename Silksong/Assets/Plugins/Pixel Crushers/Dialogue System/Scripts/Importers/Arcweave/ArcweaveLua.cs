#if USE_ARCWEAVE

using UnityEngine;
using Language.Lua;

namespace PixelCrushers.DialogueSystem.ArcweaveSupport
{

    /// <summary>
    /// Implements Arcscript built-in functions for the Dialogue System's Lua environment.
    /// Notes:
    /// - Uses LuaInterpreter.
    /// - Does not implement reset() or resetAll().
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class ArcweaveLua : MonoBehaviour
    {
        [Tooltip("Typically leave unticked so temporary Dialogue Managers don't unregister your functions.")]
        public bool unregisterOnDisable = false;

        void OnEnable()
        {
            Lua.RegisterFunction(nameof(abs), null, SymbolExtensions.GetMethodInfo(() => abs(0)));
            Lua.RegisterFunction(nameof(sqr), null, SymbolExtensions.GetMethodInfo(() => sqr(0)));
            Lua.RegisterFunction(nameof(sqrt), null, SymbolExtensions.GetMethodInfo(() => sqrt(0)));
            Lua.RegisterFunction(nameof(random), null, SymbolExtensions.GetMethodInfo(() => random()));
            Lua.environment.Register(nameof(roll), roll);
            Lua.environment.Register(nameof(show), show);
        }

        void OnDisable()
        {
            if (unregisterOnDisable)
            {
                Lua.UnregisterFunction(nameof(abs));
                Lua.UnregisterFunction(nameof(sqr));
                Lua.UnregisterFunction(nameof(sqrt));
                Lua.UnregisterFunction(nameof(random));
                Lua.UnregisterFunction(nameof(roll));
                Lua.UnregisterFunction(nameof(show));
            }
        }

        public static double abs(double n)
        {
            return Mathf.Abs((float)n);
        }

        public static double sqr(double n)
        {
            return (n * n);
        }

        public static double sqrt(double n)
        {
            return Mathf.Sqrt((float)n);
        }

        public static double random()
        {
            return UnityEngine.Random.value; // Note: Returns 1 inclusive, but Arcscript random() is 1 exclusive.
        }

        public static LuaValue roll(LuaValue[] values)
        {
            int m = (int)(values[0] as LuaNumber).Number;
            int n = (values.Length > 1 && values[1] is LuaNumber) ? (int)(values[1] as LuaNumber).Number : 1;
            double result = 0;
            for (int i = 0; i < (int)n; i++)
            {
                result += UnityEngine.Random.Range(1, (int)m + 1);
            }
            return new LuaNumber(result);
        }

        public static LuaValue show(LuaValue[] values)
        {
            var s = string.Empty;
            foreach (var value in values)
            {
                s += value.ToString();
            }
            return new LuaString(s);
        }

    }
}

#endif
