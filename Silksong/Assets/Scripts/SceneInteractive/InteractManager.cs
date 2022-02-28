using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractManager
{
    private Dictionary<EInteractiveItemType, Action> _interactiveActions { get; }
        = new Dictionary<EInteractiveItemType, Action>()
    {
        { EInteractiveItemType.NONE,        NormalInteract.Instance.Interact    },
        { EInteractiveItemType.DIALOG,      DialogInteract.Instance.Interact    },
        { EInteractiveItemType.JUDGE,       JudgeInteract.Instance.Interact     },
        { EInteractiveItemType.FULLTEXT,    FullTextInteract.Instance.Interact  },
    };

    // 当前正在交互的npc对象
    private GameObject _interactObject;

    private Dictionary<int, GameObject> _tmpObjects
        = new Dictionary<int, GameObject>()
    {
        { 0, null }, // exit
        { 1, null }, // enter
    };

    private bool _isOnInteract;

    public bool IsOnTrigger => !!InteractObject;

    public bool IsOnInteract
    {
        get => _isOnInteract;
        set { _isOnInteract = value; }
    }

    public InteractiveSO InteractiveItem
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

    public GameObject InteractObject
    {
        get => _interactObject;
        set
        {
            if (!IsOnInteract)
            {
                _interactObject = value;
            }
            else
            {
                if (value)
                {
                    _tmpObjects[1] = value;
                }
                else
                {
                    _tmpObjects[0] = _interactObject;
                }
            }
        }
    }

    private static InteractManager s_instance;
    public static InteractManager Instance
    {
        get
        {
            if (s_instance == null)
                s_instance = new InteractManager();
            return s_instance;
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

    public void StopEvent()
    {
        //todo:停止键鼠监测事件 停止正在进行的动作 动画
        Debug.Log("StopEvent");
        IsOnInteract = true;
        PlayerInput.Instance.ToggleFrozen(true);
    }

    public void ContinueEvent()
    {
        //todo:继续键鼠监测事件 继续原来的事件
        Debug.Log("ContinueEvent");
        IsOnInteract = false;
        PlayerInput.Instance.ToggleFrozen(false);

        if (_tmpObjects[0])
        {
            InteractObject = null;
            _tmpObjects[0] = null;
        }

        if (_tmpObjects[1])
        {
            InteractObject = _tmpObjects[1];
            _tmpObjects[1] = null;
        }
    }

    public void Interact()
    {
        if (IsOnTrigger && !IsOnInteract)
            if (_interactiveActions.ContainsKey(InteractiveItem.ItemType))
            {
                StopEvent();
                Debug.Log(InteractiveItem.ItemType);
                _interactiveActions[InteractiveItem.ItemType]();
            }
            else
                Debug.LogError(InteractiveItem.ItemType + " not add actions dictionary");
    }
}

