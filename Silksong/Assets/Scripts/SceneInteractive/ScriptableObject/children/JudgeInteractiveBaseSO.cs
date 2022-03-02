using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "judge", menuName = "Interactive/judge")]
public class JudgeInteractiveBaseSO : InteractiveBaseSO
{
    [HideInInspector]
    [SerializeField] private EInteractiveItemType _itemType = EInteractiveItemType.JUDGE;
    public override EInteractiveItemType ItemType => _itemType;

    [Tooltip("the tip displayed in the judgment box")]
    [SerializeField] private string _tip = default;
    public string Tip => _tip;

    public virtual void Yes(UnityAction callback)
    {
        Debug.Log("yes");
        callback();
    }

    public virtual void No(UnityAction callback)
    {
        Debug.Log("no");
        callback();
    }

    protected override void DoInteract()
    {
        UIComponentManager.Instance.SetText("Judge/Image/Content", Tip);
        UIComponentManager.Instance.ShowUI("Judge");
        UIComponentManager.Instance.UIAddListener("Judge/Image/LButton", () => {
            Yes(base.DoInteract);
        });
        UIComponentManager.Instance.UIAddListener("Judge/Image/RButton", () => {
            No(base.DoInteract);
        });
    }

    protected override void Finish()
    {
        UIComponentManager.Instance.HideUI("Judge");
        base.Finish();
    }
}