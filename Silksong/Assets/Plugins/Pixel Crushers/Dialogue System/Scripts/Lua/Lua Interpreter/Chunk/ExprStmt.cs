using System;
using System.Collections.Generic;
using System.Text;

namespace Language.Lua
{
    public partial class ExprStmt : Statement
    {
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            this.Expr.Evaluate(enviroment);
            isBreak = false;
            return null;
        }
    }
}
