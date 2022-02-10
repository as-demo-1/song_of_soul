using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NormalInteract : InteractTriggerBase<NormalInteract>
{
    private Dictionary<EFuncInteractItemType, Action> _actions { get; }
        = new Dictionary<EFuncInteractItemType, Action>()
    {
        { EFuncInteractItemType.SAVE, () => {
            // todo: save
            InteractManager.Instance.NPCController.SaveSystem.SaveDataToDisk();
        }},
    };

    protected override void InteractEvent()
    {
        if (_actions.ContainsKey(FuncInteractItemType))
            _actions[FuncInteractItemType]();
        else
            Debug.LogError(FuncInteractItemType + " not add actions dictionary");

        ContinueEvent();
    }

    private EFuncInteractItemType FuncInteractItemType =>
        (InteractManager.Instance.InteractiveItem as FuncInteractiveSO)
            .FuncInteractItemType;

}

public enum EFuncInteractItemType
{
    SAVE    // 存档点
}

