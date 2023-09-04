#if USE_YARN2

using System;
using System.Collections.Generic;


namespace PixelCrushers.DialogueSystem.Yarn
{
    // +----------------------------------------------------------------------------
    // + Enum: BuiltInCommand
    // +----------------------------------------------------------------------------
    public enum BuiltInCommand : int
    {
        // Yarn built-in commands
        Stop,
        Wait,

        // Dialogue System built-in commands
        ClearAndAddStringFormatArg,
        AddStringFormatArg,
        StartConversation,
        StopConversation,
        PlaySequence,

        // User defined command (or typo)
        Unknown,
    }

    public static partial class BuiltInCommandExt
    {
        private static IReadOnlyDictionary<BuiltInCommand, string> _yarnNameMap = new Dictionary<BuiltInCommand, string>()
        {
            { BuiltInCommand.Stop, "stop" },
            { BuiltInCommand.Wait, "wait" },
            // { BuiltInCommand.PlaySequence, "seq" }, // This is actually a DS command, but we support it in Yarn scripts
        };

        private static IReadOnlyDictionary<BuiltInCommand, string> _luaNameMap = new Dictionary<BuiltInCommand, string>()
        {
            // Run sequence is defined in the Yarn commmands above, since we support it in all Yarn scripts
            // It is still, however, considered a Dialogue System built-in command, not a Yarn one
            { BuiltInCommand.ClearAndAddStringFormatArg, "ds_clr_add_fmt_arg" },
            { BuiltInCommand.AddStringFormatArg, "ds_add_fmt_arg" },
            { BuiltInCommand.StopConversation, "ds_stp_cnv" },
        };

        private static HashSet<BuiltInCommand> _yarnCommands = new HashSet<BuiltInCommand>() { BuiltInCommand.Stop, BuiltInCommand.Wait };
        private static HashSet<BuiltInCommand> _dsCommands = new HashSet<BuiltInCommand>() { BuiltInCommand.PlaySequence };

        public static bool IsStop(this BuiltInCommand entry) => entry == BuiltInCommand.Stop;
        public static bool IsWait(this BuiltInCommand entry) => entry == BuiltInCommand.Wait;
        public static bool IsClearAndAddStringFormatArg(this BuiltInCommand entry) => entry == BuiltInCommand.ClearAndAddStringFormatArg;
        public static bool IsAddStringFormatArg(this BuiltInCommand entry) => entry == BuiltInCommand.AddStringFormatArg;
        // public static bool IsStartConversation(this BuiltInCommand entry) => entry == BuiltInCommand.StartConversation;
        public static bool IsStopConversation(this BuiltInCommand entry) => entry == BuiltInCommand.StopConversation;
        public static bool IsPlaySequence(this BuiltInCommand entry) => entry == BuiltInCommand.PlaySequence;
        public static bool IsPlaySequence(string name) => name == "seq" || name == "sequence";
        public static bool IsUnknown(this BuiltInCommand entry) => entry == BuiltInCommand.Unknown;

        public static bool IsDialogueSystemCommand(this BuiltInCommand entry) => _dsCommands.Contains(entry);
        public static bool IsYarnCommand(this BuiltInCommand entry) => _yarnCommands.Contains(entry);

        public static string LuaName(this BuiltInCommand entry) => _luaNameMap[entry];
        public static string YarnName(this BuiltInCommand entry) => entry.IsYarnCommand() ? _yarnNameMap[entry] : null;

        static BuiltInCommandExt()
        {
            // Add all of Yarn's built-in commands to the Dialogue System's map of commands,
            // and prepend 'ds_' to all of Yarn's command names to make their Lua complements.
            var luaMap = (Dictionary<BuiltInCommand, string>)_luaNameMap;
            foreach (var entry in _yarnNameMap) luaMap[entry.Key] = $"ds_{entry.Value}";
        }

        public static BuiltInCommand FromYarnSymbol(string symbol)
        {
            foreach (var entry in _yarnNameMap) if (entry.Value == symbol) return entry.Key;
            return BuiltInCommand.Unknown;
        }
    }

    // +----------------------------------------------------------------------------
    // + Enum: BuiltInFunction
    // +----------------------------------------------------------------------------
    public enum BuiltInFunction : int
    {
        Visited,
        VisitedCount,
        Random,
        RandomRange,
        Dice,
        Round,
        RoundPlaces,
        Floor,
        Ceil,
        Inc,
        Dec,
        Decimal,
        Int,
        Unknown,
    }

