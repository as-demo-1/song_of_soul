using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "container", menuName = "NewInact/container")]
public class InactContainerSO : ScriptableObject, IInact
{
    [SerializeField]
    [HideInInspector]
    private int _icid = default;
    public int Icid => _icid;
    [SerializeField]
    private List<InactItemSO> _inactItemsSO = default;
    public List<InactItemSO> InactItemsSO => _inactItemsSO;
    [SerializeField]
    [HideInInspector]
    private InactWorldRootSO _parent;
    public InactWorldRootSO GetParent => _parent;

    public void Init(InactWorldRootSO parent)
    {
        _icid = InteractSystem.Instance.SetIcd(this);
        _parent = parent;
        foreach (InactItemSO inactItem in InactItemsSO)
        {
            inactItem.Init(this);
        }
    }
}
