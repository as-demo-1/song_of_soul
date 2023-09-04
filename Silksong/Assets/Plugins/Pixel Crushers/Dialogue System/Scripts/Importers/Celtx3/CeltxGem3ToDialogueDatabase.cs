#if USE_CELTX3
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.Celtx
{
    /// <summary>
    /// This class does the actual work of converting Celtx Data (Raw) into a dialogue database.
    /// </summary>
    public class CeltxGem3ToDialogueDatabase
    {
        Template template = Template.FromDefault();
        DialogueDatabase database;
        dynamic celtxDataObject;
        bool importGameplayAsEmptyNodes;

        Dictionary<string, Actor> actorLookupViaCeltxId = new Dictionary<string, Actor>();
        Dictionary<string, Item> itemLookupViaCeltxId = new Dictionary<string, Item>();
        Dictionary<string, Location> locationLookupViaCeltxId = new Dictionary<string, Location>();
        Dictionary<string, Variable> variableLookupViaCeltxId = new Dictionary<string, Variable>();
        Dictionary<string, DialogueEntry> portalsPendingLinks = new Dictionary<string, DialogueEntry>();
        Dictionary<string, dynamic> radioVarOptionsLookupViaCeltxId = new Dictionary<string, dynamic>();
        Dictionary<string, CeltxCondition> celtxConditionLookupViaCeltxId = new Dictionary<string, CeltxCondition>();
        Dictionary<string, string> catalogTypeByBreakdownId = new Dictionary<string, string>();
        Dictionary<string, string> catalogIdByBreakdownId = new Dictionary<string, string>();
        Dictionary<string, string> customNameByBreakdownId = new Dictionary<string, string>();
        Dictionary<string, string> customTypeByBreakdownId = new Dictionary<string, string>();

        public DialogueDatabase ProcessCeltxGem3DataObject(dynamic celtxDataObject, DialogueDatabase database, bool importGameplayAsEmptyNodes, bool importGameplayScriptText, bool importBreakdownCatalogContent, bool checkSequenceSyntax)
        {
            try
            {
                this.database = database;
                this.celtxDataObject = celtxDataObject;
                this.database.version = celtxDataObject.meta.version;
                this.importGameplayAsEmptyNodes = importGameplayAsEmptyNodes;

                GenerateActorsFromCharacters();
                GenerateLocationsFromCatalog();
                GenerateItemFromCatalog();
                ImportCeltxVariables();

                GenerateConditions();

                GenerateConversationsFromLanes();

                if (checkSequenceSyntax) CheckSequenceSyntax();

                ConvertBlankNodesToGroupNodes();
            }
            catch (System.Exception e)
            {
                LogError(MethodBase.GetCurrentMethod(), e);
            }

            return database;
        }

#region General Helper Methods

        private void AppendToField(List<Field> fields, string title, string value, FieldType fieldType)
        {
            string currentFieldValue = Field.LookupValue(fields, title);
            if (value == null || value.Equals("") || currentFieldValue != null && currentFieldValue.Equals(value)) { return; }

            string updatedString;
            if (currentFieldValue == null || currentFieldValue.Equals("") || currentFieldValue.Equals("[]")) { updatedString = value; }
            else { updatedString = currentFieldValue + " " + value; }
            Field.SetValue(fields, title, updatedString, fieldType);
        }

        private DialogueEntry CreateAdditionalEntryForSequence(Conversation conversation, DialogueEntry currentEntry, DialogueEntry initialEntry, int entryCount)
        {
            DialogueEntry newEntry = CreateNextDialogueEntryForConversation(conversation, initialEntry.Title + "-" + entryCount,
                Field.LookupValue(initialEntry.fields, CeltxFields.CeltxId) + "-" + entryCount);
            newEntry.ActorID = currentEntry.ActorID;
            newEntry.ConversantID = currentEntry.ConversantID;
            LinkDialogueEntries(currentEntry, newEntry, null);
            return newEntry;
        }

        private void LinkDialogueEntries(DialogueEntry source, DialogueEntry destination, string conditionId)
        {
            try
            {
                if (conditionId == null)
                {
                    source.outgoingLinks.Add(new Link(source.conversationID, source.id, destination.conversationID, destination.id));
                }
                else
                {
                    CeltxCondition condition = celtxConditionLookupViaCeltxId[conditionId];
                    DialogueEntry entryToLinkToCondition = source;
                    if (condition.delay)
                    {
                        DialogueEntry delayEntry = CreateNextDialogueEntryForConversation(database.GetConversation(source.conversationID),
                            "[D]-" + condition.name,
                            "D-" + Field.LookupValue(source.fields, CeltxFields.CeltxId) + "-" + Field.LookupValue(destination.fields, CeltxFields.CeltxId), true);
                        source.outgoingLinks.Add(new Link(source.conversationID, source.id, delayEntry.conversationID, delayEntry.id));
                        entryToLinkToCondition = delayEntry;
                    }

                    DialogueEntry conditionEntry = CreateNextDialogueEntryForConversation(database.GetConversation(source.conversationID),
                        "[COND]" + condition.name,
                        "C-" + Field.LookupValue(source.fields, CeltxFields.CeltxId) + "-" + Field.LookupValue(destination.fields, CeltxFields.CeltxId), true);
                    conditionEntry.conditionsString = condition.luaConditionString;
                    entryToLinkToCondition.outgoingLinks.Add(new Link(entryToLinkToCondition.conversationID, entryToLinkToCondition.id, conditionEntry.conversationID, conditionEntry.id));
                    conditionEntry.outgoingLinks.Add(new Link(conditionEntry.conversationID, conditionEntry.id, destination.conversationID, destination.id));
                }
            }
            catch (System.Exception e)
            {
                LogError(MethodBase.GetCurrentMethod(), e, source.Title + "-" + destination.Title + "-" + conditionId);
            }
        }

        private DialogueEntry CreateNextDialogueEntryForConversation(Conversation conversation, string title, string celtxId, bool isGroup = false)
        {
            try
            {
                DialogueEntry dialogueEntry = template.CreateDialogueEntry(template.GetNextDialogueEntryID(conversation), conversation.id, title);
                conversation.dialogueEntries.Add(dialogueEntry);
                dialogueEntry.isGroup = isGroup;
                dialogueEntry.ActorID = conversation.ActorID;
                dialogueEntry.ConversantID = conversation.ConversantID;
                if (celtxId != null)
                {
                    Field.SetValue(dialogueEntry.fields, CeltxFields.CeltxId, celtxId);
                    //celtxData.dialogueEntryLookupByCeltxId.Add(celtxId, dialogueEntry);
                }
                return dialogueEntry;
            }
            catch (System.Exception e)
            {
                LogError(MethodBase.GetCurrentMethod(), e, celtxId, title);
            }
            return null;
        }

        /// <summary>
        /// Check syntax of all sequences in all dialogue entries in database.
        /// Log warning for any entries with syntax errors.
        /// </summary>
        private void CheckSequenceSyntax()
        {
            if (database == null) return;
            var parser = new SequenceParser();
            foreach (Conversation conversation in database.conversations)
            {
                foreach (DialogueEntry entry in conversation.dialogueEntries)
                {
                    var sequence = entry.Sequence;
                    if (string.IsNullOrEmpty(sequence)) continue;
                    var result = parser.Parse(sequence);
                    if (result == null || result.Count == 0)
                    {
                        var text = entry.Title;
                        if (string.IsNullOrEmpty(text)) text = entry.subtitleText;
                        LogWarning("Dialogue entry " + conversation.id + ":" + entry.id +
                            " in conversation '" + conversation.Title + "' has syntax error in [SEQ] Sequence: " + sequence,
                            Field.LookupValue(entry.fields, "Celtx ID"), entry.Title);
                    }
                }
            }
        }

        private void ConvertBlankNodesToGroupNodes()
        {
            foreach (Conversation conversation in database.conversations)
            {
                foreach (DialogueEntry entry in conversation.dialogueEntries)
                {
                    var isBlankNode = entry.id != 0 && // <START> nodes can't be group nodes.
                        string.IsNullOrEmpty(entry.DialogueText) &&
                        string.IsNullOrEmpty(entry.MenuText) &&
                        string.IsNullOrEmpty(entry.Sequence);
                    if (isBlankNode)
                    {
                        entry.isGroup = true;
                    }
                }
            }
        }
#endregion

#region Non-Dialogue Processing
        private void GenerateActorsFromCharacters()
        {
            var characters = celtxDataObject.subdocuments.catalog.character;
            foreach (var characterObject in characters)
            {
                var characterData = characterObject.Value;
                string characterDesc = (string)characterData.desc;
                bool isPlayer = false;
                if (StringStartsWithTag(characterDesc, "[PLAYER]"))
                {
                    isPlayer = true;
                }
                Actor actor = database.GetActor((string)characterData.name);
                if (actor == null)
                {
                    actor = template.CreateActor(template.GetNextActorID(database), (string)characterData.name, isPlayer);
                    AppendToField(actor.fields, CeltxFields.CeltxId, (string)characterData.id, FieldType.Text);
                    AppendToField(actor.fields, DialogueSystemFields.Description, GetTextWithoutTag(characterDesc), FieldType.Text);
                    database.actors.Add(actor);
                }
                else
                {
                    var originalActor = (database.syncInfo.syncActors && database.syncInfo.syncActorsDatabase != null)
                        ? database.syncInfo.syncActorsDatabase.GetActor(actor.Name) : null;
                    if (originalActor != null)
                    {
                        AppendToField(actor.fields, CeltxFields.CeltxId, (string)characterData.id, FieldType.Text);
                        AppendToField(actor.fields, DialogueSystemFields.Description, GetTextWithoutTag(characterDesc), FieldType.Text);
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(database.syncInfo.syncActorsDatabase);
#endif
                    }
                }
                AppendToField(actor.fields, CeltxFields.Pictures, GetPictureString(characterData), FieldType.Files);
                actorLookupViaCeltxId[(string)characterData.id] = actor;
            }
        }

        private string GetPictureString(dynamic catalogItemAttrs)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("[");
                if (catalogItemAttrs.item_data.media != null && catalogItemAttrs.item_data.media.Count > 0)
                {
                    var mediaCount = catalogItemAttrs.item_data.media.Count;
                    for (int i = 0; i < mediaCount; i++)
                    {
                        var media = catalogItemAttrs.item_data.media[i];
                        stringBuilder.Append(media.name);

                        if (i == mediaCount - 1)
                        {
                            stringBuilder.Append("]");
                        }
                        else
                        {
                            stringBuilder.Append(";");
                        }
                    }
                    return stringBuilder.ToString();
                }
                else
                {
                    return "[]";
                }
            }
            catch (System.Exception e)
            {
                LogError(MethodBase.GetCurrentMethod(), e, catalogItemAttrs.id, catalogItemAttrs.title);
            }
            return "[]";
        }

        private void GenerateLocationsFromCatalog()
        {
            var locations = celtxDataObject.subdocuments.catalog.location;
            foreach (var locationObject in locations)
            {
                var locationData = locationObject.Value;
                Location location = database.GetLocation((string)locationData.name);
                if (location == null)
                {
                    location = template.CreateLocation(template.GetNextLocationID(database), (string)locationData.name);
                    AppendToField(location.fields, CeltxFields.CeltxId, (string)locationData.id, FieldType.Text);
                    AppendToField(location.fields, DialogueSystemFields.Description, (string)locationData.desc, FieldType.Text);
                    database.locations.Add(location);
                }
                else
                {
                    var originalLocation = (database.syncInfo.syncLocations && database.syncInfo.syncLocationsDatabase != null)
                        ? database.syncInfo.syncLocationsDatabase.GetLocation(location.Name) : null;
                    if (originalLocation != null)
                    {
                        AppendToField(location.fields, CeltxFields.CeltxId, (string)locationData.id, FieldType.Text);
                        AppendToField(location.fields, DialogueSystemFields.Description, (string)locationData.desc, FieldType.Text);
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(database.syncInfo.syncLocationsDatabase);
#endif
                    }
                    locationLookupViaCeltxId[(string)locationData.id] = location;
                }
            }
        }

        private void GenerateItemFromCatalog()
        {
            var items = celtxDataObject.subdocuments.catalog.item;
            foreach (var itemObject in items)
            {
                var itemData = itemObject.Value;
                Item item = database.GetItem((string)itemData.name);
                if (item == null)
                {
                    item = template.CreateItem(template.GetNextItemID(database), (string)itemData.name);
                    AppendToField(item.fields, CeltxFields.CeltxId, (string)itemData.id, FieldType.Text);
                    AppendToField(item.fields, DialogueSystemFields.Description, (string)itemData.desc, FieldType.Text);
                    database.items.Add(item);
                }
                else
                {
                    var originalItem = (database.syncInfo.syncItems && database.syncInfo.syncItemsDatabase != null)
                        ? database.syncInfo.syncItemsDatabase.GetItem(item.Name) : null;
                    if (originalItem != null)
                    {
                        AppendToField(item.fields, CeltxFields.CeltxId, (string)itemData.id, FieldType.Text);
                        AppendToField(item.fields, DialogueSystemFields.Description, (string)itemData.desc, FieldType.Text);
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(database.syncInfo.syncItemsDatabase);
#endif
                    }
                }
                itemLookupViaCeltxId[(string)itemData.id] = item;
            }
        }

        private void ImportCeltxVariables()
        {
            foreach (var variableObject in celtxDataObject.variables)
            {
                var variableData = variableObject.Value;
                string variableDefaultValue = "";
                FieldType fieldType = FieldType.Text;
                var variableType = (string)variableData.type;

                switch (variableType)
                {
                    case "boolean":
                        fieldType = FieldType.Boolean;

                        variableDefaultValue = (string)variableData.config.defaultValue;
                        break;

                    case "number":
                    case "range":
                        fieldType = FieldType.Number;

                        variableDefaultValue = (string)variableData.config.defaultValue;
                        break;

                    case "date":
                    case "text":
                    case "textarea":
                    case "time":
                        fieldType = FieldType.Text;

                        variableDefaultValue = (string)variableData.config.defaultValue;
                        break;

                    case "radio":
                        fieldType = FieldType.Text;

                        variableDefaultValue = (string)variableData.config.options[(int)variableData.config.defaultValue].value;
                        radioVarOptionsLookupViaCeltxId.Add((string)variableData.id, variableData.config.options);
                        break;
                }

                var variable = database.GetVariable((string)variableData.name);
                if (variable == null)
                {
                    int variableId = template.GetNextVariableID(database);
                    variable = template.CreateVariable(variableId, (string)variableData.name, variableDefaultValue, fieldType);
                    database.variables.Add(variable);
                }
                else
                {
                    var originalVariable = (database.syncInfo.syncVariables && database.syncInfo.syncVariablesDatabase != null)
                        ? database.syncInfo.syncVariablesDatabase.GetVariable(variableObject.name) : null;
                    if (originalVariable != null)
                    {
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(database.syncInfo.syncVariablesDatabase);
#endif
                    }
                }

                AppendToField(variable.fields, CeltxFields.Description, (string)variableData.desc, FieldType.Text);
                variableLookupViaCeltxId.Add((string)variableData.id, variable);
            }
        }
