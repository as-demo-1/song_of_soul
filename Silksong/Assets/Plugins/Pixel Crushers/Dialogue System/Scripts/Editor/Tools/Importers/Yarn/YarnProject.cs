#if USE_YARN
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.Assertions;

using Yarn;
using Yarn.Compiler;

namespace PixelCrushers.DialogueSystem.Yarn
{
    public struct YarnVariable
    {
        public readonly string name;
        public readonly bool isGlobal;
        public readonly Operand.ValueOneofCase type;
        public readonly string initialValue;

        public static YarnVariable Bool(string name, string initialValue="false", bool isGlobal=true)
        {
            return new YarnVariable(Operand.ValueOneofCase.BoolValue, name, initialValue, isGlobal);
        }

        public static YarnVariable Float(string name, string initialValue="0", bool isGlobal=true)
        {
            return new YarnVariable(Operand.ValueOneofCase.FloatValue, name, initialValue, isGlobal);
        }

        public static YarnVariable String(string name, string initialValue=null, bool isGlobal=true)
        {
            return new YarnVariable(Operand.ValueOneofCase.StringValue, name, initialValue, isGlobal);
        }

        public static YarnVariable None(string name, string initialValue=null, bool isGlobal=true)
        {
            return new YarnVariable(Operand.ValueOneofCase.None, name, initialValue, isGlobal);
        }

        public static YarnVariable Local(string name)
        {
            return new YarnVariable(Operand.ValueOneofCase.None, name, "nil", false);
        }

        public YarnVariable(Operand.ValueOneofCase yarnType, string name, string initialValue, bool isGlobal=true)
        {
            this.name = name;
            this.type = yarnType;
            this.initialValue = initialValue;
            this.isGlobal = isGlobal;
        }
    }

    public class YarnCommand
    {
        // Yarn built-in commands
        public const string WaitName = "wait";
        public const string StopName = "stop";

        // Dialogue System custom commands
        // These never show up in the final database,
        // we're just exploiting Yarn's custom command functionality
        // in order to pass info along to our parser.
        public const string SetSequenceName = "sequence";

        // Command props
        public readonly string name;
        public readonly int parameterCount;

        public static YarnCommand CallFunc(string name, int parameterCount)
        {
            // All CallFuncs have their parameters resolved at runtime,
            // so we don't actually care (and cannot know) what they are at this point.
            // Just add a placeholder for each argument expected.
            // var parameters = new List<string>();
            // for (var count = 0; count < parameterCount; ++count) parameters.Add("param");
            return new YarnCommand(name, parameterCount);
        }

        public static YarnCommand Command(string cmdString)
        {
            var cmdTokens = cmdString.Split(' ');
            var cmdName = cmdTokens[0];
            return new YarnCommand(cmdName, cmdTokens.Length - 1);
        }

        public bool IsSetSequence
        {
            get
            {
                return name == SetSequenceName;
            }
        }

        public bool IsBuiltIn
        {
            get
            {
                // NOTE: May need to specify that this is kicked off from a RunCommand,
                //       I don't know if it's legal to call "stop" or "wait" from a CallFunc.
                //       That scenario doesn't make sense to me, but I haven't tested it.
                // return YarnCommandType.Command == type && (IsStop || IsWait);
                return IsStop || IsWait;
            }
        }

        public bool IsStop
        {
            get
            {
                return name == StopName;
            }
        }

        public bool IsWait
        {
            get
            {
                return name == WaitName;
            }
        }

        public YarnCommand(string name, int parameterCount)
        {
            this.name = name;
            this.parameterCount = parameterCount;
        }
    }

    public class NodeLabelMaps
    {
        public const int LabelNotFound = -1;

        public IReadOnlyDictionary<string, int> LabelToInstructionMap { get; private set; }
        public IReadOnlyDictionary<int, string> InstructionTolabelMap { get; private set; }
        public IReadOnlyDictionary<string, YarnStatement> LabelToStatementMap { get; internal set; }

        public NodeLabelMaps(Node node)
        {
            LabelToInstructionMap = node.Labels;

            var instructionToLabelMap = new Dictionary<int, string>();
            InstructionTolabelMap = instructionToLabelMap;
            foreach (var labelEntry in node.Labels)
            {
                instructionToLabelMap[labelEntry.Value] = labelEntry.Key;
            }
        }

        public string GetLabel(int index)
        {
            string label = null;
            InstructionTolabelMap.TryGetValue(index, out label);

            return label;
        }

        public int GetInstructionIndex(string label)
        {
            var index = LabelNotFound;
            if (LabelToInstructionMap.ContainsKey(label))
            {
                index = LabelToInstructionMap[label];
            }

            return index;
        }
    }

    public abstract class YarnStatement
    {
        public BlockStatement Parent { get; internal set; }
        public string Label { get; internal set; }
        public int TotalInstructions { get; protected set; }
        public YarnStatementType Type { get; protected set; }
        public YarnStatement BranchStatement { get; internal set; }
        public YarnStatement ExitStatement { get; internal set; }
        public bool HasDialogueEntries { get { return dialogueEntries.Count > 0; } }
        private List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();

        public ConversationNode ConversationNode {
            // This will obviously fail if the parent hierarchy is not properly set up
            get
            {
                if (Type.IsConversation()) return this as ConversationNode;

                var node = Parent;
                while (node != null && !node.Type.IsConversation()) node = node.Parent;

                return node as ConversationNode;
            }
        }

        public IReadOnlyList<DialogueEntry> DialogueEntries { get { return dialogueEntries; } }

        public YarnStatement(YarnStatementType type)
        {
            Type = type;
            TotalInstructions = 0;
        }

        public void AddDialogueEntry(DialogueEntry entry)
        {
            dialogueEntries.Add(entry);
        }
    }

    public class BasicStatement : YarnStatement
    {
        public IReadOnlyList<Instruction> Instructions { get; private set; }
        public Instruction LastInstruction { get; private set; }
        // MainInstruction is the dominant instruction of the group. For example, a SetVariable may have the following instruction list:
        //  1. Push float
        //  2. Set Variable
        //  3. Pop
        // The main instruction here is the SetVariable one, and defines the functionality of the BasicStatement.
        // TODO: Need to properly populate MainInstruction, right now it kinda only works properly for SetVariable.
        //       Usually this is the LastInstruction, but need to really make sure that's true in every other case besides SetVariable.
        //       After those changes are made, look at every reference to LastInstruction, and in places where MainInstruction should
        //       be used instead, switch from LastInstruction to MainInstruction.
        public Instruction MainInstruction { get; private set; }

        public string BranchLabel { get; private set; }
        public bool HasArguments { get; private set; } = false;

        public DialogueEntry DialogueEntry { get { return DialogueEntries[0]; } }

        public bool HasConditionalOptionDialogueEntry { get { return DialogueEntries.Count >= 2; } }
        public DialogueEntry ConditionalOptionDialogueEntry { get { return DialogueEntries[1]; } }
        public DialogueEntry CalculateRunLineArgumentsDialogueEntry { get { return DialogueEntries[1]; } }

