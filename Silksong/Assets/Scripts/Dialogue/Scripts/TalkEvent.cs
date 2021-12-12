using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System;
//using DG.Tweening;

public class TalkEvent : MonoBehaviour
{
    public GameObject NPCTalkPanel;
    public Text NPCText;
    public Text NPCName;


    public List<int> TalkFlags = new List<int>();//-1代表闲聊，大于0的数字代表剧情对话，比如有一次剧情对话就放入-1,1，有两次的话再加一个2，有三次剧情对话就是[-1,1,2,3]按顺序触发，对话结束把对应的flag置于-1或者直接删除
    public List<int> StartTalkNo = new List<int>();//把属于这个角色的每次对话的第一句话的编号放进这个List，剧情对话和TalkFlags的元素对应，闲聊对话就在几个元素之间随机选择

    public IfTalkCondition[] ifTalkConditions;
    public Dictionary<int, int[]> ConditionNos = new Dictionary<int, int[]>(); //前面是TalkFlags的对话编号，用于定位ConditionID,后面是ConditionNo，定位到Condition中


    public List<int> Condition = new List<int>();//判断条件是否满足的列表


    [HideInInspector]
    public int ID = 0;//用来决定是第一句话还是已经是后面的对话了
    [HideInInspector]
    public int count = 0;



    void Start()
    {

        foreach (IfTalkCondition item in ifTalkConditions)
        {
            ConditionNos.Add(item.ConditionID, item.ConditionNo);
        }

    }

    void StartTalk()
    {
        for (int num = 0; num < TalkFlags.Count; num++) //判断是否满足条件
        {
            if (TalkFlags[num] != -1) //如果不是-1，进入对话，如果是-1，说明当前num对应的对话已经对话过了
            {
                for (int j = 0; j < ConditionNos[num].Length; j++) //判断控制这个对话的条件是否全部为1
                {
                    if (Condition[j] != 1) //如果有一个条件不为1，说明这一条剧情对话还不能触发
                    {
                        break;
                    }
                    else
                    {
                        count += 1;
                    }
                }
                if (count == ConditionNos[num].Length)
                {
                    NPCTalkPanel.SetActive(true);
                    TalkFlags[num] = -1;
                    ID = StartTalkNo[num];
                    NPCText.text = TalkManager.Instance.TalkContent[ID];
                    NPCName.text = TalkManager.Instance.TalkNPC[ID];
                }
                count = 0;
            }
        }
    }

    void Next()
    {
        //ID = StartTalkNo[0];
        if (Input.GetMouseButtonDown(0))
        {
            if (TalkManager.Instance.TalkNo[ID] != -1)//如果NextID不为0
            {
                ID = TalkManager.Instance.TalkNo[ID];//ID等于NextID
                NPCText.text = TalkManager.Instance.TalkContent[ID];//更新文本
                NPCName.text = TalkManager.Instance.TalkNPC[ID]; //更新说话人
            }
            else
            {
                NPCTalkPanel.SetActive(false);
                ID = 0;
            }
        } 
    }


    // Update is called once per frame
    void Update()
    {
        if (ID == 0)
        {
            StartTalk();
        }
        else
        {
            Next();
        }
    }
}