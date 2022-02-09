using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractLoad : MonoBehaviour
{
    public InteractiveContainerSO InteractiveContainer;
    public GameObject ItemPrefab;

    // Use this for initialization
    void Start()
    {
        foreach (InteractiveSO interactiveItem in InteractiveContainer.InteractiveItemList)
        {
            GameObject go = Instantiate(ItemPrefab, transform);
            go.transform.position = interactiveItem.Coord;

            SpriteRenderer npcSprite = go.GetComponent<SpriteRenderer>();
            NPCController npcController = go.AddComponent<NPCController>();
            //TalkController npcTalkController = go.AddComponent<TalkController>();

            npcSprite.sprite = interactiveItem.Icon;
            npcSprite.flipX = !interactiveItem.IsFaceRight;
            npcController.InteractiveItem = interactiveItem;

            // 当可交互类型为npc的时候
            if (interactiveItem.ItemType == EInteractiveItemType.DIALOG)
            {
                initDialog(interactiveItem.ID);
            }
            //Debug.Log(npcTalkController.DialogueSection);
            //TalkManager.Instance.NPCAllCondition[interactiveItem.ID] = TalkManager.Instance.TalkStatus;
        }
    }

    private void initDialog(int id)
    {
        foreach (DialogueSectionSO DialogueItem in TalkManager.Instance.DialogueContainer.DialogueSectionList)
        {
            if (DialogueItem.NPCID == id)
            {
                TalkManager.Instance.NPCAllContent[DialogueItem.NPCID] = new Dictionary<int, List<string>>();
                TalkManager.Instance.Name[DialogueItem.NPCID] = DialogueItem.NPCName;
                TalkManager.Instance.NPCAllCondition[DialogueItem.NPCID] = new Dictionary<int, string>();

                for (int num = 0; num < DialogueItem.DialogueList.ToArray().Length; num++) //存储对话内容
                {
                    List<string> TalkContent = new List<string>();
                    for (int j = 0; j < DialogueItem.DialogueList[num].Content.ToArray().Length; j++)
                    {
                        TalkContent.Add(DialogueItem.DialogueList[num].Content[j]);
                    }
                    TalkManager.Instance.NPCAllContent[DialogueItem.NPCID][num] = TalkContent;
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
