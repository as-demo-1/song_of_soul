using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPC", menuName = "Interactive/NPC")]
public class NPCSO : InteractiveSO
{
    [Tooltip("The talk_coo offset of the NPC")]
    [SerializeField] private Vector3 _talk_coord = default;
    [Tooltip("The state list of the NPC")]
    [SerializeField] private List<NPC_State_SO_Config> _stateList = default;

    public Vector3 TalkCoord => _talk_coord;
    // todo:
    // npc动画
    public List<NPC_State_SO_Config> StateList => _stateList;
}
