#if USE_CELTX3
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.Celtx
{
    public class CeltxCondition
    {
        public string id;
        public string catalogId;
        public string name;
        public string description;
        public bool delay;
        public string luaConditionString = null;

        public Dictionary<string, CeltxConditionLiteral> literalsLookup = new Dictionary<string, CeltxConditionLiteral>();
    }
    public class CeltxConditionLiteral
    {
        public dynamic literalData;
        public string followingOp;
    }
}
#endif