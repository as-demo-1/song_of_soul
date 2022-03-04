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
    private int _id = 0; 
    private static int num = 1; //作为对话到第几段对话的指示器
    private static int i = 0;

    //public DialogueSectionSO DialogueSection;

    


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

    public void StartTalk(int NPCID, UnityAction callback)
    {
        TalkPanel.SetActive(true);
        //Debug.Log(NPCID);
        if (num >= TalkManager.Instance.NPCAllContent[NPCID].Count) //已经过完剧情对话了，播放闲聊对话
        {
            if (i < TalkManager.Instance.NPCAllContent[NPCID][0].Count)
            {

                NPCText.text = TalkManager.Instance.NPCAllContent[NPCID][0][i];
                NPCName.text = TalkManager.Instance.Name[NPCID];
                i += 1;
            }
            else
            {
                i = 0;
                TalkPanel.SetActive(false);
                callback();
            }
            _id = 0;
        }
        else
        {
            if (!TalkManager.Instance.NPCAllCondition[NPCID].ContainsKey(num)) //如果TalkCondition里不包含这段对话的键，说明这段对话可以无条件触发
            {
                if (i < TalkManager.Instance.NPCAllContent[NPCID][num].Count)
                {
                    Debug.Log(i);
                    NPCText.text = TalkManager.Instance.NPCAllContent[NPCID][num][i];
                    NPCName.text = TalkManager.Instance.Name[NPCID];
                    i += 1;
                    //Debug.Log(i);
                }
                else
                {
                    i = 0;
                    num += 1;
                    TalkPanel.SetActive(false);
                    callback();
                }
            }
            else
            {
                //检验条件
                ConditionStatus();
                foreach (int key in TalkManager.Instance.NPCAllCondition[NPCID].Keys)
                {
                    if (!TalkManager.Instance.TalkStatusJudge[TalkManager.Instance.NPCAllCondition[NPCID][key]]) //如果有一个条件未满足
                    {
                        Debug.Log(TalkManager.Instance.TalkStatusJudge[TalkManager.Instance.NPCAllCondition[NPCID][key]]);
                        _id = 0;
                        break;
                    }
                    _id = 1;
                }
                if (_id == 0)
                {
                    if (i < TalkManager.Instance.NPCAllContent[NPCID][0].Count)
                    {
                        Debug.Log("进行闲聊对话");
                        NPCText.text = TalkManager.Instance.NPCAllContent[NPCID][0][i];
                        NPCName.text = TalkManager.Instance.Name[NPCID];
                        i += 1;
                    }
                    else
                    {
                        i = 0;
                        TalkPanel.SetActive(false);
                        callback();
                    }
                }
                if (_id == 1)
                {
                    if (i < TalkManager.Instance.NPCAllContent[NPCID][num].Count)
                    {

                        NPCText.text = TalkManager.Instance.NPCAllContent[NPCID][num][i];
                        NPCName.text = TalkManager.Instance.Name[NPCID];
                        i += 1;
                    }
                    else
                    {
                        num += 1;
                        i = 0;
                        TalkPanel.SetActive(false);
                        callback();
                    }
                }
                _id = 0;
            }
        }
    }


    public void ConditionStatus()
    {
        foreach (DialogueSectionSO DialogueItem in TalkManager.Instance.DialogueContainer.DialogueSectionList)
        {
            for (int x = 0; x < DialogueItem.DialogueList.ToArray().Length; x++)
            {
                if (DialogueItem.DialogueList[x].StatusList.ToArray().Length != 0) //如果这段对话有条件控制，把控制这段话的条件的Name装入TalkStatus字典
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
