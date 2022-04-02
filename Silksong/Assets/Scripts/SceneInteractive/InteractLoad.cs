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
        /*TalkManager.Instance.GossipContent = new Dictionary<int, string>();
        TalkManager.Instance.PlotContent = new Dictionary<int, string>();
        TalkManager.Instance.NPCContent = new Dictionary<int, List<int>>();*/
        foreach (InteractiveBaseSO interactiveItem in InteractiveContainer.InteractiveItemList)
        {
            int id = interactiveItem.InteractiveID;
            //Debug.Log("Interative");
            interactiveItem.Init(this);

            //新的储存对话代码
            foreach (DialogueSectionSO section in TalkSOManager.Instance.DialogueSectionListInstance)
            {
                if (section.NPCID == id)
                {
                    //Debug.Log(section.NPCID);
                    TalkManager.Instance.Name[section.NPCID] = section.NPCName;
                    TalkManager.Instance.NPCPlotContent[section.NPCID] = new List<int>();
                    TalkManager.Instance.NPCGossipContent[section.NPCID] = new List<int>();

                    foreach (DialogueSO item in section.DialogueList)
                    {
                        //Debug.Log(item.StartID);
                        TalkManager.Instance.Condition[item.StartID] = new List<string>();
                        if (item.Type.Equals("plot"))
                        {
                            TalkManager.Instance.NPCPlotContent[section.NPCID].Add(item.StartID);
                            for (int i = 0; i < item.Content.Count; i++)
                            {
                                TalkManager.Instance.PlotContent.Add(item.StartID + i, item.Content[i]);
                            }
                            TalkManager.Instance.NPCPlotContent[section.NPCID].Sort((x, y) => x.CompareTo(y));
                        }
                        else
                        {
                            TalkManager.Instance.NPCGossipContent[section.NPCID].Add(item.StartID);
                            for (int i = 0; i < item.Content.Count; i++)
                            {
                                TalkManager.Instance.GossipContent.Add(item.StartID + i, item.Content[i]);
                            }
                            TalkManager.Instance.NPCGossipContent[section.NPCID].Sort((x, y) => x.CompareTo(y));
                        }
                        if (item.StatusList != null)
                        {
                            foreach (string conditionname in item.StatusList)
                            {
                                TalkManager.Instance.Condition[item.StartID].Add(conditionname);
                            }
                        }
                    }
                    foreach (DialogueStatusSO statusItem in section.DialogueStatusList)
                    {
                        TalkManager.Instance.TalkStatusJudge.Add(statusItem.ConditionName, statusItem.Judge);
                    }
                }
            }
            
                
            
        }
    }
}
