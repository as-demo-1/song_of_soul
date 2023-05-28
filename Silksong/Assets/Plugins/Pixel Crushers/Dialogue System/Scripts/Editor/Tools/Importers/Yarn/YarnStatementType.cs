#if USE_YARN

using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.Yarn
{
    public enum YarnStatementType
    {
        Basic = 0,
        // AddOption,
        ShortcutOption,
        ShortcutOptionList,
        Conversation,
        IfBlock,
        IfClause,
        // ElseIfClause, // Technically this is unneeded, but the extra info may help with debugging
        // ElseClause, // Technically this is unneeded, but the extra info may help with debugging
    }

    public static class YarnStatementTypeExtensions
    {
        // private static HashSet<YarnStatementType> IfSubClauseTypes = new HashSet<YarnStatementType>
        // {
        //     YarnStatementType.IfClause,
        //     YarnStatementType.ElseIfClause,
        //     YarnStatementType.ElseClause,
        // };

        public static bool IsBasic(this YarnStatementType stmtType)
        {
            return stmtType == YarnStatementType.Basic;
        }

        public static bool IsShortcutOption(this YarnStatementType stmtType)
        {
            return stmtType == YarnStatementType.ShortcutOption;
        }

        public static bool IsShortcutOptionList(this YarnStatementType stmtType)
        {
            return stmtType == YarnStatementType.ShortcutOptionList;
        }

        public static bool IsConversation(this YarnStatementType stmtType)
        {
            return stmtType == YarnStatementType.Conversation;
        }

        public static bool IsIfBlock(this YarnStatementType stmtType)
        {
            return stmtType == YarnStatementType.IfBlock;
        }

        // public static bool IsIfSubClause(this YarnStatementType stmtType)
        // {
        //     return IfSubClauseTypes.Contains(stmtType);
        // }

        public static bool IsIfClause(this YarnStatementType stmtType)
        {
            return stmtType == YarnStatementType.IfClause;
        }

        // public static bool IsElseIfClause(this YarnStatementType stmtType)
        // {
        //     return stmtType == YarnStatementType.ElseIfClause;
        // }

        // public static bool IsElseClause(this YarnStatementType stmtType)
        // {
        //     return stmtType == YarnStatementType.ElseClause;
        // }
    }


    public enum IfClauseType
    {
        If,
        ElseIf,
        Else,
    }

    public static class IfClauseTypeExtensions
    {
        public static bool IsIf(this IfClauseType ifType)
        {
            return ifType == IfClauseType.If;
        }

        public static bool IsElseIf(this IfClauseType ifType)
        {
            return ifType == IfClauseType.ElseIf;
        }

        public static bool IsElse(this IfClauseType ifType)
        {
            return ifType == IfClauseType.Else;
        }
    }
}

#endif