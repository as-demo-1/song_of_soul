using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractManager
{
    private EInteractiveItemType _itemType;
    private bool _isOnTrigger;

    private EFuncInteractItemType _funcInteractItemType;
    private Vector3 _interactiveItemPos;
    private int _interactiveItemID;

    public EInteractiveItemType ItemType
    {
        get => _itemType;
        set { _itemType = value; }
    }

    public bool IsOnTrigger
    {
        get => _isOnTrigger;
        set { _isOnTrigger = value; }
    }

    public Vector3 InteractiveItemPos
    {
        get => _interactiveItemPos;
        set { _interactiveItemPos = value; }
    }

    public int InteractiveItemID
    {
        get => _interactiveItemID;
        set { _interactiveItemID = value; }
    }
    public EFuncInteractItemType FuncInteractItemType
    {
        get => _funcInteractItemType;
        set { _funcInteractItemType = value; }
    }

    private Dictionary<EInteractiveItemType, Action> _interactiveAction
            = new Dictionary<EInteractiveItemType, Action>()
    {
        { EInteractiveItemType.NONE,        NormalInteract.Instance.Interact    },
        { EInteractiveItemType.DIALOG,      DialogInteract.Instance.Interact    },
        { EInteractiveItemType.JUDGE,       JudgeInteract.Instance.Interact     },
        { EInteractiveItemType.FULLTEXT,    FullTextInteract.Instance.Interact  },
    };

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


    public void Interact()
    {
        if (_isOnTrigger)
            _interactiveAction[_itemType]();
    }
}