        public string OptionConditionsVariable { get; internal set; }
        public string Sequence { get; internal set; }

        public BasicStatement(IReadOnlyList<Instruction> instructions) : base(YarnStatementType.Basic)
        {
            Instructions = instructions;
            TotalInstructions = Instructions.Count;

            LastInstruction = Instructions[Instructions.Count - 1];
            // Default MainInstruction to the last instruction, that's usually the case.
            // Need to adjust for SetVariable commands, though.
            MainInstruction = LastInstruction;
            foreach (var instruction in instructions)
            {
                if (instruction.Opcode.IsStoreVariable())
                {
                    MainInstruction = instruction;
                    break;
                }
            }

            BranchLabel = GetBranchLabel();

            Assert.IsNotNull(LastInstruction, "All Basic Statements should have their last instruction specified");
            Assert.IsNotNull(MainInstruction, "All Basic Statements should have a main instruction specified");

            CheckForArguments();
        }

        private string GetBranchLabel()
        {
            string label = null;

            if (LastInstruction.Opcode.IsJumpIfFalse()
                || LastInstruction.Opcode.IsJumpTo())
            {
                label = LastInstruction.Operands[0].StringValue;
            }
            if (LastInstruction.Opcode.IsRunNode() && LastInstruction.Operands.Count > 0)
            {
                label = LastInstruction.Operands[0].StringValue;
            }
            else if (LastInstruction.Opcode.IsAddOption())
            {
                label = LastInstruction.Operands[1].StringValue;
            }

            return label;
        }

        private void CheckForArguments()
        {
            foreach (var instruction in Instructions)
            {
                if (instruction.Opcode.IsPushOpCode())
                {
                    HasArguments = true;
                    break;
                }
            }
        }
    }

    public abstract class BlockStatement : YarnStatement
    {
        // Block entrance label, blank for If related blocks,
        // set for ShortcutOption related blocks.
        public string EntranceLabel { get; internal set; }

        // Block exit label, the block can either include or exclude the instruction at this label.
        // IfBlocks exclude the instruction at this label
        // IfClauses exclude it
        // ShortcutOption blocks exclude it
        public string ExitLabel { get; internal set; }

        public YarnStatement FirstStatement { get { return GetFirstStatement(); } }
        public YarnStatement LastStatement { get { return GetLastStatement(); } }
        public bool HasStartDialogueEntry { get { return DialogueEntries.Count >= 1; } }
        public bool HasEndDialogueEntry { get { return DialogueEntries.Count >= 2; } }

        // NOTE: These flags are clues to the DS database builder.
        //       They will be propagated up the chain through all parent blocks
        //       so that the database builder will know what's deep inside nested blocks from
        //       the very top level.
        public bool ContainsDialogue { get; private set; }
        public bool ContainsOptions { get; private set; }
        public bool ContainsCommands { get; private set; }
        public bool ContainsStartNewConversation { get; private set; }

        public DialogueEntry StartDialogueEntry { get { return DialogueEntries[0]; } }
        public DialogueEntry EndDialogueEntry { get { return DialogueEntries[1]; } }

        public IReadOnlyList<YarnStatement> Statements { get { return statements; } }

        private List<YarnStatement> statements = new List<YarnStatement>();

        public BlockStatement(YarnStatementType type, BlockStatement parent) : base(type)
        {
            Parent = parent;
        }

        public virtual void AddStatement(YarnStatement stmt)
        {
            statements.Add(stmt);
            stmt.Parent = this;

            // Just rebuild the statement links every time a new clause is added
            for (var index = 0; index < Statements.Count; ++index)
            {
                var currStmt = Statements[index];
            }

            CalculateTotalInstructions();
            CheckContents(this);
        }

        public void CheckContents(BlockStatement block)
        {
            // var debugLine = $"IfStatement::AddBlock() - exit label: {ExitLabel} total blocks: {blocks.Count}";

            TotalInstructions = 0;

            foreach (var stmt in block.Statements)
            {
                TotalInstructions += stmt.TotalInstructions;

                if (stmt is BasicStatement)
                {
                    var basicStmt = (BasicStatement)stmt;

                    if (basicStmt.LastInstruction.Opcode.IsAddOption())
                    {
                        ContainsOptions = true;
                    }
                    else if (basicStmt.LastInstruction.Opcode.IsRunCommand())
                    {
                        ContainsCommands = true;
                    }
                    else if (basicStmt.LastInstruction.Opcode.IsRunLine())
                    {
                        ContainsDialogue = true;
                    }
                    else if (basicStmt.LastInstruction.Opcode.IsRunNode())
                    {
                        ContainsStartNewConversation = true;
                    }
                }
                else if (stmt is BlockStatement)
                {
                    var childBlock = (BlockStatement)stmt;
                    CheckContents(childBlock);
                }
                else
                {
                    // Should never be able to reach here
                }
            }

            // NOTE: Might be a better place to kick off these calls, but this should work
            var parentStmt = this.Parent;
            while (parentStmt != null)
            {
                Parent.ContainsOptions |= this.ContainsOptions;
                Parent.ContainsCommands |= this.ContainsCommands;
                Parent.ContainsDialogue |= this.ContainsDialogue;
                Parent.ContainsStartNewConversation |= this.ContainsStartNewConversation;
                parentStmt = parentStmt.Parent;
            }
        }

        private void CalculateTotalInstructions()
        {
            TotalInstructions = 0;
            foreach (var stmt in Statements)
            {
                TotalInstructions += stmt.TotalInstructions;
            }
        }

        private YarnStatement GetFirstStatement()
        {
            if (Statements.Count > 0)
            {
                return Statements[0];
            }

            return null;
        }

        private YarnStatement GetLastStatement()
        {
            if (Statements.Count > 0)
            {
                return Statements[Statements.Count - 1];
            }

            return null;
        }
    }

    public class IfClause : BlockStatement
    {
        public IfClauseType ClauseType { get; private set; }
        public BasicStatement Conditions { get; private set; }
        public bool HasConditions { get { return Conditions != null; } }
        public string JumpIfFalseLabel { get { return Conditions?.BranchLabel; } }

        // Just an alias for EndDialogueEntry, no need to create a separate one just for this class.
        public DialogueEntry FallbackDialogueEntry {
            get
            {
                return DialogueEntries[1];
            }
         }

        public IfClause(BlockStatement parent) : base(YarnStatementType.IfClause, parent)
        {
        }

        public override void AddStatement(YarnStatement stmt)
        {

            // Don't add the conditions to the block
            var doAdd = true;

            // First added statement determines whether this is an If/ElseIf/Else clause.
            if (Statements.Count <= 0 && !HasConditions)
            {
                ClauseType = IfClauseType.Else;

                if (stmt.Type.IsBasic())
                {
                    var basicStmt = (BasicStatement)stmt;
                    // Debug.Log($"Adding first statement to IfClause, last instruction: {basicStmt.LastInstruction.Opcode}");

                    // If this first statement is a JumpIfFalse, we have an If or ElseIf
                    if (basicStmt.LastInstruction.Opcode.IsJumpIfFalse())
                    {
                        var ifBlock = (IfBlock)Parent;
                        ClauseType = ifBlock.IfClauses.Count > 1 ? IfClauseType.ElseIf : IfClauseType.If;
                        Conditions = basicStmt;
                        doAdd = false;
                    }
                }
            }

            if (doAdd)
            {
                base.AddStatement(stmt);
            }
        }
    }

