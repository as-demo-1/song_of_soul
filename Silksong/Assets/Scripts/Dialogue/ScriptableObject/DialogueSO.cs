
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
<<<<<<< Updated upstream
=======
    [Tooltip("The NPCID")]
    [SerializeField] int _npcid = default;
    [Tooltip("Type of Content")]
    [SerializeField] string _type = default;
>>>>>>> Stashed changes

    [Tooltip("控制这段对话的条件")]
    [SerializeField] private List<string> _StatusList = default;

    
    public int StartID => _startid;
    public int EndID => _endid;
<<<<<<< Updated upstream
=======
    public int NPCID => _npcid;
    public string Type => _type;
>>>>>>> Stashed changes
    public List<string> Content => _content;
    public List<string> StatusList => _StatusList;
}