    public static partial class BuiltInFunctionExt
    {
        private static IReadOnlyDictionary<BuiltInFunction, string> _yarnNameMap = new Dictionary<BuiltInFunction, string>()
        {
            { BuiltInFunction.Visited, "visited" },
            { BuiltInFunction.VisitedCount, "visited_count" },
            { BuiltInFunction.Random, "random" },
            { BuiltInFunction.RandomRange, "random_range" },
            { BuiltInFunction.Dice, "dice" },
            { BuiltInFunction.Round, "round" },
            { BuiltInFunction.RoundPlaces, "round_places" },
            { BuiltInFunction.Floor, "floor" },
            { BuiltInFunction.Ceil, "ceil" },
            { BuiltInFunction.Inc, "inc" },
            { BuiltInFunction.Dec, "dec" },
            { BuiltInFunction.Decimal, "decimal" },
            { BuiltInFunction.Int, "int" },
        };

        private static IReadOnlyDictionary<BuiltInFunction, string> _luaNameMap = new Dictionary<BuiltInFunction, string>();

        public static bool IsVisited(this BuiltInFunction entry) => entry == BuiltInFunction.Visited;
        public static bool IsVisitedCount(this BuiltInFunction entry) => entry == BuiltInFunction.VisitedCount;
        public static bool IsRandom(this BuiltInFunction entry) => entry == BuiltInFunction.Random;
        public static bool IsRandomRange(this BuiltInFunction entry) => entry == BuiltInFunction.RandomRange;
        public static bool IsDice(this BuiltInFunction entry) => entry == BuiltInFunction.Dice;
        public static bool IsRound(this BuiltInFunction entry) => entry == BuiltInFunction.Round;
        public static bool IsRoundPlaces(this BuiltInFunction entry) => entry == BuiltInFunction.RoundPlaces;
        public static bool IsFloor(this BuiltInFunction entry) => entry == BuiltInFunction.Floor;
        public static bool IsCeil(this BuiltInFunction entry) => entry == BuiltInFunction.Ceil;
        public static bool IsInc(this BuiltInFunction entry) => entry == BuiltInFunction.Inc;
        public static bool IsDec(this BuiltInFunction entry) => entry == BuiltInFunction.Dec;
        public static bool IsDecimal(this BuiltInFunction entry) => entry == BuiltInFunction.Decimal;
        public static bool IsInt(this BuiltInFunction entry) => entry == BuiltInFunction.Int;
        public static bool IsUnknown(this BuiltInFunction entry) => entry == BuiltInFunction.Unknown;

        // Lua name is the same as the Yarn name
        public static string LuaName(this BuiltInFunction entry) => _luaNameMap[entry];
        public static string YarnName(this BuiltInFunction entry) => _yarnNameMap[entry];

        static BuiltInFunctionExt()
        {
            // Prepend 'ds_' to all built-in Yarn functions to create their Lua complements
            var luaMap = (Dictionary<BuiltInFunction, string>)_luaNameMap;
            foreach (var entry in _yarnNameMap) luaMap[entry.Key] = $"ds_{entry.Value}";
        }

        public static BuiltInFunction FromYarnSymbol(string symbol)
        {
            foreach (var entry in _yarnNameMap) if (entry.Value == symbol) return entry.Key;
            return BuiltInFunction.Unknown;
        }
    }

    // +----------------------------------------------------------------------------
    // + Enum: BuiltInOperator
    // +----------------------------------------------------------------------------
    public enum BuiltInOperator : int
    {
        Invalid,
        Parentheses,
        EqualTo,
        GreaterThan,
        GreaterThanOrEqualTo,
        LessThan,
        LessThanOrEqualTo,
        NotEqualTo,
        Or,
        And,
        Xor,
        Not,
        Assign,
        UnaryMinus,
        Add,
        Minus,
        Multiply,
        Divide,
        Modulo,
        AddAssign,
        MinusAssign,
        MultiplyAssign,
        DivideAssign,
    }

    public static partial class BuiltInOperatorExt
    {
        private static readonly IReadOnlyDictionary<string, BuiltInOperator> SymbolToBuiltInOperatorMap;

