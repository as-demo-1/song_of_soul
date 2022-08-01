using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "func", menuName = "NewInact/func")]
public class InactChildFuncSO : InactChildBaseSO
{
    public InactChildBaseSO NextChild => _nextChild;
}
