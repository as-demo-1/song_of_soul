using System;
using System.Collections.Generic;
using System.Text;

namespace Language.Lua
{
    public partial class WhileStmt : Statement
    {
        public Expr Condition;

        public Chunk Body;

    }
}