        static BuiltInOperatorExt()
        {
            SymbolToBuiltInOperatorMap = new Dictionary<string, BuiltInOperator>
            {
                // OpenParens,
                // CloseParens,
                { "()", BuiltInOperator.Parentheses },

                // EqualTo, // ==, eq, is
                // GreaterThan, // >, gt
                // GreaterThanOrEqualTo, // >=, gte
                // LessThan, // <, lt
                // LessThanOrEqualTo, // <=, lte
                // NotEqualTo, // !=, neq
                { "==", BuiltInOperator.EqualTo },
                { "eq", BuiltInOperator.EqualTo },
                { "is", BuiltInOperator.EqualTo },
                { ">", BuiltInOperator.GreaterThan },
                { "gt", BuiltInOperator.GreaterThan },
                { ">=", BuiltInOperator.GreaterThanOrEqualTo },
                { "gte", BuiltInOperator.GreaterThanOrEqualTo },
                { "<", BuiltInOperator.LessThan },
                { "lt", BuiltInOperator.LessThan },
                { "<=", BuiltInOperator.LessThanOrEqualTo },
                { "lte", BuiltInOperator.LessThanOrEqualTo },
                { "!=", BuiltInOperator.NotEqualTo },
                { "neq", BuiltInOperator.NotEqualTo },

                // Logical operators
                // Or, // ||, or
                { "||", BuiltInOperator.Or },
                { "or", BuiltInOperator.Or },
                // And, // &&, and
                { "&&", BuiltInOperator.And },
                { "and", BuiltInOperator.And },
                // Xor, // ^, xor
                { "^", BuiltInOperator.Xor },
                { "xor", BuiltInOperator.Xor },
                // Not, // !, not
                { "!", BuiltInOperator.Not },
                { "not", BuiltInOperator.Not },

                // Assign, // =, to
                { "=", BuiltInOperator.Assign },
                { "to", BuiltInOperator.Assign },

                // Add, // +
                { "+", BuiltInOperator.Add },
                // Minus, // -
                { "-", BuiltInOperator.Minus },
                // Multiply, // *
                { "*", BuiltInOperator.Multiply },
                // Divide, // /
                { "/", BuiltInOperator.Divide },
                // Modulo, // %
                { "%", BuiltInOperator.Modulo },

                // AddAssign, // +=
                { "+=", BuiltInOperator.AddAssign },
                // MinusAssign, // -=
                { "-=", BuiltInOperator.MinusAssign },
                // MultiplyAssign, // *=
                { "*=", BuiltInOperator.MultiplyAssign },
                // DivideAssign, // /=
                { "/=", BuiltInOperator.DivideAssign },

            };
        }

        public static BuiltInOperator FromYarnSymbol(string symbol)
        {
            if (!SymbolToBuiltInOperatorMap.ContainsKey(symbol)) return BuiltInOperator.Invalid;

            return SymbolToBuiltInOperatorMap[symbol];
        }

