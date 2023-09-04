using System;
using System.Collections.Generic;
using System.Text;

namespace Language.Lua
{
    public partial class LocalVar : Statement
    {
        public List<string> NameList = new List<string>();

        public List<Expr> ExprList = new List<Expr>();

    }
}