    public class IfBlock : BlockStatement
    {
        public bool HasElse { get { return CheckForElseClause(); } }
        public List<IfClause> IfClauses = new List<IfClause>();

        public IfBlock(BlockStatement parent, string exitBlockLabel) : base(YarnStatementType.IfBlock, parent)
        {
            ExitLabel = exitBlockLabel;
        }

        public override void AddStatement(YarnStatement stmt)
        {
            // Assert.IsTrue(false, "Use AddIf/ElseIf/Else methods instead");
            base.AddStatement(stmt);
            if (stmt.Type.IsIfClause())
            {
                IfClauses.Add((IfClause)stmt);
            }

        }

        public IfClause CreateAndAddIfClause(string exitLabel=null)
        {
            // if (_prefs.debug) Debug.Log($"IfClause::CreateAndAddClause - type: {type} for if block: {BranchLabel}");

            Assert.IsFalse(HasElse, $"Attempting to add IfBlock clause when Else clause is already present, exit label: {ExitLabel}");
            var ifClause = new IfClause(this);
            ifClause.ExitLabel = exitLabel;
            AddStatement(ifClause);
            return ifClause;
        }

        private bool CheckForElseClause()
        {
            for (var index = 0; index < IfClauses.Count; ++index)
            {
                var ifClause = IfClauses[index];

                if (ifClause.ClauseType.IsElse())
                {
                    Assert.AreEqual(index + 1, IfClauses.Count, "Else clause should always be the last stmt of an IfBlock");
                    return true;
                }
            }

            return false;
        }
    }

    public class ShortcutOption : BlockStatement
    {
        public BasicStatement Conditions { get; private set; }
        public BasicStatement Option { get; private set; }
        public bool HasConditions { get { return Conditions != null; } }

        public ShortcutOption(ShortcutOptionList parent, BasicStatement option) : this(parent, option, null) {}

        public ShortcutOption(ShortcutOptionList parent, BasicStatement option, BasicStatement conditions) : base(YarnStatementType.ShortcutOption, parent)
        {
            Option = option;
            Conditions = conditions;
            EntranceLabel = option.BranchLabel;
        }
    }

    public class ShortcutOptionList : BlockStatement
    {
        public IReadOnlyList<ShortcutOption> ShortcutOptions { get { return _shortcutOptions; } }
        private List<ShortcutOption> _shortcutOptions = new List<ShortcutOption>();

        public ShortcutOptionList(BlockStatement parent) : base(YarnStatementType.ShortcutOptionList, parent) {}

        public ShortcutOption CreateAndAddShortcutOption(BasicStatement option)
        {
            return CreateAndAddShortcutOption(option, null);
        }

        public ShortcutOption CreateAndAddShortcutOption(BasicStatement option, BasicStatement conditions)
        {
            var opt = new ShortcutOption(this, option, conditions);
            AddStatement(opt);
            return opt;
        }

        public override void AddStatement(YarnStatement stmt)
        {
            base.AddStatement(stmt);

            if (stmt.Type.IsShortcutOption())
            {
                _shortcutOptions.Add((ShortcutOption)stmt);
            }
        }
    }


    public class ConversationNode : BlockStatement
    {
        public Node Node { get; private set; }
        public IReadOnlyDictionary<string, string> Metadata { get; private set; }
        public string Name { get { return Node.Name; } }
        public NodeLabelMaps LabelMaps { get; private set; }
        public Conversation Conversation { get; set; }
        public IReadOnlyList<BasicStatement> Options { get; internal set; }
        public List<DialogueEntry> OptionDialogueEntries { get; set; } = new List<DialogueEntry>();

        public ConversationNode(Node node, IReadOnlyDictionary<string, string> metadata) : base(YarnStatementType.Conversation, null)
        {
            Node = node;
            Type = YarnStatementType.Conversation;
            Metadata = metadata;
            LabelMaps = new NodeLabelMaps(node);
            // Debug.Log($"ConversationNode::ctor() - name: {Name} adding metadata with title: {metadata[YarnProject.NodeMetadataTitleKey]}");
        }
    }

    // +== YarnProject Class
    public class YarnProject
    {
        public const string OptionConditionsVariableNameFormat = "${0}_opt_cond_{{0}}";
        public const string NodeHeaderTerminationToken = "---";
        public const string NodeTerminationToken = "===";
        public const string NodeMetadataRegex = "(.+?):(.*)";
        public const string NodeMetadataTitleKey = "title";
        public const string NodeMetadataActorKey = "actor";
        public const string NodeMetadataConversantKey = "conversant";

        // This is the list of all built-in functions that can be names for CallFunc instructions.
        // It is important to note that these names do seem capable of clashing with a user-defined function,
        // so it's probably worth testing to see what happens if I test out a script that deliberately clashes with these names.
        // I'm not entirely sure why these strings were used instead of their operators, or some other type of string value
        // that would be an illegal variable name in Yarn script. 
        public static readonly List<string> BuiltInFunctions = new List<string>()
        {
            // Logical Operators
            // "EqualTo",  // Equality: eq or is or ==
            // "neq",      // Inequality: neq or !
            // ">",        // Greater than: gt or >
            // "<",        // Less than: lt or <
            // "<=",       // Less than or equal to: lte or <=
            // ">=",       // Greater than or equal to: gte or >=
            // "Or",       // Boolean OR: or or ||
            // "Xor",      // Boolean XOR: xor or ^
            // "Not",      // Boolean Negation: !
            // "And",      // Boolean AND: and or &&

            // // Math operators
            // "+",    // Addition: +
            // "-",    // Subtraction: -
            // "*",    // Multiplication: *
            // "/",    // Division: /
            // "%",    // Truncating Remainder Division: %
            // "(",    // Brackets: ( to open the brackets and ) to close them
            // ")",    // Brackets: ( to open the brackets and ) to close them

            // From Yarn.TokenType internal enum (Compiler.cs)
            "EqualTo", // ==, eq, is
            "GreaterThan", // >, gt
            "GreaterThanOrEqualTo", // >=, gte
            "LessThan", // <, lt
            "LessThanOrEqualTo", // <=, lte
            "NotEqualTo", // !=, neq

            // Logical operators
            "Or", // ||, or
            "And", // &&, and
            "Xor", // ^, xor
            "Not", // !, not

            // this guy's special because '=' can mean either 'equal to'
            // or 'becomes' depending on context
            "EqualToOrAssign", // =, to

            "UnaryMinus", // -; this is differentiated from Minus
                        // when parsing expressions

            "Add", // +
            "Minus", // -
            "Multiply", // *
            "Divide", // /
            "Modulo", // %

            "AddAssign", // +=
            "MinusAssign", // -=
            "MultiplyAssign", // *=
            "DivideAssign", // /=

        };

