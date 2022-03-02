using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "fulltext", menuName = "Interactive/fulltext")]
public class FulltextInteractiveBaseSO : SingleInteractiveBaseSO
{
    [HideInInspector]
    [SerializeField] private EInteractiveItemType _itemType = EInteractiveItemType.FULLTEXT;
    public override EInteractiveItemType ItemType => _itemType;

    [Tooltip("the tip displayed in the fulltext")]
    [SerializeField] private string _tip = default;
    public string Tip => _tip;

    protected override void DoInteract()
    {
        UIComponentManager.Instance.SetText(InteractConstant.UIFullTextText, Tip);
        UIComponentManager.Instance.ShowUI(InteractConstant.UIFullText);
        UIComponentManager.Instance.UIAddListener(InteractConstant.UIFullTextClose, base.DoInteract);
    }

    protected override void Finish()
    {
        UIComponentManager.Instance.HideUI(InteractConstant.UIFullText);
        base.Finish();
    }
}
