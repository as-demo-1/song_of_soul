using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCondition
{
    private static ChangeCondition _instance;
    public static ChangeCondition Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ChangeCondition();
            }

            return _instance;
        }
    }

    // todo: 判断当前前状态进行改变
   /* public void JudgeCondition(int ConditionID)
    {
        DialogueStatusSO status = TalkManager.Instance.DialogueStatus[ConditionID];

        if (status.IfDialogueList[0].ConditionID == 1)
        {
            if (TalkConditionState.money > 100)
            {
                Debug.Log("金钱达到100");
                status.Condition[0] = 1;
            }
        }
        else if (status.IfDialogueList[0].ConditionID == 2)
        {
            if (!TalkConditionState.isLock)
            {
                Debug.Log("已解锁");
                status.Condition[0] = 1;
            }
        }
    }*/
}
