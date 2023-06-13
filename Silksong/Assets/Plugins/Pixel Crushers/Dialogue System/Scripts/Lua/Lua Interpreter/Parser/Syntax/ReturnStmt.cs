using System;
using System.Collections.Generic;
using System.Text;

namespace Language.Lua
{
    public partial class ReturnStmt : Statement
    {
        public List<Expr> ExprList = new List<Expr>();

    }
}
