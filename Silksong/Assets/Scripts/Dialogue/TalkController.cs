using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalkController
{
    private Transform _UI_trans;

    private GameObject TalkPanel;
    private Text NPCText;
    private Text NPCName;

    private static TalkController _instance;
    public static TalkController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TalkController();
                _instance.loadUI();
            }

            return _instance;
        }
    }

    private static int _id = default;   //如果id为0，说明这是对话的第一句，用来判定是进行的是剧情对话还是闲聊
    private int _count = default;

    private void loadUI()
    {
        _UI_trans = GameObject.Find("UI").transform;
        TalkPanel = FindUI("Talk");
        NPCText = FindUI("Talk/TalkPanel/NPCTalkPanel/NPCText").GetComponent<Text>();
        NPCName = FindUI("Talk/TalkPanel/NPCTalkPanel/NPCName").GetComponent<Text>();
        // todo: test
        testBtn();
    }

    // todo: 测试按钮，点击会修改对话状态
    private void testBtn()
    {
        UIAddListener("Test/money", () =>
        {
            TalkConditionState.money += 10;
            Text txt = FindUI("Test/money/Text").GetComponent<Text>();
            txt.text = "当前金币："+ TalkConditionState.money + " ，点击增加";
        });
        UIAddListener("Test/lock", () =>
        {
            TalkConditionState.isLock = !TalkConditionState.isLock;
            Text txt = FindUI("Test/lock/Text").GetComponent<Text>();
            txt.text = "当前对话" + (TalkConditionState.isLock ? "已" : "未") + "解锁";
        });
    }

    // todo: 挂载按钮事件，测试用
    protected void UIAddListener(string path, UnityEngine.Events.UnityAction action)
    {
        GameObject go = FindUI(path);

        if (go != null)
        {
            Button btn = go.GetComponent<Button>();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }

    private GameObject FindUI(string path)
    {
        return _UI_trans.Find(path).gameObject;
    }

    public void StartTalk(int NPCID)
    {
        TalkPanel.SetActive(true);
        for (int num = 0; num < TalkManager.Instance.TalkStatus[NPCID].TalkFlags.Count; num++) //如果之后有多个条件达成才能触发的对话，在这里添加判定条件再起一个if判断
        {
            if (!TalkManager.Instance.TalkStatus[NPCID].ConditionNos.ContainsKey(TalkManager.Instance.TalkStatus[NPCID].TalkFlags[num]))
            {
                if (TalkManager.Instance.TalkStatus[NPCID].TalkFlags[num] != -1) // 如果TalkFlags有不为-1的值，说明还有剧情对话没过完  
                {
                    TalkManager.Instance.TalkStatus[NPCID].TalkFlags[num] = -1;
                    _id = TalkManager.Instance.TalkStatus[NPCID].StartTalkNo[num];
                    NPCText.text = TalkManager.Instance.TalkContent[_id];
                    NPCName.text = TalkManager.Instance.TalkNPC[_id];
                    break;
                }
                _id = TalkManager.Instance.TalkStatus[NPCID].StartTalkNo[0];
                NPCText.text = TalkManager.Instance.TalkContent[_id];
                NPCName.text = TalkManager.Instance.TalkNPC[_id];
            }
            else
            {
                // todo:在触发对话前先改变条件状态
                ChangeCondition.Instance.JudgeCondition(NPCID);
                for (int j = 0; j < TalkManager.Instance.TalkStatus[NPCID].ConditionNos[num].Count; j++)
                {
                    if (TalkManager.Instance.TalkStatus[NPCID].Condition[j] != 1) //只要有一个条件不为1，说明这一条剧情对话还不能触发
                    {
                        _id = TalkManager.Instance.TalkStatus[NPCID].StartTalkNo[0];
                        NPCText.text = TalkManager.Instance.TalkContent[_id];
                        NPCName.text = TalkManager.Instance.TalkNPC[_id];
                        break;
                    }
                    else
                    {
                        _count += 1;
                    }
                }
                if (_count == TalkManager.Instance.TalkStatus[NPCID].ConditionNos[num].Count)
                {
                    TalkManager.Instance.TalkStatus[NPCID].TalkFlags[num] = -1;
                    _id = TalkManager.Instance.TalkStatus[NPCID].StartTalkNo[num];
                    NPCText.text = TalkManager.Instance.TalkContent[_id];
                    NPCName.text = TalkManager.Instance.TalkNPC[_id];
                }
                _count = 0;
            }
        }
    }

    public void NextTalk(int NPCID)
    {
        //_id = StartTalkNo[0];
        if (TalkManager.Instance.TalkNo[_id] != -1)//如果Next_id不为-1
        {
            _id = TalkManager.Instance.TalkNo[_id];//_id等于Next_id
            NPCText.text = TalkManager.Instance.TalkContent[_id];//更新文本
            NPCName.text = TalkManager.Instance.TalkNPC[_id]; //更新说话人
        }
        else
        {
            TalkPanel.SetActive(false);
            _id = 0;
            DialogInteract.Instance.ContinueEvent();
        }
    }

    public void TalkAction(int NPCID)
    {

        if (_id == 0)
        {
            StartTalk(NPCID);
        }
        else
        {
            NextTalk(NPCID);
        }
    }
}
