using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FuncInteractive", menuName = "Interactive/FuncInteractive")]
public class FuncInteractiveSO : InteractiveSO
{
    [Tooltip("The types of functional interactive items")]
    [SerializeField] private EFuncInteractItemType _funcInteractItemType = default;

    public EFuncInteractItemType FuncInteractItemType => _funcInteractItemType;
}
