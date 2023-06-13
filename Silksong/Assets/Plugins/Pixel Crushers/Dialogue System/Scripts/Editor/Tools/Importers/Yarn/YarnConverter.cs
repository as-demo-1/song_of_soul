#if USE_YARN
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.Assertions;

using Yarn;


namespace PixelCrushers.DialogueSystem.Yarn
{
    // A few type aliases to hold the localized string tables.
    // The layout is Dict<conversation_name, Dict<text_key, Dict<locale, text>>>
    using LocaleToStringMap = System.Collections.Generic.Dictionary<string, string>;
    using TextKeyToLocalizedStringMap = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;
    using ConversationToLocalizedStringMap = System.Collections.Generic.Dictionary<string, Dictionary<string, Dictionary<string, string>>>;

    public class LocalizedYarnString
    {
        public readonly string locale;
        public readonly string key;
        public readonly string text;
        public readonly string file;
        public readonly string node;

        public LocalizedYarnString(string locale, string key, string text, string file, string node)
        {
            this.locale = locale;
            this.key = key;
            this.text = text;
            this.file = file;
            this.node = node;
        }
    }

    public enum ExpressionType
    {
    }

    public class Expression
    {
    }

    public class YarnConverter
    {
        public static class EntryTitle
        {
            // Block entries
            public const string ConversationStart = "START";
            public const string IfBlockStart = "If Block Start";
            public const string IfBlockEnd = "If Block End";
            public const string IfClauseStart = "{0} Clause Start";
            public const string IfClauseEnd = "{0} Clause End";
            public const string IfClauseFallback = "{0} Clause Fallback";
            public const string ShortcutOptionStart = "Shortcut Option Start";
            public const string ShortcutOptionEnd = "Shortcut Option End";
            public const string ShortcutOptionListStart = "Shortcut Option List Start";
            public const string ShortcutOptionListEnd = "Shortcut Option List End";

            // Basic entries
            public const string FormatOption = "Format Option";
            public const string FormatRunLine = "Format RunLine";
            public const string TriggerOption = "Trigger Option";
            public const string AddOption = "Add Option";
            public const string OptionGroup = "Option Group";
            public const string RunCommand = "Run Command";
            public const string RunLine = "Run Line";
            public const string RunNode = "Run Node";
            // public const string ShowOptions = "Show Options";
            public const string Stop = "Stop";
            public const string StoreVariable = "Store Variable";
            public const string UnreachableEntry = "Unreachable Entry";
            public const string UnsupportedWait = "Unsupported: Wait Command";
        }

        public static class EntryDescription
        {
            // Block entries
            public const string ConversationStart = "Conversation Start - name: {0}";
            public const string ConversationEnd = "Conversation End - name: {0}";
            public const string IfBlockStart = "If Block Start - Yarn exit label: {0}";
            public const string IfBlockEnd = "If Block End - Yarn exit label: {0}";
            public const string IfClauseStart = "{0} Clause Start - Yarn exit label: {1}";
            public const string IfClauseEnd = "{0} Clause End - Yarn exit label: {1}";
            public const string IfClauseFallback = "{0} Clause Fallback - Yarn exit label: {1}";
            public const string ShortcutOptionStart = "Shortcut Option Start - Yarn branch label: {0}, Yarn exit label: {1}";
            public const string ShortcutOptionEnd = "Shortcut Option End - Yarn branch label: {0}, Yarn exit label: {1}";
            public const string ShortcutOptionListStart = "Shortcut Option List Start - Yarn exit label: {0}";
            public const string ShortcutOptionListEnd = "Shortcut Option List End - Yarn exit label: {0}";

            // Basic entries
            public const string FormatOption = "Format Option - line key: {0}, conversation: {1}, formatting option's dialogue";
            public const string FormatRunLine = "Format RunLine - line key: {0}, conversation: {1}, formatting run line's dialogue";
            public const string TriggerOption = "Trigger Option - line key: {0}, conversation: {1}, flag: {2}, set to true so option will be display at Show Options";
            public const string AddOption = "Add Option - line key: {0}, conversation: {1}";
            public const string OptionGroup = "Option Group - conversation: {0}";
            public const string RunCommand = "Run Command - cmd: '{0}'";
            public const string RunLine = "Run Line - line key: {0}";
            public const string RunNode = "Run Node - conversation: {0}";
            // public const string ShowOptions = "Present all choices to the player";
            public const string Stop = "Yarn Stop instruction encountered, immediately ending conversation";
            public const string StoreVariable = "Store Variable: name: {0}";
            public const string UnreachableEntry = "Unreachable Entry after Run Node - node: {0}";
            public const string UnsupportedWait = "Wait commands with runtime arguments are not supported, must explicitly state wait duration, for example: <<wait 5>>";
        }

        public static class EntrySequence
        {
            public const string ConversationStart = "None()";
            public const string ContinueDialogue = "Continue()";
            public const string DelayDialogue = "Delay({0})";
        }

        public static class Lua
        {
            public const string YarnInstructionList = "yarn_instructions";
            public const string ExpressionResultsList = "exp_results";
            public const string EvaluateYarnExpression = "EvaluateYarnExpression";
            public const string RunCommandRuntimeArgumentList = "run_cmd_args";
            // public const string RunCommandArgumentRegex = "\\{\\d+\\}";
            public static readonly Regex RunCommandRuntimeArgumentRegex = new Regex("\\{\\d+\\}");

            // This string needs to be run through string.Format with the following arguments:
            //  0 - The full function call of the command, arguments and all
            //  1 - The lua table containing the Yarn instructions, i.e. a call to: GenerateYarnInstructionListScript(BasicStatement)
            public static readonly string RunCommandWithRuntimeArguments =
$@"local {RunCommandRuntimeArgumentList} = {Lua.EvaluateYarnExpression}({{1}})
{{0}}
";

            // This string needs to be run through string.Format with the following arguments
            // 0 - The variable name (without the '$' from Yarn)
            public const string LuaVariableTableAccess = "Variable['{0}']";

            // This string needs to be run through string.Format with the following arguments
            // 0 - The custom command name
            public const string LuaCustomCommand = "_G['{0}']";

            // This string needs to be run through string.Format with the following arguments
            //  0 - The lua table containing the Yarn instructions, i.e. a call to: GenerateYarnInstructionListScript(BasicStatement)
            public static readonly string ConditionsString = $"{Lua.EvaluateYarnExpression}({{0}})[1]\n";

            // This string needs to be run through string.Format with the following arguments
            //  0 - Formatted Lua Variable table access, i.e. a call to: FormatLuaVariableTableAccess(string)
            //  1 - The lua table containing the Yarn instructions, i.e. a call to: GenerateYarnInstructionListScript(BasicStatement)
            public static readonly string SetVariable = $"{{0}} = {Lua.EvaluateYarnExpression}({{1}})[1]\n";

            // This string needs to be run through string.Format with the following arguments
            //  0 - Formatted Lua variable, i.e. call: FormatLuaVariableAccess(string)
            public static readonly string OptionConditionsString = $"{{0}} = true\n";

            // This string needs to be run through string.Format with the following arguments:
            //  0 - The dialogue entry's conversation id
            //  1 - The dialogue entry's id
            //  2 - The lua table containing the Yarn instructions, i.e. a call to: GenerateYarnInstructionListScript(BasicStatement)
            // NOTE: The string arg order is a little weird, but it matches the GenerateFormattedStringArguments method signature a little more closely.
            public static readonly string FormatStringArguments =
$@"local is_first = true
for _, value in ipairs({Lua.EvaluateYarnExpression}({{2}})) do
    if is_first then
        {ClearAndAddStringFormatArgumentLuaName}({{0}}, {{1}}, tostring(value))
    else
        {AddStringFormatArgumentLuaName}({{0}}, {{1}}, tostring(value))
    end

    is_first = false
end
";
        }

        public const string DefaultPlayerActorName = "Player";
        public const int DefaultPlayerActorId = 1;
        public const string DefaultNpcActorName = "NPC";
        public const int DefaultNpcActorId = 2;

        public const string StringInterpolationVariableNameFormat = "${0}_fmt_line_{{0}}";

        public const string ClearAndAddStringFormatArgumentLuaName = "clr_add_str_fmt_arg";
        public const string AddStringFormatArgumentLuaName = "add_str_fmt_arg";

        private YarnProject _yarnProject = null;

        private DialogueDatabase _dialogueDb = null;
        private Template _template;
        private Actor _playerActor = null;
        private Actor _defaultNpcActor = null;

        private Dictionary<string, YarnBuiltInOperator> nameToBuiltInOperatorMap = new Dictionary<string, YarnBuiltInOperator>();

        // This will be necessary to create conversation links between dialogue entries
        private Dictionary<Instruction, DialogueEntry> _instructionToDialogueEntryMap = new Dictionary<Instruction, DialogueEntry>();

        private ConversationToLocalizedStringMap localizedStringTable = new ConversationToLocalizedStringMap();
        private YarnConverterPrefs _prefs;

        public static string FormatYarnOperand(Operand operand)
        {
            var result = string.Empty;

            if (operand.ValueCase == Operand.ValueOneofCase.BoolValue)
            {
                result = operand.BoolValue ? "true" : "false";
            }
            else if (operand.ValueCase == Operand.ValueOneofCase.FloatValue)
            {
                result = operand.FloatValue.ToString();
            }
            else if (operand.ValueCase == Operand.ValueOneofCase.None)
            {
                result = "nil";
            }
            else
            {
                result = $"'{operand.StringValue}'";
            }

            return result;
        }

        public static string FormatLuaVariableTableAccess(string yarnVariableName)
        {
            // $"Variable['{FormatLuaVariableName(yarnVariableName)}']";
            return string.Format(Lua.LuaVariableTableAccess, FormatLuaVariableName(yarnVariableName));
        }

        // Unnecessary, but I don't feel like removing it, and it kinda matches with the LuaTableAccess method
        public static string FormatLuaVariableAccess(string yarnVariableName)
        {
            return FormatLuaVariableName(yarnVariableName);
        }

        // Yarn variables start with the '$' char, this strips them if present.
        public static string FormatLuaVariableName(string yarnVariableName)
        {
            Assert.IsTrue(yarnVariableName[0] == '$', $"Found Yarn variable that did not start with '$': {yarnVariableName}");
            return yarnVariableName.Substring(1);
        }