        public Program Program { get; private set; }
        public Dictionary<string, YarnVariable> Variables { get; private set; } = new Dictionary<string, YarnVariable>();
        public Dictionary<string, YarnCommand> Commands { get; private set; } = new Dictionary<string, YarnCommand>();
        public Dictionary<string, YarnStatementType> VariableToOptionMap { get; private set; } = new Dictionary<string, YarnStatementType>();
        public IReadOnlyDictionary<string, ConversationNode> Nodes { get; private set; }
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> MetadataMap { get { return _metadataMap; } }

        // May want to change this to non-const
        public IReadOnlyDictionary<string, StringInfo> StringTable { get; private set; }
        private Dictionary<int, BlockStatement> _blockStatementMap = new Dictionary<int, BlockStatement>();
        public Dictionary<string, IReadOnlyDictionary<string, string>> _metadataMap = new Dictionary<string, IReadOnlyDictionary<string, string>>();

        private YarnConverterPrefs _prefs;

        // public YarnProject(params string[] filenames)
        public YarnProject(YarnConverterPrefs prefs)
        {
            _prefs = prefs;
            Compile(_prefs.sourceFiles);
            PrintNodes();
            ParseNodes();
            // PrintConversations();
        }

        public string CreateUniqueVariableName(string varNameFormat)
        {
            int varCnt = 0;
            string varName = null;
            do
            {
                ++varCnt;
                varName = string.Format(varNameFormat, varCnt);
            } while (Variables.ContainsKey(varName));

            return varName;
        }

        public string CreateAndAddUniqueLocalVariableName(string varNameFormat)
        {
            var varName = CreateUniqueVariableName(varNameFormat);
            Variables[varName] = YarnVariable.Local(varName);
            return varName;
        }

        // private void ParseMetadata(string[] filenames)
        private void ParseMetadata(List<string> filenames)
        {
            var headers = new List<List<string>>();

            foreach (var filename in filenames)
            {
                // First, extract each header (one per node) from our Yarn File
                var isInHeader = true;
                var programLines = File.ReadAllLines(filename);
                var headerLines = new List<string>();
                foreach (var programLine in programLines)
                {
                    var line = programLine.Trim();

                    if (line == NodeHeaderTerminationToken)
                    {
                        headers.Add(headerLines);
                        headerLines = new List<string>();
                        isInHeader = false;

                        continue;
                    }
                    else if (line == NodeTerminationToken)
                    {
                        isInHeader = true;
                        continue;
                    }
                    else if (string.Empty == line)
                    {
                        continue;
                    }

                    if (isInHeader)
                    {
                        headerLines.Add(line);
                    }
                }

                foreach (var header in headers)
                {
                    var metadata = new Dictionary<string, string>();
                    var title = string.Empty;

                    foreach (var line in header)
                    {
                        MatchCollection mc = Regex.Matches(line, NodeMetadataRegex);
                        if (mc.Count <= 0 || mc[0].Groups.Count < 3)
                        {
                            continue;
                        }

                        var key = mc[0].Groups[1].Captures[0].Value.Trim();
                        var value = mc[0].Groups[2].Captures[0].Value.Trim();
                        metadata[key] = value;

                        // Using this specific comparison because it's the same one the Yarn compiler uses
                        // for metadata keys.
                        if (key.Equals(NodeMetadataTitleKey, StringComparison.InvariantCulture))
                        {
                            title = value;
                        }
                    }

                    // Yarn allows nodes without title properties in its metadata. Seems like an obvious parser error to me,
                    // but maybe there's something I don't know? Yarn's own documentation states that it is required:
                    //      https://yarnspinner.dev/docs/syntax/#title
                    // Still, this code may be overly defensive, because it may be a good thing for our Converter to blow up
                    // where Yarn's compiler should but does not. It could save users some headache down the road.
                    // Assert.IsFalse("" == title, "Invalid metadata (yarn parser error?), title should always be present in metadata");
                    // Assert.IsFalse(metadata.Count <= 0, "Metadata should always be > 1, should have at least title field");
                    if (!(string.Empty == title || metadata.Count <= 0))
                    {
                        _metadataMap[title] = metadata;
                    }
                }
            }
        }

        // private void Compile(params string[] filenames)
        private void Compile(List<string> filenames)
        {
            var mainProgram = new Program();
            var mainStringTable = new Dictionary<string, StringInfo>();

            ParseMetadata(filenames);

            foreach (var filename in filenames)
            {
                if (_prefs.debug) Debug.Log("Parsing Yarn program found at: " + filename);
                var program = new Program();
                var stringTable = new Dictionary<string, StringInfo>() as IDictionary<string, StringInfo>;
                Compiler.CompileFile(filename, out program, out stringTable);
                if (_prefs.debug) Debug.Log("Yarn program name: " + program.Name + " nodes: " + program.Nodes.Count);
                mainProgram = Program.Combine(mainProgram, program);

                // Probably a little easier to combine these dictionaries with linq,
                // but I'm not that familiar with it so I'll do it the old-fashioned way.
                foreach (var stringTableEntry in stringTable)
                {
                    // if (_prefs.debug) Debug.Log($"Adding string entry with key: {stringTableEntry.Key}, value: {stringTableEntry.Value.nodeName}");
                    // I would prefer to check that if two identical keys are found,
                    // we enusre that their StringInfo is also identical.
                    // But because I do not know the full rules around StringInfo,
                    // and there is no Equals method or equality operator implemented,
                    // I will trust that the two StringInfos are equivalent.

                    Assert.IsFalse(mainStringTable.ContainsKey(stringTableEntry.Key), $"Oops, found dupe string table key: {stringTableEntry.Key}");
                    if (!mainStringTable.ContainsKey(stringTableEntry.Key))
                    {
                        mainStringTable.Add(stringTableEntry.Key, stringTableEntry.Value);
                    }
                }
            }

            StringTable = mainStringTable;
            Program = mainProgram;
        }

        private void ParseNodes()
        {
            var conversations = new Dictionary<string, ConversationNode>();
            Nodes = conversations;

            foreach (var nodeEntry in Program.Nodes)
            {
                var nodeName = nodeEntry.Key;
                var node = nodeEntry.Value;
                conversations[nodeName] = ParseNode(node);
                // if (_prefs.debug) Debug.Log("Parsed yarn node: " + convo.Name + " total statements: " + convo.Block.Count);
            }
        }

