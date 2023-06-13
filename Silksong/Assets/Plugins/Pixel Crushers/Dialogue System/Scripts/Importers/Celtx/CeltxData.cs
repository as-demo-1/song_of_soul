#if USE_CELTX
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.Celtx
{
    public class CeltxData
    {
        public List<CeltxCondition> conditions = new List<CeltxCondition>();
        public List<SequenceLinkingData> sequenceLinkingDataList = new List<SequenceLinkingData>();
        public List<string> idsWithIncomingLinks = new List<string>();

        public Dictionary<string, int> actorIdLookupByCxCharacterCatalogId = new Dictionary<string, int>();
        public Dictionary<string, DialogueEntry> dialogueEntryLookupByCeltxId = new Dictionary<string, DialogueEntry>();
        public Dictionary<string, string> variableLookupByCeltxId = new Dictionary<string, string>();
        public Dictionary<string, List<CxCustomVar>> customVarListLookupByCxSequenceId = new Dictionary<string, List<CxCustomVar>>();
    }

    public class SequenceLinkingData
    {
        public string id;
        public string name;
        public List<string> linksProcessed = new List<string>();
        public bool sequenceProcessingComplete;
        public List<string> linkedIds = new List<string>();
        public bool isRoot = false;
    }

    public class CeltxCondition
    {
        public string id;
        public string catalogId;
        public string name;
        public string description;
        public bool delay;
        public string luaConditionString = null;

        public List<CxLiteral> literals;
        public List<CxOnObj> links;
    }
}
#endif