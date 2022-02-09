using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class TalkManager : MonoBehaviour
{
    public DialogContainerSO DialogueContainer;

    //所有条件都储存在这个字典中
    public Dictionary<string, bool> TalkStatusJudge = new Dictionary<string,bool>();//前面是条件名称，后面判断是否达成

    
    public Dictionary<int, List<string>> TalkContentDic = new Dictionary<int, List<string>>(); //每一个List<string>都是一段对话
    public Dictionary<int, string> TalkStatus = new Dictionary<int, string>(); //前面是第几段对话，后面用于检索条件列表是否达成，表示第int段对话由string(s)条件控制

    public Dictionary<int, Dictionary<int, List<string>>> NPCAllContent = new Dictionary<int, Dictionary<int, List<string>>>();
    public Dictionary<int, Dictionary<int, string>> NPCAllCondition = new Dictionary<int, Dictionary<int, string>>();
    public Dictionary<int, string> Name = new Dictionary<int, string>();

    private bool _isInit;
    public bool IsInit => _isInit;

    private static TalkManager _instance;

    // Start is called before the first frame update
    void Awake()
    {
        _instance = this;
        Init();
    }

    public static TalkManager Instance => _instance;

    public void Init()
    {
        if (_isInit) return;

        _isInit = true;
    }
}

public static class TalkConditionState
{
    public static int money;
    public static bool isLock;
}

