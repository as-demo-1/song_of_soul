using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class TalkManager
{
    public Dictionary<int, string> TalkContent = new Dictionary<int, string>();//int是对话id，string是对话内容
    public Dictionary<int, int> TalkNo = new Dictionary<int, int>(); //key是当前对话ID，value是NextID
    public Dictionary<int, string> TalkNPC = new Dictionary<int, string>();//int是对话ID，string是说话的NPC名字

    public Dictionary<int, DialogueStatusSO> TalkStatus = new Dictionary<int, DialogueStatusSO>();

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

        foreach (DialogueSO dialogueItem in TalkSOManager.Instance.DialogueContainer.DialogueList)
        {
            TalkContent[dialogueItem.ID] = dialogueItem.Detail;
            TalkNo[dialogueItem.ID] = dialogueItem.NextID;
            TalkNPC[dialogueItem.ID] = dialogueItem.NPCName;
        }

        foreach (DialogueStatusSO dialogueStatus in TalkSOManager.Instance.DialogueContainer.DialogueStatusList)
        {
            TalkStatus[dialogueStatus.NPCID] = dialogueStatus;
        }

        _isInit = true;
    }
}

public static class TalkConditionState
{
    public static int money;
    public static bool isLock;
}
