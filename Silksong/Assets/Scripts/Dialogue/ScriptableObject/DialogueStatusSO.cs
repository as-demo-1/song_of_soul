
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueStatus", menuName = "Dialogue/DialogueStatus")]
public class DialogueStatusSO : ScriptableObject
{
    [Tooltip("ConditionName")]
    [SerializeField] private string _conditionname = default;
<<<<<<< Updated upstream
    [Tooltip("ConditionID")]
    [SerializeField] private string _conditionid = default;
=======
>>>>>>> Stashed changes
    [Tooltip("是否达成")]
    [SerializeField] private bool _judge = default;

    public bool Judge => _judge;
    public string ConditionName => _conditionname;
<<<<<<< Updated upstream
    public string ConditionID => _conditionid;
=======
>>>>>>> Stashed changes


    //public bool IsInit => _isInit;

}
