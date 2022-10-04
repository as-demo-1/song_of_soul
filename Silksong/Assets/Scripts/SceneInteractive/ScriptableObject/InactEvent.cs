using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "event", menuName = "NewInact/event")]
public class InactEvent : InactChildFuncSO
{
    [SerializeField] private EventType _eventType = default;
    public EventType EventType => _eventType;
    // todo: 扩展事件传递的类型，根据具体需求修改

    public override void DoInteract()
    {
        _status = EInteractStatus.DO_INTERACT;
        EventDispatch();
        Next();
    }

    protected virtual void EventDispatch()
    {
        EventManager.Instance.Dispatch(EventType);
    }
}
