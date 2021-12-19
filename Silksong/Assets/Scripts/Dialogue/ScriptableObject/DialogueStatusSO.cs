
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueStatus", menuName = "Dialogue/DialogueStatus")]
public class DialogueStatusSO : ScriptableObject
{
    [Tooltip("NPC ID")]
    [SerializeField] private int _npcID = default;
    [Tooltip("Start ID List")]
    [SerializeField] private List<int> _startTalkNo = default;
    [Tooltip("TalkFlags")]
    [SerializeField] private List<int> _talkFlags = default;
    [Tooltip("IfDialogueSO")]
    [SerializeField] private List<IfDialogueSO> _ifDialogueList = default;
    [Tooltip("Condition")]
    [SerializeField] private List<int> _condition = default;
    [Tooltip("定位到Condition中")]
    [SerializeField] private Dictionary<int, List<int>> _conditionNos = new Dictionary<int, List<int>>(); //前面是TalkFlags的对话编号，用于定位ConditionID,后面是ConditionNo，定位到Condition中
    [SerializeField] private bool _isInit = default;

    public int NPCID => _npcID;
    public List<int> StartTalkNo => _startTalkNo;
    public List<int> TalkFlags => _talkFlags;
    public List<IfDialogueSO> IfDialogueList => _ifDialogueList;
    public List<int> Condition => _condition;
    [HideInInspector]
    public Dictionary<int, List<int>> ConditionNos => _conditionNos;

    public bool IsInit => _isInit;

    public void SetConditionNos()
    {
        if (_isInit) return;
        foreach (IfDialogueSO item in IfDialogueList)
        {
            _conditionNos.Add(item.ConditionID, item.ConditionNo);
        }
        _isInit = true;
    }

    private void Awake()
    {
        SetConditionNos();
    }
}
