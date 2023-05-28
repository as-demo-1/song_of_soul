using System;
using System.Collections.Generic;
using System.Text;

namespace Language.Lua
{
    public partial class FunctionCall : Access
    {
        public override LuaValue Evaluate(LuaValue baseValue, LuaTable enviroment)
        {
            LuaFunction function = baseValue as LuaFunction;

            if (function != null)
            {
				//[PixelCrushers] Removed (not WinRT compatible, and not needed for Dialogue System)
                //if (function.Function.Method.DeclaringType.FullName == "Language.Lua.Library.BaseLib" &&
                //    (function.Function.Method.Name == "loadstring" || function.Function.Method.Name == "dofile"))
                //{
                //    if (this.Args.String != null)
                //    {
                //        return function.Function.Invoke(new LuaValue[] { this.Args.String.Evaluate(enviroment), enviroment });
                //    }
                //    else
                //    {
                //        return function.Function.Invoke(new LuaValue[] { this.Args.ArgList[0].Evaluate(enviroment), enviroment });
                //    }
                //}

                if (this.Args.Table != null)
                {
                    return function.Function.Invoke(new LuaValue[] { this.Args.Table.Evaluate(enviroment) });
                }
                else if (this.Args.String != null)
                {
					return function.Function.Invoke(new LuaValue[] { this.Args.String.Evaluate(enviroment) });
                }
                else
                {
					//[PixelCrushers] Was: List<LuaValue> args = this.Args.ArgList.ConvertAll(arg => arg.Evaluate(enviroment));
					List<LuaValue> args = LuaInterpreterExtensions.EvaluateAll(this.Args.ArgList, enviroment);
					return function.Function.Invoke(LuaMultiValue.UnWrapLuaValues(args.ToArray()));
                }
            }
            else
            {
                //[PixelCrushers Was: throw new Exception("Invoke function call on non function value.");
                throw new Exception("Tried to invoke a function call on a non-function value. If you're calling a function, is it registered with Lua?");
            }
        }
    }
}
