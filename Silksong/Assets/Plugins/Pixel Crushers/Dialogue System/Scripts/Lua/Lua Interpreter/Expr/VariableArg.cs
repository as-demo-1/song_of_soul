using System;
using System.Collections.Generic;
using System.Text;

namespace Language.Lua
{
    public partial class VariableArg : Term
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            return enviroment.GetValue(this.Name);
        }
    }
}
