using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuncInteractiveBaseSO : InteractiveSO
{
    public virtual EFuncInteractItemType FuncInteractItemType { get; }

    [Tooltip("The type of the item")]
    [SerializeField] private EInteractiveItemType _itemType = EInteractiveItemType.NONE;
    public override EInteractiveItemType ItemType => _itemType;

    public virtual void DoAction() { }
}
