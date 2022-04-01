using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;


public class InteractLoad : MonoBehaviour
{
    [SerializeField]
    public InteractiveContainerSO InteractiveContainer;
    public GameObject ItemPrefab;
    public SaveSystem SaveSystem;

    public static InteractiveContainerSO InterContainer;

    //public static InterContainer.InteractiveItemList InteractiveSOList = new List<InteractiveBaseSO>();
    //public static List<InteractiveBaseSO> InteractiveSOList = InterContainer.InteractiveItemList;

    //private static InteractLoad _instance;
    public static InteractLoad Instance;
    

    // Use this for initialization
    void Start()
    {
        foreach (InteractiveBaseSO interactiveItem in InteractiveContainer.InteractiveItemList)
        {
            int id = interactiveItem.InteractiveID;
            //Debug.Log("Interative");
            interactiveItem.Init(this);

            foreach (DialogueSectionSO DialogueItem in TalkSOManager.Instance.DialogueSectionListInstance)
            {
                //Debug.Log(id);
                int i = 0;//每添加一次闲聊对话这个i+1
                if (DialogueItem.NPCID == id)
                {
                    TalkManager.Instance.NPCAllContent[DialogueItem.NPCID] = new Dictionary<int, List<string>>();
                    TalkManager.Instance.Name[DialogueItem.NPCID] = DialogueItem.NPCName;
                    TalkManager.Instance.NPCAllCondition[DialogueItem.NPCID] = new Dictionary<int, string>();
                    TalkManager.Instance.NPCAllGossip[DialogueItem.NPCID] = new Dictionary<int, List<string>>();
                    TalkManager.Instance.gossipRand[DialogueItem.NPCID] = new List<int>();

                    for (int num = 0; num < DialogueItem.DialogueList.ToArray().Length; num++) //存储对话内容
                    {
                        List<string> TalkContent = new List<string>();
                        for (int j = 0; j < DialogueItem.DialogueList[num].Content.ToArray().Length; j++)
                        {
                            TalkContent.Add(DialogueItem.DialogueList[num].Content[j]);
                        }
                        if (DialogueItem.DialogueList[num].Type.Equals("plot"))
                        {
                            TalkManager.Instance.NPCAllContent[DialogueItem.NPCID][num - i] = TalkContent;
                        }
                        else
                        {
                            TalkManager.Instance.NPCAllGossip[DialogueItem.NPCID][i] = TalkContent;
                            TalkManager.Instance.gossipRand[DialogueItem.NPCID].Add(i);
                            i += 1;
                        }
                    }

                    //储存条件列表，同时把条件的Name和是否达成装入TalkManager的TalkStatusJudge，这里装，改变条件在别的地方改变
                    for (int num = 0; num < DialogueItem.DialogueList.ToArray().Length; num++)
                    {
                        if (DialogueItem.DialogueList[num].StatusList.ToArray().Length != 0) //如果这段对话有条件控制，把控制这段话的条件的Name装入TalkStatus字典
                        {
                            for (int j = 0; j < DialogueItem.DialogueStatusList.ToArray().Length; j++)
                            {
                                TalkManager.Instance.NPCAllCondition[DialogueItem.NPCID][num] = DialogueItem.DialogueList[num].StatusList[j];
                                //如果字典里还没有这个条件则写入，默认未达成
                                if (!TalkManager.Instance.TalkStatusJudge.ContainsKey(DialogueItem.DialogueStatusList[j].ConditionName))
                                {
                                    TalkManager.Instance.TalkStatusJudge[DialogueItem.DialogueStatusList[j].ConditionName] = DialogueItem.DialogueStatusList[j].Judge;
                                }
                            }
                        }
                    }

                    //把条件的Name和是否达成装入TalkManager的TalkStatusJudge，这里装，改变条件在别的地方改变
                }
            }
        }
    }
}
