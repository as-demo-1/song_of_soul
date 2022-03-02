using System.Collections.Generic;
using UnityEngine;

public abstract class InteractiveBaseSO : ScriptableObject
{
    [Tooltip("The id of interactive item")]
    [SerializeField] private int _interactiveID = default;
    [Tooltip("The name of the item")]
    [SerializeField] private string _name = default;

    public int InteractiveID => _interactiveID;
    public string Name => _name;
    public virtual EInteractiveItemType ItemType { get; }

    public void Init(InteractLoad load)
    {
        InitChild(load);
    }

    public void SetPosition(GameObject go, Vector3 pos)
    {
        SetPositionChild(go, pos);
    }

    public void Interact()
    {
        BeforeInteract();
    }

    // todo: 交互前的动画
    protected virtual void BeforeInteract()
    {
        DoInteract();
    }

    protected virtual void DoInteract()
    {
        AfterInteract();
    }

    // todo: 交互结束的动画
    protected virtual void AfterInteract()
    {
        Finish();
    }

    protected virtual void Finish()
    {
        InteractManager.Instance.Finish();
    }


    protected virtual void SetPositionChild(GameObject go, Vector3 pos)
    {
    }

    protected virtual GameObject InitChild(InteractLoad load)
    {
        return default;
    }
}