#endregion

#region Condition Processing

        private void GenerateConditions()
        {
            foreach (var conditionObject in celtxDataObject.conditions)
            {
                var conditionData = conditionObject.Value;
                var conditionId = (string)conditionData.id;
                var conditionName = (string)conditionData.name;
                var conditionDesc = (string)conditionData.desc;
                var conditionClause = (string)conditionData.clause;

                var conditionLiterals = conditionData.literals;

                CeltxCondition celtxCondition = new CeltxCondition();
                celtxCondition.id = conditionId;
                celtxCondition.name = conditionName;
                celtxCondition.description = conditionDesc;

                if (conditionDesc != null)
                {
                    celtxCondition.delay = conditionDesc.Contains("[D]");
                }

                if (celtxCondition.delay) celtxCondition.description = conditionDesc.Substring(0, "[D]".Length);

                foreach (var literalObject in conditionLiterals)
                {
                    CeltxConditionLiteral literal = new CeltxConditionLiteral();
                    literal.literalData = literalObject.Value;
                    celtxCondition.literalsLookup.Add(literalObject.Name, literal);
                }
                celtxConditionLookupViaCeltxId.Add(conditionId, celtxCondition);

                SetFollowingOperators(celtxCondition, conditionClause);
            }

            foreach (var celtxCondition in celtxConditionLookupViaCeltxId.Values)
            {
                GenerateLuaCondition(celtxCondition);
            }
        }

        // Split the condition clause to note whether each literal is followed by "and" or "or"
        private void SetFollowingOperators(CeltxCondition condition, string clause)
        {
            try
            {
                List<string> subSections = new List<string>();
                List<string> orSplit = new List<string>();
                subSections.AddRange(System.Text.RegularExpressions.Regex.Split(clause, "(\\+|\\.)"));

                if (subSections.ElementAt(subSections.Count - 1).Equals("")) { subSections.RemoveAt(subSections.Count - 1); }

                for (int i = 0; i < subSections.Count - 1; i += 2)
                {
                    CeltxConditionLiteral literal = condition.literalsLookup[subSections.ElementAt(i)];
                    literal.followingOp = subSections.ElementAt(i + 1);
                }
            }
            catch (System.Exception e)
            {
                LogError(MethodBase.GetCurrentMethod(), e, condition.id, condition.name);
            }
        }

        private void GenerateLuaCondition(CeltxCondition condition)
        {
            try
            {
                if (condition.literalsLookup.Count == 0)
                {
                    LogMessage(LogLevel.W,
                            "Condition has no literals(literals list is null). Lua script generation will be skipped for this condition. Please add literals to the condition in Celtx", condition.id);
                    return;
                }

                StringBuilder conditionString = new StringBuilder();
                string followingOp = null;
                foreach (string literalKey in condition.literalsLookup.Keys)
                {
                    if (followingOp != null) { conditionString.Append(" " + followingOp + " "); }

                    CeltxConditionLiteral conditionLiteral = condition.literalsLookup[literalKey];
                    var literal = conditionLiteral.literalData;
                    var literalType = (string)literal.type;

                    if (literalType.Equals("variable"))
                    {
                        var variableId = (string)literal.id;
                        Variable variable = variableLookupViaCeltxId[variableId];

                        conditionString.Append("Variable");
                        conditionString.Append("[\"" + variable.Name + "\"]");

                        string comparisonVal;

                        comparisonVal = (string)literal.comparisonValue.value;

                        Variable var = variableLookupViaCeltxId[variableId];

                        if (comparisonVal.Equals("Any"))
                        {
                            conditionString.Append(" ~= nil");
                        }
                        else
                        {
                            conditionString.Append(" " + GetLuaComparisonOperator((string)literal.comparisonOperator) + " ");

                            if (var.Type == FieldType.Text)
                            {
                                conditionString.Append("\"" + comparisonVal + "\"");
                            }
                            else
                            {
                                conditionString.Append(comparisonVal);
                            }
                        }

                    }
                    else if (literalType.Equals("condition"))
                    {
                        conditionString.Append("(");
                        conditionString.Append(GetNestedConditionString((string)literal.conditionId));
                        conditionString.Append(") == ");
                        conditionString.Append(literal.comparisonValue.value);
                    }
                    else
                    {
                        var predicate = (string)literal.predicate;
                        if (predicate.Equals("exists"))
                        {
                            conditionString.Append("exists(\"");
                        }
                        else
                        {
                            conditionString.Append("not exists(");
                        }
                        conditionString.Append((string)literal.name);
                        conditionString.Append("\")");
                    }

                    if (conditionLiteral.followingOp.Equals(".")) { followingOp = "and"; }
                    else if (conditionLiteral.followingOp.Equals("+")) { followingOp = "or"; }
                    else
                    {
                        LogMessage(LogLevel.W,
                            "Unsupported following operator(" + conditionLiteral.followingOp + ") in condition. Only \".\"(AND) and \"+\"(OR) are supported", condition.id + "-" + (string)literalKey);
                        continue;
                    }
                }
                condition.luaConditionString = conditionString.ToString();
            }
            catch (System.Exception e)
            {
                LogError(MethodBase.GetCurrentMethod(), e, condition.id, condition.name);
            }
        }

        private string GetNestedConditionString(string conditionId)
        {
            try
            {
                CeltxCondition condition = celtxConditionLookupViaCeltxId[conditionId];
                if (condition.luaConditionString == null)
                {
                    GenerateLuaCondition(condition);
                }

                return condition.luaConditionString;
            }
            catch (System.Exception e)
            {
                LogError(MethodBase.GetCurrentMethod(), e, conditionId);
            }
            return null;
        }

        private string GetLuaComparisonOperator(string literalOp)
        {
            switch (literalOp)
            {
                case "<":
                case "<=":
                case ">":
                case ">=":
                    return literalOp;

                case "=":
                    return "==";

                case "!=":
                    return "~=";

                case "exists":
                    LogMessage(LogLevel.W,
                        "Literal operator(" + literalOp + ") not supported. Defaulting to \"==\"");
                    return "==";

                case "does not exist":
                    LogMessage(LogLevel.W,
                        "Literal operator(" + literalOp + ") not supported. Defaulting to \"~=\"");
                    return "~=";

                default:
                    LogMessage(LogLevel.W,
                        "Unknown comparison operator(" + literalOp + ") in literal. Defaulting to \"==\"");
                    return "==";
            }
        }

