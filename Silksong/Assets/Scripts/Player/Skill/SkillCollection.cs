using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillCollection", menuName = "ScriptableObjects/SkillCollection", order = 1)]
public class SkillCollection : ScriptableObject
{
    // potential singleton implementation

    public List<PlayerSkill> AllSkills;

}
