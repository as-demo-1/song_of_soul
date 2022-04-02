using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class TalkManager
{
    //所有条件都储存在这个字典中
    public Dictionary<string, bool> TalkStatusJudge = new Dictionary<string,bool>();//前面是条件名称，后面判断是否达成

    //-----------------------------------------------
    public Dictionary<int, string> PlotContent = new Dictionary<int, string>();//前面是每个对话的ID，后面是剧情对话内容
    public Dictionary<int, string> GossipContent = new Dictionary<int, string>();//前面是对话ID，后面是闲聊对话内容
    public Dictionary<int, List<int>> NPCPlotContent = new Dictionary<int, List<int>>();//前面是NPCID，后面是这个NPC的所有剧情对话的StartID
    public Dictionary<int, List<int>> NPCGossipContent = new Dictionary<int, List<int>>();//前面是NPCID，后面是这个NPC的所有闲聊对话的StartID
    public Dictionary<int, List<string>> Condition = new Dictionary<int, List<string>>();//前面是对话StartID，后面是控制对话的条件
    public Dictionary<int, string> Name = new Dictionary<int, string>();


    private bool _isInit;
    public bool IsInit => _isInit;

    private static TalkManager _instance;
    public static TalkManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TalkManager();
            }

            if (!_instance.IsInit)
            {
                _instance.Init();
            }

            return _instance;
        }
    }

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

