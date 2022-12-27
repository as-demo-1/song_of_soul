using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum SkillName
{
    FlameGeyser,
    ShadowBlade,
    LightningChain,
    ArcaneBlast,
    IceStorm
}

public abstract class SoulSkill : Hitter
{
    public SkillName skillName;
    public uint constPerSec = 0;
    public uint constPerAttack = 0;
    protected PlayerCharacter playerCharacter;
    protected PlayerController playerController;
    public String atcEventName = "Soul Skill ATK";
    protected abstract PlayerCharacter GetPlayerCharacter();
    protected abstract PlayerController GetPlayerController();
    public void Awake()
    {
        playerCharacter = GetPlayerCharacter();
        playerController = GetPlayerController();
    }
}
