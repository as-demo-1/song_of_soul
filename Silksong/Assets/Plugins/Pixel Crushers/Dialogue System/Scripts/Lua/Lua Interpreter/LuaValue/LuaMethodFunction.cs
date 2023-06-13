using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Language.Lua
{
	/// <summary>
	/// [PixelCrushers] This class wraps the Lua Interpreter's LuaFunction around
	/// a C# style method identified by its MethodInfo.
	/// </summary>
    public class LuaMethodFunction : LuaFunction
    {

		public object Target { get; set; }
		public MethodInfo Method { get; set; }

		public LuaMethodFunction(object target, MethodInfo method) : base(null)
		{
			this.Function = InvokeMethod;
			this.Target = target;
			this.Method = method;
		}
	
		public LuaValue InvokeMethod(LuaValue[] args)
		{
			if (Method == null || args == null) return LuaNil.Nil;
			object[] objectArgs = new object[args.Length];
			for (int i = 0; i < args.Length; i++) {
				objectArgs[i] = LuaInterpreterExtensions.LuaValueToObject(args[i]);
			}
            try
            {
                object result = Method.Invoke(Target, objectArgs);
                return LuaInterpreterExtensions.ObjectToLuaValue(result);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                throw e;
            }
        }

    }
}
