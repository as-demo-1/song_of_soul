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

    public static InteractLoad Instance;
    

    // Use this for initialization
    void Start()
    {
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
                        if (item.StatusList.Count != 0)
                        {
                            TalkManager.Instance.Condition[item.StartID] = new List<string>();
                        }
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
                            //Debug.Log(item.StartID);
                            for (int i = 0; i < item.Content.Count; i++)
                            {
                                TalkManager.Instance.GossipContent.Add(item.StartID + i, item.Content[i]);
                            }
                            TalkManager.Instance.NPCGossipContent[section.NPCID].Sort((x, y) => x.CompareTo(y));
                        }
                    }
                    foreach (DialogueStatusSO statusItem in section.DialogueStatusList)
                    {
                        if (!TalkManager.Instance.TalkStatusJudge.ContainsKey(statusItem.ConditionName))
                        {
                            TalkManager.Instance.TalkStatusJudge.Add(statusItem.ConditionName, statusItem.Judge); 
                        }
                    }
                }
            }
            
                
            
        }
        foreach (KeyValuePair<int, List<string>> pair in ExcelLoad.ConditionList)
        {
            if (!TalkManager.Instance.Condition.ContainsKey(pair.Key))
            {
                TalkManager.Instance.Condition.Add(pair.Key, pair.Value) ;
                Debug.Log(pair.Key);
                Debug.Log(pair.Value);
            }
            else
            {
                foreach (string conditionname in pair.Value)
                {
                    if (!TalkManager.Instance.Condition[pair.Key].Contains(conditionname))
                    {
                        TalkManager.Instance.Condition[pair.Key].Add(conditionname);
                    }
                }
            }
        }
    }
}
