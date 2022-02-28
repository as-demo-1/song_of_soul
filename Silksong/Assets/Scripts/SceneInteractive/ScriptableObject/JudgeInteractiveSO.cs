using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeInteractiveSO : InteractiveSO
{
    [Tooltip("The type of the item")]
    [SerializeField] private EInteractiveItemType _itemType = EInteractiveItemType.JUDGE;
    public override EInteractiveItemType ItemType => _itemType;
}
