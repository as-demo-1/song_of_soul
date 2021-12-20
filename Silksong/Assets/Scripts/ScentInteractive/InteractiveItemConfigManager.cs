using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveItemConfigManager
{
    public Dictionary<int, InteractiveItemConfig> config = new Dictionary<int, InteractiveItemConfig>()
    {
        { 1001, new InteractiveItemConfig(1001, "椅子", InteractiveItemType.NONE, "休息")},
        { 1002, new InteractiveItemConfig(1002, "信", InteractiveItemType.DIALOG, "聆听")},
        { 1003, new InteractiveItemConfig(1003, "锁住的门", InteractiveItemType.JUDGE, "查看")},
        { 1004, new InteractiveItemConfig(1004, "遗迹1", InteractiveItemType.FULLWINDOW, "查看")},
    };

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
}

public enum InteractiveItemType
{
    NONE = -1,
    DIALOG,
    JUDGE,
    FULLWINDOW
}

public class InteractiveItemConfig
{
    private int m_ID;
    private string m_name;
    private InteractiveItemType m_itemType;
    private string m_content;

    public InteractiveItemConfig(int ID, string name, InteractiveItemType itemType, string content)
    {
        m_ID = ID;
        m_name = name;
        m_itemType = itemType;
        m_content = content;
    }

    public int getID()
    {
        return m_ID;
    }

    public string getName()
    {
        return m_name;
    }

    public InteractiveItemType getItemType()
    {
        return m_itemType;
    }

    public string getContent()
    {
        return m_content;
    }
}
