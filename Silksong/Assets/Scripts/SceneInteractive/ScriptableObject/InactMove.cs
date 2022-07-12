using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "move", menuName = "NewInact/move")]
public class InactMove : InactChildFuncSO
{
    [SerializeField]
    private float _move_step;
    public float MoveStep => _move_step;
    public override void DoInteract()
    {
        _status = EInteractStatus.DO_INTERACT;
        Debug.Log(_status);

        InteractPlayerTest.Instance.PlayerKeyActions.Enqueue(new MoveEventAction(
            _inactItemSO.Go.transform.position,
            InteractSystem.Instance.GetPlayerTarget(_inactItemSO.Iid).Go.transform.position,
            MoveStep, Next));
    }
}
