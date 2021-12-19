
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Dialogue")]
public class DialogueSO : ScriptableObject
{
    [Tooltip("The id of the dialogue")]
    [SerializeField] private int _id = default;
    [Tooltip("The name of the NPC")]
    [SerializeField] private string _NPCName = default;
    [Tooltip("The next id of the dialogue")]
    [SerializeField] private int _nextID = default;
    [Tooltip("The detail of the dialogue")]
    [SerializeField] private string _detail = default;
    
    public int ID => _id;
    public string NPCName => _NPCName;
    public int NextID => _nextID;
    public string Detail => _detail;
}
