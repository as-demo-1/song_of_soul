using System;
using System.Collections.Generic;
using System.Text;

namespace Language.Lua
{
    public partial class NilLiteral : Term
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            return LuaNil.Nil;
        }
    }
}
