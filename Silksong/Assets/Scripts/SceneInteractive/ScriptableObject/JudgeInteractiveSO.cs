using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "judge", menuName = "Interactive/judge")]
public class JudgeInteractiveSO : InteractiveSO
{
    [Tooltip("The type of the item")]
    [SerializeField] private EInteractiveItemType _itemType = EInteractiveItemType.JUDGE;
    public override EInteractiveItemType ItemType => _itemType;

    [Tooltip("the tip displayed in the judgment box")]
    [SerializeField] private string _tip = default;
    public string Tip => _tip;

    public virtual void Yes()
    {
        Debug.Log("yes");
    }

    public virtual void No()
    {
        Debug.Log("no");
    }
}
