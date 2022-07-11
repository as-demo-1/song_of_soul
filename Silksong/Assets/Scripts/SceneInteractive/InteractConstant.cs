
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

public enum EInact
{
    WORLD_ROOT,
    CONTAINER,
    ITEM
}

public enum EInact2
{
    NORMAL,
    PLAYER_TARGET,
    THIS_TARGET
}

public enum EInteractStatus
{
    INIT,
    BEFORE_INTERACT,
    DO_INTERACT,
    DO_INTERACT_Y,
    DO_INTERACT_N,
    AFTER_INTERACT,
    FINISH
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