        public static string FormatLuaCustomCommand(string cmdName)
        {
            return string.Format(Lua.LuaCustomCommand, cmdName);
        }

        // Static initializer
        public YarnConverter()
        {
            // Populate the map of built-in operators
            var enumVals = Enum.GetValues(typeof(YarnBuiltInOperator)) as YarnBuiltInOperator[];
            foreach (var enumVal in enumVals)
            {
                nameToBuiltInOperatorMap[enumVal.Name()] = enumVal;
            }
        }

        public void Convert(YarnConverterPrefs prefs, YarnProject project, DialogueDatabase dialogueDb)
        {
            _prefs = prefs;
            _yarnProject = project;
            _dialogueDb = dialogueDb;
            _template = Template.FromDefault();

            CacheLocalizedStrings();
            CreateLuaGlobalVariables();
            CreateDefaultActors();
            CreateConversations();
            GenerateGlobalUserScript();

            var conversation = _dialogueDb.conversations[0];
            if (_prefs.debug) Debug.Log($"After conversion complete, start dialogue entry title: ${conversation.dialogueEntries[0].Title} actor id: {conversation.dialogueEntries[0].ActorID} conversant id: {conversation.dialogueEntries[0].ConversantID}");
        }

        private void CreateDefaultActors()
        {
            // Create the player actor, check if the player name has been specified
            var playerActorName = DefaultPlayerActorName;
            if (!string.IsNullOrEmpty(_prefs.playerName))
            {
                if (_prefs.debug) Debug.Log($"Should be setting player actor name: {_prefs.playerName}");
                playerActorName = _prefs.playerName;
            }
            _playerActor = GetOrCreateActor(playerActorName, true, DefaultPlayerActorId);

            // Create the default NPC actor
            _defaultNpcActor = GetOrCreateActor(DefaultNpcActorName, false, DefaultNpcActorId);
        }

        // NOTE: Just pointing out that I find Yarn's localization design to be a little brittle, so it's possible bugs may creep up around this.
        // Localization files are kept in CSV format, here is an example excerpt from "Sally (de).csv":
        //  id,text,file,node,lineNumber
        //  line:794945,"Spieler: Hey Sally. ",Sally,Sally,7
        //  line:2dc39b,"Sally: Oh! Hi. ",Sally,Sally,8
        //  line:34de2f,"Sally: Du hast dich einfach so angeschlichen. ",Sally,Sally,9
        //          ...
        //
        // One thing to keep in mind here is that locale is NOT stored in the file itself, it is part of the filename - "Sally (de).csv".
        // The locale is pulled out by a regex, I am currently not making that editable by the user.
        // Yarn 2.0 keeps locale info inside the CSV, as another column in each row
        // The only information the importer cares about are the columns:
        //  (0) id   - This is the line's unique key (I believe this is unique only to a Yarn Node, not an entire project)
        //  (1) text - The localized text
        //  (2) filename - The localized text, and we really don't even care about this
        //  (3) node - The Node's name, same as our Dialogue System Conversation title field.
        private List<LocalizedYarnString> ParseLocalizedStrings()
        {
            if (_prefs.debug) Debug.Log($"YarnConverterWindow::ParseLocalizedStrings()");

            var localizedStrings = new List<LocalizedYarnString>();

            foreach (var filename in _prefs.localizedStringFiles)
            {
                MatchCollection mc = Regex.Matches(filename, _prefs.localeRegex);

                if (mc.Count <= 0 || mc[0].Groups.Count < 2)
                {
                    if (_prefs.debug) Debug.Log($"YarnConverterWindow::ParseLocalizedStrings() - Localized string file did not match expected filename regex: {_prefs.localeRegex}");
                    continue;
                }

                var locale = mc[0].Groups[1].Captures[0].Value;

                if (_prefs.debug) Debug.Log($"YarnConverterWindow::ParseLocalizedStrings() - Parsing file: {filename}");
                // var csvFileContents = CSVUtility.ReadCSVFile(filename, prefs.encodingType.SwitchScope());
                var csvFileContents = CSVUtility.ReadCSVFile(filename, _prefs.encodingType);

                for (var row = 1; row < csvFileContents.Count; ++row)
                {
                    var currentLine = csvFileContents[row];
                    if (_prefs.debug) Debug.Log($"YarnConverterWindow::ParseLocalizedStrings() - Parsing row: {row} current line: '{string.Join(",", currentLine)}'");

                    var localizedString = new LocalizedYarnString(locale, currentLine[0], currentLine[1], currentLine[2], currentLine[3]);
                    localizedStrings.Add(localizedString);
                }
            }

            return localizedStrings;
        }

        // The ConversationToLocalizedStringMap data structure may seem a bit odd at first, but it's set up for easy retrieval.
        // The idea is that we dive down to a list of <locale, text> pairs given a conversation and text line key.
        // Populating will look like: localizedStringMap[conversation_name][line_key][locale] = text
        // Retrieval will look like: Map<locale, text> = localizedStringMap[conversation_name][line_key]
        // The default locale is not stored in the ConversationToLocalizedStringMap data structure,
        // because I'm not exactly sure if there's even a way to determine that after these localized Yarn text files are generated.
        // By the time this Yarn -> DS converter is run, how can it possibly be determined?
        // public void CacheLocalizedStrings(LocaleToStringFileMapEntry[] localizedFileMap, Template template)
        // public void CacheLocalizedStrings(List<LocalizedYarnString> localizedStrings)
        public void CacheLocalizedStrings()
        {
            if (_prefs.debug) Debug.Log($"YarnToDialogueSystemConverter::CacheLocalizedStrings()");
            var localizedStrings = ParseLocalizedStrings();
            if (_prefs.debug) Debug.Log($"YarnToDialogueSystemConverter::CacheLocalizedStrings() - total string count: {localizedStrings.Count}");

            // Keep track of each unique locale
            var locales = new HashSet<string>();

            foreach (var localizedString in localizedStrings)
            {
                locales.Add(localizedString.locale);

                // Create and add the conversation_name -> TextKeyToLocalizedStringMap
                if (!localizedStringTable.ContainsKey(localizedString.node))
                {
                    localizedStringTable[localizedString.node] = new TextKeyToLocalizedStringMap();
                }

                // Create and add the line_key -> LocaleToStringMap
                if (!localizedStringTable[localizedString.node].ContainsKey(localizedString.key))
                {
                    localizedStringTable[localizedString.node][localizedString.key] = new LocaleToStringMap();
                }

                // Finally, set the local -> text pairing
                localizedStringTable[localizedString.node][localizedString.key][localizedString.locale] = localizedString.text;
                if (_prefs.debug) Debug.Log($"YarnToDialogueSystemConverter::CacheLocalizedStrings() - localizedStringTable[{localizedString.node}][{localizedString.key}][{localizedString.locale}] = '{localizedString.text}'");
            }

            // Create a sorted list out of the unique locales, then update template
            var sortedLocales = new List<string>(locales);
            sortedLocales.Sort();
            foreach (var locale in sortedLocales)
            {
                if (_prefs.debug) Debug.Log($"YarnToDialogueSystemConverter::CacheLocalizedStrings() - adding locale: {locale}");
                var localeField = new Field(locale, null, FieldType.Localization);
                _template.dialogueEntryFields.Add(localeField);
            }
        }

        private void CreateLuaGlobalVariables()
        {
            var sortedVarNames = new List<string>(_yarnProject.Variables.Keys);
            sortedVarNames.Sort();

            foreach (var varName in sortedVarNames)
            {
                var variable = _yarnProject.Variables[varName];

                if (!variable.isGlobal)
                {
                    continue;
                }

                var luaVarName = FormatLuaVariableName(variable.name);

                // Don't smash user variables if they've already been created in the DB
                if (_dialogueDb.GetVariable(luaVarName) != null)
                {
                    continue;
                }

                var fieldType = FieldType.Text;
                switch (variable.type)
                {
                    case Operand.ValueOneofCase.BoolValue:
                        fieldType = FieldType.Boolean;
                        break;

                    case Operand.ValueOneofCase.FloatValue:
                        fieldType = FieldType.Number;
                        break;

                    case Operand.ValueOneofCase.None:
                    case Operand.ValueOneofCase.StringValue:
                    default:
                        fieldType = FieldType.Text;
                        break;
                }

                var luaVar = _template.CreateVariable(_template.GetNextVariableID(_dialogueDb), luaVarName, variable.initialValue, fieldType);
                _dialogueDb.variables.Add(luaVar);
            }
        }

        private void CreateConversations()
        {
            // First, populate the database with all conversation dialogue groups (and lines).
            // We will resolve how this conditionally branches later on.
            foreach (var nodeEntry in _yarnProject.Nodes)
            {
                var nodeName = nodeEntry.Key;
                var node = nodeEntry.Value;
                node.Conversation = GetOrCreateConversation(node);
                // if (_prefs.debug) Debug.Log($"After creating conversation, start dialogue entry actor id: {node.Conversation.dialogueEntries[0].ActorID} conversant id: {node.Conversation.dialogueEntries[0].ConversantID}");
                CreateConversationDialogueEntries(node);

                if (_prefs.debug) Debug.Log($"After creating all entries, start dialogue entry title: ${node.Conversation.dialogueEntries[0].Title} actor id: {node.Conversation.dialogueEntries[0].ActorID} conversant id: {node.Conversation.dialogueEntries[0].ConversantID}");
            }

            // Next, create all dialogue links
            foreach (var nodeEntry in _yarnProject.Nodes)
            {
                var nodeName = nodeEntry.Key;
                var node = nodeEntry.Value;
                ResolveDialogueEntryLinks(node);

                if (_prefs.debug) Debug.Log($"After resolving all links, start dialogue entry title: ${node.Conversation.dialogueEntries[0].Title} actor id: {node.Conversation.dialogueEntries[0].ActorID} conversant id: {node.Conversation.dialogueEntries[0].ConversantID}");
            }

            foreach (var nodeEntry in _yarnProject.Nodes)
            {
                var nodeName = nodeEntry.Key;
                var node = nodeEntry.Value;
                CullUnreachableEntries(node.Conversation);

                if (_prefs.debug) Debug.Log($"After culling extra unreachable entries, start dialogue entry title: ${node.Conversation.dialogueEntries[0].Title} actor id: {node.Conversation.dialogueEntries[0].ActorID} conversant id: {node.Conversation.dialogueEntries[0].ConversantID}");
            }
        }

