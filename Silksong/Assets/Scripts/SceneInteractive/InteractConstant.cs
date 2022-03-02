
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
