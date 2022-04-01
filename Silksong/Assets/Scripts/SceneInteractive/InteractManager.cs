using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractManager
{
    public GameObject CollidingObject { get; set; }

    public InteractiveBaseSO InteractiveItem
    {
        get
        {
            if (InteractObject)
            {
                return GetInteractiveItemComponent<InteractiveObjectTrigger>()
                    .InteractiveItem;
            }
            else
                return null;
        }
    }

    public Transform InteractObjectTransform
    {
        get
        {
            if (InteractObject)
            {
                return InteractObject.transform;
            }
            else
                return null;
        }
    }

    public T GetInteractiveItemComponent<T>()
    {
        if (InteractObject)
        {
            return InteractObject.GetComponent<T>();
        }
        return default;
    }

    public void Finish()
    {
        Status = EInteractStatus.FINISH_INTERACT;
        FinishInteract();
    }

    public void Interact()
    {
        if (Status == EInteractStatus.NO_INTERACT)
        {
            if (!!CollidingObject && !InteractObject)
            {
                Status = EInteractStatus.BEGIN_INTERACT;
                BeginInteract();
                InteractObject = CollidingObject;
                InteractiveItem.Interact();
            }
        }
    }

    public static InteractManager Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = new InteractManager();
            }

            return s_instance;
        }
    }

    private GameObject InteractObject { get; set; }

    private static InteractManager s_instance;

    private EInteractStatus Status = EInteractStatus.NO_INTERACT;

    private void BeginInteract()
    {
        Status = EInteractStatus.CANT_INTERACT;
        StopEvent();
        Status = EInteractStatus.INTERACTING;
    }

    private void FinishInteract()
    {
        Status = EInteractStatus.CANT_INTERACT;
        ContinueEvent();
        Status = EInteractStatus.NO_INTERACT;
    }

    private void StopEvent()
    {
        //todo:停止键鼠监测事件 停止正在进行的动作 动画
        Debug.Log("StopEvent");
        PlayerInput.Instance.ToggleFrozen(true);
    }

    private void ContinueEvent()
    {
        //todo:继续键鼠监测事件 继续原来的事件
        Debug.Log("ContinueEvent");
        InteractObject = null;
        PlayerInput.Instance.ToggleFrozen(false);
    }
}
