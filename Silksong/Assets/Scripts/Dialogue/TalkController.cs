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
    private static int _id = 0; //判断是否满足条件的指示器
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
                //PlayAudio
                if (AudioPlay.filename.Contains(i.ToString()))
                {
                    AudioPlay.Instance.PlayAudio(i.ToString());
                }
                i += 1;
                //Debug.Log(_end + "-" + i);
            }
            else
            {
                if (TalkManager.Instance.GossipContent.ContainsKey(i))
                {
                    NPCText.text = TalkManager.Instance.GossipContent[i];
                    NPCName.text = TalkManager.Instance.Name[NPCID];
                    //PlayAudio
                    if (AudioPlay.filename.Contains(i.ToString()))
                    {
                        AudioPlay.Instance.PlayAudio(i.ToString());
                    }
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
            //Debug.Log("Plot");
            if (_end == 0) //第一句对话
            {
                TalkID = TalkManager.Instance.NPCPlotContent[NPCID][TalkCount];
                int rnum = Random.Range(0, TalkManager.Instance.NPCGossipContent[NPCID].Count);
                i = TalkManager.Instance.NPCGossipContent[NPCID][rnum];
                startID = TalkID;
                if (!TalkManager.Instance.Condition.ContainsKey(TalkID)) //如果条件列表不包含这个StartID，说明可以无条件触发
                {
                    //Debug.Log(TalkID);
                    Debug.Log("Plot,No Condition");
                    NPCText.text = TalkManager.Instance.PlotContent[TalkID];
                    NPCName.text = TalkManager.Instance.Name[NPCID];
                    //PlayAudio
                    if (AudioPlay.filename.Contains(TalkID.ToString()))
                    {
                        AudioPlay.Instance.PlayAudio(TalkID.ToString());
                    }
                    TalkID += 1;
                }
                else
                {
                    Debug.Log("Plot,Condition");
                    //Debug.Log(TalkID);
                    ConditionStatus(NPCID);
                    //Debug.Log(TalkManager.Instance.Condition[TalkID].Count);
                    foreach (string conditionname in TalkManager.Instance.Condition[TalkID])
                    {
                        if (!TalkManager.Instance.TalkStatusJudge[conditionname]) //如果有一个条件未满足
                        {
                            Debug.Log(conditionname);
                            _id = 0;
                            Debug.Log("False");
                            break;
                        }
                        Debug.Log("True");
                        _id = 1;
                    }
                    if (_id == 0)
                    {
                        //startID = i;
                        NPCText.text = TalkManager.Instance.GossipContent[i];
                        NPCName.text = TalkManager.Instance.Name[NPCID];
                        //PlayAudio
                        if (AudioPlay.filename.Contains(i.ToString()))
                        {
                            AudioPlay.Instance.PlayAudio(i.ToString());
                        }
                        i += 1;
                    }
                    else
                    {
                        //startID = TalkID;
                        NPCText.text = TalkManager.Instance.PlotContent[TalkID];
                        NPCName.text = TalkManager.Instance.Name[NPCID];
                        //PlayAudio
                        if (AudioPlay.filename.Contains(TalkID.ToString()))
                        {
                            AudioPlay.Instance.PlayAudio(TalkID.ToString());
                        }
                        TalkID += 1;
                    }
                }

                _end = 1;
            }
            else //后续对话
            {
                if (!TalkManager.Instance.Condition.ContainsKey(startID)) //如果TalkCondition里不包含这段对话的StartID，说明这段对话可以无条件触发
                {
                    Debug.Log("Plot,no condition");
                    if (TalkManager.Instance.PlotContent.ContainsKey(TalkID))
                    {
                        NPCText.text = TalkManager.Instance.PlotContent[TalkID];
                        NPCName.text = TalkManager.Instance.Name[NPCID];
                        //PlayAudio
                        if (AudioPlay.filename.Contains(TalkID.ToString()))
                        {
                            AudioPlay.Instance.PlayAudio(TalkID.ToString());
                        }
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
                else
                {
                    ConditionStatus(NPCID);
                    foreach (string conditionname in TalkManager.Instance.Condition[startID])
                    {
                        if (!TalkManager.Instance.TalkStatusJudge[conditionname]) //如果有一个条件未满足
                        {
                            Debug.Log(conditionname);
                            _id = 0;
                            Debug.Log("False");
                            break;
                        }
                        Debug.Log("True");
                        _id = 1;
                    }
                    Debug.Log("Plot,condition");
                    Debug.Log(_id);
                    if (_id == 0)
                    {
                        //条件不满足，进入闲聊
                        if (TalkManager.Instance.GossipContent.ContainsKey(i))
                        {
                            NPCText.text = TalkManager.Instance.GossipContent[i];
                            NPCName.text = TalkManager.Instance.Name[NPCID];
                            //PlayAudio
                            if (AudioPlay.filename.Contains(i.ToString()))
                            {
                                AudioPlay.Instance.PlayAudio(i.ToString());
                            }
                            i += 1;
                        }
                        else
                        {
                            i = 0;
                            _end = 0;
                            TalkPanel.SetActive(false);
                            //Debug.Log(startID);
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
                            //PlayAudio
                            if (AudioPlay.filename.Contains(TalkID.ToString()))
                            {
                                AudioPlay.Instance.PlayAudio(TalkID.ToString());
                            }
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


    public void ConditionStatus(int npcid)
    {
        foreach (DialogueSectionSO DialogueItem in TalkSOManager.Instance.DialogueSectionListInstance)
        {
            if (DialogueItem.NPCID == npcid)
            {
                foreach (DialogueStatusSO statusso in DialogueItem.DialogueStatusList)
                {
                    TalkManager.Instance.TalkStatusJudge[statusso.ConditionName] = statusso.Judge;
                }
            }
        }



    }

}
