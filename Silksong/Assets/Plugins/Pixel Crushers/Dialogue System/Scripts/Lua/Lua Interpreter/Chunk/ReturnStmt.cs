using System;
using System.Collections.Generic;
using System.Text;

namespace Language.Lua
{
    public partial class ReturnStmt : Statement
    {
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            throw new NotImplementedException();
        }
    }
}
