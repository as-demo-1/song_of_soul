using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NormalInteract : InteractTriggerBase<NormalInteract>
{
    private Dictionary<EFuncInteractItemType, Action> _actions
        = new Dictionary<EFuncInteractItemType, Action>()
    {
        { EFuncInteractItemType.SAVE, () => {
            Debug.Log("已存档");
        }},
    };

    protected override void InteractEvent()
    {
        Debug.Log("无界面");
        _actions[InteractManager.Instance.FuncInteractItemType]();
        ContinueEvent();
    }
}

public enum EFuncInteractItemType
{
    SAVE    // 存档点
}

