
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueContainer", menuName = "Dialogue/DialogueContainer")]
public class DialogContainerSO : ScriptableObject
{
    [Tooltip("All dialogue")]
    [SerializeField] private List<DialogueSO> _dialogueList = default;
    [Tooltip("Dialogue status")]
    [SerializeField] private List<DialogueStatusSO> _dialogueStatusList = default;
    public List<DialogueSO> DialogueList => _dialogueList;
    public List<DialogueStatusSO> DialogueStatusList => _dialogueStatusList;
}