#endregion

#region LaneProcessing
        private void GenerateConversationsFromLanes()
        {
            foreach (var laneObject in celtxDataObject.lanes)
            {
                var laneData = laneObject.Value;
                //Debug.Log(laneData);
                //Debug.Log(laneData.name + " - " + laneData.id + laneData.desc);

                string conversationName = CreateHierarchicalConversationName(laneData);

                // Create the conversation for the lane
                Conversation conversation = template.CreateConversation(template.GetNextConversationID(database), conversationName);
                conversation.Description = laneData.desc;
                AppendToField(conversation.fields, "CeltxId", (string)laneData.id, FieldType.Text);
                database.conversations.Add(conversation);

                // Get the nodes and edges for the lane
                var laneSubdocument = celtxDataObject.subdocuments[(string)laneData.id];

                GenerateDialogueEntriesAndLinks(conversation, laneSubdocument);
            }
        }

        private string CreateHierarchicalConversationName(dynamic laneData)
        {
            // Get name hierarchy for lane name
            StringBuilder nameBuilder = new StringBuilder();
            var laneName = laneData.name;

            nameBuilder.Append(laneName);

            var parentInfo = laneData.parent;

            do
            {
                var parentId = (string)parentInfo.id;
                var parentKind = (string)parentInfo.kind;

                string parentName = "";
                dynamic parentObject = null;

                switch (parentKind)
                {
                    case "episode":
                        var episodes = celtxDataObject.episodes;
                        parentObject = episodes[parentId];
                        parentName = parentObject.name;
                        break;

                    case "scene":
                        var scenes = celtxDataObject.scenes;
                        parentObject = scenes[parentId];
                        parentName = parentObject.name;
                        break;

                    case "mode":
                        var modes = celtxDataObject.modes;
                        parentObject = modes[parentId];
                        parentName = parentObject.name;
                        break;
                }
                nameBuilder.Insert(0, parentName + " / ");
                parentInfo = parentObject.parent;
            }
            while (parentInfo != null);

            return nameBuilder.ToString();
        }

        private void GenerateDialogueEntriesAndLinks(Conversation conversation, dynamic laneSubdocument)
        {
            Dictionary<string, DialogueEntry> dialogueEntryToDict = new Dictionary<string, DialogueEntry>();
            Dictionary<string, DialogueEntry> dialogueEntryFromDict = new Dictionary<string, DialogueEntry>();

            catalogTypeByBreakdownId.Clear();
            catalogIdByBreakdownId.Clear();
            customNameByBreakdownId.Clear();
            customTypeByBreakdownId.Clear();
            foreach (var breakdown in laneSubdocument.breakdowns)
            {
                var breakdownData = breakdown.Value;
                var breakdown_id = breakdownData.id.ToString();
                catalogTypeByBreakdownId[breakdown_id] = breakdownData.type.ToString();
                catalogIdByBreakdownId[breakdown_id] = breakdownData.catalog_id.ToString();
                customNameByBreakdownId[breakdown_id] = breakdownData.name.ToString();
                customTypeByBreakdownId[breakdown_id] = breakdownData.custom_type.ToString();
            }

            var laneNodes = laneSubdocument.nodes;

            bool rootProcessed = false;
            DialogueEntry startEntry = null;

            // Make list of all non-root nodes so we can identify root node:
            HashSet<string> nonRootNodeIds = new HashSet<string>();
            foreach (var edgeObject in laneSubdocument.edges)
            {
                var edgeData = edgeObject.Value;
                var to = (string)edgeData.to;
                nonRootNodeIds.Add(to);
            }

            // Create <START> node:
            startEntry = CreateNextDialogueEntryForConversation(conversation, "START", "");


            // Set conversation's ActorID and ConversantID:
            foreach (var nodeObject in laneNodes)
            {
                var nodeData = nodeObject.Value;
                var nodeId = (string)nodeData.id;
                var nodeName = (string)nodeData.name;

                var scriptObject = GetNodeScriptObject(nodeId, laneSubdocument);

                if (!rootProcessed && !nonRootNodeIds.Contains(nodeId))
                {
                    List<Actor> actors = GetCharactersFromRootPage(scriptObject);
                    if (actors.Count >= 2)
                    {
                        conversation.ActorID = actors[0].id;
                        conversation.ConversantID = actors[1].id;
                    }
                    else if (actors.Count == 1)
                    {
                        conversation.ConversantID = conversation.ActorID = actors[0].id;
                    }
                    else
                    {
                        Debug.LogWarning("Node " + nodeName + " with id " + nodeId + " has no actors.");
                    }
                    startEntry.ActorID = conversation.ActorID;
                    startEntry.ConversantID = conversation.ConversantID;

                    Field.SetValue(startEntry.fields, CeltxFields.CeltxId, (string)nodeId);
                }
            }


            // Create dialogue entries and links for the nodes and edges
            foreach (var nodeObject in laneNodes)
            {
                var nodeData = nodeObject.Value;
                var nodeId = (string)nodeData.id;
                var nodeName = (string)nodeData.name;
                var nodeDesc = (string)nodeData.desc;
                var nodeType = (string)nodeData.type;

                DialogueEntry lastEntry = null;

                var scriptObject = GetNodeScriptObject(nodeId, laneSubdocument);

                if (!rootProcessed && !nonRootNodeIds.Contains(nodeId))
                {
                    List<Actor> actors = GetCharactersFromRootPage(scriptObject);
                    if (actors.Count >= 2)
                    {
                        conversation.ActorID = actors[0].id;
                        conversation.ConversantID = actors[1].id;
                    }
                    else if (actors.Count == 1)
                    {
                        conversation.ConversantID = conversation.ActorID = actors[0].id;
                    }
                    else
                    {
                        Debug.LogWarning("Node " + nodeName + " with id " + nodeId + " has no actors.");
                    }
                    startEntry.ActorID = conversation.ActorID;
                    startEntry.ConversantID = conversation.ConversantID;

                    Field.SetValue(startEntry.fields, CeltxFields.CeltxId, (string)nodeId);
                    AppendToField(startEntry.fields, DialogueSystemFields.Description, (string)nodeDesc, FieldType.Text);
                }

                lastEntry = CreateNextDialogueEntryForConversation(conversation, nodeName, (string)nodeId);
                AppendToField(lastEntry.fields, DialogueSystemFields.Description, (string)nodeDesc, FieldType.Text);

                // Set variables
                AddSetVariableScriptForNode(lastEntry, nodeData.customVars);

                if (!rootProcessed && !nonRootNodeIds.Contains(nodeId))
                {
                    startEntry.outgoingLinks.Add(new Link(startEntry.conversationID, startEntry.id, lastEntry.conversationID, lastEntry.id));
                    dialogueEntryFromDict.Add(nodeId, lastEntry);
                    dialogueEntryToDict.Add(nodeId, lastEntry);

                    lastEntry.isGroup = true;

                    rootProcessed = true;
                }
                else
                {
                    // When a node contains a script (the node is a sequence), linking to the node will get the first dialogueEntry created from the node
                    // Likewise, linking from the node will get the last dialogueEntry created from the node
                    dialogueEntryToDict.Add(nodeId, lastEntry);

                    if (scriptObject != null)
                    {
                        lastEntry = ProcessSequenceNodePage(conversation, lastEntry, scriptObject);
                    }

                    dialogueEntryFromDict.Add(nodeId, lastEntry);
                }

                // Portals
                if (nodeType.Equals("portal"))
                {
                    ProcessPortalNode(nodeData, nodeId, lastEntry);
                }
            }

            LinkDialogueEntriesFromEdges(laneSubdocument.edges, dialogueEntryFromDict, dialogueEntryToDict);
        }

        private void ProcessPortalNode(dynamic nodeData, string nodeId, DialogueEntry lastEntry)
        {
            var portalRef = (string)nodeData.portalRef;
            if (portalsPendingLinks.ContainsKey(portalRef))
            {
                DialogueEntry portalForRef = portalsPendingLinks[portalRef];

                var currentPortalType = (string)nodeData.portalType;
                DialogueEntry fromDialogueObject;
                DialogueEntry toDialogueObject;
                if (currentPortalType.Equals("to"))
                {
                    toDialogueObject = lastEntry;
                    fromDialogueObject = portalForRef;
                }
                else
                {
                    toDialogueObject = portalForRef;
                    fromDialogueObject = lastEntry;
                }
                fromDialogueObject.outgoingLinks.Add(new Link(fromDialogueObject.conversationID, fromDialogueObject.id, toDialogueObject.conversationID, toDialogueObject.id));
                portalsPendingLinks.Remove(portalRef);
            }
            else
            {
                portalsPendingLinks.Add(nodeId, lastEntry);
            }
        }

        private void LinkDialogueEntriesFromEdges(dynamic laneEdges, Dictionary<string, DialogueEntry> dialogueEntryFromDict, Dictionary<string, DialogueEntry> dialogueEntryToDict)
        {
            foreach (var edgeObject in laneEdges)
            {
                var edgeData = edgeObject.Value;
                var to = (string)edgeData.to;
                var from = (string)edgeData.from;
                string conditionId = null;
                try
                {
                    conditionId = (string)edgeData.condition;
                }
                catch (System.Exception)
                {
                    conditionId = null;
                }

                DialogueEntry toDialogueObject, fromDialogueObject;
                if (!dialogueEntryToDict.TryGetValue(to, out toDialogueObject))
                {
                    Debug.LogWarning($"LinkDialogueEntriesFromEdges cannot locate destination entry {to}.");
                    continue;
                }
                if (!dialogueEntryFromDict.TryGetValue(from, out fromDialogueObject))
                {
                    Debug.LogWarning($"LinkDialogueEntriesFromEdges cannot locate origin entry {from}.");
                    continue;
                }

                LinkDialogueEntries(fromDialogueObject, toDialogueObject, conditionId);
            }
        }

        private DialogueEntry ProcessSequenceNodePage(Conversation conversation, DialogueEntry initialNodeEntry, dynamic scriptObject)
        {
            DialogueEntry currentEntry = initialNodeEntry;
            DialogueEntry breakdownEntry = currentEntry;
            bool createdGameplayEntry = false;

            try
            {
                var pageContents = scriptObject.content[0].content[0].content;
                int entryCount = 1;
                bool createNewEntry = false;

                foreach (var contentObject in pageContents)
                {
                    string contentType = GetContentType(contentObject);
                    if (!(contentType.Equals("cxgameplay") ||
                        contentType.Equals("cxcharacter") ||
                        contentType.Equals("cxparenthetical") ||
                        contentType.Equals("cxdialog") ||
                        contentType.Equals("cxcharacter_item") ||
                        contentType.Equals("cxdirective")))
                    {
                        Debug.LogWarning("Skipping content with type: " + contentType + " for node " + initialNodeEntry.Title);
                        continue;
                    }

                    if (importGameplayAsEmptyNodes && contentType == "cxgameplay")
                    {
                        var firstText = string.Empty;
                        foreach (var content in contentObject.content)
                        {
                            if (content.type == "text")
                            {
                                firstText = content.text;
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(firstText) &&
                            !(firstText.StartsWith("[SEQ]", System.StringComparison.OrdinalIgnoreCase) ||
                              firstText.StartsWith("[COND]", System.StringComparison.OrdinalIgnoreCase) ||
                              firstText.StartsWith("[SCRIPT]", System.StringComparison.OrdinalIgnoreCase)))
                        {
                            entryCount++;
                            DialogueEntry newEntry = CreateNextDialogueEntryForConversation(conversation, string.Empty, (string)contentObject.attrs.id, true);                            
                            newEntry.ActorID = currentEntry.ActorID;
                            newEntry.ConversantID = currentEntry.ConversantID;
                            newEntry.Title = GetContentText(contentObject, currentEntry, null);
                            LinkDialogueEntries(currentEntry, newEntry, null);
                            currentEntry = newEntry;
                            breakdownEntry = currentEntry;
                            createdGameplayEntry = true;
                            createNewEntry = true;
                        }
                    }

                    if (createNewEntry)
                    {
                        entryCount++;
                        currentEntry = CreateAdditionalEntryForSequence(conversation, currentEntry, initialNodeEntry, entryCount);
                        createNewEntry = false;
                        if (!createdGameplayEntry) breakdownEntry = currentEntry;
                    }

                    string id = contentObject.attrs.id;
                    Field.SetValue(currentEntry.fields, CeltxFields.CeltxId, id);

                    createdGameplayEntry = false;

                    string combinedText = GetContentText(contentObject, currentEntry, breakdownEntry); // Note: May add fields to currentEntry and/or breakdownEntry.

                    switch (contentType)
                    {
                        case "cxgameplay":
                            if (StringStartsWithTag(combinedText, "[SEQ]"))
                            {
                                currentEntry.Sequence = GetTextWithoutTag(combinedText);
                            }
                            else if (StringStartsWithTag(combinedText, "[COND]"))
                            {
                                if (!string.IsNullOrEmpty(currentEntry.conditionsString)) currentEntry.conditionsString += ";\n";
                                currentEntry.conditionsString += GetTextWithoutTag(combinedText);
                            }
                            else if (StringStartsWithTag(combinedText, "[SCRIPT]"))
                            {
                                if (!string.IsNullOrEmpty(currentEntry.userScript)) currentEntry.userScript += ";\n";
                                currentEntry.userScript += GetTextWithoutTag(combinedText);
                            }
                            else
                            {
                                currentEntry.Title = combinedText;
                            }
                            break;

                        case "cxdirective":
                            if (StringStartsWithTag(combinedText, "[COND]"))
                            {
                                if (!string.IsNullOrEmpty(currentEntry.conditionsString)) currentEntry.conditionsString += ";\n";
                                currentEntry.conditionsString += GetTextWithoutTag(combinedText);
                            }
                            else if (StringStartsWithTag(combinedText, "[SCRIPT]"))
                            {
                                if (!string.IsNullOrEmpty(currentEntry.userScript)) currentEntry.userScript += ";\n";
                                currentEntry.userScript += GetTextWithoutTag(combinedText);
                            }
                            else
                            {
                                currentEntry.Sequence = combinedText;
                            }
                            break;

                        case "cxcharacter_item":
                            currentEntry.ActorID = GetActorForCharacterItemContent(contentObject).id;
                            if (currentEntry.ActorID == conversation.ActorID)
                            {
                                currentEntry.ConversantID = conversation.ConversantID;
                            }
                            else
                            {
                                currentEntry.ConversantID = conversation.ActorID;
                            }

                            break;

                        case "cxparenthetical":
                            if (StringStartsWithTag(combinedText, "[C]"))
                            {
                                currentEntry.ConversantID = database.GetActor(GetTextWithoutTag(combinedText).ToUpper()).id;
                            }
                            else if (StringStartsWithTag(combinedText, "[VO]"))
                            {
                                AppendToField(currentEntry.fields, CeltxFields.VoiceOverFile, GetTextWithoutTag(combinedText), FieldType.Files);
                            }
                            else
                            {
                                currentEntry.MenuText = combinedText;
                            }
                            break;

                        case "cxdialog":
                            currentEntry.DialogueText = combinedText;

                            createNewEntry = true;
                            break;
                    }

                    breakdownEntry = currentEntry;
                }

            }
            catch (System.Exception e)
            {
                LogError(MethodBase.GetCurrentMethod(), e, initialNodeEntry.Title);
            }

            return currentEntry;
        }

        private void AddSetVariableScriptForNode(DialogueEntry entry, dynamic customVars)
        {
            StringBuilder luaScript = new StringBuilder();

            foreach (var customVar in customVars)
            {
                var variableId = (string)customVar.id;
                var newValue = customVar.value;

                if (luaScript.Length != 0) { luaScript.Append("; "); }

                if (!variableLookupViaCeltxId.ContainsKey(variableId)) Debug.LogError("Celtx Import: Can't find variable with ID " + variableId);

                Variable var = variableLookupViaCeltxId[variableId];

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("Variable");
                stringBuilder.Append("[\"" + var.Name + "\"]");
                stringBuilder.Append(" = ");

                if (var.Type == FieldType.Boolean)
                {
                    stringBuilder.Append(((string)newValue).ToLower());
                }
                else if (var.Type == FieldType.Number)
                {
                    stringBuilder.Append((string)newValue);
                }
                else if (var.Type == FieldType.Text)
                {
                    if (radioVarOptionsLookupViaCeltxId.ContainsKey(variableId))
                    {
                        stringBuilder.Append(radioVarOptionsLookupViaCeltxId[variableId][(int)newValue].value);
                    }
                    else
                    {
                        stringBuilder.Append("\"" + (string)newValue + "\"");
                    }
                }
                luaScript.Append(stringBuilder.ToString());
            }
            entry.userScript = luaScript.ToString();
        }

        private List<Actor> GetCharactersFromRootPage(dynamic scriptObject)
        {
            List<Actor> actors = new List<Actor>();
            dynamic pageContent = null;
            if (scriptObject != null)
            {
                foreach (var contentLv0 in scriptObject.content)
                {
                    if (contentLv0 != null)
                    {
                        foreach (var contentLv1 in contentLv0.content)
                        {
                            if (contentLv1 != null)
                            {
                                pageContent = contentLv1.content;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            if (pageContent != null)
            {
                foreach (var contentObject in pageContent)
                {
                    if (contentObject == null) continue;
                    if (GetContentType(contentObject).Equals("cxcharacter_item"))
                    {
                        actors.Add(actorLookupViaCeltxId[(string)contentObject.attrs.catalog_id]);
                    }
                }
            }
            return actors;
        }

        private string GetContentType(dynamic contentObject)
        {
            return (string)contentObject.type;
        }

        private dynamic GetNodeScriptObject(string nodeId, dynamic laneSubdocument)
        {
            return laneSubdocument.scripts[nodeId];
        }

        private Actor GetActorForCharacterItemContent(dynamic characterObject)
        {
            var characterCatalogId = characterObject.attrs.catalog_id;
            return actorLookupViaCeltxId[(string)characterCatalogId];
        }
#endregion

#region Text Processing

        private bool StringStartsWithTag(string stringToCheck, string targetTag)
        {
            if (stringToCheck == null || targetTag == null) { return false; }

            return stringToCheck.StartsWith(targetTag, System.StringComparison.OrdinalIgnoreCase);
        }

        private string GetTextWithoutTag(string text)
        {
            if (string.IsNullOrEmpty(text)) { return ""; }

            //return System.Text.RegularExpressions.Regex.Split(text, "(\\[.*\\])")[2].Trim();
            var endTagPos = text.IndexOf(']');
            return (endTagPos == -1) ? "" : text.Substring(endTagPos + 1);
        }

        private string GetContentText(dynamic contentObject, DialogueEntry currentEntry, DialogueEntry breakdownEntry)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (contentObject != null && contentObject.content != null)
            {
                foreach (var subContentObject in contentObject.content)
                {
                    if (GetContentType(subContentObject).Equals("text"))
                    {
                        stringBuilder.Append(GetMarkedText(subContentObject, currentEntry, breakdownEntry));
                    }
                }
            }

            return stringBuilder.ToString();
        }

        private string GetMarkedText(dynamic textContent, DialogueEntry currentEntry, DialogueEntry breakdownEntry)
        {
            try
            {
                string text = textContent.text;
                if (textContent.marks != null && textContent.marks.Count > 0)
                {
                    string prependString = "";
                    string appendString = "";
                    foreach (var mark in textContent.marks)
                    {
                        string markType = (string)mark.type;
                        switch (markType)
                        {
                            case "strong":
                                prependString = prependString + "<b>";
                                appendString = "</b>" + appendString;
                                break;

                            case "em":
                                prependString = prependString + "<i>";
                                appendString = "</i>" + appendString;
                                break;

                            case "underline":
                                prependString = prependString + "<u>";
                                appendString = "</u>" + appendString;
                                break;
                            case "strikethrough":
                                prependString = prependString + "<s>";
                                appendString = "</s>" + appendString;
                                break;
                            case "cxbreakdown":
                                if (breakdownEntry != null)
                                {
                                    var breakdown_attrs = mark.attrs;
                                    string attrName = breakdown_attrs.name;
                                    string type = (string)breakdown_attrs.type;
                                    string catalog_id = (string)breakdown_attrs.catalog_id;
                                    AddBreakdownField(currentEntry, breakdownEntry, mark.id, type, catalog_id, attrName);
                                }
                                break;
                            case "cxmultitagbreakdown":
                                if (breakdownEntry != null)
                                {
                                    var multitagbreakdown_attrs = mark.attrs;
                                    foreach (var asset in multitagbreakdown_attrs.assetList)
                                    {
                                        string attrName = multitagbreakdown_attrs.name;
                                        string breakdown_id = (string)asset;
                                        string multitag_type;
                                        if (catalogTypeByBreakdownId.TryGetValue(breakdown_id, out multitag_type))
                                        {
                                            string multitag_catalog_id;
                                            if (catalogIdByBreakdownId.TryGetValue(breakdown_id, out multitag_catalog_id))
                                            {
                                                AddBreakdownField(currentEntry, breakdownEntry, breakdown_id, multitag_type, multitag_catalog_id, attrName);
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    return prependString + text + appendString;
                }
                else
                {
                    return text;
                }
            }
            catch (System.Exception e)
            {
                LogError(MethodBase.GetCurrentMethod(), e, textContent.attrs.id);
                return "";
            }
        }

        private void AddBreakdownField(DialogueEntry currentEntry, DialogueEntry breakdownEntry, string breakdown_id,
            string type, string catalog_id, string attrName)
        {
            switch (type)
            {
                case "character":
                    AddActorBreakdownField(currentEntry, catalog_id);
                    break;
                case "item":
                    AddItemBreakdownField(breakdownEntry, catalog_id);
                    break;
                case "location":
                    AddLocationBreakdownField(breakdownEntry, catalog_id);
                    break;
                default:
                    AddCustomBreakdownField(breakdownEntry, breakdown_id, catalog_id);
                    break;
            }
        }

        private void AddToField(List<Field> fields, string fieldTitle, string value, FieldType fieldType)
        {
            Field field = Field.Lookup(fields, fieldTitle);
            if (field != null)
            {
                if (string.IsNullOrEmpty(field.value))
                {
                    field.value = value;
                }
                else
                {
                    if (value.Contains(';')) Debug.LogWarning($"Appending '{value}' to {fieldTitle} using ';' as separator, but '{value}' also contains ';'");
                    field.value += ';' + value;
                }
                field.type = fieldType;
            }
            else
            {
                fields.Add(new Field(fieldTitle, value, fieldType));
            }
        }

        private void AddActorBreakdownField(DialogueEntry currentEntry, string catalog_id)
        {
            Actor actor;
            if (actorLookupViaCeltxId.TryGetValue(catalog_id, out actor))
            {
                string fieldTitle = GetAvailableFieldTitle(currentEntry, "BreakdownActor");
                AddToField(currentEntry.fields, fieldTitle, actor.id.ToString(), FieldType.Actor);
            }
            else
            {
                Debug.LogWarning($"Can't find actor with Celtx ID {catalog_id} to reference in breakdown.");
            }
        }

        private void AddItemBreakdownField(DialogueEntry currentEntry, string catalog_id)
        {
            Item item;
            if (itemLookupViaCeltxId.TryGetValue(catalog_id, out item))
            {
                string fieldTitle = GetAvailableFieldTitle(currentEntry, "BreakdownItem");
                AddToField(currentEntry.fields, fieldTitle, item.id.ToString(), FieldType.Item);
            }
            else
            {
                Debug.LogWarning($"Can't find item with Celtx ID {catalog_id} to reference in breakdown.");
            }
        }

        private void AddLocationBreakdownField(DialogueEntry currentEntry, string catalog_id)
        {
            Location location;
            if (locationLookupViaCeltxId.TryGetValue(catalog_id, out location))
            {
                string fieldTitle = GetAvailableFieldTitle(currentEntry, "BreakdownLocation");
                AddToField(currentEntry.fields, fieldTitle, location.id.ToString(), FieldType.Location);
            }
            else
            {
                Debug.LogWarning($"Can't find item with Celtx ID {catalog_id} to reference in breakdown.");
            }
        }

        private void AddCustomBreakdownField(DialogueEntry currentEntry, string breakdown_id, string catalog_id)
        {
            string customName = null;
            var custom = celtxDataObject.subdocuments.catalog.custom;
            foreach (var customObject in custom)
            {
                var customData = customObject.Value;
                string customId = (string) customData.id;
                if (string.Equals(customId, catalog_id))
                {
                    customName = (string) customData.name;
                    break;
                }
            }

            string customType;
            if (customName != null && customTypeByBreakdownId.TryGetValue(breakdown_id, out customType))
            {
                if (string.IsNullOrEmpty(customType)) customType = GetAvailableFieldTitle(currentEntry, "BreakdownCustom");
                AddToField(currentEntry.fields, customType, customName, FieldType.Text);
            }

            //--- By customer request, we use breakdown name in field instead of custom value.
            ////---Was: (But in JSON multiple breakdown types share the same catalog_id. Bug?)	
            //var custom = celtxDataObject.subdocuments.catalog.custom;
            //foreach (var customObject in custom)
            //{
            //    var customData = customObject.Value;
            //    string customId = (string)customData.id;
            //    if (string.Equals(customId, catalog_id))
            //    {
            //        string customName = (string)customData.name;
            //        string fieldTitle = (string)customData.custom_type;
            //        if (string.IsNullOrEmpty(fieldTitle)) fieldTitle = GetAvailableFieldTitle(currentEntry, "BreakdownCustom");
            //        AddToField(currentEntry.fields, fieldTitle, customName, FieldType.Text);
            //        break;
            //    }
            //}
        }

        private string GetAvailableFieldTitle(DialogueEntry currentEntry, string defaultFieldTitle)
        {
            if (!Field.FieldExists(currentEntry.fields, defaultFieldTitle)) return defaultFieldTitle;
            int num = 2;
            int safeguard = 0;
            while (Field.FieldExists(currentEntry.fields, defaultFieldTitle + num) && ++safeguard < 99)
            {
                num++;
            }
            return defaultFieldTitle + num;
        }

#endregion

#region Logging

        private string GetNullDataString(string nullFieldName)
        {
            return nullFieldName + " is null - ";
        }

        private string GetErrorString(MethodBase methodName, System.Exception ex)
        {
            return "Error in " + methodName + " : " + ex.ToString();
        }

        private void LogError(MethodBase methodName, System.Exception ex, string celtxId = null, string name = null)
        {
            string messageCore = GetErrorString(methodName, ex);
            LogMessage(LogLevel.E, messageCore, celtxId, name);
        }

        private void LogWarning(string messageCore, string celtxId, string name)
        {
            LogMessage(LogLevel.W, messageCore, celtxId, name);
        }

        private void LogMessage(LogLevel logLevel, string messageCore, string celtxId = null, string name = null)
        {
            StringBuilder logMessage = new StringBuilder();
            if (celtxId != null) { logMessage.Append("Celtx Object : " + celtxId + "(" + name + ")"); }
            logMessage.Append(" | " + messageCore);
            if (logLevel == LogLevel.W)
            {
                Debug.LogWarning(logMessage.ToString());
            }
            else if (logLevel == LogLevel.E)
            {
                Debug.LogError(logMessage.ToString());
            }
            else
            {
                Debug.Log(logMessage.ToString());
            }
        }

        enum LogLevel
        {
            W, E, I
        }

#endregion

#region TODO Items
        // TODO: Waiting on Celtx team
        //private bool IsPlayerCharacter(CxAttrs characterAttrs)
        //{
        //    try
        //    {
        //        if (characterAttrs.item_data == null)
        //        {
        //            LogWarning(GetNullDataString("attrs.item_data") +
        //                "Cannot determine if character is a player. Please ensure Character Type is set in the catalog", characterAttrs.id, characterAttrs.title);
        //            return false;
        //        }

        //        if (characterAttrs.item_data.character_type == null)
        //        {
        //            LogWarning(GetNullDataString("attrs.item_data.character_type") +
        //                "Defaulting to non-player Actor. Please ensure Character Type is set in the catalog", characterAttrs.id, characterAttrs.title);
        //            return false;
        //        }

        //        return characterAttrs.item_data.character_type.Equals("pc");
        //    }
        //    catch (System.Exception e)
        //    {
        //        LogError(MethodBase.GetCurrentMethod(), e, characterAttrs.id, characterAttrs.title);
        //    }
        //    return false;
        //}
#endregion
    }
}
#endif
