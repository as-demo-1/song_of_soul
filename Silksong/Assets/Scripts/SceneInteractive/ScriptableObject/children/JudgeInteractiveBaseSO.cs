using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "judge", menuName = "Interactive/judge")]
public class JudgeInteractiveBaseSO : SingleInteractiveBaseSO
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
        UIComponentManager.Instance.SetText(InteractConstant.UIJudgeContent, Tip);
        UIComponentManager.Instance.ShowUI(InteractConstant.UIJudge);
        UIComponentManager.Instance.UIAddListener(InteractConstant.UIJudgeLB, () => {
            Yes(base.DoInteract);
        });
        UIComponentManager.Instance.UIAddListener(InteractConstant.UIJudgeRB, () => {
            No(base.DoInteract);
        });
    }

    protected override void Finish()
    {
        UIComponentManager.Instance.HideUI(InteractConstant.UIJudge);
        base.Finish();
    }
}