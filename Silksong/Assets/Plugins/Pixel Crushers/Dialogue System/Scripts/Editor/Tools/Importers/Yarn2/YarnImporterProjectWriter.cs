#if USE_YARN2

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.Assertions;

namespace PixelCrushers.DialogueSystem.Yarn
{

    // +---------------------------------------------------------------------------------------------------------------
    // + Class: YarnProjectWriter
    // + Description:
    // +    Takes a YarnProject and YarnConverterPrefs, then writes out a DialogueDatabase.
    // +---------------------------------------------------------------------------------------------------------------
    public class YarnImporterProjectWriter
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
            public const string Call = "Call Function";
            public const string Command = "Run Command";
            public const string Jump = "Jump to Conversation";
            public const string FormatLine = "Format Line";
            public const string Line = "Line";
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
            public const string Call = "Call Function - name: '{0}'";
            public const string Command = "Run Command - cmd: '{0}'";
            public const string Jump = "Jump to Conversation {0}";
            public const string FormatLine = "Format Line - line key: {0}, conversation: {1}, formatting line's dialogue";
            public const string Line = "Run Line - line key: {0}";
            public const string StoreVariable = "Store Variable: name: {0}";
            public const string UnreachableEntry = "Unreachable Entry after Jump to conversation: {0}";
        }

        public static class EntrySequence
        {
            public const string ConversationStart = "None()";
            public const string ContinueDialogue = "Continue()";
            public const string DelayDialogue = "Delay({0})";
        }

        public static class DialogueEntryField
        {
            public const string LocalizedDisplayName = "Display Name {0}";
        }

        public static class Lua
        {
            public const string YarnInstructionList = "yarn_instructions";
            public const string ExpressionResultsList = "exp_results";
            public const string EvaluateYarnExpression = "EvaluateYarnExpression";
            public const string RunCommandRuntimeArgumentList = "run_cmd_args";
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
            public const string VariableTableAccess = "Variable['{0}']";

            // This string needs to be run through string.Format with the following arguments
            // 0 - The custom command name
            public const string CustomFunction = "_G['{0}']";

            // This string needs to be run through string.Format with the following arguments
            //  0 - The lua table containing the Yarn instructions, i.e. a call to: GenerateYarnInstructionListScript(BasicStatement)
            public static readonly string ConditionsString = $"{Lua.EvaluateYarnExpression}({{0}})[1]\n";

            // This string needs to be run through string.Format with the following arguments
            //  0 - Formatted Lua Variable table access, i.e. a call to: FormatLuaVariableTableAccess(string)
            //  1 - The lua table containing the Yarn instructions, i.e. a call to: GenerateYarnInstructionListScript(BasicStatement)
            public static readonly string SetVariable = $"{{0}} = {{1}}\n";

            // This string needs to be run through string.Format with the following arguments
            //  0 - Formatted Lua variable, i.e. call: FormatLuaVariableAccess(string)
            public static readonly string OptionConditionsString = $"{{0}} = true\n";

        }

        // TODO: Enclose these in a static class, maybe Actor?
        public const string DefaultPlayerActorName = "Player";
        public const int DefaultPlayerActorId = 1;
        public const string DefaultNpcActorName = "NPC";
        public const int DefaultNpcActorId = 2;

        // TODO: Enclose these in a static class
        // public const string StringInterpolationVariableNameFormat = "${0}_fmt_line_{{0}}";
        // public const string ClearAndAddStringFormatArgumentLuaName = "clr_add_str_fmt_arg";
        // public const string AddStringFormatArgumentLuaName = "add_str_fmt_arg";

        public const string SequenceAttributeName = "seq";
        // Rather than trying to create one all-encompassing unreadable regex,
        // Cycling through these four (in the specified order specified where they're used) should get us what we need.
        public static readonly string StandaloneSequenceWithQuotesRegex = $"^\\[{SequenceAttributeName}=\"(.+)\"\\s*\\/\\]\\s*$";
        public static readonly string StandaloneSequenceWithoutQuotesRegex = $"^\\[{SequenceAttributeName}=([^\"]\\S*)\\s*\\/\\]\\s*$";
        public static readonly string SequenceWithQuotesRegex = $"[^\\\\]\\[{SequenceAttributeName}=\"(.+)\"\\s*\\/\\]\\s*$";
        public static readonly string SequenceWithoutQuotesRegex = $"[^\\\\]\\[{SequenceAttributeName}=([^\"]\\S*)\\s*\\/\\]\\s*$";
        
        private YarnImporterProject _yarnProject = null;

        private DialogueDatabase _dialogueDb = null;
        private Template _template;
        private Actor _playerActor = null;
        private Actor _defaultNpcActor = null;

        // Holds current conversation while we're building the DialogueDatabase tree
        private Conversation _currentConversation;

        private YarnImporterPrefs _prefs;

        public static string FormatLuaVariableTableAccess(string yarnVariableName) => string.Format(Lua.VariableTableAccess, yarnVariableName);
        public static string FormatCustomLuaFunction(string cmdName) => string.Format(Lua.CustomFunction, cmdName);

        // Because it's possible we could be merging new data into a DialogueDatabase,
        // the database must be passed in instead of created and returned from this method.
        public void Write(YarnImporterPrefs prefs, YarnImporterProject project, DialogueDatabase dialogueDb)
        {
            _prefs = prefs;
            _yarnProject = project;
            _dialogueDb = dialogueDb;
            _template = Template.FromDefault();

            PopulateDialogueDatabase();
        }

        private void PopulateDialogueDatabase()
        {
            CreateLuaGlobalVariables();
            CreateDefaultActors();
            CreateConversations();

            var sortedConvoNodes = _yarnProject.Nodes.Values.OrderBy(x => x.Name).ToList();
            foreach (var convoNode in sortedConvoNodes) CreateAndAddDialogueEntries(convoNode);
        }

        private Actor GetOrCreateActor(string name, bool isPlayer = false, int id = -1)
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
            //       If the actor does have other lines that are regular Line
            //       statements, we'll add them at that point.
            entry.ActorID = _playerActor.id;
            entry.ConversantID = _defaultNpcActor.id;
        }

        // private void CreateLuaGlobalVariables()
        // {
        // }
        // private void GetAllDeclareStatements(BlockStatement block, List<DeclareStatement> declareStmts)
        // {
        //     foreach (var stmt in block.Statements)
        //     {
        //         if (stmt.Type.IsDeclare()) declareStmts.Add((DeclareStatement)stmt);
        //         else if (stmt.Type.IsBlock()) GetAllDeclareStatements((BlockStatement)stmt, declareStmts);
        //     }
        // }

        // private void GetAllDeclareStatements(BlockStatement block, List<DeclareStatement> declareStmts)
        // {
        //     foreach (var stmt in block.Statements)
        //     {
        //         if (stmt.Type.IsDeclare()) declareStmts.Add((DeclareStatement)stmt);
        //         else if (stmt.Type.IsBlock()) GetAllDeclareStatements((BlockStatement)stmt, declareStmts);
        //     }
        // }

        private void GetAllStatements<T>(BlockStatement block, List<T> allStmts) where T : YarnStatement
        {
            foreach (var stmt in block.Statements)
            {
                if (stmt is T) allStmts.Add((T)stmt);
                else if (stmt.Type.IsBlock()) GetAllStatements((BlockStatement)stmt, allStmts);
            }
        }

        private void CreateLuaGlobalVariables()
        {
            // Get all statement types that can be used to initialize the global lua var table:
            // DeclareStatement and SetStatment
            var allDeclareStmts = new List<DeclareStatement>();
            var allSetStmts = new List<SetStatement>();
            foreach (var convo in _yarnProject.Nodes.Values)
            {
                GetAllStatements(convo, allDeclareStmts);
                GetAllStatements(convo, allSetStmts);
            }

            // Sort them by name, just for readability
            var sortedDeclareStmts = allDeclareStmts.OrderBy(x => x.Variable).ToList();
            var sortedSetStmts = allSetStmts.OrderBy(x => x.Variable).ToList();

            // First, use all DeclareStatements to create entries in the global lua var table
            foreach (var stmt in sortedDeclareStmts) CreateLuaGlobalVariable(stmt);

            // After we've gone through all DeclareStatements, if there any SetStatements with
            // variables that have not been declared, create them and default their type/value to empty strings.
            foreach (var stmt in sortedSetStmts) CreateLuaGlobalVariable(stmt);
        }

        private void CreateLuaGlobalVariable(SetStatement setStmt)
        {
            // If this database already has some data present (i.e. from a previous Declare/Set statment, or we're merging, etc ...),
            // don't smash variables or create dupes.
            if (_dialogueDb.GetVariable(setStmt.Variable) != null) return;

            var fieldType = FieldType.Text;
            var luaVarName = setStmt.Variable;
            var luaVarValue = string.Empty;

            // We can only confidently determine the variable type if this is a DeclareStatement
            if (setStmt.Type.IsDeclare())
            {
                // Grab the first expression token on the stack, that's the value used to initialize the var.
                // It also will tell us the type.
                var valueToken = setStmt.Expression.StackCopy.Pop();
                switch (valueToken.Type)
                {
                    case ExpressionTokenType.Bool:
                        fieldType = FieldType.Boolean;
                        luaVarValue = ((BoolToken)valueToken).Value.ToString();
                        break;
                    case ExpressionTokenType.Number:
                        fieldType = FieldType.Number;
                        luaVarValue = ((NumberToken)valueToken).Value.ToString();
                        break;
                    case ExpressionTokenType.Text:
                        fieldType = FieldType.Text;
                        luaVarValue = ((StringToken)valueToken).Value;
                        break;
                };
            }

            var luaVar = _template.CreateVariable(_template.GetNextVariableID(_dialogueDb), setStmt.Variable, luaVarValue, fieldType);
            _dialogueDb.variables.Add(luaVar);
        }

        private void CreateConversations()
        {
            // Create all Conversations, at this point they will be empty except for their START dialogue entry
            foreach (var nodeEntry in _yarnProject.Nodes)
            {
                CreateConversation(nodeEntry.Value);
            }
        }

        public Conversation CreateConversation(ConversationNode node)
        {
            var convoName = node.Name;
            var convoNode = node;

            var conversationId = _template.GetNextConversationID(_dialogueDb);
            var conversation = _template.CreateConversation(conversationId, convoName);

            var actor = _playerActor;
            var conversant = _defaultNpcActor;

            if (convoNode.Header.ContainsKey(YarnImporterProject.NodeHeaderActorKey))
            {
                var actorName = convoNode.Header[YarnImporterProject.NodeHeaderActorKey];
                actor = GetOrCreateActor(actorName, true);
                // if (_prefs.debug) Debug.Log($"YarnConverter::GetOrCreateConversation() - should be creating actor actor with name: {actorName}, actor: {actor}, id: {actor.id}");
            }

            if (convoNode.Header.ContainsKey(YarnImporterProject.NodeHeaderConversantKey))
            {
                var conversantName = convoNode.Header[YarnImporterProject.NodeHeaderConversantKey];
                conversant = GetOrCreateActor(conversantName);
                // if (_prefs.debug) Debug.Log($"YarnConverter::GetOrCreateConversation() - should be creating conversant actor with name: {conversantName}, actor: {conversant}, id: {conversant.id}");
            }

            // if (_prefs.debug) Debug.Log($"GetOrCreateConversation() - Set convo {node.Name} actor to {actor.Name} conversant to: {conversant.Name}");
            conversation.ActorID = actor.id;
            conversation.ConversantID = conversant.id;
            _dialogueDb.conversations.Add(conversation);

            var startDlgEntry = CreateDialogueEntry(
                conversation,
                EntryTitle.ConversationStart,
                string.Format(EntryDescription.ConversationStart, convoNode.Name));
            startDlgEntry.Sequence = EntrySequence.ConversationStart;
            if (_prefs.debug) Debug.Log($"Start dialogue entry actor id: {startDlgEntry.ActorID} conversant id: {startDlgEntry.ConversantID}");

            return conversation;
        }

        // The basic gist of all of these CreateAndAddDialogueEntries methods is that each statement type has a single entrance
        // DialogueEntry, and a single exit DialogueEntry (start and end entries can be the same if it's a single entry statement)
        // That is especially true for statements that represent blocks (ShortcutOptionList, IfBlock). All DialogueEntry instances for
        // statements inside those blocks will be created and linked in their respective CreateAndAddDialogueEntries methods.
        // NOTE: Conversation is an exception, as it is a block statement that has one entrance, but possibly multiple exit DialogueEntries.
        private DialogueEntry CreateAndAddDialogueEntries(YarnStatement stmt, DialogueEntry previousEntry)
        {
            // DialogueEntry nextDlgEntry = nullnull;
            var nextDlgEntry = previousEntry;
            switch (stmt.Type)
            {
                case StatementType.Call:
                    nextDlgEntry = CreateAndAddDialogueEntries((CallStatement)stmt, previousEntry);
                    break;

                case StatementType.Command:
                    nextDlgEntry = CreateAndAddDialogueEntries((CommandStatement)stmt, previousEntry);
                    break;

                case StatementType.Jump:
                    nextDlgEntry = CreateAndAddDialogueEntries((JumpStatement)stmt, previousEntry);
                    break;

                case StatementType.Line:
                    nextDlgEntry = CreateAndAddDialogueEntries((LineStatement)stmt, previousEntry);
                    break;

                case StatementType.Declare:
                    // nextDlgEntry = previousEntry;
                    // nextDlgEntry = CreateAndAddDialogueEntries((SetStatement)stmt, previousEntry);
                    break;

                case StatementType.Set:
                    nextDlgEntry = CreateAndAddDialogueEntries((SetStatement)stmt, previousEntry);
                    break;

                case StatementType.ShortcutOptionList:
                    nextDlgEntry = CreateAndAddDialogueEntries((ShortcutOptionList)stmt, previousEntry);
                    break;

                case StatementType.IfBlock:
                    nextDlgEntry = CreateAndAddDialogueEntries((IfBlock)stmt, previousEntry);
                    break;

                case StatementType.Conversation:
                // case StatementType.Declare:
                case StatementType.ShortcutOption:
                case StatementType.IfClause:
                default:
                    throw new Exception($"Invalid call to CreateAndAddDialogueEntries() method with YarnStatement type: {stmt.Type}");
            }

            return nextDlgEntry;
        }

        private void CreateAndAddDialogueEntries(ConversationNode convoNode)
        {
            _currentConversation = _dialogueDb.GetConversation(convoNode.Name);

            // var startTitle = EntryTitle.ConversationStart;
            // var startDescription = string.Format(EntryDescription.ConversationStart, convoNode.Name);
            // var endDescription = string.Format(EntryDescription.ConversationEnd, convoNode.Name);

            // var startDlgEntry = CreateDialogueEntry(_currentConversation, startTitle, startDescription);
            // if (_prefs.debug) Debug.Log($"Start dialogue entry actor id: {startDlgEntry.ActorID} conversant id: {startDlgEntry.ConversantID}");

            // startDlgEntry.Sequence = EntrySequence.ConversationStart;

            // START entry already exists, it's created when the conversation is created.
            var previousEntry = _currentConversation.dialogueEntries[0];
            foreach (var stmt in convoNode.Statements) previousEntry = CreateAndAddDialogueEntries(stmt, previousEntry);
        }

        private DialogueEntry CreateAndAddDialogueEntries(CallStatement stmt, DialogueEntry previousEntry)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectWriter::CreateAndAddDialogueEntries(CallStatement)");
            var expStack = GenerateLuaExpression(stmt.Expression);
            Assert.AreEqual(1, expStack.Count, $"Lua function call should resolve to a single expression for function name: {stmt.Name}");

            var callEntry = CreateDialogueEntry(
                _currentConversation,
                YarnImporterProjectWriter.EntryTitle.Call,
                string.Format(YarnImporterProjectWriter.EntryDescription.Call, stmt.Name));
            // callEntry.userScript = GenerateLuaCommandCall(stmt);
            callEntry.userScript = GenerateLuaExpression(stmt.Expression).Pop();
            callEntry.Sequence = EntrySequence.ContinueDialogue;
            CreateLink(previousEntry, callEntry);

            return callEntry;
        }

        private DialogueEntry CreateAndAddDialogueEntries(JumpStatement stmt, DialogueEntry previousEntry)
        {
            // CreateAndAddDialogueEntries((CommandStatement)stmt, previousEntry);
            var jumpEntry = CreateDialogueEntry(
                _currentConversation,
                YarnImporterProjectWriter.EntryTitle.Jump,
                string.Format(YarnImporterProjectWriter.EntryDescription.Jump, stmt.Destination));
            // callEntry.userScript = GenerateLuaCommandCall(stmt);
            // jumpEntry.userScript = GenerateLuaExpression(stmt.Expression).Pop();
            jumpEntry.Sequence = EntrySequence.ContinueDialogue;
            CreateLink(previousEntry, jumpEntry);

            var dstConvo = _dialogueDb.GetConversation(stmt.Destination);
            var jumpLink = CreateLink(jumpEntry, dstConvo.dialogueEntries[0]);
            jumpLink.isConnector = true;

            // It is perfectly legal to put unreachable statements in a Yarn node right after a <<jump DestinationConversation>> statement.
            // To make sure those statements are represented in the Dialogue Database, an unreachable parent node is used.
            // This behaves just like a Stop node, where its conditionsString is always false.
            var unreachableCodeEntry = CreateDialogueEntry(
                _currentConversation,
                YarnImporterProjectWriter.EntryTitle.UnreachableEntry,
                string.Format(YarnImporterProjectWriter.EntryDescription.UnreachableEntry, stmt.Destination));

            unreachableCodeEntry.conditionsString = "false";
            var unreachableLink = CreateLink(jumpEntry, unreachableCodeEntry);
            unreachableLink.priority = ConditionPriority.BelowNormal;
            // previousEntry = unreachableCodeEntry;

            return unreachableCodeEntry;
        }

        private DialogueEntry CreateAndAddDialogueEntries(CommandStatement stmt, DialogueEntry previousEntry)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectWriter::CreateDialogueEntries(CommandStatement)");

            // Special case for the sequence command. Grab the previous line entry and modify its sequence.
            // No new DialogueEntry is created in this instance, so just pass back the previousEntry with no modification.
            if (BuiltInCommandExt.IsPlaySequence(stmt.Name))
            {
                var seqEntry = CreateDialogueEntry(
                    _currentConversation,
                    YarnImporterProjectWriter.EntryTitle.Command,
                    string.Format(YarnImporterProjectWriter.EntryDescription.Command, stmt.Name));
                seqEntry.Sequence = GenerateSequence(stmt);
                CreateLink(previousEntry, seqEntry);
                return seqEntry;
            }

            // Special case for the wait command.
            if (stmt.CommandType.IsWait())
            {
                var seq = "Delay({0})";
                var delayAmount = string.Empty;
                if (stmt.HasExpression)
                {
                    Assert.AreEqual(1, stmt.ExpressionCount, "Wait command can only have a single number argument");
                    var expStack = GenerateLuaExpression(stmt.Expression);
                    seq = $"Delay([lua({expStack.Pop()})])";
                }
                else
                {
                    // var delayAmount = stmt.StringTokens[0].Value;
                    Assert.IsTrue(stmt.StringTokens.Count >= 1, "Must have at least a single command statement argument for <<wait>>");
                    seq = $"Delay({stmt.StringTokens[0].Value})";
                }

                var waitEntry = CreateDialogueEntry(
                    _currentConversation,
                    YarnImporterProjectWriter.EntryTitle.Command,
                    string.Format(YarnImporterProjectWriter.EntryDescription.Command, stmt.Name));
                waitEntry.Sequence = seq;
                CreateLink(previousEntry, waitEntry);
                return waitEntry;
            }

            var cmdEntry = CreateDialogueEntry(
                _currentConversation,
                YarnImporterProjectWriter.EntryTitle.Command,
                string.Format(YarnImporterProjectWriter.EntryDescription.Command, stmt.Name));
            cmdEntry.userScript = GenerateLuaCommandCall(stmt);
            cmdEntry.Sequence = EntrySequence.ContinueDialogue;
            CreateLink(previousEntry, cmdEntry);

            return cmdEntry;
        }

        private DialogueEntry CreateAndAddDialogueEntries(LineStatement stmt, DialogueEntry previousEntry)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectWriter::CreateDialogueEntries(LineStatement) - line id: {stmt.LineId}");

            // If the line stmt has runtime arguments, that means this it has string format parameters.
            // We need to modify the userScript field of the previous statement to prepare those.
            if (stmt.HasExpression)
            {
                var fmtLineEntry = CreateDialogueEntry(_currentConversation,
                    YarnImporterProjectWriter.EntryTitle.FormatLine,
                    string.Format(YarnImporterProjectWriter.EntryDescription.FormatLine, stmt.LineId, _currentConversation.Title));

                fmtLineEntry.Sequence = EntrySequence.ContinueDialogue;
                CreateLink(previousEntry, fmtLineEntry);
                previousEntry = fmtLineEntry;
            }

            var lineEntry = CreateAndAddDialogueEntry(stmt, previousEntry);
            return lineEntry;
        }

        private DialogueEntry CreateAndAddDialogueEntry(LineStatement stmt, DialogueEntry previousEntry)
        {
            // if (_prefs.debug) Debug.Log($"YarnProjectWriter::CreateAndAddDialogueEntry(LineStatement) - line id: {stmt.LineId}");

            var title = YarnImporterProjectWriter.EntryTitle.Line;
            var desc = string.Format(YarnImporterProjectWriter.EntryDescription.Line, stmt.LineId);
            var lineEntry = CreateDialogueEntry(_currentConversation, title, desc);
            SetDialogueEntryActors(lineEntry, stmt.LineId);
            SetDialogueEntryText(lineEntry, _currentConversation.Title, stmt.LineId);
            if (_prefs.debug) Debug.Log($"YarnProjectWriter::CreateAndAddDialogueEntry(LineStatement) - line id: {stmt.LineId}, text: {lineEntry.DialogueText}");

            if (stmt.HasConditions)
            {
                lineEntry.conditionsString = GenerateLuaExpression(stmt.Conditions).Pop();
                lineEntry.falseConditionAction = "Passthrough";
            }

            if (stmt.HasExpression)
            {
                if (!string.IsNullOrEmpty(previousEntry.userScript)) previousEntry.userScript += "\n";
                previousEntry.userScript += $"{GenerateFormattedStringArguments(_currentConversation.id, lineEntry.id, stmt.Expression)}";
            }

            SetLineEntrySequence(lineEntry);
            CreateLink(previousEntry, lineEntry);

            return lineEntry;
        }

        private DialogueEntry CreateAndAddDialogueEntries(SetStatement stmt, DialogueEntry previousEntry)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectWriter::CreateDialogueEntries(SetStatement) - var: {stmt.Variable}");
            var title = YarnImporterProjectWriter.EntryTitle.StoreVariable;
            var desc = string.Format(YarnImporterProjectWriter.EntryDescription.StoreVariable, stmt.Variable);
            var dlgEntry = CreateDialogueEntry(_currentConversation, title, desc);

            var luaVar = FormatLuaVariableTableAccess(stmt.Variable);
            var expStack = GenerateLuaExpression(stmt.Expression);
            expStack.Pop();
            dlgEntry.userScript = $"{luaVar} = {expStack.Pop()}";
            dlgEntry.Sequence = EntrySequence.ContinueDialogue;

            CreateLink(previousEntry, dlgEntry);

            return dlgEntry;
        }

        private DialogueEntry CreateAndAddDialogueEntries(ShortcutOptionList optList, DialogueEntry previousEntry)
        {
            // if (_prefs.debug) Debug.Log($"YarnProjectWriter::CreateAndAddDialogueEntries(ShortcutOptionList)");
            var optListStartEntry = CreateDialogueEntry(_currentConversation, EntryTitle.ShortcutOptionListStart, EntryDescription.ShortcutOptionListStart);
            optListStartEntry.isGroup = optList.Options.Count > 1;
            optListStartEntry.Sequence = EntrySequence.ContinueDialogue;
            CreateLink(previousEntry, optListStartEntry);

            var optListEndEntry = CreateDialogueEntry(_currentConversation, EntryTitle.ShortcutOptionListEnd, EntryDescription.ShortcutOptionListEnd);
            optListEndEntry.Sequence = EntrySequence.ContinueDialogue;

            var areAllOptionsConditional = true;
            foreach (var option in optList.Options)
            {
                var line = option.Line;
                if (!line.HasConditions) areAllOptionsConditional = false;
                var optEndEntry = CreateAndAddDialogueEntries(option, optListStartEntry);
                CreateLink(optEndEntry, optListEndEntry);
            }

            if (areAllOptionsConditional)
            {
                // This link exists solely to allow the Shortcut Options List to fall through in case
                // there are no shortcut options that can be entered into. This is possible if all shortcut options
                // have conditions that fail.
                var fallbackLink = CreateLink(optListStartEntry, optListEndEntry);
                fallbackLink.priority = ConditionPriority.BelowNormal;
            }

            return optListEndEntry;
        }

        private DialogueEntry CreateAndAddDialogueEntries(ShortcutOption option, DialogueEntry optListStart)
        {
            // The way ShortcutOptions are represented in Yarn (and therefore the YarnProject::ShortcutOption also)
            // doesn't match up that well with how they need to be represented in the DialogueDatabase.
            // So we're going to massage the data a bit to better fit what we're trying to do:
            //  1. Use the LineStatement (the first statement of the ShortcutOption and is the player choice text) to create the option start entry
            //      a. This may or may not have conditions, no big deal if the conditions stack is empty.
            //  2. Set this dialogue entry actors using the SetPlayerChoice method.
            //  3. Loop through and create dialogue entries for all statements in the block, except for the first (the player choice Line statement)
            //
            //  We're effectively merging the two statements (option block, first statement (the line statement) to create our option start entry
            var optStartEntry = CreateAndAddDialogueEntry(option.Line, optListStart);
            optStartEntry.Title = EntryTitle.ShortcutOptionStart;
            Field.SetValue(optStartEntry.fields, "Description", EntryDescription.ShortcutOptionStart, FieldType.Text);
            SetPlayerChoiceActors(optStartEntry);

            if (_prefs.debug) Debug.Log($"YarnProjectWriter::CreateDialogueEntries(ShortcutOption) - has conditions: {option.Line.HasConditions}");
            if (option.Line.HasConditions)
            {
                // If there are conditions, reset the falseConditionAction as it's set to Passthrough by the
                // call to CreateAndAddDialogueEntries() for this line statement
                optStartEntry.conditionsString = GenerateLuaExpression(option.Line.Conditions).Pop();
                optStartEntry.falseConditionAction = "Block";
            }

            // We've already handled the LineStatement directly above, so ignore the first statement in the list
            var previousEntry = optStartEntry;
            for (var i = 1; i < option.Statements.Count; ++i)
            {
                var stmt = option.Statements[i];
                var nextEntry = CreateAndAddDialogueEntries(stmt, previousEntry);
                previousEntry = nextEntry;
            }

            return previousEntry;
        }

        private DialogueEntry CreateAndAddDialogueEntries(IfBlock ifBlock, DialogueEntry previousEntry)
        {
            var ifBlockStartEntry = CreateDialogueEntry(_currentConversation, EntryTitle.IfBlockStart, EntryDescription.IfBlockStart);
            ifBlockStartEntry.isGroup = true;
            ifBlockStartEntry.Sequence = EntrySequence.ContinueDialogue;
            CreateLink(previousEntry, ifBlockStartEntry);

            var ifBlockEndEntry = CreateDialogueEntry(_currentConversation, EntryTitle.IfBlockEnd, EntryDescription.IfBlockEnd);
            ifBlockEndEntry.Sequence = EntrySequence.ContinueDialogue;

            var ifClauseStartList = new List<DialogueEntry>();
            var ifClauseEndList = new List<DialogueEntry>();
            var fallbackEntryList = new List<DialogueEntry>();
            var previousIfClause = ifBlockStartEntry;
            foreach (var ifClause in ifBlock.IfClauses)
            {
                (DialogueEntry ifClauseStart, DialogueEntry ifClauseEnd) = CreateAndAddDialogueEntries(ifClause, ifBlockStartEntry, ifBlockEndEntry);
                ifClauseStartList.Add(ifClauseStart);
                ifClauseEndList.Add(ifClauseEnd);

                if (ifClause.HasConditions)
                {
                    var fbEntry = CreateIfClauseFallbackEntry(ifClause.ClauseType);
                    fallbackEntryList.Add(fbEntry);
                }
            }

            // Okay, we have to resolve all of our dialogue entry links now.
            var previousFbEntry = ifBlockStartEntry;
            for (var i = 0; i < ifBlock.IfClauses.Count; ++i)
            {
                // Don't really need this
                var ifClause = ifBlock.IfClauses[i];
                var ifClauseStartEntry = ifClauseStartList[i];
                var ifClauseEndEntry = ifClauseEndList[i];

                // If this is our first iteration, we have to prime the pump so to say.
                // Create the link between the IfBlock start, and our first if clause start
                // If this is our last iteration, create the final link to the IfBlock end (drain the pump?).
                if (i == 0)
                {
                    CreateLink(ifBlockStartEntry, ifClauseStartEntry);
                }

                // Now, we'll go through the rest of the if block structure and properly set up our
                // if clause and fallback entry links.
                // We know that there will be a fallback link for each if/elseif statement, and none for an else.
                // It is also possible for an IfBlock to end an on an elseif statement, but that just means that
                // the number of fallback entries equals the number of ifclause entries. If there's an else statement
                // in the block, there will be one fewer fallback entries.
                if (i < fallbackEntryList.Count)
                {
                    // Setting up a fallback link requires:
                    //  1. Create the new fallback entry
                    //  2. Create a link from the previous fallback entry (or the IfBlock start if there is none) into this new fallback entry
                    //  3. Set the priority of the fallback link to below normal, so that the if/elseif conditional link takes priority
                    var fbEntry = fallbackEntryList[i];
                    var fbLink = CreateLink(previousFbEntry, fbEntry);
                    fbLink.priority = ConditionPriority.BelowNormal;

                    // Either link to the start of the next if clause, or the end of the if block if there is none.
                    // We only enter this else statement if there's no else statement in this IfBlock
                    if (i < ifClauseStartList.Count - 1) CreateLink(fbEntry, ifClauseStartList[i + 1]);
                    else CreateLink(fbEntry, ifBlockEndEntry);

                    // We know that the previous link is now a group, since we added a second link to the new fallback entry
                    // This new fallback entry replaces the previous one, setting us up or the next loop iteration
                    previousFbEntry.isGroup = true;
                    previousFbEntry = fbEntry;
                }

                CreateLink(ifClauseEndEntry, ifBlockEndEntry);
            }

            return ifBlockEndEntry;
        }

        private (DialogueEntry startEntry, DialogueEntry endEntry) CreateAndAddDialogueEntries(IfClause ifClause, DialogueEntry blockStartEntry, DialogueEntry blockEndEntry)
        {
            var startTitle = string.Format(EntryTitle.IfClauseStart, ifClause.ClauseType);
            var startDesc = string.Format(EntryDescription.IfClauseStart, ifClause.ClauseType);
            var ifClauseStartEntry = CreateDialogueEntry(_currentConversation, startTitle, startDesc);
            ifClauseStartEntry.Sequence = EntrySequence.ContinueDialogue;

            var endTitle = string.Format(EntryTitle.IfClauseEnd, ifClause.ClauseType);
            var endDesc = string.Format(EntryDescription.IfClauseEnd, ifClause.ClauseType);
            var ifClauseEndEntry = CreateDialogueEntry(_currentConversation, endTitle, endDesc);
            ifClauseEndEntry.Sequence = EntrySequence.ContinueDialogue;

            var previousEntry = ifClauseStartEntry;
            foreach (var stmt in ifClause.Statements)
            {
                var nextEntry = CreateAndAddDialogueEntries(stmt, previousEntry);
                previousEntry = nextEntry;
            }

            // if (_prefs.debug) Debug.Log($"YarnProjectWriter::CreateAndAddDialogueEntries() - prev: {previousEntry} ifclause: {ifClauseEndEntry}");
            CreateLink(previousEntry, ifClauseEndEntry);

            if (ifClause.HasConditions)
            {
                ifClauseStartEntry.conditionsString = GenerateLuaExpression(ifClause.Conditions).Pop();
            }

            return (ifClauseStartEntry, ifClauseEndEntry);
        }

        private DialogueEntry CreateIfClauseFallbackEntry(IfClauseType clauseType)
        {
            var entry = CreateDialogueEntry(
                _currentConversation,
                string.Format(YarnImporterProjectWriter.EntryTitle.IfClauseFallback, clauseType),
                string.Format(YarnImporterProjectWriter.EntryDescription.IfClauseFallback, clauseType));
            entry.Sequence = EntrySequence.ContinueDialogue;
            return entry;
        }

        private Link CreateLink(DialogueEntry from, DialogueEntry to)
        {
            var link = new Link(from.conversationID, from.id, to.conversationID, to.id);
            from.outgoingLinks.Add(link);

            return link;
        }

        // NOTE: This method's name probably isn't the best.
        //       It does set the dialogue entry text, but also performs much more than that:
        //          1. It sets the dialogue entry text from the Node's string table (using the default locale)
        //          2. It looks into the (previously) cached localized string tables to add all translations of the text
        //          3. If specified, it extracts the actor's name from the text and updates the DialogueEntry.ActorID field
        //          4. If specified, it strips the actor name prefix from the text before storing it in the database.
        private void SetDialogueEntryText(DialogueEntry entry, string nodeName, string lineId)
        {
            if (_prefs.debug) Debug.Log($"YarnConverter::SetDialogueEntryText() - Setting dialogue text for entry with title: {entry.Title}, node: {nodeName}, key: {lineId}");
            // if (_prefs.debug) Debug.Log($"CreateLocalizedDialogueEntryText({nodeName})");
            var actor = _dialogueDb.GetActor(entry.ActorID);

            var lclString = _yarnProject.LocalizedStringTable[lineId];
            entry.DialogueText = TruncateDialogueTextPrefix(lclString.DefaultText);

            if (_prefs.debug) Debug.Log($"CreateLocalizedDialogueEntryText({nodeName}) - found localized string for line id: {lineId}");
            foreach (var stringMapEntry in lclString.Localizations)
            {
                var locale = stringMapEntry.Key;
                var text = stringMapEntry.Value;
                if (_prefs.debug) Debug.Log($"YarnProjectWriter::SetDialogueEntryText({nodeName}) - found string map: lcl: {locale} txt: {text}");
                var lclActorName = ExtractActorNameFromDialogueText(text);
                if (lclActorName != null)
                {
                    var fieldName = string.Format(DialogueEntryField.LocalizedDisplayName, locale);
                    Field.SetValue(actor.fields, fieldName, lclActorName, FieldType.Localization);
                    text = TruncateDialogueTextPrefix(text);
                }

                Field.SetValue(entry.fields, locale, text, FieldType.Localization);
            }
        }

        // NOTE: This regex pattern is expected to appear at the front of the dialogue text ONLY.
        //       The logic will not work if it's buried in the middle or end of the string.
        private string TruncateDialogueTextPrefix(string text)
        {
            var linePrefixRegex = _prefs.linePrefixRegex?.Trim();
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

        private void SetLineEntrySequence(DialogueEntry lineEntry)
        {
            // Regex's must be checked in this order
            var regexsToCheck = new List<string>()
            {
                StandaloneSequenceWithQuotesRegex,
                StandaloneSequenceWithoutQuotesRegex,
                SequenceWithQuotesRegex,
                SequenceWithoutQuotesRegex
            };

            // First regex to match sets the sequence
            foreach (var regex in regexsToCheck)
            {
                var mc = Regex.Matches(lineEntry.DialogueText, regex);
                if (mc.Count == 1 && mc[0].Groups.Count == 2)
                {
                    lineEntry.Sequence = mc[0].Groups[1].Captures[0].Value;
                    return;
                }
            }
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

        private void SetDialogueEntryActors(DialogueEntry entry, string lineId)
        {
            if (_prefs.debug) Debug.Log($"Setting dialogue entry actors for entry with title {entry.Title}");
            // Always extract the actor using the default locale's dialogue entry text.
            // We will localize the actor names in a different method (SetDialogueEntryText).
            var text = _yarnProject.LocalizedStringTable[lineId];

            // Changing this, defaulting to the player actor for all spoken text seems like the wrong thing to do.
            Actor actor = _defaultNpcActor;
            var actorName = ExtractActorNameFromDialogueText(text.DefaultText);
            if (actorName != null) actor = GetOrCreateActor(actorName, false);

            entry.ActorID = actor.id;

            // If the actor is the player, conversant ID is set to the default NPC.
            // If the actor is NOT the player, the conversant ID is set to the player.
            entry.ConversantID = (actor == _playerActor) ? _defaultNpcActor.id : _playerActor.id;
        }

        private string ExtractActorNameFromDialogueText(string text)
        {
            var actorRegex = _prefs.actorRegex?.Trim();
            if (string.IsNullOrEmpty(actorRegex)) return null;

            MatchCollection mc = Regex.Matches(text, _prefs.actorRegex);

            // There have to be at least two capture groups in order for the actor name to be successfully extracted.
            if (mc.Count <= 0 || mc[0].Groups.Count < 2) return null;

            var actorName = mc[0].Groups[1].Captures[0].Value;
            // if (_prefs.debug) Debug.Log($"ExtractActorNameFromDialogueText() - capture group count: {mc[0].Groups[1].Captures.Count} extracted actor name: '{actorName}'");
            if (_prefs.debug) Debug.Log($"Extracted actor with name: {actorName}");
            return actorName;
        }

        // Public so that it can be directly unit tested
        public string GenerateFormattedStringArguments(int conversationId, int dialogueEntryId, YarnExpression expression)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectWriter::GenerateFormattedStringArguments()");

            var argStack = GenerateLuaExpression(expression);
            var fmtArgsScript = string.Empty;
            var isFirst = true;
            while (argStack.Count > 0)
            {
                var paramList = $"{conversationId}, {dialogueEntryId}, tostring({argStack.Pop()})";

                if (isFirst) fmtArgsScript += $"{BuiltInCommand.ClearAndAddStringFormatArg.LuaName()}({paramList})";
                else fmtArgsScript += $"\n{BuiltInCommand.AddStringFormatArg.LuaName()}({paramList})";

                isFirst = false;
            }

            return fmtArgsScript;
        }

        public string GenerateLuaCommandCall(CommandStatement cmd)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectWriter::GenerateLuaCommandCall({cmd.Name})");
            var cmdName = cmd.CommandType.IsUnknown() ? cmd.Name : cmd.CommandType.LuaName();

            var expStack = GenerateLuaExpression(cmd.Expression);
            Assert.AreEqual(cmd.ExpressionCount, expStack.Count, $"Command statement arguments and generated expression stack must be equal for command: '{cmd.Text}' ");
            var luaScript = $"{cmdName}(";
            var sep = string.Empty;
            // if (_prefs.debug) Debug.Log($"command string: {cmd.Text} total string tokens: {cmd.StringTokens.Count}");
            foreach (var token in cmd.StringTokens)
            {
                // if (_prefs.debug) Debug.Log($"  token: {token.Value}");
                if (token.Type.IsString())
                {
                    var cmdItems = CommandTextSplitter.SplitCommandText(token.Value);
                    // if (_prefs.debug) Debug.Log($"token: '{token.Value}' total split items: {cmdItems.Count}");
                    foreach (var item in cmdItems)
                    {
                        // double dblVal = 0.5D;
                        var isPrimitive = Double.TryParse(item.Text, out double dblVal);
                        isPrimitive |= (item.Text == "true" || item.Text == "false");

                        if (isPrimitive) luaScript += $"{sep}{item.Text}";
                        else luaScript += $"{sep}'{item.Text}'";

                        sep = ", ";
                    }
                }
                else
                {
                    luaScript += $"{sep}{expStack.Pop()}";
                    sep = ", ";
                }
            }

            luaScript += ")";
            return luaScript;
        }

        // Public so that it can be directly unit tested
        public Stack<string> GenerateLuaExpression(YarnExpression exp)
        {
            var expStack = exp.StackCopy;

            // This is where all of the parsed expressions will end up
            // each item on this stack is its own individual expression that needs to be run.
            //
            // Basically we're going to keep looping through, popping an expression token of the expStack and then:
            //  1. If it's (like a) primitive there's nothing to do, just push it on the arg stack
            //  2. If it's an operator or function call, pull off all its arguments and push the full call on the arg stack
            var argStack = new Stack<string>();
            while (expStack.Count > 0)
            {
                var token = expStack.Pop();

                if (token.Type.IsBool()) argStack.Push(token.ToString().ToLower());
                else if (token.Type.IsNumber()) argStack.Push(token.ToString());
                else if (token.Type.IsNull()) argStack.Push("nil");
                else if (token.Type.IsVariable()) argStack.Push(FormatLuaVariableTableAccess(token.ToString()));
                else if (token.Type.IsText()) argStack.Push($"'{token.ToString()}'");
                else if (token.Type.IsOperator())
                {
                    var opToken = (OperatorToken)token;
                    // if (_prefs.debug) Debug.Log($"YarnProjectWriter::GenerateYarnExpressionLuaScript() - pushing arg with token type: {token.Type} value: {((OperatorToken)token).Value}");
                    // if (_prefs.debug) Debug.Log($"YarnProjectWriter::GenerateYarnExpressionLuaScript() - found operator: {opToken.Value} arg cnt: {opToken.ArgCount}");
                    var lhs = "";
                    var op = opToken.Value.LuaOperator();
                    var rhs = "";
                    if (opToken.Value.IsParentheses())
                    {
                        argStack.Push($"({argStack.Pop()})");
                    }
                    else if (opToken.ArgCount == 1)
                    {
                        if (opToken.Value.IsNot()) argStack.Push($"{op} {argStack.Pop()}");
                        else argStack.Push($"{op}{argStack.Pop()}");
                    }
                    else if (opToken.ArgCount == 2)
                    {
                        lhs = argStack.Pop();
                        rhs = argStack.Pop();
                        argStack.Push($"{lhs} {op} {rhs}");
                    }
                }
                else if (token.Type.IsFunction())
                {
                    var fnToken = (FunctionToken)token;
                    var fnParams = new List<string>();
                    for (var i = 0; i < fnToken.ArgCount; ++i) fnParams.Add(argStack.Pop());
                    var fnName = fnToken.FunctionType.IsUnknown() ? fnToken.Name : fnToken.FunctionType.LuaName();
                    var fnCall = $"{fnName}({string.Join(", ", fnParams)})";
                    argStack.Push(fnCall);
                }
            }

            return argStack;
        }

        public string GenerateSequence(CommandStatement cmd)
        // public string GenerateSequenceWithExpression(Com)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectWriter::GenerateSequence({cmd.Text})");
            var luaScript = string.Empty;

            // The syntax for a sequence must be similar to the following: SequenceName(optional_args)
            // So no need to completely parse this apart like a typical command.
            var splitIndex = cmd.Text.IndexOf(' ');
            var sequenceString = cmd.Text;
            if (splitIndex < sequenceString.Length) sequenceString = cmd.Text.Substring(splitIndex + 1).Trim();
            Debug.Log($"YarnProjectWriter::GenerateSequence() - sequenceString: {sequenceString}, exp count: {cmd.ExpressionCount}");

            if (cmd.HasExpression)
            {
                var expStack = GenerateLuaExpression(cmd.Expression);
                Assert.AreEqual(cmd.ExpressionCount, expStack.Count, $"Command statement arguments and generated expression stack must be equal for command: '{cmd.Text}' ");

                var fmtArgs = new List<string>();
                while (expStack.Count > 0)
                {
                    // Delay( [lua( 3+5 )] )
                    var exp = expStack.Pop();

                    Debug.Log($"YarnProjectWriter::GenerateSequence() - exp: {sequenceString}, exp count: {cmd.ExpressionCount}, exp: {exp}");

                    var fmtArg = $"[lua({exp})]";
                    fmtArgs.Add(fmtArg);
                }

                sequenceString = string.Format(sequenceString, fmtArgs.ToArray());
            }

            return sequenceString;
        }
    }
}
#endif
