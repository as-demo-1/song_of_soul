using System;
using System.Collections.Generic;
using System.Text;

namespace Language.Lua
{
    public partial class StringLiteral : Term
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            return new LuaString(this.Text);
        }
    }
}
