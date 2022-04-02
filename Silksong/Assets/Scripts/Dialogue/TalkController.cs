using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TalkController : MonoBehaviour
{
    private Transform _UI_trans;

    private GameObject TalkPanel;
    private Text NPCText;
    private Text NPCName;
    private int _id = 0; //判断是否满足条件的指示器
    private static int i = 0;

    private static int TalkID = 0;//对话ID，指示进行到了哪里
    private static int startID;//指示每段对话的第一句对话的ID
    private static int TalkCount = 0;//指示剧情对话到了NPC的第几段对话
    private static int _end = 0;//指示一段对话是否已经结束






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

    //private static int _id = default;   //如果id为0，说明这是对话的第一句，用来判定是进行的是剧情对话还是闲聊
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
            txt.text = "当前金币：" + TalkConditionState.money + " ，点击增加";
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

    public void StartTalk(int NPCID, UnityAction callback)
    {
        TalkPanel.SetActive(true);
        //Gossip Dialogue
        if (!TalkManager.Instance.NPCPlotContent.ContainsKey(NPCID) || TalkCount >= TalkManager.Instance.NPCPlotContent[NPCID].Count)
        {
            if (_end == 0)
            {
                int rnum = Random.Range(0, TalkManager.Instance.NPCGossipContent[NPCID].Count);
                i = TalkManager.Instance.NPCGossipContent[NPCID][rnum];
                startID = i;
                _end = 1;
                NPCText.text = TalkManager.Instance.GossipContent[i];
                NPCName.text = TalkManager.Instance.Name[NPCID];
                i += 1;
            }
            else
            {
                //Debug.Log(_end + "-" + i);
                if (TalkManager.Instance.GossipContent.ContainsKey(i))
                {
                    NPCText.text = TalkManager.Instance.GossipContent[i];
                    NPCName.text = TalkManager.Instance.Name[NPCID];
                    i += 1;
                }
                else
                {
                    i = 0;
                    _end = 0;
                    TalkPanel.SetActive(false);
                    DialogueChangeItem.Instance.ItemAct(startID);
                    callback();

                }
                _id = 0;
            }
        }

        //PlotDialogue
        else
        {
            if (_end == 0)
            {
                TalkID = TalkManager.Instance.NPCPlotContent[NPCID][TalkCount];
                int rnum = Random.Range(0, TalkManager.Instance.NPCGossipContent[NPCID].Count);
                i = TalkManager.Instance.NPCGossipContent[NPCID][rnum];

                if (!TalkManager.Instance.Condition.ContainsKey(TalkID))
                {
                    startID = TalkID;
                    NPCText.text = TalkManager.Instance.PlotContent[TalkID];
                    NPCName.text = TalkManager.Instance.Name[NPCID];
                    TalkID += 1;
                }
                else
                {
                    ConditionStatus();
                    foreach (string conditionname in TalkManager.Instance.Condition[TalkID])
                    {
                        if (!TalkManager.Instance.TalkStatusJudge[conditionname]) //如果有一个条件未满足
                        {
                            _id = 0;
                            break;
                        }
                        _id = 1;
                    }
                    if (_id == 0)
                    {
                        startID = i;
                        NPCText.text = TalkManager.Instance.GossipContent[i];
                        NPCName.text = TalkManager.Instance.Name[NPCID];
                        i += 1;
                    }
                    else
                    {
                        startID = TalkID;
                        NPCText.text = TalkManager.Instance.PlotContent[TalkID];
                        NPCName.text = TalkManager.Instance.Name[NPCID];
                        TalkID += 1;
                    }
                }

                _end = 1;
            }
            else
            {
                if (!TalkManager.Instance.Condition.ContainsKey(TalkID)) //如果TalkCondition里不包含这段对话的键，说明这段对话可以无条件触发
                {
                    //Debug.Log(TalkManager.Instance.NPCAllContent[NPCID].Count); //1
                    if (TalkManager.Instance.PlotContent.ContainsKey(TalkID))
                    {
                        NPCText.text = TalkManager.Instance.PlotContent[TalkID];
                        NPCName.text = TalkManager.Instance.Name[NPCID];
                        TalkID += 1;
                        //Debug.Log(i);
                    }
                    else
                    {
                        TalkCount += 1;
                        _end = 0;
                        TalkPanel.SetActive(false);
                        DialogueChangeItem.Instance.ItemAct(startID);
                        callback();
                    }
                }
                else
                {
                    //检验条件
                    ConditionStatus();
                    foreach (string conditionname in TalkManager.Instance.Condition[TalkID])
                    {
                        if (!TalkManager.Instance.TalkStatusJudge[conditionname]) //如果有一个条件未满足
                        {
                            _id = 0;
                            break;
                        }
                        _id = 1;
                    }
                    if (_id == 0)
                    {
                        //条件不满足，进入闲聊
                        if (TalkManager.Instance.GossipContent.ContainsKey(i))
                        {
                            NPCText.text = TalkManager.Instance.GossipContent[i];
                            NPCName.text = TalkManager.Instance.Name[NPCID];
                            i += 1;
                        }
                        else
                        {
                            i = 0;
                            _end = 0;
                            TalkPanel.SetActive(false);
                            DialogueChangeItem.Instance.ItemAct(startID);
                            callback();
                        }
                    }
                    else if (_id == 1)
                    {
                        if (TalkManager.Instance.PlotContent.ContainsKey(TalkID))
                        {
                            NPCText.text = TalkManager.Instance.PlotContent[TalkID];
                            NPCName.text = TalkManager.Instance.Name[NPCID];
                            TalkID += 1;
                        }
                        else
                        {
                            TalkCount += 1;
                            _end = 0;
                            TalkPanel.SetActive(false);
                            DialogueChangeItem.Instance.ItemAct(startID);
                            callback();
                        }
                    }
                    _id = 0;
                }
            }
        }
    }


    public void ConditionStatus()
    {
        foreach (DialogueSectionSO DialogueItem in TalkSOManager.Instance.DialogueSectionListInstance)
        {
            for (int x = 0; x < DialogueItem.DialogueList.Count; x++)
            {
                if (DialogueItem.DialogueList[x].StatusList.Count != 0) //如果这段对话有条件控制，把控制这段话的条件的Name装入TalkStatus字典
                {
                    for (int j = 0; j < DialogueItem.DialogueStatusList.ToArray().Length; j++)
                    {
                        TalkManager.Instance.TalkStatusJudge[DialogueItem.DialogueStatusList[j].ConditionName] = DialogueItem.DialogueStatusList[j].Judge;
                    }
                }
            }
        }
    }

}
