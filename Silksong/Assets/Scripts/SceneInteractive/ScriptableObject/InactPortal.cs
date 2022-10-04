using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "portal", menuName = "NewInact/portal")]
public class InactPortal : InactChildFuncSO
{
    public override void DoInteract()
    {
        _status = EInteractStatus.DO_INTERACT;
        Debug.Log(_status);
        InteractSystem.Instance.GetOtherIid42(_inactItemSO.Iid).GoThere();
        Next();
    }
}
