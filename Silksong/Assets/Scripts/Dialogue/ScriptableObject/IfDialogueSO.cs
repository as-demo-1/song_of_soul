using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IfDialogue", menuName = "Dialogue/IfDialogue")]
public class IfDialogueSO : ScriptableObject
{
    [Tooltip("The name of the dialogue")]
    [SerializeField] private string _conditionName = default;
    [Tooltip("The next id of the dialogue")]
    [SerializeField] private int _conditionID = default;
    [Tooltip("The detail of the dialogue")]
    [SerializeField] private List<int> _conditionNo = default;

    public string ConditionName => _conditionName;
    public int ConditionID => _conditionID;
    public List<int> ConditionNo => _conditionNo;
}