        private ConversationNode ParseNode(Node node)
        {
            if (_prefs.debug) Debug.Log("YarnProject::ConversationNode() - Parsing node: " + node.Name);

            Assert.IsTrue(_metadataMap.ContainsKey(node.Name));
            var conversation = new ConversationNode(node, _metadataMap[node.Name]);

            var blockStack = new Stack<BlockStatement>();
            BlockStatement currentBlock = conversation;

            var optionsList = new List<BasicStatement>();
            conversation.Options = optionsList;

            // Shortcut options will be tracked by the label that marks the option's destination.
            // It's either another ConversationNode, or a child block of instructions.
            // var shortcutOptionStatements = new Dictionary<string, BlockStatement>();
            var shortcutOptionStatements = new Dictionary<string, ShortcutOption>();
            var ifBlocks = new Dictionary<string, IfBlock>();

            var labelToStatementMap = new Dictionary<string, YarnStatement>();

            // TODO: Clean this up, it's kinda nasty design
            // NOTE: Not gonna clean it up, just have to live with it :D
            // This code is awkward because of the IReadOnly data structure,
            // and that is usually because the IReadOnly is exposing a code smell.
            // Figure out a better place to stick this.
            // It's probably telling that the entire label map structure should
            // be built outside of the ConversationNode constructor and then passed in.
            // Figure this out later, too tired to care at the moment.
            conversation.LabelMaps.LabelToStatementMap = labelToStatementMap;

            BasicStatement previousStatement = null;

            var index = 0;
            while (index < node.Instructions.Count)
            {
                // if (_prefs.debug) Debug.Log("YarnProject::ParseNode() - instruction index: " + index);
                var stmt = ParseBasicStatement(conversation, index);

                if (_prefs.debug) Debug.Log($"YarnProject::ParseNode() - created statement: {stmt.Type} - label: {stmt.Label}");

                // Handle some block management in these first few If statements.
                // We'll be checking for block terminating instructions based on statement labels.
                // No adding of statements to blocks will be done in this section,
                // that is saved for later on when we inspect the last instruction of each statement.
                if (stmt.Label != null)
                {
                    labelToStatementMap[stmt.Label] = stmt;

                    // Check to see if we're handling the start of a shortcut option destination
                    if (shortcutOptionStatements.ContainsKey(stmt.Label))
                    {
                        if (_prefs.debug) Debug.Log($"Should be parsing content for shortcut option with label: {stmt.Label}");
                        var nextScOpt = shortcutOptionStatements[stmt.Label];

                        // If the current block is already a Shortcut Option, it's possible we're staring the next Shortcut Option entry
                        // in the same Shortcut Options List. In order to check that, make sure the current Shortcut Option and the next
                        // Shortcut Option have the same exact Shortcut Options List parent.
                        // If they do, we can safely close the current Shortcut Option block and grab its last instruction.
                        // This will be a JumpTo, and the JumpTo's destination label marks the end of the entire Shortcut Options List.
                        // While this label doesn't affect parsing directly, it's good to keep track of to make sure we are parsing correctly.
                        if (currentBlock.Type.IsShortcutOption() && currentBlock.Parent == nextScOpt.Parent)
                        {
                            var scOpt = (ShortcutOption)currentBlock;
                            // if (_prefs.debug) Debug.Log($"CHECKING STUFF OUT: stmt label: {stmt.Label} sc opt en label: {scOpt.EntranceLabel} total stmts in sc opt: {scOpt.Statements.Count}");
                            Assert.IsTrue(currentBlock.LastStatement.Type.IsBasic(), $"Expected basic statement for last instruction of shortcut options block, found: {currentBlock.LastStatement.Type}");
                            var jumpToInstruction = (BasicStatement)currentBlock.LastStatement;
                            Assert.IsTrue(jumpToInstruction.LastInstruction.Opcode.IsJumpTo(), $"Expected JumpTo as final instruction for shortcut options block, found: {jumpToInstruction.LastInstruction.Opcode}");
                            var optListExitLabel = jumpToInstruction.LastInstruction.Operands[0].StringValue;
                            if (currentBlock.Parent.ExitLabel == null)
                            {
                                currentBlock.Parent.ExitLabel = optListExitLabel;
                            }
 
                            Assert.AreEqual(optListExitLabel, currentBlock.Parent.ExitLabel, "Parser Error (bug): Mismatched Shortcut Option block JumpTo label and Shortcut Options List exit label");
                        }
                        else
                        {
                            // Only push the current block if we haven't started processing the list of shortcut options
                            blockStack.Push(currentBlock);
                        }

                        // If we've found the start of a shortcut option destination,
                        // we need to make it the current block.
                        // We will pop it off the stack when we find a matching JumpTo.
                        if (_prefs.debug) Debug.Log($"Processing instructions for shortcut options block with label: {stmt.Label}");
                        currentBlock = nextScOpt;
                    }
                    else if (currentBlock.Type.IsIfClause())
                    {
                        var ifClause = (IfClause)currentBlock;
                        var ifBlock = (IfBlock)currentBlock.Parent;

                        // We need to check whether or not this statement marks the end of the IfClause.
                        // Because this is determined by the statement's label, and not its instruction type,
                        // it is not a good fit for the block of stmt LastInstruction opcode checks below.
                        // NOTE: Even though the label is the only info we need, the instruction is ALWAYS a Pop.
                        if (_prefs.debug) Debug.Log($"In IfClause with JIF label: {ifClause.JumpIfFalseLabel}, current stmt has label: {stmt.Label}");
                        if (stmt.Label == ifClause.JumpIfFalseLabel)
                        {
                            // Immediately end this IfClause.
                            Assert.IsTrue(stmt.LastInstruction.Opcode.IsPop(), $"Expected Pop instruction at IfClause.JumpIfFalse label {ifClause.JumpIfFalseLabel}");

                            // We've encountered a (cleanup) Pop instruction, we're going to ignore it.
                            // They are always ignored by the YarnConverter, and mess up the expected order of Statements.
                            // currentBlock.AddStatement(stmt);
                            currentBlock = blockStack.Pop();

                            // Peek at the next stmt.
                            // If its label is the parent IfBlock's ExitLabel, we're done.
                            // Close out this IfClause and its parent IfBlock.
                            var nextStatementIndex = index + stmt.TotalInstructions;
                            var nextStatement = ParseBasicStatement(conversation, nextStatementIndex);
                            if (nextStatement.Label != ifBlock.ExitLabel)
                            {
                                blockStack.Push(currentBlock);
                                currentBlock = ifBlock.CreateAndAddIfClause();
                                if (_prefs.debug) Debug.Log($"YarnProject::ParseNode() - created subsequent if clause - ifClause label: {stmt.Label} IfBlock exit label: {currentBlock.Parent.ExitLabel}");
                            }

                            // We've already handled the current statement, so skip to the top of the loop again
                            index += stmt.TotalInstructions;
                            previousStatement = stmt;
                            continue;
                        }
                        else if (stmt.Label == ifBlock.ExitLabel)
                        {
                            currentBlock = blockStack.Pop();
                        }
                    }
                }

                // Down here, we will be inspecting the last instruction of the current statement,
                // and performing some logic based on its OpCode as well as the currentBlock's type.
                if (stmt.LastInstruction.Opcode.IsJumpIfFalse())
                {
                    // One of the first things to check is whether or not this is the start of some
                    // conditionally executed statement. We can determine that by checking if the current
                    // statement ends in a JumpIfFalse instruction.
                    // We determine what kind of statement this is by grabbing the very next statement.
                    var jumpIfFalseLabel = stmt.LastInstruction.Operands[0].StringValue;
                    var branchType = GetConditionalBlockType(conversation, jumpIfFalseLabel);
                    // if (_prefs.debug) Debug.Log($"INSTRUCTION OPCODE IS JUMP_IF_FALSE!!!!!!!!!!!! branchType: {branchType} JIF label: {jumpIfFalseLabel}");

                    // If this is a conditional shortcut option, we just save the conditions for now.
                    // The next loop will grab the AddOption statement, and we'll combine these two
                    // and create the proper ShortcutOption nested inside a ShortcutOptionList block.
                    // NOTE: We could just grab the following YarnStatement (an AddOption) and properly create
                    //       the ShortcutOption right now. However, I chose this solution because I don't want to
                    //       duplicate the code for properly setting up a ShortcutOptions block: once for regular
                    //       ShortcutOptions, and again here for conditional ShortcutOptions. This code is tricky
                    //       enough as it is, I figured this was the cleaner approach.
                    if (branchType.IsIfClause())
                    {
                        var exitBlockLabel = GetIfBlockExitLabel(conversation, jumpIfFalseLabel);
                        // if (_prefs.debug) Debug.Log("BRANCH TYPE IS IF_BLOCK!!!!!!!!!!!!");

                        // If the exit label for this IfBlock doesn't exist in our map,
                        // we've encountered the start of a brand new If statement in our Yarn program.
                        if (!ifBlocks.ContainsKey(exitBlockLabel))
                        {
                            // We know we'll be creating a new current block,
                            // so push the old one
                            blockStack.Push(currentBlock);

                            // IfBlocks are never placed on the block stack,
                            // and if they need to be directly referenced,
                            // the can be retrieved from an IfClause's Parent property.
                            var ifBlock = new IfBlock(currentBlock, exitBlockLabel);
                            ifBlocks[exitBlockLabel] = ifBlock;
                            Assert.AreEqual(exitBlockLabel, ifBlock.ExitLabel, $"IfBlock branch label: {ifBlock.ExitLabel} does not match exitBlockLabel: {exitBlockLabel}");
                            currentBlock.AddStatement(ifBlock);

                            currentBlock = ifBlock.CreateAndAddIfClause();

                            // if (_prefs.debug) Debug.Log($"YarnProject::ParseNode() - created if block/clause - ifClause JIF label: {currentBlock.Label} IfBlock exit label: {currentBlock.Parent.ExitLabel}");
                            if (_prefs.debug) Debug.Log($"YarnProject::ParseNode() - created if block/clause - ifClause JIF label: {jumpIfFalseLabel} IfBlock exit label: {currentBlock.Parent.ExitLabel}");
                        }

                        currentBlock.AddStatement(stmt);
                    }
                    else if (branchType.IsShortcutOption())
                    {
                        ShortcutOptionList optList = null;
                        if (currentBlock.Type.IsShortcutOptionList())
                        {
                            optList = (ShortcutOptionList)currentBlock;
                        }
                        else
                        {
                            optList = new ShortcutOptionList(currentBlock);
                            currentBlock.AddStatement(optList);
                            blockStack.Push(currentBlock);
                            currentBlock = optList;
                            if (_prefs.debug) Debug.Log($"Creating new Shortcut Options List on branch label: {stmt.BranchLabel}");
                        }

                        var conditions = stmt;

                        // Grab the AddOption statement after the JumpIfFalse
                        var addOptionStmtIndex = index + stmt.TotalInstructions;
                        var addOptionStmt = ParseBasicStatement(conversation, addOptionStmtIndex);
                        var opt = optList.CreateAndAddShortcutOption(addOptionStmt, conditions);
                        shortcutOptionStatements[addOptionStmt.BranchLabel] = opt;
                        Assert.IsTrue(addOptionStmt.LastInstruction.Opcode.IsAddOption(), $"Expected AddOption instruction after JumpIfFalse label {opt.Conditions.BranchLabel}");

                        // Grab the Pop after the AddOption statement, always present with conditional shortcut options
                        // This is similar to how IfClauses with no else leave trailing Pops that need to be cleaned up.
                        var popStmtIndex = addOptionStmtIndex + addOptionStmt.TotalInstructions;
                        var popStmt = ParseBasicStatement(conversation, popStmtIndex);
                        Assert.IsTrue(popStmt.LastInstruction.Opcode.IsPop(), $"Expected Pop instruction at end of conditioanl ShortcutOption with JumpIfFalse label {opt.Conditions.BranchLabel}");
                        // opt.AddStatement(nextStatement);

                        if (_prefs.debug) Debug.Log($"Grabbed conditional shortcut option pop with label: {opt.Conditions.BranchLabel}");

                        index = popStmtIndex + popStmt.TotalInstructions;
                        stmt = popStmt;
                        continue;
                    }
                }
                else if (stmt.LastInstruction.Opcode.IsAddOption())
                {
                    var jumpToLabel = stmt.BranchLabel;
                    if (_prefs.debug) Debug.Log($"YarnProject::ParseNode() - checking if add option label {jumpToLabel} is a shortcut options {shortcutOptionStatements.ContainsKey(jumpToLabel)}");

                    // Check to see if there's a node with this jumpTo label.
                    // Regular options can only jump externally to different nodes (different conversations).
                    // Shortcut options can only jump internally within a given conversation,
                    // to an instruction with the specified JumpTo label.
                    // If we see that there's a node with the same name as the JumpTo label,
                    // we know that this is a regular option and not a shortcut option.
                    if (Program.Nodes.ContainsKey(jumpToLabel))
                    {
                        currentBlock.AddStatement(stmt);
                        if (!currentBlock.Type.IsConversation())
                        {
                            var optVarNameFmt = String.Format(OptionConditionsVariableNameFormat, stmt.ConversationNode.Name);
                            var varName = CreateUniqueVariableName(optVarNameFmt);
                            Variables[varName] = YarnVariable.Local(varName);
                            stmt.OptionConditionsVariable = varName;
                        }

                        // Debug.Log($"Adding options statement with main opcode: {stmt.MainInstruction.Opcode} last opcode: {stmt.LastInstruction.Opcode}");
                        optionsList.Add(stmt);
                    }
                    else
                    {
                        // We've encountered a shortcut option, so we need to create a new block statement.
                        ShortcutOptionList optList = null;
                        if (currentBlock.Type.IsShortcutOptionList())
                        {
                            optList = (ShortcutOptionList)currentBlock;
                        }
                        else
                        {
                            optList = new ShortcutOptionList(currentBlock);
                            currentBlock.AddStatement(optList);
                            blockStack.Push(currentBlock);
                            currentBlock = optList;
                            if (_prefs.debug) Debug.Log($"Creating new Shortcut Options List on JumpTo label: {jumpToLabel}");
                        }

                        var opt = optList.CreateAndAddShortcutOption(stmt);
                        if (_prefs.debug) Debug.Log($"YarnProject::ParseNode() - created add shortcut option block: {opt.Type} - label: {opt.Label}");
                        shortcutOptionStatements[jumpToLabel] = opt;
                    }
                }
                else if (stmt.LastInstruction.Opcode.IsShowOptions() && currentBlock.Type.IsShortcutOptionList())
                {
                    var optList = (ShortcutOptionList)currentBlock;
                    currentBlock.AddStatement(stmt);
                }
                else if (stmt.LastInstruction.Opcode.IsJump() && currentBlock.Type.IsShortcutOptionList())
                {
                    // End the shortcut options block
                    currentBlock.AddStatement(stmt);
                    currentBlock = blockStack.Pop();
                    // if (_prefs.debug) Debug.Log($"Exiting shortcut options list");
                }
                else if (stmt.LastInstruction.Opcode.IsPop() && currentBlock.Type.IsShortcutOption())
                {
                    // The final instruction must be a Pop and ONLY an Pop.
                    // Not a Pop as the final instruction of any other type of Statement (e.g. a StoreVariable)
                    if (stmt.TotalInstructions == 1)
                    {
                        var scOpt = (ShortcutOption)currentBlock;

                        if (_prefs.debug) Debug.Log($"Closing out shortcut options list with exit label: {currentBlock.Parent.ExitLabel}");

                        if (currentBlock.Parent.ExitLabel == null)
                        {
                            currentBlock.Parent.ExitLabel = stmt.Label;
                        }
                        Assert.AreEqual(stmt.Label, currentBlock.Parent.ExitLabel, "Parser Error (bug): Mismatched Pop instruction label and Shortcut Options List exit label");

                        // Ignore the pop
                        // currentBlock.Parent.AddStatement(stmt);
                        currentBlock = blockStack.Pop();
                    }
                    else
                    {
                        currentBlock.AddStatement(stmt);
                    }
                }
                else if (stmt.LastInstruction.Opcode.IsStop())
                {
                    // If the Node's final instruction is a stop, ignore it. Yarn adds this if there are no options.
                    // This is getting ignored because it might be a little confusing to frequently see
                    // a completely unnecessary stop entry right before a conversation's end entry in
                    // a dialogue tree with no player choices.
                    // All other stop commands in the dialogue tree will be present.
                    //
                    // Also I'm removing it here rather in YarnConverter, which is the more appropriate place
                    // to ignore it, because the logic is much much simpler (I'm lazy).
                    if (index < node.Instructions.Count - 1)
                    {
                        currentBlock.AddStatement(stmt);
                    }
                }
                else
                {
                    // Check to see if the current YarnStatement is our custom set sequence Yarn Command.
                    // If so, grab the previous YarnStatement and set its SetSequence property.
                    // If not, just add the current YarnStatement to the current block
                    string sequence = null;
                    if (stmt.LastInstruction.Opcode.IsRunCommand())
                    {
                        var cmdString = stmt.LastInstruction.Operands[0].StringValue;

                        var sequenceRegex = new Regex($"^{YarnCommand.SetSequenceName}\\s+");
                        if (sequenceRegex.IsMatch(cmdString))
                        {
                            sequence = cmdString.Substring(YarnCommand.SetSequenceName.Length).Trim();
                        }
                    }

                    if (sequence != null)
                    {
                        if (previousStatement != null)
                        {
                            if (_prefs.debug) Debug.Log($"YarnProject::ParseNode() - found sequence command and previous stmt, setting SetSequence property to: {sequence}");
                            previousStatement.Sequence = sequence;
                        }
                    }
                    else
                    {
                        if (_prefs.debug) Debug.Log($"Adding stmt to block of type: {currentBlock.Type}");
                        currentBlock.AddStatement(stmt);
                    }
                }

                index += stmt.TotalInstructions;
                previousStatement = stmt;
            }

            if (_prefs.debug) Debug.Log($"End of parse node, block stack size: {blockStack.Count}");
            ResolveConversationLinks(conversation);

            Assert.IsTrue(currentBlock == conversation, $"Oops, BlockStatement stack mishandled while parsing node, current block type: {currentBlock?.Type}");
            return conversation;
        }

