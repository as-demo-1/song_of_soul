using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractTriggerBase<T> where T: class, new()
{
    public bool IsOnTrigger;
    public InteractiveItemType TriggerItemType;
    protected bool m_isOnInteract = false;

    private static T s_instance;
    public static T Instance
    {
        get
        {
            if (s_instance == null)
                s_instance = new T();

            return s_instance;
        }
    }

    public void Interact()
    {
        if (m_isOnInteract || !IsOnTrigger)
            return;

        m_isOnInteract = true;
        StopEvent();
        InteractEvent();
        m_isOnInteract = false;
        PlayerInput.Instance.IsFrozen = false;
    }
    protected abstract void InteractEvent();
    protected virtual void StopEvent() {
        //todo:停止键鼠监测事件 停止正在进行的动作 动画
        PlayerInput.Instance.IsFrozen = true;
    }
}
