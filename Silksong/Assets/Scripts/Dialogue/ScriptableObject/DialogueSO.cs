
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Dialogue")]
public class DialogueSO : ScriptableObject
{
    [Tooltip("The start id of the dialogue")]
    [SerializeField] private int _startid = default;
    [Tooltip("The end id of the dialogue")]
    [SerializeField] private int _endid = default;
    [Tooltip("The content of the dialogue")]
    [SerializeField] private List<string> _content = default;

    [Tooltip("控制这段对话的条件")]
    [SerializeField] private List<string> _StatusList = default;

    
    public int StartID => _startid;
    public int EndID => _endid;
    public List<string> Content => _content;
    public List<string> StatusList => _StatusList;
}
