using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "save", menuName = "NewInact/save")]
public class InactSave : InactChildFuncSO
{
    [SerializeField]
    private SaveSystem _save_system;
    public SaveSystem SaveSystem => _save_system;
    public override void DoInteract()
    {
        _status = EInteractStatus.DO_INTERACT;
        SaveSystem.SaveDataToDisk();
        Next();
    }
}
