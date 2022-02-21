using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSkill
{
    public enum SkillName
    {
        None,
        VengefulSpirit,
        DesolateDive,
        DescendingDark,
    }
    public SkillName Name;

    public Transform SkillPrefab;

    public string Description;

    public int Damage;

    public int ManaCost; 

    public float CoolDown;

}
