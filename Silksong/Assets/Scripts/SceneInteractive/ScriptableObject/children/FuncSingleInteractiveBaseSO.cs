using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FuncSingleInteractiveBaseSO: SingleInteractiveBaseSO
{
    public virtual EFuncInteractItemType FuncInteractItemType { get; }

    [HideInInspector]
    [SerializeField] private EInteractiveItemType _itemType = EInteractiveItemType.NONE;
    public override EInteractiveItemType ItemType => _itemType;
}
