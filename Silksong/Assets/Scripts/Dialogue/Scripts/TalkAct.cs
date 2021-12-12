using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System;
//using DG.Tweening;

public class TalkAct : MonoBehaviour
{
    public GameObject Talk;
    public Text NPCText;
    public Text NPCName;


    public List<int> TalkFlags = new List<int>();//-1�������ģ�����0�����ִ������Ի���������һ�ξ���Ի��ͷ���-1,1�������εĻ��ټ�һ��2�������ξ���Ի�����[-1,1,2,3]��˳�򴥷����Ի������Ѷ�Ӧ��flag����-1����ֱ��ɾ��
    public List<int> StartTalkNo = new List<int>();//�����������ɫ��ÿ�ζԻ��ĵ�һ�仰�ı�ŷŽ����List������Ի���TalkFlags��Ԫ�ض�Ӧ�����ĶԻ����ڼ���Ԫ��֮�����ѡ��

    public IfTalkCondition[] ifTalkConditions;
    public Dictionary<int,int[]> ConditionNos= new Dictionary<int, int[]>(); //ǰ����TalkFlags�ĶԻ���ţ����ڶ�λConditionID,������ConditionNo����λ��Condition��


    public List<int> Condition = new List<int>();//�ж������Ƿ�������б�


    [HideInInspector] 
    public static int ID = 0;//���idΪ0��˵�����ǶԻ��ĵ�һ�䣬�����ж��ǽ��е��Ǿ���Ի���������
    [HideInInspector]
    public int T = 0;//���������Ƿ���Update
    [HideInInspector]
    public int count = 0;

    void OnMouseDown() //�޶�ֻ�е������ʱ�Ż���Update
    {
        T = 1;
    }

    void Awake()
    {
        foreach (IfTalkCondition item in ifTalkConditions)
        {
            ConditionNos.Add(item.ConditionID,item.ConditionNo);
        }

    }

    public void StartTalk()
    {
        Talk.SetActive(true);
        for (int num = 0; num < TalkFlags.Count; num++) //���֮���ж��������ɲ��ܴ����ĶԻ�������������ж���������һ��if�ж�
        {
            if (!ConditionNos.ContainsKey(TalkFlags[num]))
            {
                if (TalkFlags[num] != -1) // ���TalkFlags�в�Ϊ-1��ֵ��˵�����о���Ի�û����  
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
                    if (Condition[j] != 1) //ֻҪ��һ��������Ϊ1��˵����һ������Ի������ܴ���
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

    public void NextTalk()
    {
        //ID = StartTalkNo[0];
        if (TalkManager.Instance.TalkNo[ID] != -1)//���NextID��Ϊ-1
        {
            ID = TalkManager.Instance.TalkNo[ID];//ID����NextID
            NPCText.text = TalkManager.Instance.TalkContent[ID];//�����ı�
            NPCName.text = TalkManager.Instance.TalkNPC[ID]; //����˵����
        }
        else
        {
            Talk.SetActive(false);
            T = 0;
            ID = 0;
            DialogInteract.Instance.ContinueEvent();
        }
    }

    public void TalkAction()
    {

        if (ID == 0)
        {
            StartTalk();
        }
        else
        {
            NextTalk();
        }
    }
}

//�ж�������ConditionName�����������硰��������Ƿ����100������ĳ��BOSS�Ƿ񱻴�ܡ�֮���������Condition[ConditionNo]�ж��Ƿ��Ѿ���ɣ�0��ʾδ��ɣ�1��ʾ�Ѵ��
//ConditionNameֻ��Ϊ�˷������������ʵ���ж�����Ҫ����IfTalks���±�����λCondition��Condition[ConditionNo]���������жϣ���˼��ConditionName�Ǹ��˿��ģ����Ǹ����������
//ConditionID�洢������Ҫ����ConditionName���ܴ����ĶԻ�����ţ�Ҫ�͵�ǰNPC��TalkFlags�о���Ի��ı�ű���һ�£�������һ����Ҫ�����������ܴ����ľ���Ի���TalkFlags�еı��Ϊ2����ôConditionID��Ϊ2
//���磬ConditionName�ǡ������������100������Ҫ�������������100�����ܽ����ĶԻ���TalkFlags�еı��Ϊ3����ConditionIDӦΪ3�������Ļ���������TalkFlags==3ʱ����ͬʱ�ж�Condition[ConditionNo]�Ƿ�Ϊ1��Ϊ1�Ļ��ʹ����öԻ��������0������������
//ConditionName����д���������ֻҪConditionNo�������д���б��ж�Ӧ���±����


//���磬Condition����б���װ��Ԫ����[0,0,0,0,0]
//һ�ξ���Ի���TalkFlags�еı��Ϊ3����ôConditionIDΪ 3��Ҫ��������Ի���������ConditionName���ǡ������������100���Ѿ���ɱ��һ�������BOSS������ôConditionNo������[1,2]������Condition����б����±�Ϊ1��2��Ԫ�طֱ��ǡ�
//�����������100�ͻ�ɱ��һ�������BOSS����ȻConditionNo��������Ҳ�У��벻�����ĸ�Ԫ�ش���ʲô��ʱ��ֻҪ��һ��ConditionName���ɣ�

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