        // Every statement returned from this method is a Basic yarn statement.
        // Whether or not it's the start of a Block statement cannot be determined here,
        // it can only be determined in the ParseNode method.
        private BasicStatement ParseBasicStatement(ConversationNode conversation, int startInstructionIndex)
        {
            if (_prefs.debug) Debug.Log($"YarnProject::ParseStatement() - index: {startInstructionIndex}");

            var stmtInstructions = new List<Instruction>();

            for (var index = startInstructionIndex; index < conversation.Node.Instructions.Count; ++index)
            {
                var instruction = conversation.Node.Instructions[index];
                stmtInstructions.Add(instruction);

                if (instruction.Opcode.IsStoreVariable())
                {
                    var varName = instruction.Operands[0].StringValue;

                    // Grab the instruction right before StoreVariable.
                    // This will be a push, and tells us the type of variable we're storing
                    // NOTE: Why not check stmtInstructions[0] here instead of looking back two?
                    //       Looking again at this code after a year, revisit this when Yarn2.0 changes are made.
                    //       I trust that this is done for a reason, perhaps we cannot guarantee that the Push
                    //       really is at stmtInstructions[0] ...
                    var finalPush = stmtInstructions[stmtInstructions.Count - 2];
                    var pushedValue = finalPush.Operands[0];

                    switch (pushedValue.ValueCase)
                    {
                        case Operand.ValueOneofCase.BoolValue:
                            Variables[varName] = YarnVariable.Bool(varName);
                            break;

                        case Operand.ValueOneofCase.FloatValue:
                            Variables[varName] = YarnVariable.Float(varName);
                            break;

                        case Operand.ValueOneofCase.None:
                            Variables[varName] = YarnVariable.None(varName);
                            break;

                        case Operand.ValueOneofCase.StringValue:
                            Variables[varName] = YarnVariable.String(varName);
                            break;
                    }

                    // After a StoreVariable, we should have a Pop (to correspond with the original Push)
                    // Add it to the StoreVariable instruction list.
                    // This should ALWAYS be true as there should ALWAYS be a pop after a StoreVariable cmd,
                    // maybe drop the If and make an Assert?
                    var popIndex = index + 1;
                    // Debug.Log($"Checking to see if Pop instruction at index: {popIndex}");
                    if (popIndex < conversation.Node.Instructions.Count)
                    {
                        var popInst = conversation.Node.Instructions[popIndex];
                        if (popInst.Opcode.IsPop()) stmtInstructions.Add(popInst);
                    }

                    if (_prefs.debug) Debug.Log($"Storing variable of type: {Variables[varName].type}");
                }
                else if (instruction.Opcode.IsCommandOpCode())
                {
                    YarnCommand cmd;
                    if (instruction.Opcode.IsCallFunc())
                    {
                        var argCountPushInstruction = conversation.Node.Instructions[index - 1];
                        var argCount = argCountPushInstruction.Operands[0].FloatValue;
                        var cmdName = instruction.Operands[0].StringValue;
                        cmd = YarnCommand.CallFunc(cmdName, (int)argCountPushInstruction.Operands[0].FloatValue);
                    }
                    else
                    {
                        Assert.IsTrue(instruction.Opcode.IsRunCommand(), $"Invalid opcode for custom command {instruction.Opcode}");

                        var cmdString = instruction.Operands[0].StringValue;
                        cmd = YarnCommand.Command(cmdString);
                    }

                    if (!(cmd.IsSetSequence || Commands.ContainsKey(cmd.name)))
                    {
                        Commands[cmd.name] = cmd;
                    }
                }

                if (instruction.Opcode.IsStatementTerminatingOpCode())
                {
                    break;
                }
            }

            var stmt = new BasicStatement(stmtInstructions);
            stmt.Label = conversation.LabelMaps.GetLabel(startInstructionIndex);
            return stmt;
        }

