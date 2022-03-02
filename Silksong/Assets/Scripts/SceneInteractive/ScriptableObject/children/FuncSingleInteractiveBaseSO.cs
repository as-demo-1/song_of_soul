using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FuncInteractiveBaseSO : InteractiveBaseSO
{
    public virtual EFuncInteractItemType FuncInteractItemType { get; }

    [HideInInspector]
    [SerializeField] private EInteractiveItemType _itemType = EInteractiveItemType.NONE;
    public override EInteractiveItemType ItemType => _itemType;

    protected override void DoInteract()
    {
        base.DoInteract();
    }
}
