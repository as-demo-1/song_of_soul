
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueContainer", menuName = "Dialogue/DialogueContainer")]
public class DialogContainerSO : ScriptableObject
{
    [Tooltip("DialogueSection")]
    [SerializeField] private List<DialogueSectionSO> _dialogueSection = default;
    public List<DialogueSectionSO> DialogueSectionList => _dialogueSection;
}