        private Instruction GetConditionalBlockLastInstruction(ConversationNode conversation, string jumpIfFalseLabel)
        {
            //  Here's an example of what a series of instructions for a conditional block would look like:
            //  1. JumpIfFalse <-- conditional branch, jumps to the labeled instruction if false
            //  2. ... series of instructions in block ...
            //  3. AddOption or JumpTo <-- Last instruction in block
            //  4. Pop <-- The instruction with the JumpIfFalse label, ALWAYS a Pop
            //  5. ... the rest of the program's instructions ...
            var jumpIfFalseIndex = conversation.LabelMaps.GetInstructionIndex(jumpIfFalseLabel);
            return conversation.Node.Instructions[jumpIfFalseIndex - 1];
        }

        private YarnStatementType GetConditionalBlockType(ConversationNode conversation, string jumpIfFalseLabel)
        {
            // When encountering a JumpIfFalse instruction there are two possible
            // conditionally entered block types: either an If Statement or Shortcut option.
            // We can easily tell between the two by checking the final instruction of the block.
            // If the instruction is "AddOption", it's a shortcut option
            // If the instruction is "JumpTo", it's an if statement
            var instructionToCheck = GetConditionalBlockLastInstruction(conversation, jumpIfFalseLabel);

            if (instructionToCheck.Opcode.IsAddOption())
            {
                return YarnStatementType.ShortcutOption;
            }

            Assert.IsTrue(instructionToCheck.Opcode.IsJumpTo(), $"Unexpected instruction at end of conditional block: {instructionToCheck.Opcode}");
            return YarnStatementType.IfClause;
        }