        private void CreateConversationDialogueEntries(BlockStatement block)
        {
            // Base case, we've already handled this whole branch
            if (block.HasStartDialogueEntry)
            {
                // if (_prefs.debug) Debug.Log($"YarnToDialogueSystemConverter::CreateDialogueEntries() - already block type {block.Type} with label: {block.FirstStatement.Label}");
                return;
            }

            // if (_prefs.debug) Debug.Log($"YarnToDialogueSystemConverter::CreateDialogueEntries() - processing block type {block.Type} with label: {block.FirstStatement.Label}");
            CreateStatementDialogueEntries(block);

            foreach (var stmt in block.Statements)
            {
                if (stmt.Type.IsBasic())
                {
                    // if (_prefs.debug) Debug.Log($"CREATING DIALOGUE ENTRY - convo: {node.Name}, last instruction: {oweifj}");
                    var basicStmt = (BasicStatement)stmt;
                    // if (_prefs.debug) Debug.Log($"YarnToDialogueSystemConverter::CreateDialogueEntries() - processing stmt with opcode: {basicStmt.LastInstruction.Opcode}, label: {basicStmt.Label}");
                    var opCode = basicStmt.LastInstruction.Opcode;
                    CreateStatementDialogueEntries(stmt);
                }
                else
                {
                    var childBlock = (BlockStatement)stmt;
                    // if (_prefs.debug) Debug.Log($"YarnToDialogueSystemConverter::CreateDialogueEntries() - about to process block type {block.Type} with label: {childBlock.FirstStatement.Label}");
                    CreateConversationDialogueEntries(childBlock);
                }
            }
        }

        // The basic gist of this method is that all statements resolve to a single dialogue entry that is entered into,
        // and a single dialogue entry that is exited from.
        // The entrance and exit dialogue entries can be the same entry, usually it is only BlockStatements that
        // enclose their contents in two separate entrance/exit dialogue entries.
        // The return value of this method is the exit dialogue entry of the final statement processed by this function.
        // That value is passed on to subsequent recursive calls of ResolveDialogueEntryLinks, and should be linked
        // to the entrace dialogue entry of the very first statement processed.
        private DialogueEntry ResolveDialogueEntryLinks(BlockStatement block, DialogueEntry previousEntry = null)
        {
            // if (_prefs.debug) Debug.Log("YarnToDialogueSystemConverter::ResolveDialogueEntryLinks()");

            // Initial entry for this recursive function, need to properly initialize the previousEntry argument.
            if (block.Type.IsConversation())
            {
                Assert.IsNull(previousEntry, "Initial call to ResolveDialogueEntryLinks has non-null previousEntry argument.");
                previousEntry = block.StartDialogueEntry;
            }

            YarnStatement previousStmt = null;
            foreach (var stmt in block.Statements)
            {
                if (stmt.Type.IsBasic())
                {
                    if (!stmt.HasDialogueEntries)
                    {
                        continue;
                    }

                    var basicStmt = (BasicStatement)stmt;

                    // First, check to see if we're dealing with an AddOption instruction.
                    // If so, and it is conditionally entered, set up the script that calcualates the conditions.
                    if (basicStmt.LastInstruction.Opcode.IsAddOption())
                    {
                        // Multiple dialogue entries means this option has a condition
                        if (basicStmt.HasConditionalOptionDialogueEntry)
                        {
                            var optCondDlgEntry = basicStmt.ConditionalOptionDialogueEntry;
                            CreateLink(previousEntry, optCondDlgEntry);
                            previousEntry = optCondDlgEntry;
                        }
                    }
                    else if (basicStmt.LastInstruction.Opcode.IsRunNode())
                    {
                        var dstNode = _yarnProject.Nodes[basicStmt.BranchLabel];
                        Assert.IsTrue(dstNode.Type.IsConversation(), $"RunNode can only point to a conversation, branch label: {basicStmt.BranchLabel}");
                        var dstConversation = (ConversationNode)dstNode;
                        CreateLink(previousEntry, basicStmt.DialogueEntry);
                        var runNodeLink = CreateLink(basicStmt.DialogueEntry, dstConversation.StartDialogueEntry);
                        runNodeLink.isConnector = true;

                        // It is perfectly legal to put unreachable statements in a Yarn node right after a RunNode [Node].
                        // To make sure those statements are represented in the Dialogue Database, an unreachable parent node
                        // is used. This behaves just like a Stop node, where its conditionsString is always false.
                        // If no unreachable nodes are linked to this, it will eventually get culled from the conversation.
                        var unreachableCodeEntry = basicStmt.DialogueEntries[1];
                        unreachableCodeEntry.conditionsString = "false";
                        var unreachableLink = CreateLink(basicStmt.DialogueEntry, unreachableCodeEntry);
                        unreachableLink.priority = ConditionPriority.BelowNormal;
                        previousEntry = unreachableCodeEntry;

                        // We can't process anything past a RunNode, it's an immediate jump to another conversation
                        // break;
                    }
                    else if (basicStmt.LastInstruction.Opcode.IsRunLine() && basicStmt.HasArguments)
                    {
                        // If we're dealing with a RunLine stmt that has calculated string arguments,
                        // we need to run the script that calculates the args on the previous DialogueEntry.
                        // This is because the OnConversationResponseMenu event handler runs BEFORE
                        // any RunLine statement DialogueEntries are executed (the Dialogue System is
                        // peeking ahead at outgoing links).
                        // NOTE: This is NOT a problem for any AddOption statements, as their arguments are
                        // always pre-calculated on a previous node. The reason why is explained later
                        // in the comments of the CreateStatementDialogueEntries() method, see the section
                        // that handles AddOption instructions.

                        // var cId = basicStmt.DialogueEntry.conversationID;
                        // var dId = basicStmt.DialogueEntry.id;
                        // var fmtArgsScript = $"{GenerateFormattedStringArguments(cId, dId, basicStmt)}";
                        // previousEntry.userScript += fmtArgsScript;

                        // CreateLink(previousEntry, basicStmt.DialogueEntry);
                        // previousEntry = basicStmt.DialogueEntry;

                        var fmtRunLineEntry = basicStmt.CalculateRunLineArgumentsDialogueEntry;
                        CreateLink(previousEntry, fmtRunLineEntry);
                        CreateLink(fmtRunLineEntry, basicStmt.DialogueEntry);
                        previousEntry = basicStmt.DialogueEntry;
                    }
                    else if (basicStmt.LastInstruction.Opcode.IsStop())
                    {
                        var stopLink = CreateLink(previousEntry, basicStmt.DialogueEntry);
                        stopLink.priority = ConditionPriority.High;
                        previousEntry = basicStmt.DialogueEntry;
                    }
                    else if (basicStmt.HasDialogueEntries)
                    {
                        CreateLink(previousEntry, basicStmt.DialogueEntry);
                        previousEntry = basicStmt.DialogueEntry;
                    }
                }
                else if (stmt.Type.IsIfBlock())
                {
                    // This may look a bit tricky at first, but it's very simple:
                    //  1. The If block has both an entrance and an exit dialogue entry.
                    //  2. If and ElseIf subclauses have two separate group entries:
                    //      2a. The first contains all statements executed if the conditional is true.
                    //          The conditionalString is associated with this group node, and it is given
                    //          Normal priorty. The last dialogue entry of this group node points all the 
                    //          way down to the exit node of the entire If block.
                    //      2b. The second has no conditions and is given BelowNormal priority.
                    //          If the conditions for the entry described in 2a fail, this entry is executed.
                    //          This second group entry automatically falls down to one of three entries:
                    //              1. The conditional group node of the next elseif subclause, if that is the next subclause
                    //              2. The entrance dialogue entry of the else statement, if that is the next subclause.
                    //                 This MUST be the final subclause in the If block.
                    //              3. The exit entry for the entire If block, if this is the last subclause.
                    //  3. Else statements behave just like a regular Block statement, with a single entrance dialogue entry,
                    //     and a single exit dialogue entry. There is obviously no conditional branching like an If/ElseIf subclause.
                    var ifBlock = (IfBlock)stmt;
                    CreateLink(previousEntry, ifBlock.StartDialogueEntry);
                    previousEntry = ifBlock.StartDialogueEntry;

                    foreach (var ifClause in ifBlock.IfClauses)
                    {
                        CreateLink(previousEntry, ifClause.StartDialogueEntry);

                        // May seem weird, but let's get our recursive call out of the way first.
                        // This returns the final dialogue entry of this subclause,
                        // so let's link it to the exit of the entire If block.
                        var blockEndEntry = ResolveDialogueEntryLinks(ifClause, ifClause.StartDialogueEntry);
                        CreateLink(blockEndEntry, ifBlock.EndDialogueEntry);

                        if (!ifClause.ClauseType.IsElse())
                        {
                            // If/ElseIf clause, a little more complex since two group nodes are associated with these statement types.
                            Assert.IsNotNull(ifClause.FallbackDialogueEntry, $"If clause with conditions has no matching ConditionsFailDialogueEntry, conditions: {ifBlock.ExitLabel}");

                            // Link from the previous statement's exit dialogue entry to:
                            //  1. Conditional entry of if/elseif subclause
                            //  2. Fallback entry of if/elseif subclause
                            // We have to drop the priority of the fallback link,
                            // so that it's only entered when its corresponding if entry conditons fail
                            var fallbackLink = CreateLink(previousEntry, ifClause.FallbackDialogueEntry);
                            fallbackLink.priority = ConditionPriority.BelowNormal;

                            // The previousEntry is ALWAYS the fallback entry,
                            // because the last statement of the conditional subclause always links
                            // to the very end of the entire If block.
                            previousEntry = ifClause.FallbackDialogueEntry;
                        }
                    }

                    // If the IfBlock has no else clause, we have to resolve the link to the last FallbackDialogueEntry
                    if (!ifBlock.HasElse)
                    {
                        var finalClause = ifBlock.IfClauses[ifBlock.IfClauses.Count - 1];
                        CreateLink(finalClause.FallbackDialogueEntry, ifBlock.EndDialogueEntry);
                    }

                    previousEntry = ifBlock.EndDialogueEntry;
                }
                else if (stmt.Type.IsShortcutOptionList())
                {
                    // if (_prefs.debug) Debug.Log($"YarnToDialogueSystemConverter::CreateDialogueEntries() - processing shortcut option list");
                    var scOptionsList = (ShortcutOptionList)stmt;

                    CreateLink(previousEntry, scOptionsList.StartDialogueEntry);
                    previousEntry = scOptionsList.StartDialogueEntry;

                    bool allConditionalOpts = true;
                    foreach (var childStmt in scOptionsList.ShortcutOptions)
                    {
                        Assert.IsTrue(childStmt.Type.IsShortcutOption(), "Illegal statement type in ShortcutOptionList, only ShortcutOption allowed: {childStmt.Type}");

                        var scOpt = (ShortcutOption)childStmt;
                        allConditionalOpts = allConditionalOpts && scOpt.HasConditions;

                        CreateLink(scOptionsList.StartDialogueEntry, scOpt.StartDialogueEntry);
                        var optEntry = ResolveDialogueEntryLinks(scOpt, scOpt.StartDialogueEntry);
                        CreateLink(optEntry, scOpt.EndDialogueEntry);
                        CreateLink(scOpt.EndDialogueEntry, scOptionsList.EndDialogueEntry);
                    }

                    if (allConditionalOpts)
                    {
                        // This link exists solely to allow the Shortcut Options List to fall through in case
                        // there are no shortcut options that can be entered into. This is possible if all shortcut options
                        // have conditions that fail.
                        var fallbackLink = CreateLink(scOptionsList.StartDialogueEntry, scOptionsList.EndDialogueEntry);
                        fallbackLink.priority = ConditionPriority.BelowNormal;
                    }

                    previousEntry = scOptionsList.EndDialogueEntry;
                }
                else
                {
                    // Just your regular garden variety BlockStatement
                    var childBlock = (BlockStatement)stmt;
                    previousEntry = ResolveDialogueEntryLinks(childBlock, previousEntry);
                }

                previousStmt = stmt;
            }

            if (block.Type.IsConversation())
            {
                var conversationNode = (ConversationNode)block;
                ResolveConversationOptions(conversationNode, previousEntry);
            }

            return previousEntry;
        }

