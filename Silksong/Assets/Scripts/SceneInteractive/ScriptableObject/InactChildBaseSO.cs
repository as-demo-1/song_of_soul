
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class InactChildBaseSO : ScriptableObject
{
    [SerializeField]
    [HideInInspector]
    protected InactItemSO _inactItemSO;
    [SerializeField]
    protected InactChildBaseSO _nextChild;

    [SerializeField]
    [HideInInspector]
    protected EInteractStatus _status;
    protected Dictionary<EInteractStatus, UnityAction> _eia = new Dictionary<EInteractStatus, UnityAction>();

    protected virtual void OnEnable()
    {
        _status = EInteractStatus.INIT;
        _eia[EInteractStatus.INIT] = DoInteract;
        _eia[EInteractStatus.DO_INTERACT] = Finish;
        if (_nextChild != null)
        {
            _eia[EInteractStatus.FINISH] = () =>
            {
                _status = EInteractStatus.INIT;
                _nextChild.Init(_inactItemSO);
                _nextChild.Next();
            };
        }
        else
        {
            _eia[EInteractStatus.FINISH] = Stop;
        }
    }

    public virtual void Init(InactItemSO itemSO)
    {
        itemSO.CurInactChild = this;
        _inactItemSO = itemSO;
    }

    public virtual void DoInteract()
    {
        _status = EInteractStatus.DO_INTERACT;
        Debug.Log(_status);
        Next();
    }
    public virtual void Finish()
    {
        _status = EInteractStatus.FINISH;
        Debug.Log(_status);
        Next();
    }

    public virtual void Next()
    {
        _eia[_status]();
    }

    public virtual void Stop()
    {
        _inactItemSO.Stop();
        _status = EInteractStatus.INIT;
    }
}
