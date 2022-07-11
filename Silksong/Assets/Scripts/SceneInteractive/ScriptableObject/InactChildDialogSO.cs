using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "dialog", menuName = "NewInact/dialog")]
public class InactChildDialogSO : InactChildBaseSO
{
    public InactChildBaseSO NextChild => _nextChild;
    [SerializeField]
    public int InteractiveID;

    public override void DoInteract()
    {
        _status = EInteractStatus.DO_INTERACT;
        InitDialog(InteractiveID);
        showDialog();
    }

    public void InitDialog(int id)
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

    private void showDialog()
    {
        // todo:
        // 2.调用对话系统的方法
        Debug.Log("调用对话系统方法");

        TalkController.Instance.StartTalk(InteractiveID, Next);

        UIComponentManager.Instance.UIAddListener(InteractConstant.UITalkNext, () => {
            TalkController.Instance.StartTalk(InteractiveID, Next);
        });
    }

}
