using System;
using System.Collections.Generic;
using System.Text;

namespace Language.Lua
{
    public partial class DoStmt : Statement
    {
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            return this.Body.Execute(enviroment, out isBreak);
        }
    }
}
