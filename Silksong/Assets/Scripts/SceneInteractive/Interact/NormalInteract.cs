using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NormalInteract : InteractTriggerBase<NormalInteract>
{
    protected override void InteractEvent()
    {
        (InteractManager.Instance.InteractiveItem as FuncInteractiveBaseSO)
            .DoAction();

        ContinueEvent();
    }
}

public enum EFuncInteractItemType
{
    SAVE,    // 存档点
    PORTAL,  // 传送门
}

