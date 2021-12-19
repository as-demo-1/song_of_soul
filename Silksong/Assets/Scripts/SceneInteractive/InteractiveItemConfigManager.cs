using System.Collections.Generic;

public class InteractiveItemConfigManager
{
    private Dictionary<int, InteractiveSO> _itemConfig = new Dictionary<int, InteractiveSO>();

    private static InteractiveItemConfigManager s_instance;
    public static InteractiveItemConfigManager Instance
    {
        get
        {
            if (s_instance == null)
                s_instance = new InteractiveItemConfigManager();

            return s_instance;
        }
    }

    public Dictionary<int, InteractiveSO> ItemConfig => _itemConfig;
}

public enum EInteractiveItemType
{
    NONE = -1,
    DIALOG,
    JUDGE,
    FULLWINDOW
}
