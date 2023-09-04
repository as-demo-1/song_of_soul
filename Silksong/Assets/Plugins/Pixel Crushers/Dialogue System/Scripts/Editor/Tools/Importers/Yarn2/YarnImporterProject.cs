#if USE_YARN2

using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace PixelCrushers.DialogueSystem.Yarn
{
    // +---------------------------------------------------------------------------------------------------------------
    // + Class: YarnLocalizedString
    // + Description:
    // +    Represents a string and list of all of its localizations in Yarn.
    // +    Each instance of this class will be associated with a unique Line ID, defined by the Yarn compiler.
    // +---------------------------------------------------------------------------------------------------------------
    public class YarnLocalizedString
    {
        private Dictionary<string, string> _localizations = new Dictionary<string, string>();

        public string DefaultText { get; private set; }
        public IReadOnlyDictionary<string, string> Localizations { get => _localizations; }
        public IReadOnlyDictionary<string, string> DefaultHashtags = new Dictionary<string, string>();
        public IReadOnlyDictionary<string, string> LocalizedHashtags = new Dictionary<string, string>();

        public YarnLocalizedString(string defaultText)
        {
            DefaultText = defaultText;
        }

        public void AddLocale(string locale, string text) => _localizations[locale] = text;
        public void AddDefaultHashtag(string name, string val) => ((Dictionary<string, string>)DefaultHashtags)[name] = val;
        public void AddLocalizedHashtag(string name, string val) => ((Dictionary<string, string>)LocalizedHashtags)[name] = val;
    }

    // +---------------------------------------------------------------------------------------------------------------
    // + Class: YarnExpressionToken
    // + Description:
    // +    Each YarnExpressionToken is a piece of a run-time calculated expression in a Yarn script.
    // +    Any YarnStatement can have an expression, which is a stack containing these tokens.
    // +    The YarnProjectWriter will recreate these expressions in Lua while generating the databse.
    // +---------------------------------------------------------------------------------------------------------------
    public abstract class YarnExpressionToken
    {
        public ExpressionTokenType Type { get; private set; }
        public YarnExpressionToken(ExpressionTokenType type) { Type = type; }
    }

    public class YarnExpressionToken<T> : YarnExpressionToken
    {
        public T Value { get; private set; }
        public YarnExpressionToken(ExpressionTokenType type, T value) : base(type) { Value = value; }
        public override string ToString() => Value.ToString();
    }

    public class BoolToken : YarnExpressionToken<bool> { public BoolToken(bool value) : base(ExpressionTokenType.Bool, value) {} }
    public class NumberToken : YarnExpressionToken<float> { public NumberToken(float value) : base(ExpressionTokenType.Number, value) {} }
    public class NullToken : YarnExpressionToken<object> { public NullToken() : base(ExpressionTokenType.Null, null) {} }
    public class StringToken : YarnExpressionToken<string> { public StringToken(string value) : base(ExpressionTokenType.Text, value) {} }
    public class VariableToken : YarnExpressionToken<string> { public VariableToken(string value) : base(ExpressionTokenType.Variable, value) {} }

    public class FunctionToken : YarnExpressionToken<string>
    {
        public string Name { get => Value; }
        public int ArgCount { get; private set; } = 0;
        public BuiltInFunction FunctionType { get; private set; }
        public FunctionToken(string name, int argCount) : base(ExpressionTokenType.Function, name)
        {
            FunctionType = BuiltInFunctionExt.FromYarnSymbol(Name);
            ArgCount = argCount;
            Debug.Log($"FunctionToken::FunctionToken() - func: {Name}, type: {FunctionType}, arg#: {argCount}");
        }
    }

    public class OperatorToken : YarnExpressionToken<BuiltInOperator>
    {
        public int ArgCount { get; private set; } = 0;
        public OperatorToken(BuiltInOperator value, int argCnt) : base(ExpressionTokenType.Operator, value) => ArgCount = argCnt;
    }

    // +---------------------------------------------------------------------------------------------------------------
    // + Class: YarnExpression
    // + Description:
    // +    Holds a stack of YarnExpressionTokens representing the run-time expression to be calculated.
    // +---------------------------------------------------------------------------------------------------------------
    public class YarnExpression
    {
        private Stack<YarnExpressionToken> _stack = new Stack<YarnExpressionToken>();
        public Stack<YarnExpressionToken> StackCopy { get => new Stack<YarnExpressionToken>(_stack.Reverse()); }
        public bool IsEmpty { get => _stack.Count <= 0; }
        public int TokenCount { get => _stack.Count; }
        public void PushToken(YarnExpressionToken token) => _stack.Push(token);
    }

    // +---------------------------------------------------------------------------------------------------------------
    // + Class: YarnStatement
    // + Description:
    // +    Base class for all YarnStatement types. Refer to YarnStatementType enum for all statement types.
    // +---------------------------------------------------------------------------------------------------------------
    public abstract class YarnStatement
    {
        public BlockStatement Parent { get; internal set; }
        public StatementType Type { get; protected set; }
        public bool HasConditions { get => !Conditions.IsEmpty; }
        public bool HasExpression { get => !Expression.IsEmpty; }
        public YarnExpression Conditions { get; private set; } = new YarnExpression();
        public YarnExpression Expression { get; private set; } = new YarnExpression();

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

        public YarnStatement(StatementType type) => Type = type;
        public void SetExpression(YarnExpression exp) => Expression = exp;
    }

    // +---------------------------------------------------------------------------------------------------------------
    // + Class: BasicStatement
    // + Description:
    // +    Base class for all basic statement types. Basic statements are singular, self contained statements.
    // +    Refer to YarnStatementTypeExt.IsBasic() for all basic statement types.
    // +---------------------------------------------------------------------------------------------------------------
    public class BasicStatement : YarnStatement { public BasicStatement(StatementType type) : base(type) {} }

    public struct CommandStringToken
    {
        public readonly CommandStringTokenType Type;
        public readonly String Value;

        public static CommandStringToken Text(string text) => new CommandStringToken(CommandStringTokenType.String, text);
        public static CommandStringToken Placeholder(int index) => new CommandStringToken(CommandStringTokenType.Expression, $"{{{index}}}");

        public CommandStringToken(CommandStringTokenType type, String value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString() => Value;
    }

    public class CallStatement : BasicStatement
    {
        public string Name { get; private set; }

        public CallStatement(string name) : base(StatementType.Call) => Name = name;
    }

    public class CommandStatement : BasicStatement
    {
        public string Name { get; private set; }
        public BuiltInCommand CommandType { get; private set; }
        public IReadOnlyList<CommandStringToken> StringTokens { get; private set; } = new List<CommandStringToken>();
        public int ExpressionCount { get; private set; } = 0;
        public string Text { get; private set; }

        public CommandStatement(string name, string fullText, List<CommandStringToken> args) : base(StatementType.Command)
        {
            Name = name;
            Text = fullText;
            CommandType = BuiltInCommandExt.FromYarnSymbol(Name);
            StringTokens = args;
            foreach (var argToken in StringTokens) if (argToken.Type.IsExpression()) ++ExpressionCount;
            Debug.Log($"CommandStatement::CommandStatement() - cmd: {Name}, type: {CommandType}, exp#: {ExpressionCount}");
        }
    }

    public class JumpStatement : BasicStatement
    {
        public string Destination { get; private set; }
        public JumpStatement(string destNode) : base(StatementType.Jump) => Destination = destNode;
    }

    public class LineStatement : BasicStatement
    {
        public string LineId { get; private set; }
        public IReadOnlyDictionary<string, string> Hashtags = new Dictionary<string, string>();
        public LineStatement(string lineId) : base(StatementType.Line) => LineId = lineId;
        public void SetLineId(string lineId) => LineId = lineId;
        public void AddHashtag(string name, string value) => ((Dictionary<string, string>)Hashtags)[name] = value;
    }

    public class SetStatement : BasicStatement
    {
        public string Variable { get; private set; }
        public SetStatement(string varName) : base(StatementType.Set) => Variable = varName;
    }

    public class DeclareStatement : SetStatement
    {
        public DeclareStatement(string varName) : base(varName) => Type = StatementType.Declare;
    }

    // +---------------------------------------------------------------------------------------------------------------
    // + Class: BlockStatement
    // + Description:
    // +    Base class for all basic statement types. Block statements contain other statements.
    // +    Refer to YarnStatementTypeExt.IsBlock() for all block statement types.
    // +---------------------------------------------------------------------------------------------------------------
    public class BlockStatement : YarnStatement
    {
        public IReadOnlyList<YarnStatement> Statements { get; private set; } = new List<YarnStatement>();
        public BlockStatement(StatementType type) : base(type) {}

        public virtual void AddStatement(YarnStatement stmt) => ((List<YarnStatement>)Statements).Add(stmt);
    }

    // +---------------------------------------------------------------------------------------------------------------
    // + Class: BlockStatement<T>
    // + Description:
    // +    This class just adds a little convenience, it creates a separate typed Statements array
    // +    that does a little type checking on construction, removing the need for it later on in the code.
    // +---------------------------------------------------------------------------------------------------------------
    public class BlockStatement<T> : BlockStatement where T : YarnStatement
    {
        public IReadOnlyList<T> TypedStatements { get; private set; } = new List<T>();
        public BlockStatement(StatementType type) : base(type) {}

        public override void AddStatement(YarnStatement stmt)
        {
            ((List<YarnStatement>)Statements).Add(stmt);
            ((List<T>)TypedStatements).Add((T)stmt);
        }
    }

    // +---------------------------------------------------------------------------------------------------------------
    // + Class: ConversationNode
    // + Description:
    // +    The top-level statement, it represents a Node in Yarn and a Conversation in the DialogueDatabase.
    // +---------------------------------------------------------------------------------------------------------------
    public class ConversationNode : BlockStatement
    {
        public string Name { get; protected set; }
        public IReadOnlyDictionary<string, string> Header { get; protected set; } = new Dictionary<string, string>();

        public ConversationNode() : base(StatementType.Conversation) {}

        public void AddHeader(string key, string value)
        {
            ((Dictionary<string, string>)Header)[key] = value;

            // Keep an eye out for the "title" metadata (case-sensitive), if found use it to set the node's name
            if (key.Equals(YarnImporterProject.NodeHeaderTitleKey, StringComparison.InvariantCulture)) Name = value;
        }
    }

    public class IfBlock : BlockStatement<IfClause>
    {
        public bool HasElse { get => CheckForElse(); }
        public IReadOnlyList<IfClause> IfClauses { get => TypedStatements; }

        public IfBlock() : base(StatementType.IfBlock) {}

        private bool CheckForElse()
        {
            if (Statements.Count < 1) return false;
            return ((IfClause)Statements[Statements.Count - 1]).ClauseType == IfClauseType.Else;
        }
    }

    public class IfClause : BlockStatement
    {
        public IfClauseType ClauseType { get; private set; }

        public static IfClause CreateIf() { return new IfClause(IfClauseType.If); }
        public static IfClause CreateElseIf() { return new IfClause(IfClauseType.ElseIf); }
        public static IfClause CreateElse() { return new IfClause(IfClauseType.Else); }
        public IfClause(IfClauseType type) : base(StatementType.IfClause) { ClauseType = type; }
    }

    public class ShortcutOptionList : BlockStatement<ShortcutOption>
    {
        public ShortcutOptionList() : base(StatementType.ShortcutOptionList) {}
        public IReadOnlyList<ShortcutOption> Options { get => TypedStatements; }
    }

    public class ShortcutOption : BlockStatement
    {
        public LineStatement Line { get => (LineStatement)Statements[0]; }
        public ShortcutOption() : base(StatementType.ShortcutOption) {}
    }

    public class YarnImporterProject
    {
        public const string GeneratedDeclarationNodeName = "_AutoGeneratedVariableDeclarations";
        public const string OptionConditionsVariableNameFormat = "${0}_opt_cond_{{0}}";
        public const string NodeHeaderTerminationToken = "---";
        public const string NodeTerminationToken = "===";
        public const string NodeHeaderRegex = "(.+?):(.*)";
        public const string NodeHeaderTitleKey = "title";
        public const string NodeHeaderActorKey = "actor";
        public const string NodeHeaderConversantKey = "conversant";

        private Dictionary<string, ConversationNode> _nodes = new Dictionary<string, ConversationNode>();
        private Dictionary<string, IReadOnlyList<YarnLocalizedString>> _localizedStringTable = new Dictionary<string, IReadOnlyList<YarnLocalizedString>>();

        public IReadOnlyDictionary<string, ConversationNode> Nodes { get => _nodes; }
        public IReadOnlyDictionary<string, YarnLocalizedString> LocalizedStringTable { get; private set; }
        
        private YarnImporterPrefs _prefs;

        public YarnImporterProject(YarnImporterPrefs prefs, IReadOnlyDictionary<string, YarnLocalizedString> localizedStringTable)
        {
            _prefs = prefs;
            LocalizedStringTable = localizedStringTable;
        }

        public void AddNode(ConversationNode node) => _nodes[node.Name] = node;
    }
}
#endif
