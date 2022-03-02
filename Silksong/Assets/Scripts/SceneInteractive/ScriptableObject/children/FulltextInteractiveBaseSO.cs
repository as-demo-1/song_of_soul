using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "fulltext", menuName = "Interactive/fulltext")]
public class FulltextInteractiveBaseSO : InteractiveBaseSO
{
    [HideInInspector]
    [SerializeField] private EInteractiveItemType _itemType = EInteractiveItemType.FULLTEXT;
    public override EInteractiveItemType ItemType => _itemType;

    [Tooltip("the tip displayed in the fulltext")]
    [SerializeField] private string _tip = default;
    public string Tip => _tip;

    protected override void DoInteract()
    {
        UIComponentManager.Instance.SetText("FullWindowText/Image/Text", Tip);
        UIComponentManager.Instance.ShowUI("FullWindowText");
        UIComponentManager.Instance.UIAddListener("FullWindowText/Image/Close", base.DoInteract);
    }

    protected override void Finish()
    {
        UIComponentManager.Instance.HideUI("FullWindowText");
        base.Finish();
    }
}
