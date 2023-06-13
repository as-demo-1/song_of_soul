using System;
using System.Collections.Generic;
using System.Text;

namespace Language.Lua
{
    public partial class ParamList
    {
        public List<string> NameList = new List<string>();

        public bool HasVarArg;

        public string IsVarArg;

    }
}
