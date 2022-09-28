using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "fulltext", menuName = "NewInact/fulltext")]
public class InactChildFulltextSO : InactChildBaseSO
{
    [SerializeField] protected string _tip;
    public string Tip => _tip;
    public InactChildBaseSO NextChild => _nextChild;

    public override void DoInteract()
    {
        //base.DoInteract();
        _status = EInteractStatus.DO_INTERACT;

        UIComponentManager.Instance.SetText(InteractConstant.UIFullTextText, Tip);
        UIComponentManager.Instance.ShowUI(InteractConstant.UIFullText);
        UIComponentManager.Instance.UIAddListener(InteractConstant.UIFullTextClose, () => {
            Hide();
            Next();
        });
    }
    protected void Hide()
    {
        UIComponentManager.Instance.HideUI(InteractConstant.UIFullText);
    }
}