        private string GetIfBlockExitLabel(ConversationNode conversation, string jumpIfFalseLabel)
        {
            // Okay, what we're trying to do here is find the label for the first instruction after the entire block of if statements.
            // We can do that by finding the instruction jumped to when the first if statement (this one) is false (i.e. not entered).
            // That instruction is the very first instruction after the first if statement, and will always be an unconditional JumpTo
            // whose label points to the very first instruction after the entire block of if statements. THAT's the index we want to store.
            // We store that so that when we come across that instruction index, we pop a BlockStatement off the stack,
            // exiting the entire block of if statements.
            // Also, our index should at some point always equal exactly that instruction index, or our parsing is incorrect.
            // var jumpIfFalseIndex = labelMaps.GetInstructionIndex(jumpIfFalseLabel);
            // var jumpToInstruction = instructions[jumpIfFalseIndex - 1];
            // var jumpToLabel = jumpToInstruction.Operands[0].StringValue;
            var instructionToCheck = GetConditionalBlockLastInstruction(conversation, jumpIfFalseLabel);
            return instructionToCheck.Operands[0].StringValue;
        }

        private void ResolveConversationLinks(ConversationNode convo)
        {
            var blockStack = new Stack<BlockStatement>();
            blockStack.Push(convo);
            YarnStatement previousStmt = null;

            // For easier access
            var lblToStmtMap = convo.LabelMaps.LabelToStatementMap;

            while (blockStack.Count > 0)
            {
                var block = blockStack.Pop();

                foreach (var currentStmt in block.Statements)
                {
                    if (currentStmt.Type.IsBasic())
                    {
                        var childStmt = (BasicStatement)currentStmt;
                        if (childStmt.BranchLabel != null)
                        {
                            if (Nodes.ContainsKey(childStmt.BranchLabel))
                            {
                                // if (_prefs.debug) Debug.Log("");
                                if (_prefs.debug) Debug.Log($"YarnProject::ResolveConversationLinks() - resolving branch conversation with name: {childStmt.BranchLabel}");
                                childStmt.BranchStatement = Nodes[childStmt.BranchLabel];
                            }
                            else if (lblToStmtMap.ContainsKey(childStmt.BranchLabel))
                            {
                                childStmt.BranchStatement = lblToStmtMap[childStmt.BranchLabel];
                            }

                            if (childStmt.BranchStatement != null)
                            {
                                childStmt.ExitStatement = childStmt.BranchStatement;
                            }
                        }

                        if (previousStmt != null)
                        {
                            previousStmt.ExitStatement = currentStmt;
                            previousStmt = currentStmt;
                        }

                        previousStmt = currentStmt;
                    }
                    else if (currentStmt is BlockStatement)
                    {
                        var childBlock = (BlockStatement)currentStmt;
                        blockStack.Push(childBlock);
                        previousStmt = null;
                    }
                }
            }
        }

        private void PrintNodes()
        {
            foreach (var nodeEntry in Program.Nodes)
            {
                var node = nodeEntry.Value;

                // When printing out node information, I want to display any label associated with an instruction
                // So build out this inverse map of Node.Labels to facilitate looking up an instruction's index
                // from its label name.
                var instructionToLabelMap = new Dictionary<int, string>();
                foreach (var labelEntry in node.Labels)
                {
                    instructionToLabelMap[labelEntry.Value] = labelEntry.Key;
                }

                if (_prefs.debug) Debug.Log($"Node[{node.Name}]: total instructions: {node.Instructions.Count}, total labels: {node.Labels.Count}");

                for (var index = 0; index < node.Instructions.Count; ++index)
                {
                    var instruction = node.Instructions[index];

                    string label = null;
                    instructionToLabelMap.TryGetValue(index, out label);

                    if (_prefs.debug) Debug.Log(InstructionToString(instruction, index, label));
                }
            }
        }

        public static string InstructionToString(Instruction instruction, int instructionIndex, string label)
        {
            // var output = "Instruction[{0}] - opcode: {1}";
            if (label == null) label = "None";
            var output = $"Instruction[{instructionIndex}] - opcode: {instruction.Opcode}, label: {label}, operands -";
            if (instruction.Operands.Count > 0)
            {
                for (var index = 0; index < instruction.Operands.Count; ++index)
                {
                    var operand = instruction.Operands[index];
                    output += $" [{index}] - {operand}";
                }
            }
            else
            {
                output += " None";
            }

            return output;
        }
    }
}
#endif