        public static string LuaOperator(this BuiltInOperator op)
        {
            switch (op)
            {
                // Parentheses
                case BuiltInOperator.Parentheses:
                    return "()";

                // Arithmetic
                case BuiltInOperator.Add:
                    return "+";
                case BuiltInOperator.UnaryMinus:
                case BuiltInOperator.Minus:
                    return "-";
                case BuiltInOperator.Multiply:
                    return "*";
                case BuiltInOperator.Divide:
                    return "/";
                // I believe this requires Lua 5.1+, verify which one is used in DialogueSystem
                case BuiltInOperator.Modulo:
                    return "%";

                // UNSUPPORTED: Arithmetic Assignment
                case BuiltInOperator.AddAssign:
                    // return "+";
                    return "<ADD ASSIGN (+=) OPERATOR NOT SUPPORTED, MODIFY YOUR CODE'S LOGIC>";
                case BuiltInOperator.MinusAssign:
                    // return "-";
                    return "<SUBTRACT ASSIGN (-=) OPERATOR NOT SUPPORTED, MODIFY YOUR CODE'S LOGIC>";
                case BuiltInOperator.MultiplyAssign:
                    // return "*";
                    return "<MULTIPLY ASSIGN (*=) OPERATOR NOT SUPPORTED, MODIFY YOUR CODE'S LOGIC>";
                case BuiltInOperator.DivideAssign:
                    // return "/";
                    return "<DIVIDE ASSIGN (/=) OPERATOR NOT SUPPORTED, MODIFY YOUR CODE'S LOGIC>";
                // I believe this requires Lua 5.1+, verify which one is used in DialogueSystem
                // case BuiltInOperatorType.Modulo:
                //     return "%";

                // NOTE: I have no idea why this is called EqualToOrAssign, which worries me.
                //       Shouldn't it just be called the Assign operator? I have to be missing something.
                case BuiltInOperator.Assign:
                    // return "+";
                    return "=";

                // Logical Operators
                // Or, // ||, or
                // And, // &&, and
                // Xor, // ^, xor
                // Not, // !, not
                case BuiltInOperator.Or:
                    return "or";
                case BuiltInOperator.And:
                    return "and";
                case BuiltInOperator.Xor:
                    // return "<LOGICAL XOR OPERATOR NOT SUPPORTED, MODIFY YOUR CODE'S LOGIC>";
                    return "~=";
                case BuiltInOperator.Not:
                    return "not";

                // Relational Operators
                case BuiltInOperator.EqualTo:
                    return "==";
                case BuiltInOperator.GreaterThan:
                    return ">";
                case BuiltInOperator.GreaterThanOrEqualTo:
                    return ">=";
                case BuiltInOperator.LessThan:
                    return "<";
                case BuiltInOperator.LessThanOrEqualTo:
                    return "<=";
                case BuiltInOperator.NotEqualTo:
                    return "~=";
            }

            return null;
        }

        public static string Name(this BuiltInOperator op) => op.ToString();

        public static bool IsUnary(this BuiltInOperator op) => op == BuiltInOperator.Not || op == BuiltInOperator.UnaryMinus;
        public static bool IsBinary(this BuiltInOperator op) => !op.IsUnary();
        public static bool IsInvalid(this BuiltInOperator entry) => entry == BuiltInOperator.Invalid;
        public static bool IsParentheses(this BuiltInOperator entry) => entry == BuiltInOperator.Parentheses;
        public static bool IsEqualTo(this BuiltInOperator entry) => entry == BuiltInOperator.EqualTo;
        public static bool IsGreaterThan(this BuiltInOperator entry) => entry == BuiltInOperator.GreaterThan;
        public static bool IsGreaterThanOrEqualTo(this BuiltInOperator entry) => entry == BuiltInOperator.GreaterThanOrEqualTo;
        public static bool IsLessThan(this BuiltInOperator entry) => entry == BuiltInOperator.LessThan;
        public static bool IsLessThanOrEqualTo(this BuiltInOperator entry) => entry == BuiltInOperator.LessThanOrEqualTo;
        public static bool IsNotEqualTo(this BuiltInOperator entry) => entry == BuiltInOperator.NotEqualTo;
        public static bool IsOr(this BuiltInOperator entry) => entry == BuiltInOperator.Or;
        public static bool IsAnd(this BuiltInOperator entry) => entry == BuiltInOperator.And;
        public static bool IsXor(this BuiltInOperator entry) => entry == BuiltInOperator.Xor;
        public static bool IsNot(this BuiltInOperator entry) => entry == BuiltInOperator.Not;
        public static bool IsAssign(this BuiltInOperator entry) => entry == BuiltInOperator.Assign;
        public static bool IsUnaryMinus(this BuiltInOperator entry) => entry == BuiltInOperator.UnaryMinus;
        public static bool IsAdd(this BuiltInOperator entry) => entry == BuiltInOperator.Add;
        public static bool IsMinus(this BuiltInOperator entry) => entry == BuiltInOperator.Minus;
        public static bool IsMultiply(this BuiltInOperator entry) => entry == BuiltInOperator.Multiply;
        public static bool IsDivide(this BuiltInOperator entry) => entry == BuiltInOperator.Divide;
        public static bool IsModulo(this BuiltInOperator entry) => entry == BuiltInOperator.Modulo;
        public static bool IsAddAssign(this BuiltInOperator entry) => entry == BuiltInOperator.AddAssign;
        public static bool IsMinusAssign(this BuiltInOperator entry) => entry == BuiltInOperator.MinusAssign;
        public static bool IsMultiplyAssign(this BuiltInOperator entry) => entry == BuiltInOperator.MultiplyAssign;
        public static bool IsDivideAssign(this BuiltInOperator entry) => entry == BuiltInOperator.DivideAssign;
    }

