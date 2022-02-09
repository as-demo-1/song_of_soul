
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueStatus", menuName = "Dialogue/DialogueStatus")]
public class DialogueStatusSO : ScriptableObject
{
    [Tooltip("ConditionName")]
    [SerializeField] private string _conditionname = default;
    [Tooltip("ConditionID")]
    [SerializeField] private string _conditionid = default;
    [Tooltip("是否达成")]
    [SerializeField] private bool _judge = default;

    public bool Judge => _judge;
    public string ConditionName => _conditionname;
    public string ConditionID => _conditionid;


    //public bool IsInit => _isInit;

}
