using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "root", menuName = "NewInact/root")]
public class InactWorldRootSO : ScriptableObject, IInact
{
    [SerializeField]
    [HideInInspector]
    private int _iwrid = default;
    public int Iwrid => _iwrid;
    [SerializeField]
    private List<InactContainerSO> _inactContainersSO = default;
    public List<InactContainerSO> InactContainersSO => _inactContainersSO;

    public void Init()
    {
        _iwrid = InteractSystem.Instance.SetIwrd(this);
        foreach (InactContainerSO inactContainer in InactContainersSO)
        {
            inactContainer.Init(this);
        }
    }
}
