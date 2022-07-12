using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "otherMove", menuName = "NewInact/otherMove")]
public class InactOtherMove : InactChildFuncSO
{
    [SerializeField]
    private float _move_step;
    public float MoveStep => _move_step;
    public override void DoInteract()
    {
        _status = EInteractStatus.DO_INTERACT;
        Debug.Log(_status);

        InteractController inact = _inactItemSO.Go.GetComponent<InteractController>();

        inact.MoveEventActions.Enqueue(new MoveEventAction(
            _inactItemSO.Go.transform.position,
            InteractSystem.Instance.GetThisTarget(_inactItemSO.Iid).Go.transform.position,
            MoveStep, Next));
    }
}