        private void ResolveConversationOptions(ConversationNode conversationNode, DialogueEntry previousEntry)
        {
            // Set up the links to the branching options at the end of the entire conversation.

            if (conversationNode.Options.Count > 0)
            {
                CreateLink(previousEntry, conversationNode.EndDialogueEntry);

                foreach (var option in conversationNode.Options)
                {
                    CreateLink(conversationNode.EndDialogueEntry, option.DialogueEntry);

                    var jumpToConversation = _yarnProject.Nodes[option.BranchLabel];
                    var link = CreateLink(option.DialogueEntry, jumpToConversation.StartDialogueEntry);
                    link.isConnector = true;
                }
            }
        }

        private void CullUnreachableEntries(Conversation conversation)
        {
            // if (_prefs.debug) Debug.Log("YarnConverter::CullUnreachableEntries()");
            var runNodeEntries = new List<DialogueEntry>();
            foreach (var entry in conversation.dialogueEntries)
            {
                if (entry.Title.Equals("Run Node"))
                {
                    runNodeEntries.Add(entry);
                }
            }

            // Go through each RunNode entry, and determine if its unreachable node
            // is linked to anything else. If NOT, delete the unreachable node as its unnecessary.
            foreach (var runNodeEntry in runNodeEntries)
            {
                var link = runNodeEntry.outgoingLinks[1];
                var unreachableCodeEntry = _dialogueDb.GetDialogueEntry(link.destinationConversationID, link.destinationDialogueID);
                if (unreachableCodeEntry.outgoingLinks.Count <= 0)
                {
                    runNodeEntry.outgoingLinks.Remove(link);
                    conversation.dialogueEntries.Remove(unreachableCodeEntry);
                }
            }
        }

        public string GenerateConditionsStringScript(BasicStatement stmt)
        {
            // var script = $"{Lua.EvaluateYarnExpression}({GenerateYarnInstructionListScript(stmt)})[1]\n";
            var script = string.Format(Lua.ConditionsString, GenerateYarnInstructionListScript(stmt));
            return script;
        }

        // Public so that it can be directly unit tested
        public string GenerateSetVariableScript(string varName, BasicStatement stmt)
        {
            // var script = $"{varName} = {Lua.EvaluateYarnExpression}({GenerateYarnInstructionListScript(stmt)})[1]\n";
            var script = string.Format(Lua.SetVariable,
                FormatLuaVariableTableAccess(varName),
                GenerateYarnInstructionListScript(stmt));
            return script;
        }

        // Public so that it can be directly unit tested
        public string GenerateFormattedStringArguments(int conversationId, int dialogueEntryId, BasicStatement stmt)
        {
            var script = string.Format(Lua.FormatStringArguments,
                conversationId,
                dialogueEntryId,
                GenerateYarnInstructionListScript(stmt)
            );
            // $@"
            // local is_first = true
            // for _, value in ipairs({Lua.EvaluateYarnExpression}({GenerateYarnInstructionListScript(stmt)})) do
            //     if is_first then
            //         {ClearAndAddStringFormatArgumentLuaName}({conversationId}, {dialogueEntryId}, tostring(value))
            //     else
            //         {AddStringFormatArgumentLuaName}({conversationId}, {dialogueEntryId}, tostring(value))
            //     end

            //     is_first = false
            // end
            // ";
            return script;
        }

        // Public so that it can be directly unit tested
        public string GenerateOptionsConditionsScript(string varName)
        {
            // return $"{FormatLuaVariableAccess(varName)} = true\n";
            var script = string.Format(Lua.OptionConditionsString, FormatLuaVariableAccess(varName));
            return script;
        }

        // Public so that it can be directly unit tested
        public string GenerateYarnInstructionListScript(BasicStatement stmt)
        {
            // if (_prefs.debug) Debug.Log("YarnToDialogueSystemConverter::GenerateExpressionStackTyped() - total instructions in stmt: " + stmt.TotalInstructions);
            var instructionListItems = new List<string>();

            foreach (var instruction in stmt.Instructions)
            {
                // if (_prefs.debug) Debug.Log($"YarnToDialogueSystemConverter::GenerateCalculation - processing instruction: {instruction.Opcode}");

                if (instruction.Opcode.IsPop())
                {
                    continue;
                }
                else if (instruction.Opcode.IsPushOpCode())
                {
                    var arg = string.Empty;
                    if (instruction.Opcode.IsPushVariable())
                    {
                        arg = FormatLuaVariableTableAccess(instruction.Operands[0].StringValue);
                    }
                    else
                    {
                        arg = FormatYarnOperand(instruction.Operands[0]);
                    }

                    instructionListItems.Add(arg);
                }
                else if (instruction.Opcode.IsCallFunc())
                {
                    var cmdName = instruction.Operands[0].StringValue;
                    var isBuiltInOp = nameToBuiltInOperatorMap.ContainsKey(cmdName);

                    if (isBuiltInOp)
                    {
                        var op = nameToBuiltInOperatorMap[cmdName];
                        instructionListItems.Add($"{op.LuaOperatorType()}");
                    }
                    else
                    {
                        // instructionListItems.Add($"_G['{cmdName}']");
                        instructionListItems.Add(FormatLuaCustomCommand(cmdName));
                    }
                }
                else if (instruction.Opcode.IsStatementTerminatingOpCode())
                {
                    break;
                }
                else
                {
                    Assert.IsTrue(false, $"Unexpected opcode found while generating calculation: {instruction.Opcode}");
                }
            }

            var lua = $"{{ {string.Join(", ", instructionListItems)} }}";
            return lua;
        }

        public Actor GetOrCreateActor(string name, bool isPlayer = false, int id = -1)
        {
            var actor = _dialogueDb.GetActor(name);
            if (actor == null)
            {
                id = id < 1 ? _template.GetNextActorID(_dialogueDb) : id;
                actor = _template.CreateActor(id, name, isPlayer);
                _dialogueDb.actors.Add(actor);
            }

            return actor;
        }

        public Conversation GetOrCreateConversation(ConversationNode node)
        {
            var conversation = _dialogueDb.GetConversation(node.Name);

            if (conversation == null)
            {
                var conversationId = _template.GetNextConversationID(_dialogueDb);
                conversation = _template.CreateConversation(conversationId, node.Name);

                var actor = _playerActor;
                var conversant = _defaultNpcActor;

                if (node.Metadata.ContainsKey(YarnProject.NodeMetadataActorKey))
                {
                    var actorName = node.Metadata[YarnProject.NodeMetadataActorKey];
                    actor = GetOrCreateActor(actorName);
                    // if (_prefs.debug) Debug.Log($"YarnConverter::GetOrCreateConversation() - should be creating actor actor with name: {actorName}, actor: {actor}, id: {actor.id}");
                }

                if (node.Metadata.ContainsKey(YarnProject.NodeMetadataConversantKey))
                {
                    var conversantName = node.Metadata[YarnProject.NodeMetadataConversantKey];
                    conversant = GetOrCreateActor(conversantName);
                    // if (_prefs.debug) Debug.Log($"YarnConverter::GetOrCreateConversation() - should be creating conversant actor with name: {conversantName}, actor: {conversant}, id: {conversant.id}");
                }

                // if (_prefs.debug) Debug.Log($"GetOrCreateConversation() - Set convo {node.Name} actor to {actor.Name} conversant to: {conversant.Name}");
                conversation.ActorID = actor.id;
                conversation.ConversantID = conversant.id;
                _dialogueDb.conversations.Add(conversation);

                node.Conversation = conversation;
            }

            return conversation;
        }

