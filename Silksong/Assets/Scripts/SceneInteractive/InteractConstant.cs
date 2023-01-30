
public enum EInteractiveItemType
{
    NONE = -1,
    DIALOG,
    JUDGE,
    FULLTEXT
}

public enum EFuncInteractItemType
{
    SAVE,    // 存档点
    PORTAL,  // 传送门
}

public enum EInteractStatus
{
    CANT_INTERACT = -1, // 不可交互
    NO_INTERACT,        // 未交互
    BEGIN_INTERACT,     // 开始交互
    INTERACTING,        // 交互中
    FINISH_INTERACT     // 结束交互
}

public class InteractConstant
{
    // UIFullText
    public static readonly string UIFullText = "FullWindowText";
    public static readonly string UIFullTextText = "FullWindowText/Image/Text";
    public static readonly string UIFullTextClose = "FullWindowText/Image/Close";

    // Judge
    public static readonly string UIJudge = "Judge";
    public static readonly string UIJudgeContent = "Judge/Image/Content";
    public static readonly string UIJudgeLB = "Judge/Image/LButton";
    public static readonly string UIJudgeRB = "Judge/Image/RButton";

    // Tip
    public static readonly string UITip = "Tip";
    public static readonly string UITipContainer = "Tip/Container";
    public static readonly string UITipText = "Tip/Container/Text";

    // Talk
    public static readonly string UITalkNext = "Talk/TalkPanel/Next";
}
