using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System;
//using DG.Tweening;

public class TalkAct : MonoBehaviour
{
    public GameObject NPCTalkPanel;
    public Text NPCText;
    public Text NPCName;


    public List<int> TalkFlags = new List<int>();//-1代表闲聊，大于0的数字代表剧情对话，比如有一次剧情对话就放入-1,1，有两次的话再加一个2，有三次剧情对话就是[-1,1,2,3]按顺序触发，对话结束把对应的flag置于-1或者直接删除
    public List<int> StartTalkNo = new List<int>();//把属于这个角色的每次对话的第一句话的编号放进这个List，剧情对话和TalkFlags的元素对应，闲聊对话就在几个元素之间随机选择

    public IfTalkCondition[] ifTalkConditions;
    public Dictionary<int,int[]> ConditionNos= new Dictionary<int, int[]>(); //前面是TalkFlags的对话编号，用于定位ConditionID,后面是ConditionNo，定位到Condition中


    public List<int> Condition = new List<int>();//判断条件是否满足的列表


    [HideInInspector] 
    public static int ID = 0;//如果id为0，说明这是对话的第一句，用来判定是进行的是剧情对话还是闲聊
    [HideInInspector]
    public int T = 0;//用来决定是否唤醒Update
    [HideInInspector]
    public int count = 0;



    void OnMouseDown() //限定只有点击人物时才唤醒Update
    {
        T = 1;
    }

    void Start()
    {

        foreach (IfTalkCondition item in ifTalkConditions)
        {
            ConditionNos.Add(item.ConditionID,item.ConditionNo);
        }

    }

    void StartTalk()
    {
        if (Input.GetMouseButtonDown(0))
        {
            NPCTalkPanel.SetActive(true);
            for (int num = 0; num < TalkFlags.Count; num++) //如果之后有多个条件达成才能触发的对话，在这里添加判定条件再起一个if判断
            {
                if (!ConditionNos.ContainsKey(TalkFlags[num]))
                {
                    if (TalkFlags[num] != -1) // 如果TalkFlags有不为-1的值，说明还有剧情对话没过完  
                    {
                        TalkFlags[num] = -1;
                        ID = StartTalkNo[num];
                        NPCText.text = TalkManager.Instance.TalkContent[ID];
                        NPCName.text = TalkManager.Instance.TalkNPC[ID];
                        break;
                    }
                    ID = StartTalkNo[0];
                    NPCText.text = TalkManager.Instance.TalkContent[ID];
                    NPCName.text = TalkManager.Instance.TalkNPC[ID];
                }
                else
                {
                    for (int j = 0; j < ConditionNos[num].Length; j++)
                    {
                        if (Condition[j] != 1) //只要有一个条件不为1，说明这一条剧情对话还不能触发
                        {
                            ID = StartTalkNo[0];
                            NPCText.text = TalkManager.Instance.TalkContent[ID];
                            NPCName.text = TalkManager.Instance.TalkNPC[ID];
                            break;
                        }
                        else
                        {
                            count += 1;
                        }
                    }
                    if (count == ConditionNos[num].Length)
                    {
                        TalkFlags[num] = -1;
                        ID = StartTalkNo[num];
                        NPCText.text = TalkManager.Instance.TalkContent[ID];
                        NPCName.text = TalkManager.Instance.TalkNPC[ID];
                    }
                    count = 0;
                }
            }

        }
    }

    void Next()
    {
        //ID = StartTalkNo[0];
        if (Input.GetMouseButtonDown(0))
        {
            if (TalkManager.Instance.TalkNo[ID] != -1)//如果NextID不为-1
            {
                ID = TalkManager.Instance.TalkNo[ID];//ID等于NextID
                NPCText.text = TalkManager.Instance.TalkContent[ID];//更新文本
                NPCName.text = TalkManager.Instance.TalkNPC[ID]; //更新说话人
            }
            else
            {
                NPCTalkPanel.SetActive(false);
                T = 0;
                ID = 0;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (T == 1)
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
}

//判断条件，ConditionName是条件，比如“金币数量是否大于100”、“某个BOSS是否被打败”之类的条件，Condition[ConditionNo]判断是否已经达成，0表示未达成，1表示已达成
//ConditionName只是为了方便添加条件，实际判定还是要依靠IfTalks的下标来定位Condition，Condition[ConditionNo]才能用于判断，意思是ConditionName是给人看的，不是给计算机看的
//ConditionID存储的是需要满足ConditionName才能触发的对话的序号，要和当前NPC的TalkFlags中剧情对话的编号保持一致，比如有一段需要满足条件才能触发的剧情对话在TalkFlags中的编号为2，那么ConditionID就为2
//比如，ConditionName是“金币数量大于100”，需要“金币数量大于100”才能解锁的对话在TalkFlags中的编号为3，则ConditionID应为3，这样的话当遍历到TalkFlags==3时，就同时判断Condition[ConditionNo]是否为1，为1的话就触发该对话，如果是0，就切入闲聊
//ConditionName可以写多个条件，只要ConditionNo这个数组写上列表中对应的下标就行


//比如，Condition这个列表里装的元素是[0,0,0,0,0]
//一段剧情对话在TalkFlags中的编号为3，那么ConditionID为 3；要触发这个对话的条件（ConditionName）是“金币数量大于100，已经击杀第一个区域的BOSS”，那么ConditionNo可以是[1,2]，代表Condition这个列表中下标为1和2的元素分别是↓
//金币数量大于100和击杀第一个区域的BOSS（当然ConditionNo是其他的也行，想不起来哪个元素代表什么的时候只要看一下ConditionName即可）

[Serializable]
public abstract class IfTalk {}

[Serializable]
public class IfTalkCondition : IfTalk
{

    public string ConditionName;

    public int ConditionID;

    public int[] ConditionNo;


    public IfTalkCondition(string ConditionName, int ConditionID,int[] ConditionNo)
    {
        this.ConditionName = ConditionName;
        this.ConditionID = ConditionID;
        this.ConditionNo = ConditionNo;

    }
}