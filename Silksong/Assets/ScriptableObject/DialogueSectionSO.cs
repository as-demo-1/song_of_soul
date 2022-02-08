using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueSection", menuName = "Dialogue/DialogueSection")]
public class DialogueSectionSO : ScriptableObject
{
    [Tooltip("Dialogue")]
    [SerializeField] private List<DialogueSO> _dialogueList = default;
    [Tooltip("Dialogue status")]
    [SerializeField] private List<DialogueStatusSO> _dialogueStatusList = default;
    [Tooltip("Dialogue NPC")]
    [SerializeField] private string _dialoguename = default;
    [Tooltip("Dialogue NPC ID")]
    [SerializeField] private int _npcID = default;

    public List<DialogueSO> DialogueList => _dialogueList;
    public List<DialogueStatusSO> DialogueStatusList => _dialogueStatusList;
    public string NPCName => _dialoguename;
    public int NPCID => _npcID;
}
