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

    public Vector3 InteractiveItemPos => _interactiveItemPos;
    public int InteractiveItemID => _interactiveItemID;
    public EFuncInteractItemType FuncInteractItemType => _funcInteractItemType;

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

    public void SetItemType(EInteractiveItemType itemType)
    {
        _itemType = itemType;
    }

    public void SetIsOnTrigger(bool isOnTrigger)
    {
        _isOnTrigger = isOnTrigger;
    }

    public void SetInteractiveItemPos(Vector3 interactiveItemPos)
    {
        _interactiveItemPos = interactiveItemPos;
    }

    public void SetInteractiveItemID(int interactiveItemID)
    {
        _interactiveItemID = interactiveItemID;
    }

    public void SetFuncInteractItemType(EFuncInteractItemType funcInteractItemType)
    {
        _funcInteractItemType = funcInteractItemType;
    }



    public void Interact()
    {
        if (_isOnTrigger)
            _interactiveAction[_itemType]();
    }
}