    // +----------------------------------------------------------------------------
    // + Enum: CommandStringTokenType
    // +----------------------------------------------------------------------------
    public enum CommandStringTokenType : int
    {
        String,
        Expression,
    }

    public static partial class CommandStringTokenTypeExt
    {
        public static bool IsString(this CommandStringTokenType entry) => entry == CommandStringTokenType.String;
        public static bool IsExpression(this CommandStringTokenType entry) => entry == CommandStringTokenType.Expression;
    }

    // +----------------------------------------------------------------------------
    // + Enum: ExpressionTokenType
    // +----------------------------------------------------------------------------
    public enum ExpressionTokenType : int
    {
        Bool,
        Function,
        Number,
        Null,
        Text,
        Variable,
        Operator,
    }

    public static partial class ExpressionTokenTypeExt
    {
        public static bool IsBool(this ExpressionTokenType entry) => entry == ExpressionTokenType.Bool;
        public static bool IsFunction(this ExpressionTokenType entry) => entry == ExpressionTokenType.Function;
        public static bool IsNumber(this ExpressionTokenType entry) => entry == ExpressionTokenType.Number;
        public static bool IsNull(this ExpressionTokenType entry) => entry == ExpressionTokenType.Null;
        public static bool IsText(this ExpressionTokenType entry) => entry == ExpressionTokenType.Text;
        public static bool IsVariable(this ExpressionTokenType entry) => entry == ExpressionTokenType.Variable;
        public static bool IsOperator(this ExpressionTokenType entry) => entry == ExpressionTokenType.Operator;
    }

    // +----------------------------------------------------------------------------
    // + Enum: IfClauseType
    // +----------------------------------------------------------------------------
    public enum IfClauseType : int
    {
        If,
        ElseIf,
        Else,
    }

    public static partial class IfClauseTypeExt
    {
        public static bool IsIf(this IfClauseType entry) => entry == IfClauseType.If;
        public static bool IsElseIf(this IfClauseType entry) => entry == IfClauseType.ElseIf;
        public static bool IsElse(this IfClauseType entry) => entry == IfClauseType.Else;
    }

    // +----------------------------------------------------------------------------
    // + Enum: StatementType
    // +----------------------------------------------------------------------------
    public enum StatementType : int
    {
        Call,
        Command,
        Declare,
        Jump,
        Line,
        Set,
        Conversation,
        ShortcutOption,
        ShortcutOptionList,
        IfBlock,
        IfClause,
    }

    public static partial class StatementTypeExt
    {
        private static HashSet<StatementType> BasicStatements = new HashSet<StatementType>
        {
            StatementType.Call,
            StatementType.Command,
            StatementType.Declare,
            StatementType.Jump,
            StatementType.Line,
            StatementType.Set,
        };

        private static HashSet<StatementType> BlockStatements = new HashSet<StatementType>
        {
            StatementType.Conversation,
            StatementType.ShortcutOption,
            StatementType.ShortcutOptionList,
            StatementType.IfBlock,
            StatementType.IfClause,
        };

        public static bool IsBasic(this StatementType entry) => BasicStatements.Contains(entry);
        public static bool IsBlock(this StatementType entry) => BlockStatements.Contains(entry);

        public static bool IsCall(this StatementType entry) => entry == StatementType.Call;
        public static bool IsCommand(this StatementType entry) => entry == StatementType.Command;
        public static bool IsDeclare(this StatementType entry) => entry == StatementType.Declare;
        public static bool IsJump(this StatementType entry) => entry == StatementType.Jump;
        public static bool IsLine(this StatementType entry) => entry == StatementType.Line;
        public static bool IsSet(this StatementType entry) => entry == StatementType.Set;
        public static bool IsConversation(this StatementType entry) => entry == StatementType.Conversation;
        public static bool IsShortcutOption(this StatementType entry) => entry == StatementType.ShortcutOption;
        public static bool IsShortcutOptionList(this StatementType entry) => entry == StatementType.ShortcutOptionList;
        public static bool IsIfBlock(this StatementType entry) => entry == StatementType.IfBlock;
        public static bool IsIfClause(this StatementType entry) => entry == StatementType.IfClause;
    }
}

#endif