        private void CreateStatementDialogueEntries(YarnStatement stmt)
        {
            var conversation = stmt.ConversationNode?.Conversation;
            if (stmt.Type.IsBasic())
            {
                var basicStmt = (BasicStatement)stmt;

                string text = null;

                // var opCode = basicStmt.LastInstruction.Opcode;
                var opCode = basicStmt.MainInstruction.Opcode;

                string textKey = null;
                if (opCode.IsAddOption() || opCode.IsRunLine())
                {
                    textKey = basicStmt.LastInstruction.Operands[0].StringValue;
                    text = _yarnProject.StringTable[textKey].text;
                }

                if (opCode.IsAddOption())
                {
                    var title = YarnConverter.EntryTitle.AddOption;
                    var desc = string.Format(YarnConverter.EntryDescription.AddOption, textKey, stmt.ConversationNode.Name);
                    var optDlgEntry = CreateDialogueEntry(conversation, title, desc);
                    SetPlayerChoiceActors(optDlgEntry);
                    SetDialogueEntryText(optDlgEntry, conversation.Title, textKey);
                    if (_prefs.debug) Debug.Log($"YarnConverter - SetAddOptionText from key {textKey} - {optDlgEntry.DialogueText}");
                    basicStmt.AddDialogueEntry(optDlgEntry);

                    // NOTE: The default entry (YarnStatement.DialogueEntry) actually corresponds to any potential
                    //       conditions necessary to display this option. OptionConditionsVariable is just a flag used
                    //       mark whether or not the conditions have been met, so that at the end of the conversation when
                    //       it's time to display the options, we know which ones are valid.
                    if (basicStmt.OptionConditionsVariable != null)
                    {
                        var luaOptVar = FormatLuaVariableName(basicStmt.OptionConditionsVariable);
                        // var optVar = FormatLuaVariableName(basicStmt.OptionConditionsVariable);
                        var optVarTitle = YarnConverter.EntryTitle.TriggerOption;
                        var optVarDescription = string.Format(YarnConverter.EntryDescription.TriggerOption, textKey, basicStmt.BranchLabel, luaOptVar);

                        // When encountered, this entry runs a script setting the option conditions variable to true,
                        // meanining it should be added to the list of available options at the end of the conversation.
                        var optCondDlgEntry = CreateDialogueEntry(conversation, optVarTitle, optVarDescription);

                        // We also want to calculate any arguments for the option's DialogueText string at this specific point in the code.
                        // If we do it somewhere else, we can't guarantee our values will be calculated at the right state.
                        // var userScript = $"{optVar} = true;";
                        // var userScript = GenerateOptionsConditionsScript(optVar);

                        optCondDlgEntry.userScript = GenerateOptionsConditionsScript(basicStmt.OptionConditionsVariable);
                        optCondDlgEntry.Sequence = EntrySequence.ContinueDialogue;
                        basicStmt.AddDialogueEntry(optCondDlgEntry);

                        // Set the conditions of the actual option dialogue entry that will be displayed to the user
                        // to the true/false value of the conditions variable.
                        // basicStmt.OptionConditionsDialogueEntry.conditionsString = optVar;
                        // optDlgEntry.conditionsString = $"true --{luaOptVar}";
                        optDlgEntry.conditionsString = luaOptVar;
                    }

                    if (basicStmt.HasArguments)
                    {
                        // If we're just setting up an option whose text needs formatting,
                        // set that up here. Otherwise, we treat it as an option that is conditionally displayed.
                        if (!basicStmt.HasConditionalOptionDialogueEntry)
                        {
                            var optVarTitle = YarnConverter.EntryTitle.FormatOption;
                            var optVarDescription = string.Format(YarnConverter.EntryDescription.FormatOption, textKey, basicStmt.BranchLabel);
                            var entry = CreateDialogueEntry(conversation, optVarTitle, optVarDescription);
                            entry.Sequence = EntrySequence.ContinueDialogue;
                            basicStmt.AddDialogueEntry(entry);
                        }

                        var optCondDlgEntry = basicStmt.ConditionalOptionDialogueEntry;
                        var cId = optDlgEntry.conversationID;
                        var dId = optDlgEntry.id;
                        var userScript = optCondDlgEntry.userScript ?? string.Empty;
                        userScript += $"{GenerateFormattedStringArguments(cId, dId, basicStmt)}";
                        optCondDlgEntry.userScript = userScript;
                    }
                }
                else if (opCode.IsRunCommand())
                {
                    var cmdString = basicStmt.LastInstruction.Operands[0].StringValue;

                    //---Was (need to preserve quoted strings): var cmdTokens = cmdString.Split(' ');
                    var tokens = new List<string>(Regex.Split(cmdString, @"(?<match>\w+)|(?<match>\""[\w\s\.\']*"")|(?<match>'[\w\s\.]*')|(?<match>\{[\w\s\.]*\})"));
                    tokens.RemoveAll(s => string.IsNullOrWhiteSpace(s));
                    var cmdTokens = tokens.ToArray();

                    var cmdName = cmdTokens[0];
                    var title = YarnConverter.EntryTitle.RunCommand;
                    var desc = string.Format(YarnConverter.EntryDescription.RunCommand, cmdString);

                    if (cmdName == YarnCommand.WaitName)
                    {
                        if (basicStmt.HasArguments)
                        {
                            Debug.LogWarning(YarnConverter.EntryDescription.UnsupportedWait);
                            title = YarnConverter.EntryTitle.UnsupportedWait;
                            desc = YarnConverter.EntryDescription.UnsupportedWait;
                            var amount = cmdTokens.Length > 1 ? cmdTokens[1] : "0";
                            var dlgEntry = CreateDialogueEntry(conversation, title, desc);
                            dlgEntry.Sequence = EntrySequence.ContinueDialogue;
                            basicStmt.AddDialogueEntry(dlgEntry);
                        }
                        else
                        {
                            var dlgEntry = CreateDialogueEntry(conversation, title, desc);
                            var amount = cmdTokens.Length > 1 ? cmdTokens[1] : "0";
                            dlgEntry.Sequence = String.Format(EntrySequence.DelayDialogue, amount);
                            basicStmt.AddDialogueEntry(dlgEntry);
                        }
                    }
                    else if (cmdName == "seq") // Handle special <<seq SequencerCommand()>>
                    {
                        var sequence = basicStmt.LastInstruction.Operands[0].StringValue.Substring("seq ".Length);
                        if (!string.IsNullOrWhiteSpace(sequence))
                        {
                            var dlgEntry = CreateDialogueEntry(conversation, "Sequence", desc);
                            dlgEntry.Sequence = sequence;
                            basicStmt.AddDialogueEntry(dlgEntry);
                        }
                    }
                    // else if (basicStmt.HasArguments)
                    else
                    {
                        var cmd = FormatLuaCustomCommand(cmdName) + "(";
                        var sep = string.Empty;

                        var calculatedArgIndex = 1;
                        for (var index = 1; index < cmdTokens.Length; ++index)
                        {
                            var cmdToken = cmdTokens[index];

                            // Put quotes around strings:
                            var isString = !(IsNumber(cmdToken) || IsBool(cmdToken));
                            var needsQuote = isString &&
                                !(cmdToken.Length >= 2 && IsQuoteChar(cmdToken[0]) && IsQuoteChar(cmdToken[cmdToken.Length - 1]));
                            var quote = needsQuote ? "'" : string.Empty;

                            // If we've found a placeholder for a runtime calculated arg,
                            // pull it from the calculated arg table in the lua script.
                            if (basicStmt.HasArguments && Lua.RunCommandRuntimeArgumentRegex.IsMatch(cmdToken))
                            {
                                cmd += $"{sep}{Lua.RunCommandRuntimeArgumentList}[{calculatedArgIndex}]";
                                ++calculatedArgIndex;
                            }
                            else
                            {
                                cmd += $"{sep}{quote}{cmdToken}{quote}";
                            }

                            sep = ", ";

                        }
                        cmd += ")";

                        var dlgEntry = CreateDialogueEntry(conversation, title, desc);

                        if (basicStmt.HasArguments)
                        {
                            dlgEntry.userScript = string.Format(
                                Lua.RunCommandWithRuntimeArguments, cmd, GenerateYarnInstructionListScript(basicStmt));
                        }
                        else
                        {
                            dlgEntry.userScript = cmd + "\n";
                        }

                        dlgEntry.Sequence = EntrySequence.ContinueDialogue;
                        basicStmt.AddDialogueEntry(dlgEntry);
                    }
                }
                else if (opCode.IsRunLine())
                {
                    var title = YarnConverter.EntryTitle.RunLine;
                    var desc = string.Format(YarnConverter.EntryDescription.RunLine, textKey);
                    var dlgEntry = CreateDialogueEntry(conversation, title, desc);
                    SetDialogueEntryActors(dlgEntry, textKey);
                    SetDialogueEntryText(dlgEntry, conversation.Title, textKey);
                    dlgEntry.Sequence = EntrySequence.ContinueDialogue;
                    basicStmt.AddDialogueEntry(dlgEntry);

                    // If the runline stmt has runtime arguments, we need to create a node to calculate them
                    if (basicStmt.HasArguments)
                    {
                        var fmtRunLineTitle = YarnConverter.EntryTitle.FormatRunLine;
                        var fmtRunLineDescription = string.Format(YarnConverter.EntryDescription.FormatRunLine, textKey, conversation.Title);
                        var entry = CreateDialogueEntry(conversation, fmtRunLineTitle, fmtRunLineDescription);
                        // var fmtArgsScript = $"{GenerateFormattedStringArguments(cId, dId, basicStmt)}";
                        // previousEntry.userScript += fmtArgsScript;
                        entry.userScript = $"{GenerateFormattedStringArguments(conversation.id, dlgEntry.id, basicStmt)}";
                        entry.Sequence = EntrySequence.ContinueDialogue;
                        basicStmt.AddDialogueEntry(entry);
                    }
                }
                // Run Nodes with no node specified are associated with AddOption instructions, so ignore them
                else if (opCode.IsRunNode() && basicStmt.BranchLabel != null)
                {
                    var title = YarnConverter.EntryTitle.RunNode;
                    var desc = string.Format(YarnConverter.EntryDescription.RunNode, basicStmt.BranchLabel);
                    var dlgEntry = CreateDialogueEntry(conversation, title, desc);
                    basicStmt.AddDialogueEntry(dlgEntry);

                    var unrchTitle = YarnConverter.EntryTitle.UnreachableEntry;
                    var unrchDesc = string.Format(YarnConverter.EntryDescription.UnreachableEntry, basicStmt.BranchLabel);
                    var unreachableCodeEntry = CreateDialogueEntry(conversation, unrchTitle, unrchDesc);
                    basicStmt.AddDialogueEntry(unreachableCodeEntry);
                }
                else if (opCode.IsStoreVariable())
                {
                    var varName = basicStmt.MainInstruction.Operands[0].StringValue;
                    var script = GenerateSetVariableScript(varName, basicStmt);
                    var title = YarnConverter.EntryTitle.StoreVariable;
                    var desc = string.Format(YarnConverter.EntryDescription.StoreVariable, varName);
                    var dlgEntry = CreateDialogueEntry(conversation, title, desc);
                    dlgEntry.userScript = script;
                    dlgEntry.Sequence = EntrySequence.ContinueDialogue;
                    basicStmt.AddDialogueEntry(dlgEntry);
                    // if (_prefs.debug) Debug.Log($"CreateDialogueEntriesFromStatement() - store variable {script}");
                }
                else if (opCode.IsStop())
                {
                    var title = YarnConverter.EntryTitle.Stop;
                    var desc = YarnConverter.EntryDescription.Stop;

                    var dlgEntry = CreateDialogueEntry(conversation, title, desc);
                    dlgEntry.conditionsString = "false";
                    basicStmt.AddDialogueEntry(dlgEntry);
                }
                else if (opCode.IsPop())
                {
                    // Do nothing, just explicitly defining the case here
                    // so that the reader understands it's purposely ignored.
                }

                // Set the sequence if specified by user
                if (basicStmt.Sequence != null && basicStmt.Sequence != string.Empty)
                {
                    basicStmt.DialogueEntry.Sequence = basicStmt.Sequence;
                    // if (_prefs.debug) Debug.Log($"YarnToDialogueSystemConverter::CreateAndSetStatementDialogue() - setting sequence to: {basicStmt.DialogueEntry.Sequence} for dialogue entry with text: {basicStmt.DialogueEntry.DialogueText}");
                }
            }
            else if (stmt.Type.IsConversation())
            {
                Assert.AreEqual(conversation, (stmt as ConversationNode).Conversation, "Oops, expected statement to be a Conversation");
                var conversationNode = (ConversationNode)stmt;
                // if (_prefs.debug) Debug.Log($"YarnToDialogueSystemConverter::CreateAndSetStatementDialogue() - creating start dialogue entry for convo: {conversationNode.Name}");

                var startTitle = YarnConverter.EntryTitle.ConversationStart;
                var startDescription = string.Format(YarnConverter.EntryDescription.ConversationStart, conversationNode.Name);
                var endDescription = string.Format(YarnConverter.EntryDescription.ConversationEnd, conversationNode.Name);

                var startDlgEntry = CreateDialogueEntry(conversation, startTitle, startDescription);
                if (_prefs.debug) Debug.Log($"Start dialogue entry actor id: {startDlgEntry.ActorID} conversant id: {startDlgEntry.ConversantID}");

                startDlgEntry.Sequence = EntrySequence.ConversationStart;
                conversationNode.AddDialogueEntry(startDlgEntry);

                // Create the final options group
                if (conversationNode.Options.Count > 0)
                {
                    var optGrpTitle = YarnConverter.EntryTitle.OptionGroup;
                    var optGrpDesc = string.Format(YarnConverter.EntryDescription.OptionGroup, conversationNode.Name);
                    var endDlgEntry = CreateDialogueEntry(conversation, optGrpTitle, optGrpDesc);
                    endDlgEntry.isGroup = true;
                    conversationNode.AddDialogueEntry(endDlgEntry);
                }

                if (_prefs.debug) Debug.Log($"After adding Start dialogue entry to convo, actor id: {startDlgEntry.ActorID} conversant id: {startDlgEntry.ConversantID}");
            }
            else if (stmt.Type.IsIfBlock())
            {
                var ifBlock = (IfBlock)stmt;

                var startTitle = YarnConverter.EntryTitle.IfBlockStart;
                var endTitle = YarnConverter.EntryTitle.IfBlockEnd;
                var startDescription = string.Format(YarnConverter.EntryDescription.IfBlockStart, ifBlock.ExitLabel);
                var endDescription = string.Format(YarnConverter.EntryDescription.IfBlockEnd, ifBlock.ExitLabel);

                var startDlgEntry = CreateDialogueEntry(conversation, startTitle, startDescription);
                startDlgEntry.isGroup = true;
                ifBlock.AddDialogueEntry(startDlgEntry);

                var endDlgEntry = CreateDialogueEntry(conversation, endTitle, endDescription);
                endDlgEntry.Sequence = EntrySequence.ContinueDialogue;
                ifBlock.AddDialogueEntry(endDlgEntry);
            }
            else if (stmt.Type.IsIfClause())
            {
                var ifClause = (IfClause)stmt;
                var title = string.Format(YarnConverter.EntryTitle.IfClauseStart, ifClause.ClauseType.ToString(), ifClause.Parent.ExitLabel);
                var desc = string.Format(YarnConverter.EntryDescription.IfClauseStart, ifClause.ClauseType.ToString(), ifClause.Parent.ExitLabel);

                var startDlgEntry = CreateDialogueEntry(conversation, title, desc);
                startDlgEntry.Sequence = EntrySequence.ContinueDialogue;
                ifClause.AddDialogueEntry(startDlgEntry);

                if (!ifClause.ClauseType.IsElse())
                {
                    ifClause.StartDialogueEntry.conditionsString = GenerateConditionsStringScript(ifClause.Conditions);

                    var fallbackTitle = string.Format(YarnConverter.EntryTitle.IfClauseFallback, ifClause.ClauseType.ToString());
                    var fallbackDesc = string.Format(YarnConverter.EntryDescription.IfClauseFallback, ifClause.ClauseType.ToString(), ifClause.Parent.ExitLabel);
                    var fallbackDlgEntry = CreateDialogueEntry(conversation, fallbackTitle, fallbackDesc);
                    fallbackDlgEntry.isGroup = true;
                    ifClause.AddDialogueEntry(fallbackDlgEntry);
                }
            }
            else if (stmt.Type.IsShortcutOptionList())
            {
                var optList = (ShortcutOptionList)stmt;

                var startTitle = YarnConverter.EntryTitle.ShortcutOptionListStart;
                var startDesc = string.Format(YarnConverter.EntryDescription.ShortcutOptionListStart, optList.ExitLabel);
                var startDlgEntry = CreateDialogueEntry(conversation, startTitle, startDesc);
                startDlgEntry.isGroup = true;
                optList.AddDialogueEntry(startDlgEntry);

                var endTitle = YarnConverter.EntryTitle.ShortcutOptionListEnd;
                var endDesc = string.Format(YarnConverter.EntryDescription.ShortcutOptionListEnd, optList.ExitLabel);
                var endDlgEntry = CreateDialogueEntry(conversation, endTitle, endDesc);
                endDlgEntry.Sequence = EntrySequence.ContinueDialogue;
                optList.AddDialogueEntry(endDlgEntry);
            }
            else if (stmt.Type.IsShortcutOption())
            {
                var opt = (ShortcutOption)stmt;

                var textKey = opt.Option.LastInstruction.Operands[0].StringValue;
                var text = _yarnProject.StringTable[textKey].text;

                if (_prefs.debug) Debug.Log($"YarnToDialogueSystemConverter::CreateAndSetStatementDialogue() - creating shortcut option with text: {text}");

                var startTitle = YarnConverter.EntryTitle.ShortcutOptionStart;
                var startDesc = string.Format(YarnConverter.EntryDescription.ShortcutOptionStart, opt.Option.BranchLabel, opt.Parent.ExitLabel);
                var startDlgEntry = CreateDialogueEntry(conversation, startTitle, startDesc);
                SetPlayerChoiceActors(startDlgEntry);
                SetDialogueEntryText(startDlgEntry, conversation.Title, textKey);
                opt.AddDialogueEntry(startDlgEntry);

                var endTitle = YarnConverter.EntryTitle.ShortcutOptionEnd;
                var endDesc = string.Format(YarnConverter.EntryDescription.ShortcutOptionEnd, opt.Option.BranchLabel, opt.Parent.ExitLabel);
                var endDlgEntry = CreateDialogueEntry(conversation, endTitle, endDesc);
                endDlgEntry.Sequence = EntrySequence.ContinueDialogue;
                opt.AddDialogueEntry(endDlgEntry);

                if (opt.HasConditions)
                {
                    opt.StartDialogueEntry.conditionsString = GenerateConditionsStringScript(opt.Conditions);
                }
            }
        }

        private bool IsNumber(string token)
        {
            double d;
            return double.TryParse(token, out d);
        }

        private bool IsBool(string token)
        {
            bool b;
            return bool.TryParse(token, out b);
        }

        private bool IsQuoteChar(char c)
        {
            return c == '\'' || c == '"';
        }

        private DialogueEntry CreateDialogueEntry(Conversation conversation, string title = null, string description = null)
        {
            var dialogueEntry = _template.CreateDialogueEntry(_template.GetNextDialogueEntryID(conversation), conversation.id, title);

            // We will always default to Dialogue Entries being spoken by the default NPC (i.e. not spoken by the Player)
            // When we set the text for this entry, we'll properly determine who speaks the line.
            dialogueEntry.ActorID = _defaultNpcActor.id;
            dialogueEntry.ConversantID = _playerActor.id;

            // Actor ID is set when the DialogueText field is populated.
            // Conversant ID is never set, as it is not a concept explicitly defined in Yarn.
            conversation.dialogueEntries.Add(dialogueEntry);
            Field.SetValue(dialogueEntry.fields, "Description", description, FieldType.Text);

            return dialogueEntry;
        }

        // This method creates any actors it finds (e.g. "Sally: Hello!" creates the NPC Sally)
        private void SetDialogueEntryActors(DialogueEntry entry, string textKey)
        {
            if (_prefs.debug) Debug.Log($"Setting dialogue entry actors for entry with title {entry.Title}");
            // Always extract the actor using the default locale's dialogue entry text.
            // We will localize the actor names in a different method (SetDialogueEntryText).
            var defaultText = _yarnProject.StringTable[textKey].text;

            Actor actor = _playerActor;
            var actorName = ExtractActorNameFromDialogueText(defaultText);
            if (actorName != null)
            {
                actor = GetOrCreateActor(actorName, false);
            }

            entry.ActorID = actor.id;

            // If the actor is the player, conversant ID is set to the default NPC.
            // If the actor is NOT the player, the conversant ID is set to the player.
            entry.ConversantID = (actor != _playerActor) ? _playerActor.id : _defaultNpcActor.id;
        }

        private void SetPlayerChoiceActors(DialogueEntry entry)
        {
            if (_prefs.debug) Debug.Log($"YarnConverter::SetPlayerChoiceActors() - Setting actor ids for entry with title: {entry.Title}");

            // NOTE: Technically, in Yarn this may not be spoken by the player.
            //       However, when we convert to the Dialogue System
            //       these lines will always be delivered by the player.
            //       For that reason I am not extracting actor names from these
            //       dialogue text strings. Is this the correct call?
            //       I don't know, but if the person in Yarn speaking this is NOT
            //       the player, then if they have no other lines that are just
            //       regular dialogue text, we'll be sticking an actor in the
            //       database who has no lines. So why even bother?
            //       If the actor does have other lines that are regular RunLine
            //       statements, we'll add them at that point.
            entry.ActorID = _playerActor.id;
            entry.ConversantID = _defaultNpcActor.id;
        }

        // NOTE: This method's name probably isn't the best.
        //       It does set the dialogue entry text, but also performs much more than that:
        //          1. It sets the dialogue entry text from the Node's string table (using the default locale)
        //          2. It looks into the (previously) cached localized string tables to add all translations of the text
        //          3. If specified, it extracts the actor's name from the text and updates the DialogueEntry.ActorID field
        //          4. If specified, it strips the actor name prefix from the text before storing it in the database.
        private void SetDialogueEntryText(DialogueEntry entry, string nodeName, string textKey)
        {
            if (_prefs.debug) Debug.Log($"YarnConverter::SetDialogueEntryText() - Setting dialogue text for entry with title: {entry.Title}, node: {nodeName}, key: {textKey}");
            // if (_prefs.debug) Debug.Log($"CreateLocalizedDialogueEntryText({nodeName})");
            var actor = _dialogueDb.GetActor(entry.ActorID);

            var defaultText = _yarnProject.StringTable[textKey].text;
            entry.DialogueText = TruncateDialogueTextPrefix(defaultText);

            if (localizedStringTable.ContainsKey(nodeName))
            {
                var keyMap = localizedStringTable[nodeName];
                if (_prefs.debug) Debug.Log($"CreateLocalizedDialogueEntryText({nodeName}) - found key map");
                if (keyMap.ContainsKey(textKey))
                {
                    if (_prefs.debug) Debug.Log($"CreateLocalizedDialogueEntryText({nodeName}) - found text key: {textKey}");
                    var stringMap = keyMap[textKey];
                    foreach (var stringMapEntry in stringMap)
                    {
                        var locale = stringMapEntry.Key;
                        var text = stringMapEntry.Value;
                        if (_prefs.debug) Debug.Log($"CreateLocalizedDialogueEntryText({nodeName}) - found string map: lcl: {locale} txt: {text}");
                        var lclActorName = ExtractActorNameFromDialogueText(text);
                        if (lclActorName != null)
                        {
                            var fieldName = $"Display Name {locale}";
                            Field.SetValue(actor.fields, fieldName, lclActorName, FieldType.Localization);

                            text = TruncateDialogueTextPrefix(text);
                        }

                        Field.SetValue(entry.fields, locale, text, FieldType.Localization);
                    }
                }
            }
        }

        private string ExtractActorNameFromDialogueText(string text)
        {
            var actorRegex = _prefs.actorRegex?.Trim();
            if (string.IsNullOrEmpty(actorRegex))
            {
                return _playerActor.Name;
            }

            MatchCollection mc = Regex.Matches(text, _prefs.actorRegex);

            // There have to be at least two capture groups in order for the actor name to be successfully extracted.
            if (mc.Count <= 0 || mc[0].Groups.Count < 2)
            {
                return null;
            }

            var actorName = mc[0].Groups[1].Captures[0].Value;
            // if (_prefs.debug) Debug.Log($"ExtractActorNameFromDialogueText() - capture group count: {mc[0].Groups[1].Captures.Count} extracted actor name: '{actorName}'");
            Debug.Log($"Extracted actor with name: {actorName}");
            return actorName;
        }

        // NOTE: This regex pattern is expected to appear at the front of the dialogue text ONLY.
        //       The logic will not work if it's buried in the middle or end of the string.
        private string TruncateDialogueTextPrefix(string text)
        {
            var linePrefixRegex = _prefs.linePrefixRegex?.Trim();
            // if (linePrefixRegex == string.Empty)
            if (string.IsNullOrEmpty(linePrefixRegex))
            {
                return text;
            }

            MatchCollection mc = Regex.Matches(text, _prefs.linePrefixRegex);
            if (mc.Count <= 0)
            {
                return text;
            }

            var matchedSubstring = mc[0].Groups[0].Value;
            var truncatedText = text.Substring(matchedSubstring.Length);
            // if (_prefs.debug) Debug.Log($"StripActorFromDialogueText() - stripped text: '{truncatedText}'");
            return truncatedText;
        }

        private Link CreateLink(DialogueEntry from, DialogueEntry to)
        {
            var link = new Link(from.conversationID, from.id, to.conversationID, to.id);
            from.outgoingLinks.Add(link);

            return link;
        }

        public void GenerateGlobalUserScript()
        {
            // TODO: Find a minifier that actually works with this version of Lua
            // To see the non-minified version, look at the file UnityProjectRoot/WorkspaceLua/EvaluateExpression.lua
            // Always modify the non-minified version, and then once it is working properly, minify it and stick it here.
            var script =
@"-- WARNING: This script MUST be run in order for Yarn functionality to work.
--          When modifying the Global User Script field, add on to the end of this script.
YarnOperatorType = {
    -- Call Func, everything else is a built-in operator
    CallFunc = nil,

    -- Arithmetic
    UnaryMinus = function(arg) return -arg end, -- only unary arithmetic op, rest are binary. Not sure why it isn't called negate tho ...
    Add = function(lhs, rhs)
        -- Take Yarn string concatenation with the '+' operator into account
        if type(lhs) == 'string' or type(rhs) == 'string' then
            return tostring(lhs) .. tostring(rhs)
        else
            return lhs + rhs
        end
    end,
    Minus = function(lhs, rhs) return lhs - rhs end,
    Multiply = function(lhs, rhs) return lhs * rhs end,
    Divide = function(lhs, rhs) return lhs / rhs end,
    Modulo = function(lhs, rhs) return lhs % rhs end,

    -- Arithmetic Assign: UNSUPPORTED, SHOULD NEVER SHOW UP IN CODE
    AddAssign = function(lhs, rhs) return nil end,
    MinusAssign = function(lhs, rhs) return nil end,
    MultiplyAssign = function(lhs, rhs) return nil end,
    DivideAssign = function(lhs, rhs) return nil end,
    EqualToOrAssign = function(lhs, rhs) return nil end,

    -- Logical Operators
    Or = function(lhs, rhs) return lhs or rhs end,
    And = function(lhs, rhs) return lhs and rhs end,
    Xor = function(lhs, rhs)
        return (lhs and (not rhs)) or ((not lhs) and rhs)
    end,
    Not = function(lhs, rhs) return not rhs end,

    -- Relational Operators
    EqualTo = function(lhs, rhs) return lhs == rhs end,
    GreaterThan = function(lhs, rhs) return lhs > rhs end,
    GreaterThanOrEqualTo = function(lhs, rhs) return lhs >= rhs end,
    LessThan = function(lhs, rhs) return lhs < rhs end,
    LessThanOrEqualTo = function(lhs, rhs) return lhs <= rhs end,
    NotEqualTo = function(lhs, rhs) return lhs ~= rhs end
}

-- Computes an expression (or set of expressions)
function EvaluateYarnExpression(instruction_list)
    -- Holds a list of operands
    -- print ('total instructions is nil: ' .. (instruction_list == nil))
    local arg_stack = {}

    -- Loop through the list of instructions from Yarn
    -- Each instruction will either be a type of Push:
    --   + Number/Bool/String
    --   + Null (not sure if this can actually ever show up here)
    --   + Variable
    for _, instruction in ipairs(instruction_list) do
        -- print('DOES THIS FKN EVEN WORK!?!?!')
        local inst_type = type(instruction)
        -- print('instruction loop - type: ' .. (inst_type or 'nil'))

        if type(instruction) == 'function' then
            -- The list of args that will be immediately passed to the current operation/function
            local operator_args = {}

            -- Top operand on the stack is the number argument count for the operation
            local arg_cnt = table.remove(arg_stack)
            -- print('arg_cnt: ' .. (arg_cnt or 'nil'))

            -- Populate operator_args
            for i = 1, arg_cnt do
                local result = table.remove(arg_stack)
                -- print('adding arg to operator_args: ' .. result)
                table.insert(operator_args, result)
            end

            -- local result = instruction(table.unpack(operator_args))
            local result = instruction(ReverseExpandArgsHack(arg_cnt, operator_args))
            -- print('result of operation/function: ' .. (result or 'nil'))
            table.insert(arg_stack, result)
        else
            -- If we're not performing an operation, push the current value on the stack
            -- print('pushing: ' .. (instruction or 'nil'))
            table.insert(arg_stack, instruction)
        end
    end

    -- If there are multiple expressions, this table will have multiple results.
    -- Because the table was used as a stack, its results will be in reverse order,
    -- so maybe reverse this before returning?
    -- print('returning from EvaluateYarnExpression, first value: ' .. arg_stack[1])
    return arg_stack
end

function ReverseExpandArgsHack(num, args)
    if num < 1 then
        return nil
    elseif num == 1 then
        return args[1]
    elseif num == 2 then
        return args[2], args[1]
    elseif num == 3 then
        return args[3], args[2], args[1]
    elseif num == 4 then
        return args[4], args[3], args[2], args[1]
    elseif num == 5 then
        return args[5], args[4], args[3], args[2], args[1]
    elseif num == 6 then
        return args[6], args[5], args[4], args[3], args[2], args[1]
    elseif num == 7 then
        return args[7], args[6], args[5], args[4], args[3], args[2], args[1]
    elseif num == 8 then
        return args[8], args[7], args[6], args[5], args[4], args[3], args[2], args[1]
    elseif num == 9 then
        return args[9], args[8], args[7], args[6], args[5], args[4], args[3], args[2], args[1]
    end

    return args[10], args[9], args[8], args[7], args[6], args[5], args[4], args[3], args[2], args[1]
end
";

            if (_prefs.overwrite)
            {
                _dialogueDb.globalUserScript = script;
            }
            else
            {
                // NOTE: Make sure this script is ALWAYS at the top of the Global User Script
                //       It should warn users when modifying by hand. Not much that can be done
                //       to warn users if they're doing it through code, unfortunately.
                _dialogueDb.globalUserScript = script + _dialogueDb.globalUserScript;
            }
        }

        public void GenerateCustomCommandBaseClass()
        {
            var registerCalls = new List<string>();
            var unregisterCalls = new List<string>();

            var sortedCmdNames = new List<string>(_yarnProject.Commands.Keys);
            sortedCmdNames.Sort();
            foreach (var cmdName in sortedCmdNames)
            {
                var cmd = _yarnProject.Commands[cmdName];
                if (cmd.IsBuiltIn || nameToBuiltInOperatorMap.ContainsKey(cmd.name))
                {
                    continue;
                }

                var paramList = string.Empty;
                var sep = string.Empty;
                for (var cnt = 0; cnt < cmd.parameterCount; ++cnt)
                {
                    paramList += sep + "string.Empty";
                    sep = ", ";
                }

                var regCall = $"Lua.RegisterFunction(\"{cmdName}\", this, SymbolExtensions.GetMethodInfo(() => <MethodName>({paramList})));";
                registerCalls.Add(regCall);

                var unregCall = $"Lua.UnregisterFunction(\"{cmdName}\");";
                unregisterCalls.Add(unregCall);
            }

            var srcCode =
$@"// WARNING: Do not modify this file. It is automatically generated on every Yarn project import.
//          To add/change behavior, make a subclass of <CLASSNAME>.
//          Generated on: {DateTime.Now}

using System;
using System.Collections.Generic;
using UnityEngine;
using Yarn;

namespace PixelCrushers.DialogueSystem.Yarn
{{
    public class <CLASSNAME> : MonoBehaviour
    {{
        public const string ClearAndAddStringFormatArgumentLuaName = ""{ClearAndAddStringFormatArgumentLuaName}"";
        public const string AddStringFormatArgumentLuaName = ""{AddStringFormatArgumentLuaName}"";

        // Can't pass an array of values from Lua to C#, only primitives.
        // So arguments to string.Format functions must be individually added to a list
        // by calling AddStringFormatArgument().
        private Dictionary<int, Dictionary<int, List<string>>> stringFormatArgumentMap = new Dictionary<int, Dictionary<int, List<string>>>();

        private static bool isRegistered = false;
        private static bool didIRegister = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {{
            isRegistered = false;
            didIRegister = false;
        }}

        public virtual void OnEnable()
        {{
            if (!isRegistered)
            {{
                isRegistered = true;
                didIRegister = true;
                RegisterFunctions();
            }}
        }}

        public virtual void OnDisable()
        {{
            if (isRegistered && didIRegister)
            {{
                isRegistered = false;
                didIRegister = false;
                UnregisterFunctions();
            }}
        }}

        public virtual void RegisterFunctions()
        {{
            // Debug.Log(""YarnCustomCommands::OnEnable()"");
            // These calls register your custom commands. You must replace two sections of each register call:
            // <MethodName>: Replace this with your custom command implementation's method call
            // <Parameters>: Replace this with a list of arguments, of the proper type, to complete your method call
            //               It is fine to use the default value for argument types, e.g.:
            //                  BoolValue: false
            //                  FloatValue: 0F
            //                  None: null or 0
            //                  StringValue: string.Empty
            Lua.RegisterFunction(ClearAndAddStringFormatArgumentLuaName, this, SymbolExtensions.GetMethodInfo(() => ClearAndAddStringFormatArgument(0D, 0D, string.Empty)));
            // Debug.Log($""YarnCustomCommands::OnEnable() - Registering lua function: {{ClearAndAddStringFormatArgumentLuaName}}"");
            Lua.RegisterFunction(AddStringFormatArgumentLuaName, this, SymbolExtensions.GetMethodInfo(() => AddStringFormatArgument(0D, 0D, string.Empty)));
            // Debug.Log($""YarnCustomCommands::OnEnable() - Registering lua function: {{AddStringFormatArgumentLuaName}}"");
        }}

        public virtual void UnregisterFunctions()
        {{
            // Debug.Log(""YarnCustomCommands::OnDisable()"");
            // Note: If this script is on your Dialogue Manager & the Dialogue Manager is configured
            // as Don't Destroy On Load (on by default), don't unregister Lua functions.
            Lua.UnregisterFunction(ClearAndAddStringFormatArgumentLuaName);
            // Debug.Log($""YarnCustomCommands::OnEnable() - Unregistering lua function: {{ClearAndAddStringFormatArgumentLuaName}}"");
            Lua.UnregisterFunction(AddStringFormatArgumentLuaName);
            // Debug.Log($""YarnCustomCommands::OnDisable() - Unregistering lua function: {{AddStringFormatArgumentLuaName}}"");
        }}

        public virtual void OnConversationLine(Subtitle subtitle)
        {{
            // Debug.Log($""YarnCustomCommands::OnConversationLine(\""{{subtitle}}\"")"");
            if (!string.IsNullOrEmpty(subtitle.formattedText.text?.Trim()))
            {{
                subtitle.formattedText.text = FormatText(
                    subtitle.dialogueEntry.conversationID,
                    subtitle.dialogueEntry.id,
                    subtitle.formattedText.text);
            }}
        }}

        public virtual void OnConversationResponseMenu(Response[] responses)
        {{
            // Debug.Log(""YarnCustomCommands::OnConversationResponseMenu()"");
            foreach (Response response in responses)
            {{
                if (!string.IsNullOrEmpty(response.formattedText.text?.Trim()))
                {{
                    response.formattedText.text = FormatText(
                        response.destinationEntry.conversationID,
                        response.destinationEntry.id,
                        response.formattedText.text);
                }}
            }}
        }}

        public virtual void OnConversationEnd(Transform actor)
        {{
            // Debug.Log(""YarnCustomCommands::OnConversationEnd()"");
            ClearConversationStringArgumentsMap(DialogueManager.instance.lastConversationID);
        }}

        // Calls string.Format (if there are any args) on the text, then runs it through Yarn's format function logic.
        private string FormatText(int conversationId, int dialogueEntryId, string text)
        {{
            // Debug.Log($""YarnCustomCommands::FormatText({{conversationId}}, {{dialogueEntryId}}, \""{{text}}\"")"");

            // Grab the string, then format it if necessary.
            // We know this if there are format arguments for the dialogue entry in string arguments map.
            var displayText = text;

            if (stringFormatArgumentMap.ContainsKey(conversationId) && stringFormatArgumentMap[conversationId].ContainsKey(dialogueEntryId))
            {{
                var args = stringFormatArgumentMap[conversationId][dialogueEntryId];

                if (args.Count > 0)
                {{
                    displayText = String.Format(displayText, args.ToArray());
                }}
            }}

            // Run it through Yarn's string format function handler
            // var locale = Localization.Language;
            var result = Dialogue.ExpandFormatFunctions(displayText, Localization.Language);
            return result;
        }}

        private void ClearConversationStringArgumentsMap(int conversationId)
        {{
            // Debug.Log($""YarnCustomCommands::ClearConversationStringArgumentsMap({{conversationId}})"");
            var cId = DialogueManager.instance.lastConversationID;
            if (stringFormatArgumentMap.ContainsKey(cId))
            {{
                // Probably unnecessary to do before clearing the map,
                // but let's just do it anyway.
                foreach (var argsEntry in stringFormatArgumentMap[cId])
                {{
                    argsEntry.Value.Clear();
                }}

                stringFormatArgumentMap.Clear();
            }}
        }}

        // Call this method with the first argument for string.Format. This clears the list before adding anything,
        // making sure that no extra args are present from previous runs. This is extra defensive, as all string arg
        // lists associated with a conversation are cleared on conversation start and end but hey, can't hurt to be careful!
        public void ClearAndAddStringFormatArgument(double conversationId, double dialogueEntryId, string arg)
        {{
            // Debug.Log($""YarnCustomCommands::ClearAndAddStringFormatArgument({{conversationId}}, {{dialogueEntryId}}, \""{{arg}})\"")"");
            var cId = (int)conversationId;
            var dId = (int)dialogueEntryId;

            if (stringFormatArgumentMap.ContainsKey(cId) && stringFormatArgumentMap[cId].ContainsKey(dId))
            {{
                stringFormatArgumentMap[cId][dId].Clear();
            }}

            AddStringFormatArgument(conversationId, dialogueEntryId, arg);
        }}

        public void AddStringFormatArgument(double conversationId, double dialogueEntryId, string arg)
        {{
            // Debug.Log($""YarnCustomCommands::AddStringFormatArgument({{conversationId}}, {{dialogueEntryId}}, \""{{arg}})\"")"");
            var cId = (int)conversationId;
            var dId = (int)dialogueEntryId;

            if (!stringFormatArgumentMap.ContainsKey(cId))
            {{
                stringFormatArgumentMap[cId] = new Dictionary<int, List<string>>();
            }}

            if (!stringFormatArgumentMap[cId].ContainsKey(dId))
            {{
                stringFormatArgumentMap[cId][dId] = new List<string>();
            }}

            stringFormatArgumentMap[cId][dId].Add(arg);
        }}

        // These methods do nothing, but are implemented as placeholders
        // in case functionality needs to be added in the future.
        public virtual void OnConversationStart(Transform actor) {{}}
        public virtual void OnConversationCancelled(Transform actor) {{}}
        public virtual void OnPrepareConversationLine(DialogueEntry entry) {{}}
        public virtual void OnConversationLineEnd(Subtitle subtitle) {{}}
        public virtual void OnConversationLineCancelled(Subtitle subtitle) {{}}
        public virtual void OnConversationTimeout() {{}}
        public virtual void OnLinkedConversationStart(Transform actor) {{}}
        public virtual void OnTextChange(UnityEngine.UI.Text text) {{}}

        public virtual void OnBarkStart(Transform actor) {{}}
        public virtual void OnBarkEnd(Transform actor) {{}}
        public virtual void OnBarkLine(Subtitle subtitle) {{}}
    }}
}}
";

            var filename = _prefs.customCommandsSourceFile;
            if (string.IsNullOrEmpty(filename))
            {
                filename = YarnConverterPrefs.DefaultCustomCommandsSourceFile;
            }
            var className = Path.GetFileNameWithoutExtension(filename);

            using (StreamWriter srcFile = new StreamWriter(filename))
            {
                srcFile.Write(srcCode.Replace("<CLASSNAME>", className));
            }
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif

            Debug.Log($"Yarn custom commands base class file: {filename}");
        }
    }
}
#endif
